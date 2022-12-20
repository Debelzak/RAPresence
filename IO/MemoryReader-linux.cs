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
        private byte[] searchPattern = {0x7b,0x22,0x6e,0x6f,0x6e,0x63,0x65,0x22,0x3a};  //{"nonce":

        public void AttachProccess(string processName)
        {
            Process? process = Process.GetProcessesByName(processName).FirstOrDefault();
            
            if(process is null)
            {
                throw new Exception("Process not found");
            }

            this.process = process;
        }

        public void ReadMemory(ref long address, int bufferLength)
        {  
            if (process is null)
            throw new Exception(string.Format("No process defined"));

            if(address == 0x00)
            {
                Logger.Info("Address is undefined, searching...");
                address = FindAddress();
            }

            using (FileStream fs = new FileStream(@"/proc/"+this.process.Id+"/mem", FileMode.Open, FileAccess.Read))
            {
                fs.Seek(address, SeekOrigin.Begin);

                try
                {
                    byte[] buffer = new byte[bufferLength];

                    Logger.Info("Reading {0} bytes at address 0x{1}.", bufferLength, address.ToString("x"));
                    int bytesRead = fs.Read(buffer, 0, buffer.Length);

                    if(Encoding.UTF8.GetString(buffer).Contains(Encoding.UTF8.GetString(searchPattern)))
                    {
                        List<byte> returnBuffer = new List<byte>();
                        for(int index=0; index<buffer.Length; index++)
                        {
                            if(buffer[index] == 0x00)
                            {
                                break;
                            }
                            
                            returnBuffer.Add(buffer[index]);
                        }

                        this.result = Encoding.UTF8.GetString(returnBuffer.ToArray());
                    }
                    else
                    {
                        throw new Exception(string.Format("String could not be found at address 0x{0}", address.ToString()));
                    }

                }
                catch (Exception ex)
                {
                    Logger.Error(ex.Message);
                }
            }
        }

        private Int64 FindAddress()
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

                                Logger.Info("{0} bytes read.", bytesRead);

                                // If the start of the string is found, we copy the part we want to another variable and break the loop
                                if(Encoding.UTF8.GetString(regionBuffer).Contains(Encoding.UTF8.GetString(searchPattern)))
                                {
                                    int offset = IndexOf(regionBuffer, searchPattern);
                                    long address = start + offset;

                                    Logger.Info("String found at address: 0x{0}", address.ToString("x"));

                                    return address;
                                }

                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex.Message);
                            }
                        }
                    }
                }
            }

            throw new Exception("Could not find the string address");
        }

        public string GetResult()
        {
            if(this.result is not null) return this.result;
            throw new Exception("It was not possible to catch the value");
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