using Application.User.DTOs;
using Application.User.Mappers;
using Domain.User;
using FluentAssertions;

namespace Application.Tests.User.Mappers;

public class UserMapperTests
{
    [Fact]
    public void ToEntity_Should_Map_DTO_To_Entity_Correctly()
    {
        // Arrange
        var dto = new UserEntityDTO
        {
            Name = " John Doe ",
            Id = " 123 ",
            GoogleId = "google-xyz"
        };

        // Act
        var entity = UserMapper.ToEntity(dto);

        // Assert
        entity.Name.Should().Be("John Doe");
        entity.Id.Should().Be("123");
        entity.GoogleId.Should().Be("google-xyz");
    }

    [Fact]
    public void ToDTO_Should_Map_Entity_To_DTO_Correctly()
    {
        // Arrange
        var entity = new UserEntity("John Doe", "123", "google-xyz");

        // Act
        var dto = UserMapper.ToDTO(entity);

        // Assert
        dto.Name.Should().Be("John Doe");
        dto.Id.Should().Be("123");
        dto.GoogleId.Should().Be("google-xyz");
    }
}
