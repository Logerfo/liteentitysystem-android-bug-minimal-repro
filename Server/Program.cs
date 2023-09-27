using Shared;

namespace Server;

internal class Program
{
    static async Task Main(string[] args)
    {
        GameServer gameServer = new();
        using PeriodicTimer timer = new(TimeSpan.FromSeconds(1d / NetworkGeneral.FPS));
        while (await timer.WaitForNextTickAsync())
            gameServer.Update();
    }
}