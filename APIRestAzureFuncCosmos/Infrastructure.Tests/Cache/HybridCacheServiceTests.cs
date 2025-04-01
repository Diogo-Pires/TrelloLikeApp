using FluentAssertions;
using Moq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Infrastructure.Cache;
using Infrastructure.Exceptions;
using Shared.Consts;

namespace Infrastructure.Tests.Cache
{
    public class HybridCacheServiceTests
    {
        private readonly Mock<IMemoryCache> _memoryCacheMock;
        private readonly Mock<IDistributedCache> _distributedCacheMock;
        private readonly HybridCacheService _hybridCacheService;

        public HybridCacheServiceTests()
        {
            _memoryCacheMock = new Mock<IMemoryCache>();
            _distributedCacheMock = new Mock<IDistributedCache>();
            _hybridCacheService = new HybridCacheService(_memoryCacheMock.Object, _distributedCacheMock.Object);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetOrSetAsync_Should_Return_Cached_Value_From_Memory()
        {
            // Arrange
            var key = "testKey";
            var tag = "testTag";
            var fullKey = $"{key}:1";
            var expectedValue = "cachedValue";
            object? cachedObject = expectedValue;

            _memoryCacheMock.Setup(m => m.TryGetValue(fullKey, out cachedObject))
                .Returns(true);

            // Act
            var result = await _hybridCacheService.GetOrSetAsync(key, tag, () => System.Threading.Tasks.Task.FromResult<string?>(null));

            // Assert
            result.Should().Be(expectedValue);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetOrSetAsync_Should_Return_Cached_Value_From_Redis()
        {
            // Arrange
            var key = "testKey";
            var tag = "testTag";
            var fullKey = $"{key}:1";
            var expectedValue = "cachedValue";
            var serializedValue = JsonConvert.SerializeObject(expectedValue);

            _memoryCacheMock.Setup(m => m.TryGetValue(fullKey, out It.Ref<object>.IsAny)).Returns(false);
            _distributedCacheMock.Setup(d => d.GetStringAsync(fullKey, default)).ReturnsAsync(serializedValue);

            // Act
            var result = await _hybridCacheService.GetOrSetAsync(key, tag, () => System.Threading.Tasks.Task.FromResult<string?>(null));

            // Assert
            result.Should().Be(expectedValue);
            _memoryCacheMock.Verify(m => m.Set(fullKey, expectedValue, It.IsAny<TimeSpan>()), Times.Once);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetOrSetAsync_Should_Fetch_From_DB_If_Not_Cached()
        {
            // Arrange
            var key = "testKey";
            var tag = "testTag";
            var fullKey = $"{key}:1";
            var expectedValue = "dbValue";

            _memoryCacheMock.Setup(m => m.TryGetValue(fullKey, out It.Ref<object>.IsAny)).Returns(false);
            _distributedCacheMock.Setup(d => d.GetStringAsync(fullKey, default)).ReturnsAsync((string?)null);

            // Act
            var result = await _hybridCacheService.GetOrSetAsync(key, tag, () => System.Threading.Tasks.Task.FromResult<string?>(expectedValue));

            // Assert
            result.Should().Be(expectedValue);
            _distributedCacheMock.Verify(d => d.SetStringAsync(fullKey, It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>(), default), Times.Once);
            _memoryCacheMock.Verify(m => m.Set(fullKey, expectedValue, It.IsAny<TimeSpan>()), Times.Once);
        }

        [Fact]
        public async System.Threading.Tasks.Task SetIfNotExistsAsync_Should_Set_Cache_If_Not_Exists()
        {
            // Arrange
            var key = "testKey";
            var tag = "testTag";
            var fullKey = $"{key}:1";
            var data = "newData";

            _distributedCacheMock.Setup(d => d.GetStringAsync(fullKey, default)).ReturnsAsync((string?)null);
            _memoryCacheMock.Setup(m => m.TryGetValue(fullKey, out It.Ref<object>.IsAny)).Returns(false);

            // Act
            await _hybridCacheService.SetIfNotExistsAsync(key, tag, data);

            // Assert
            _distributedCacheMock.Verify(d => d.SetStringAsync(fullKey, It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>(), default), Times.Once);
            _memoryCacheMock.Verify(m => m.Set(fullKey, data, It.IsAny<TimeSpan>()), Times.Once);
        }

        [Fact]
        public async System.Threading.Tasks.Task RemoveAsync_Should_Remove_Keys_From_Cache()
        {
            // Arrange
            var key = "testKey";
            var tag = "testTag";
            var fullKey = $"{key}:1";

            // Act
            await _hybridCacheService.RemoveAsync(key, tag);

            // Assert
            _memoryCacheMock.Verify(m => m.Remove(fullKey), Times.Once);
            _distributedCacheMock.Verify(d => d.RemoveAsync(fullKey, default), Times.Once);
        }

        [Fact]
        public async System.Threading.Tasks.Task IncrementVersion_Should_Update_Tag_Version()
        {
            // Arrange
            var tag = "testTag";
            var versionKey = $"tag:{tag}version";
            object? cachedVersion = null;

            _memoryCacheMock.Setup(m => m.TryGetValue(versionKey, out cachedVersion)).Returns(false);
            _distributedCacheMock.Setup(d => d.GetStringAsync(versionKey, default)).ReturnsAsync("1");

            // Act
            await _hybridCacheService.IncrementVersion(tag);

            // Assert
            _memoryCacheMock.Verify(m => m.Set(versionKey, "2", It.IsAny<TimeSpan>()), Times.Once);
            _distributedCacheMock.Verify(d => d.SetStringAsync(versionKey, "2", It.IsAny<DistributedCacheEntryOptions>(), default), Times.Once);
        }

        [Fact]
        public async System.Threading.Tasks.Task IncrementVersion_Should_Throw_Exception_On_Invalid_Version()
        {
            // Arrange
            var tag = "testTag";
            var versionKey = $"tag:{tag}version";
            object? cachedVersion = null;

            _memoryCacheMock.Setup(m => m.TryGetValue(versionKey, out cachedVersion)).Returns(false);
            _distributedCacheMock.Setup(d => d.GetStringAsync(versionKey, default)).ReturnsAsync("invalid");

            // Act
            Func<System.Threading.Tasks.Task> act = async () => await _hybridCacheService.IncrementVersion(tag);

            // Assert
            await act.Should().ThrowAsync<CacheException>().WithMessage(Constants.CACHE_VERSION_WRONG_FORMAT);
        }
    }
}
