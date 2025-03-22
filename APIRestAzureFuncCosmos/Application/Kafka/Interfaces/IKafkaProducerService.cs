namespace Application.Kafka.Interfaces;

public interface IKafkaProducerService
{
    System.Threading.Tasks.Task ProduceAsync<T>(string topic, T message);
}
