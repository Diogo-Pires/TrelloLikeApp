using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Domain.Task.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum TaskEntityStatus
{
    Pending,
    InProgress,
    Completed,
    Cancelled
}