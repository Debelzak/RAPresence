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
        HandleOptions(args);
        string processName = "retroarch";

        try 
        {
            MemoryReader memoryReader = new MemoryReader();
            memoryReader.AttachProccess(processName);
            Run(memoryReader);
        }
        catch (Exception ex)
        {
            Logger.Error(ex.Message);
            return;
        }
    }

    private static void Run(MemoryReader memoryReader)
    {
        memoryReader.ReadMemory();

        PresenceStatus? presence = JsonSerializer.Deserialize<PresenceStatus>(memoryReader.GetResult());

        int timestamp = Convert.ToInt32(presence?.args?.activity?.timestamps?.start);
        DateTime? start = new DateTime(1970, 1, 1).AddSeconds(timestamp).ToLocalTime();

        Logger.Debug("State --> {0}", presence?.args?.activity?.state);
        Logger.Debug("Details --> {0}", presence?.args?.activity?.details);
        Logger.Debug("Started --> {0}", start);
        Logger.Debug("Platform Icon --> {0}", presence?.args?.activity?.assets?.large_image);
        Logger.Debug("Platform Text --> {0}", presence?.args?.activity?.assets?.large_text);
        Logger.Debug("Status Icon --> {0}", presence?.args?.activity?.assets?.small_image);
        Logger.Debug("Status Text --> {0}", presence?.args?.activity?.assets?.small_text);

        Console.WriteLine(memoryReader.GetResult());
    }

    private static void HandleOptions(string[] args)
    {
        var help = () => {
            Console.WriteLine("Tool made in order to catch retroarch's rich presence directly from memory.");
            Console.WriteLine("Usage: {0} [options]", System.Reflection.Assembly.GetCallingAssembly().GetName().Name);
            Console.WriteLine("Options: ");
            Console.WriteLine("       -h|--help: Shows this message.");
            Console.WriteLine("       -l|--loglevel <value>: Set the log verbosity level, from 0 to 3.");
            Environment.Exit(1);
        };

        string[] availableOptions = {"-h", "--help" , "-l", "--loglevel"};
        try 
        {
            for(int index=0; index<args.Length; index++)
            {
                string optionFlag = args[index];
                string? optionValue = null;

                if(args.Length > index+1) optionValue = args[index+1];

                if(!optionFlag.Contains("-"))
                {
                    continue;
                }

                if(!availableOptions.Contains(optionFlag))
                {
                    Logger.Error("{0}: invalid option", optionFlag);
                    help();
                }

                // Option [--help | -h]
                if(optionFlag == "--help" || optionFlag == "-h")
                {
                    help();
                }

                // Option [--loglevel | -l]
                if(optionFlag == "--loglevel" || optionFlag == "-l")
                {
                    if(optionValue is null)
                    throw new Exception(string.Format("{0} must contain a input value", optionFlag));

                    if(!int.TryParse(optionValue, out _))
                    throw new Exception(string.Format("{0} {1} is not a valid input", optionFlag, optionValue));

                    if(int.Parse(optionValue) < 0 || int.Parse(optionValue) > 3)
                    throw new Exception(string.Format("{0} {1} is not a valid input", optionFlag, optionValue));

                    Logger.logLevel = int.Parse(optionValue);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex.Message);
            Environment.Exit(1);
        }
    }
}