using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Windows.MemoryReader
{
    class MemoryReader
    {
        private Process process = new Process();
        private string? result;

        const int PROCESS_WM_READ = 0x0010;

        [DllImport("kernel32")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32")]
        private static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

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
            IntPtr processHandle = OpenProcess(PROCESS_WM_READ, false, this.process.Id);

            int bytesRead = 0;
            Int32 address = 0x0142C0C8;
            byte[] buffer = new byte[1024]; //Dobrar valor para unicode

            ReadProcessMemory(((int)processHandle), address, buffer, buffer.Length, ref bytesRead);
            
            List<Byte> newBuffer = new List<byte>();
            for(int i=0; i < buffer.Length; i++)
            {
                if(buffer[i] == 0x00)
                {
                    break;
                }

                newBuffer.Add(buffer[i]);
            }

            byte[] result = newBuffer.ToArray();

            this.result = Encoding.UTF8.GetString(result);
        }

        public string GetResult()
        {
            if(this.result is not null) return this.result;

            return "Não foi possível interceptar o valor";
        }
    }

}