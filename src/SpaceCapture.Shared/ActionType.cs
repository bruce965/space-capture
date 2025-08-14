using System.Text.Json.Serialization;

namespace SpaceCapture.Shared;

[JsonConverter(typeof(JsonStringEnumConverter<ActionType>))]
public enum ActionType
{
    FleetCreate,
    FleetDisband,
}

[JsonConverter(typeof(JsonStringEnumConverter<EventType>))]
public enum EventType
{
    FleetCreate,
    FleetDisband,
}
