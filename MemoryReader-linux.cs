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
                throw new Exception("Processo não encontrado");
            }

            this.process = process;
        }

        public void ReadMemory()
        {
            System.IO.StreamReader file = new System.IO.StreamReader(@"/proc/" + this.process.Id + "/maps");
            string? line;
            while ((line = file.ReadLine()) != null)
            {
                string pattern = @"([0-9A-Fa-f]+)-([0-9A-Fa-f]+) ([-r].*) (.*) (0 |.\[heap\])";
                Match m = Regex.Match(line, pattern);
                if (m.Success)
                {
                    string[] startend = (m.Value.Split(' '))[0].Split('-');
                    long start = Convert.ToInt64(startend[0], 16);
                    long end = Convert.ToInt64(startend[1], 16);
                    long len = end - start;

                    // Buffer the entire memory region
                    if (len > 0 && end > 0 && start > 0)
                    {
                        Console.WriteLine(line);
                        using (FileStream fs = new FileStream(@"/proc/"+this.process.Id+"/mem", FileMode.Open, FileAccess.Read))
                        {
                            fs.Seek(start, SeekOrigin.Begin);
                            byte[] regionBuffer = new byte[len];

                            try 
                            {
                                int bytesRead = fs.Read(regionBuffer, 0, (int)len);
                                Console.WriteLine("{0} bytes read.", bytesRead);

                                // If string was found
                                if(Encoding.UTF8.GetString(regionBuffer).Contains("{\"nonce\":"))
                                {
                                    byte[] searchPattern = {0x7b,0x22,0x6e,0x6f,0x6e,0x63,0x65,0x22,0x3a};  //{"nonce":

                                    int offset = IndexOf(regionBuffer, searchPattern);

                                    List<byte> buffer = new List<byte>();
                                    for(int i=offset; i<len; i++)
                                    {
                                        if(regionBuffer[i] == 0x00)
                                        {
                                            break;
                                        }
                                        
                                        buffer.Add(regionBuffer[i]);
                                    }

                                    this.result = Encoding.UTF8.GetString(buffer.ToArray());

                                    return;
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("ERROR: {0}", ex.Message);
                            }
                        }
                    }
                }
            }
        }

        public string GetResult()
        {
            if(this.result is not null) return this.result;

            return "Não foi possível interceptar o valor";
        }

        //Util
        public static int IndexOf(byte[] arrayToSearchThrough, byte[] patternToFind)
        {
            if (patternToFind.Length > arrayToSearchThrough.Length)
                return -1;
            for (int i = 0; i < arrayToSearchThrough.Length - patternToFind.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < patternToFind.Length; j++)
                {
                    if (arrayToSearchThrough[i + j] != patternToFind[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                {
                    return i;
                }
            }
            return -1;
        }
    }

}