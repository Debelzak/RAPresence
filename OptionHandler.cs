using System.Net;
static class OptionHandler
{
    public static void Run(string[] args, ref string ipAddress, ref int port , ref bool tcpClient, ref bool tcpServer, ref bool discord)
    {
        string[] availableOptions = {
            "-h", "--help",
            "-l", "--loglevel",
            "-c", "--client",
            "-s", "--server",
            "-i", "--ip", 
            "-p", "--port",
            "-d", "--discord"
        };

        var help = () => {
            Console.WriteLine("Tool made in order to catch retroarch's rich presence directly from memory.");
            Console.WriteLine("Usage: {0} [options]", System.Reflection.Assembly.GetCallingAssembly().GetName().Name);
            Console.WriteLine("Options: ");
            Console.WriteLine("       -h|--help: Shows this message.");
            Console.WriteLine("       -l|--loglevel <value>: Set the log verbosity level, from 0 to 3. (Default 1)");
            Console.WriteLine("       -c|--client: Uses TCP connections to send the json to a server.");
            Console.WriteLine("       -s|--server: Act as an TCP Server to receive info remotely.");
            Console.WriteLine("       -i|--ip <value>: Sets the IP the client will attempt to connect to, or the server will accepts connections from. (Default: 127.0.0.1)");
            Console.WriteLine("       -p|--port <value>: Sets the port the client will attempt to connect to, or the server will accepts connections from. (Default: 2000)");
            Console.WriteLine("       -d|--discord: Enable Discord Rich Presence. (Default: Off)");
            Environment.Exit(1);
        };

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
                    Logger.Error("{0}: invalid option.", optionFlag);
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
                    throw new Exception(string.Format("{0} must contain a input value.", optionFlag));

                    if(!int.TryParse(optionValue, out _))
                    throw new Exception(string.Format("{0} {1} is not a valid input.", optionFlag, optionValue));

                    if(int.Parse(optionValue) < 0 || int.Parse(optionValue) > 3)
                    throw new Exception(string.Format("{0} {1} is not a valid input.", optionFlag, optionValue));

                    Logger.logLevel = int.Parse(optionValue);
                }

                // Option [--client | -c]
                if(optionFlag == "--client" || optionFlag == "-c")
                {
                    tcpClient = true;
                }

                // Option [--server | -s]
                if(optionFlag == "--server" || optionFlag == "-s")
                {
                    tcpServer = true;
                }

                // Option [--ip | -i]
                if(optionFlag == "--ip" || optionFlag == "-i")
                {
                    if(!IPAddress.TryParse(optionValue, out _))
                    throw new Exception(string.Format("{0} {1} is not a valid input.", optionFlag, optionValue));

                    ipAddress = IPAddress.Parse(optionValue).ToString();
                }

                // Option [--port | -p]
                if(optionFlag == "--port" || optionFlag == "-p")
                {
                    if(!int.TryParse(optionValue, out _))
                    throw new Exception(string.Format("{0} {1} is not a valid input.", optionFlag, optionValue));

                    if(int.Parse(optionValue) < 1000 || int.Parse(optionValue) > 65535)
                    throw new Exception(string.Format("You must set a port value between 1000 and 65535."));

                    port = int.Parse(optionValue);
                }

                // Option [--server | -s]
                if(optionFlag == "--discord" || optionFlag == "-d")
                {
                    discord = true;
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