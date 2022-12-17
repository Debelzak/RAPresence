using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Linux.MemoryReader
{
    class MemoryReader
    {
        private Process process = new Process();
        private string? result;

        public void AttachProccess(string processName)
        {
            Process? process = Process.GetProcessesByName(processName).FirstOrDefault();
            if(process is null)
            {
                throw new Exception("Process not found");
            }

            this.process = process;
        }

        public void ReadMemory()
        {  
            //Creating stream reader and while 
            System.IO.StreamReader file = new System.IO.StreamReader(@"/proc/" + this.process.Id + "/maps");
            string? line;

            //Creating regex pattern to match each line we want from /proc/<pid>/maps file
            string pattern = @"([0-9A-Fa-f]+)-([0-9A-Fa-f]+) ([-r].*) (.*) (0 |.\[heap\])";

            //While loop to each line matched above, we'll loop each of the mapped memory blocks until we find what we want
            //It'll be probably at heap region
            while ((line = file.ReadLine()) != null)
            {
                Match match = Regex.Match(line, pattern);
                if (match.Success)
                {
                    //Set start, end and length of the region using the maps we got
                    string[] startend = (match.Value.Split(' '))[0].Split('-');
                    long start = Convert.ToInt64(startend[0], 16);
                    long end = Convert.ToInt64(startend[1], 16);
                    long len = end - start;

                    //Buffer the entire memory region
                    if (len > 0 && end > 0 && start > 0)
                    {
                        Logger.Info("Reading memory block: {0}", line);
                        using (FileStream fs = new FileStream(@"/proc/"+this.process.Id+"/mem", FileMode.Open, FileAccess.Read))
                        {
                            fs.Seek(start, SeekOrigin.Begin);
                            byte[] regionBuffer = new byte[len];

                            try 
                            {
                                int bytesRead = fs.Read(regionBuffer, 0, (int)len);

                                // If the start of the string is found, we copy the part we want to another variable and break the loop
                                if(Encoding.UTF8.GetString(regionBuffer).Contains("{\"nonce\":"))
                                {
                                    byte[] searchPattern = {0x7b,0x22,0x6e,0x6f,0x6e,0x63,0x65,0x22,0x3a};  //{"nonce":

                                    int offset = IndexOf(regionBuffer, searchPattern);

                                    Logger.Info("String found at address: 0x{0}", (start + offset).ToString("x") );

                                    List<byte> buffer = new List<byte>();
                                    for(int index=offset; index<len; index++)
                                    {
                                        if(regionBuffer[index] == 0x00)
                                        {
                                            break;
                                        }
                                        
                                        buffer.Add(regionBuffer[index]);
                                    }

                                    this.result = Encoding.UTF8.GetString(buffer.ToArray());

                                    return;
                                }

                                Logger.Info("{0} bytes read.", bytesRead);
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex.Message);
                            }
                        }
                    }
                }
            }
        }

        public string GetResult()
        {
            if(this.result is not null) return this.result;

            throw new Exception("Não foi possível interceptar o valor");
        }

        //Util
        public static int IndexOf(byte[] arrayToSearchThrough, byte[] patternToFind)
        {
            if (patternToFind.Length > arrayToSearchThrough.Length)
                return -1;
            for (int index = 0; index < arrayToSearchThrough.Length - patternToFind.Length; index++)
            {
                bool found = true;
                for (int j = 0; j < patternToFind.Length; j++)
                {
                    if (arrayToSearchThrough[index + j] != patternToFind[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                {
                    return index;
                }
            }
            return -1;
        }
    }

}