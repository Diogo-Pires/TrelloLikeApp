using Application.DTOs;
using Application.User.Mappers;
using Domain.Entities;

namespace Application.Tests.Mappers;

public class UserMapperTests
{
    [Fact]
    public void ToEntity_ShouldMapCorrectly()
    {
        //Arrange
        var name = "Test User";
        var email = "test@example.com";
        var dto = new UserDTO(name, email);

        //Act
        var entity = UserMapper.ToEntity(dto);

        //Assert
        Assert.Equal(name, entity.Name);
        Assert.Equal(email, entity.Id);
    }

    [Fact]
    public void ToDTO_ShouldMapCorrectly()
    {
        //Arrange
        var entity = new User("Test User", "12345");

        //Act
        var dto = UserMapper.ToDTO(entity);

        //Assert
        Assert.Equal(entity.Name, dto.Name);
        Assert.Equal(entity.Id, dto.Id);
    }
}
