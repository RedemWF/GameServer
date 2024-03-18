using GameServer.Servers;

namespace GameServer;

class Program
{
    static Server server;
    static void Main(string[] args)
    {
        server = new Server("127.0.0.1", 6688);

        foreach (var s in args)
        {
            AppCommand(s);
        }

        while (true)
        {
            string? v = Console.ReadLine();
            switch (v)
            {
                case "exit" or "e":
                    Environment.Exit(0);
                    break;
                default:
                    AppCommand(v);
                    break;
            }
        }
    }
    private static void AppCommand(string command)
    {
        switch (command)
        {
            case "":
                //TODO:
                Console.WriteLine("GetHelp:");
                break;
            case "-start" or "start":
                server.Start();
                break;
            default:
                //TODO:
                Console.WriteLine("GetHelp:");
                break;
        }
    }
}