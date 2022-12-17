using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
#if OS_LINUX
    using Linux.MemoryReader;
#else
    using Windows.MemoryReader;
#endif
class Program
{
    public static void Main(string[] args)
    {
        string processName = "retroarch";

        MemoryReader memoryReader = new MemoryReader();
        memoryReader.AttachProccess(processName);

        bool infinityLoop = true;
        int loopInterval = 5000;

        Logger.logLevel = 4;

        try
        {
            if(infinityLoop)
            {
                while(true)
                {
                    Run(memoryReader);
                    Thread.Sleep(loopInterval);
                    Console.Clear();
                }
            }
            else
            {
                Run(memoryReader);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex.Message);
            return;
        }
    }

    public static void Run(MemoryReader memoryReader)
    {
        memoryReader.ReadMemory();

        Logger.Debug("===================================");
        Logger.Debug("Json value grabbed: {0}", memoryReader.GetResult());
        Logger.Debug("===================================");

        PresenceStatus? presence = JsonSerializer.Deserialize<PresenceStatus>(memoryReader.GetResult());

        int timestamp = Convert.ToInt32(presence?.args?.activity?.timestamps?.start);
        DateTime? start = new DateTime(1970, 1, 1).AddSeconds(timestamp).ToLocalTime();

        Logger.Info(
            "=============== User Activity =============== \n" +
            "State --> {0}\n" +
            "Details --> {1}\n" +
            "Started --> {2}\n" +
            "Platform Icon --> {3}\n" +
            "Platform Text --> {4}\n" +
            "Status Icon --> {5}\n" +
            "Status Text --> {6}\n"
        , presence?.args?.activity?.state
        , presence?.args?.activity?.details
        , start
        , presence?.args?.activity?.assets?.large_image
        , presence?.args?.activity?.assets?.large_text
        , presence?.args?.activity?.assets?.small_image
        , presence?.args?.activity?.assets?.small_text
        );
    }
}