using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
class Program
{
    public static void Main(string[] args)
    {
        MemoryReader memoryReader = new MemoryReader();

        string processName = "retroarch";

        try
        {
            memoryReader.AttachProccess(processName);
            memoryReader.ReadMemory(0x0142C0C8, 1024);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return;
        }

        Console.WriteLine("Json value grabbed: {0}", memoryReader.GetResult());
        Console.WriteLine("===================================");

        PresenceStatus? presence = JsonSerializer.Deserialize<PresenceStatus>(memoryReader.GetResult());

        int timestamp = Convert.ToInt32(presence?.args?.activity?.timestamps?.start);
        DateTime start = new DateTime(1970, 1, 1).AddSeconds(timestamp).ToLocalTime();

        Console.WriteLine(
            "---- User Activity ---- \n" +
            "State: {0}\n" +
            "Details: {1}\n" +
            "Started At: {2}\n" +
            "Platform Icon: {3}\n" +
            "Platform Text: {4}\n" +
            "Status Icon: {5}\n" +
            "Status Text: {6}\n"
        , presence?.args?.activity?.state
        , presence?.args?.activity?.details
        , start
        , presence?.args?.activity?.assets?.large_image
        , presence?.args?.activity?.assets?.large_text
        , presence?.args?.activity?.assets?.small_image
        , presence?.args?.activity?.assets?.small_text
        );

        Console.ReadKey();
    }
}