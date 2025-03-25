using Application.Kafka;
using Confluent.Kafka;
using CuttingEdges.Kafka.Policies;
using Microsoft.Extensions.Logging;
using Moq;

namespace Application.Tests.Kafka;

public class KafkaProducerServiceTests
{
    private readonly Mock<IProducer<string, string>> _producerMock;
    private readonly Mock<ILogger<KafkaResiliencePolicy>> _loggerMock;
    private readonly KafkaProducerService _producerService;

    public KafkaProducerServiceTests()
    {
        _producerMock = new Mock<IProducer<string, string>>();
        _loggerMock = new Mock<ILogger<KafkaResiliencePolicy>>();

        var resiliencePolicy = new KafkaResiliencePolicy(_loggerMock.Object);
        _producerService = new KafkaProducerService(resiliencePolicy, _producerMock.Object);
    }

    [Fact]
    public async System.Threading.Tasks.Task ProduceAsync_Should_Call_ProduceAsync_With_Correct_Topic_And_Message()
    {
        //Arrange
        var produceCount = 0;
        var flushCount = 0;

        _producerMock
            .Setup(m => m.Produce(It.IsAny<string>(), It.IsAny<Message<string, string>>(), It.IsAny<Action<DeliveryReport<string, string>>>()))
            .Callback<string, Message<string, string>, Action<DeliveryReport<string, string>>>((topic, message, action) =>
                {
                    var result = new DeliveryReport<string, string>
                    {
                        Topic = topic,
                        Partition = 0,
                        Offset = 0,
                        Error = new Error(ErrorCode.NoError),
                        Message = message
                    };

                    action.Invoke(result);
                    produceCount += 1;
                });

        _producerMock.Setup(m => m.Flush(It.IsAny<TimeSpan>())).Returns(0).Callback(() => flushCount += 1);

        //Act
        await _producerService.ProduceAsync("my-topic", new Message<string, string> { Value = "my-value" }, CancellationToken.None);
        var remaining = _producerMock.Object.Flush(TimeSpan.FromSeconds(10));

        //Assert
        Assert.Equal(1, flushCount);
        Assert.Equal(0, remaining);
    }
}