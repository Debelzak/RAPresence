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

            IntPtr processHandle = OpenProcess(0x0010, false, this.process.Id);

            int bytesRead = 0;
            address = 0x0142C0C8;                 //Memory address where the string is located
            byte[] buffer = new byte[bufferLength]; //Bytes must be doubled for unicode

            Logger.Info("Reading {0} bytes at address 0x{1}.", bufferLength, address.ToString("x"));

            try
            {
                ReadProcessMemory(((int)processHandle), Convert.ToInt32(address), buffer, buffer.Length, ref bytesRead);
                
                if(Encoding.UTF8.GetString(buffer).Contains(Encoding.UTF8.GetString(searchPattern)))
                {
                    List<Byte> returnBuffer = new List<byte>();
                    for(int i=0; i < buffer.Length; i++)
                    {
                        if(buffer[i] == 0x00)
                        {
                            break;
                        }

                        returnBuffer.Add(buffer[i]);
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

        public string GetResult()
        {
            if(this.result is not null) return this.result;
            throw new Exception("It was not possible to catch the value");
        }
    }

}