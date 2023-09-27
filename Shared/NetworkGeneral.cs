namespace Shared;

public static class NetworkGeneral
{
    public const string SERVER_HOST = "10.0.2.2";
    public const int SERVER_PORT = 24000;
    public const int FPS = 60;
    public static int PacketTypesCount { get; } = Enum.GetValues(typeof(PacketType)).Length;
}
