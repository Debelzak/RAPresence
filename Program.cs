using System;
using System.Text;
using System.Text.Json;
#if OS_LINUX
    using Linux.MemoryReader;
#else
    using Windows.MemoryReader;
#endif

class Program
{
    private static string processName = "retroarch";
    private static bool discord = false;
    private static bool tcpServer = false;
    private static bool tcpClient = false;
    private static string ipAddress = "127.0.0.1";
    private static int port = 2000;
    private static long memoryAddress = 0x00;

    public static void Main(string[] args)
    {
        OptionHandler.Run(args, ref ipAddress, ref port, ref tcpClient, ref tcpServer, ref discord);

        start:
        try 
        {
            if(discord) DiscordPresence.Initialize();

            if(tcpServer)
            {
                TcpServer server = new TcpServer();
                server.Start(ipAddress, port);
            } else {
                Run();
            }

            if(discord) DiscordPresence.Dispose();
            
        }
        catch (Exception ex)
        {
            Logger.Error(ex.Message);
            if(tcpClient)
            {
                memoryAddress = 0x00;
                Thread.Sleep(1000);
                goto start;
            }
        }
    }

    private static void Run()
    {
        clientLoop:
        MemoryReader memoryReader = new MemoryReader();
        memoryReader.AttachProccess(processName);
        memoryReader.ReadMemory(ref memoryAddress, 1024);
        string result = memoryReader.GetResult();

        ParseResults(result);
        Logger.Debug(result);

        if(tcpServer)
        {
            TcpClient client = new TcpClient();
            client.Send(ipAddress, port, Encoding.UTF8.GetBytes(result));
            Thread.Sleep(5000);
            goto clientLoop;
        }
    }

    public static void ParseResults(string result)
    {
        PresenceStatus? presence = JsonSerializer.Deserialize<PresenceStatus>(result);

        Logger.Debug("State --> {0}", presence?.args?.activity?.state);
        Logger.Debug("Details --> {0}", presence?.args?.activity?.details);
        Logger.Debug("Started --> {0}", new DateTime(1970, 1, 1).AddSeconds(Convert.ToInt32(presence?.args?.activity?.timestamps?.start)));
        Logger.Debug("Platform Icon --> {0}", presence?.args?.activity?.assets?.large_image);
        Logger.Debug("Platform Text --> {0}", presence?.args?.activity?.assets?.large_text);
        Logger.Debug("Status Icon --> {0}", presence?.args?.activity?.assets?.small_image);
        Logger.Debug("Status Text --> {0}", presence?.args?.activity?.assets?.small_text);

        if(discord)
        {
            DiscordPresence.Update(presence);
        }
    }

}