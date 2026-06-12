using System.Text.Json.Serialization;

namespace AutoNex.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserRole
{
    Admin,
    Mechanic,
    Receptionist
}
