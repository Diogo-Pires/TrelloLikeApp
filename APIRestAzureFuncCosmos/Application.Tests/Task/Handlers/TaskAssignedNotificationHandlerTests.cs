using Application.Task.Commands;
using Application.Task.Handlers;
using CuttingEdges.Kafka.Interfaces;
using CuttingEdges.Kafka.Settings;
using Microsoft.Extensions.Options;
using Moq;

namespace Application.Tests.Task.Handlers;

public class TaskAssignedNotificationHandlerTests
{
    private readonly Mock<IKafkaProducerService> _kafkaProducerServiceMock;
    private readonly Mock<IOptions<KafkaSettings>> _optionsMock;
    private readonly TaskAssignedNotificationHandler _handler;

    public TaskAssignedNotificationHandlerTests()
    {
        _kafkaProducerServiceMock = new Mock<IKafkaProducerService>();
        _optionsMock = new Mock<IOptions<KafkaSettings>>();

        _optionsMock.Setup(o => o.Value).Returns(new KafkaSettings
        {
            TaskAssignedTopic = "test-topic",
            SessionTimeoutMs = 1000,
            StatisticsIntervalMs = 1000,
            TaskConsumerGroup = "test",
            TimeoutMs = 1000,
            Url = "test"
        });

        _handler = new TaskAssignedNotificationHandler(_kafkaProducerServiceMock.Object, _optionsMock.Object);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_Should_Call_ProduceAsync_With_Correct_Topic_And_Message()
    {
        // Arrange
        var command = new TaskAssignedNotificationCommand(Guid.NewGuid(), "test@gmail.com");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        //Assert
        _kafkaProducerServiceMock.Verify(
            producer => producer.ProduceAsync(
                "test-topic", 
                command,
                It.IsAny<CancellationToken>()
            ), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_Should_Throw_Exception_When_ProduceAsync_Fails()
    {
        // Arrange
        var command = new TaskAssignedNotificationCommand(Guid.NewGuid(), "test@gmail.com");

        _kafkaProducerServiceMock
            .Setup(producer => producer.ProduceAsync(It.IsAny<string>(), It.IsAny<TaskAssignedNotificationCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new System.Exception("Kafka produce failed"));

        // Act & Assert
        await Assert.ThrowsAsync<System.Exception>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_Should_Not_Call_ProduceAsync_If_Command_Is_Null()
    {
        // Arrange
        TaskAssignedNotificationCommand command = null;

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _kafkaProducerServiceMock.Verify(
            producer => producer.ProduceAsync(It.IsAny<string>(), It.IsAny<TaskAssignedNotificationCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
