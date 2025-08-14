using System.Text.Json.Serialization;

namespace SpaceCapture.Shared;

[JsonConverter(typeof(JsonStringEnumConverter<ShipType>))]
public enum ShipType
{
    [JsonStringEnumMemberName("simple")]
    Simple,

    [JsonStringEnumMemberName("colony")]
    Colony,

    [JsonStringEnumMemberName("cargo")]
    Cargo,

    [JsonStringEnumMemberName("interceptor")]
    Interceptor,

    [JsonStringEnumMemberName("bomber")]
    Bomber,
}
