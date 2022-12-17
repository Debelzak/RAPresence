using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Windows.MemoryReader
{
    class MemoryReader
    {
        [DllImport("kernel32")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);
        [DllImport("kernel32")]
        private static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

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
            IntPtr processHandle = OpenProcess(0x0010, false, this.process.Id);

            int bytesRead = 0;
            Int32 address = 0x0142C0C8;     //Memory address where the string is located
            byte[] regionBuffer = new byte[1024]; //Bytes must be doubled for unicode

            ReadProcessMemory(((int)processHandle), address, regionBuffer, regionBuffer.Length, ref bytesRead);
            
            // If the start of the string is found, we copy the part we want to another variable
            if(Encoding.UTF8.GetString(regionBuffer).Contains("{\"nonce\":"))
            {
                Logger.Info("String found at address: 0x{0}", address.ToString("x") );

                List<Byte> buffer = new List<byte>();
                for(int i=0; i < regionBuffer.Length; i++)
                {
                    if(regionBuffer[i] == 0x00)
                    {
                        break;
                    }

                    buffer.Add(regionBuffer[i]);
                }

                this.result = Encoding.UTF8.GetString(buffer.ToArray());

            }
        }

        public string GetResult()
        {
            if(this.result is not null) return this.result;

            throw new Exception("Não foi possível interceptar o valor");
        }
    }

}