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
    private static bool httpServer = false;
    private static bool tcpClient = false;
    private static string ipAddress = "127.0.0.1";
    private static int port = 2000;
    private static long memoryAddress = 0x00;
    public static string LatestResult = "";

    public static void Main(string[] args)
    {
        OptionHandler.Run(args, ref ipAddress, ref port, ref tcpClient, ref tcpServer, ref discord, ref httpServer);

        if(httpServer) Task.Run(() => HttpServer.Start());

        TcpServer server = new TcpServer();

        if(discord) DiscordPresence.Initialize();

        start:
        try 
        {
            if(tcpServer) 
            {
                server.Start(ipAddress, port);
            }
            
            Run();
        }
        catch (Exception ex)
        {
            Logger.Error(ex.Message);
            if(tcpServer)
            {
                server.Reset();
                memoryAddress = 0x00;
                Thread.Sleep(5000);
                goto start;
            }
        }
    }

    public static void Run()
    {
        if(tcpClient)
        {
            ClientLoop();
            return;
        }

        MemoryReader memoryReader = new MemoryReader();
        memoryReader.AttachProccess(processName);
        memoryReader.ReadMemory(ref memoryAddress, 1024);
        string result = memoryReader.GetResult();
        
        LatestResult = result;

        ParseResults(result);
        Logger.Debug(result);
    }

    private static void ClientLoop()
    {
        while(true)
        {
            TcpClient client = new TcpClient();
            byte[] requestPacket = Encoding.UTF8.GetBytes("REQUEST");

            string? response = client.Request(ipAddress, port, requestPacket);
            if(response is not null)
            {
                ParseResults(response);
                Logger.Debug(response);
            }

            Thread.Sleep(1000);
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