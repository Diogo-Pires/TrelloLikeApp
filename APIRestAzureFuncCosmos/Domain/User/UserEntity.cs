using Newtonsoft.Json;

namespace Domain.User;

public class UserEntity(string name, string email)
{
    [JsonProperty("id")]
    public string Id { get; private set; } = email;


    [JsonProperty("name")]
    public string Name { get; private set; } = name;
}