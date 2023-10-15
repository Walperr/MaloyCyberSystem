namespace MQTTServer.Misc;

public static class Topics
{
    public const string DATA_TOPIC = "Device/Data";
    public const string CONNECTION_KEYS_TOPIC = "Device/ConnectionKeys";
    public const string FLAT_DEVICE_WILDCARD = "Device/+";
    public const string RECURSIVE_DEVICE_WILDCARD = "Device/#";
    public const string COMMANDS_TOPIC = "Device/Commands/";
    public const string NOTIFICATIONS_TOPIC = "Device/Notification/";
    public const string SERVER_NOTIFICATIONS_TOPIC = "Server/Notifications";
}