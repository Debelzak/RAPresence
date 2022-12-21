using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

class TcpServer
{
    TcpListener? listener;
    NetworkStream? stream;
    System.Net.Sockets.TcpClient? client;

    public void Start(string? acceptIp, int port)
    {
        IPAddress listen = (IPAddress.TryParse(acceptIp, out _)) ? IPAddress.Parse(acceptIp) : IPAddress.Any;

        this.listener = new TcpListener(listen, port);
        this.listener.Start();

        Logger.Info("[SERVER] Listening connections from {0}:{1}", listen.ToString(), port);
        while(true)
        {
            client = listener.AcceptTcpClient();
            stream = client.GetStream();

            byte[] buffer = new byte[client.ReceiveBufferSize];
            stream.Read(buffer, 0, buffer.Length);

            string request = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            request = request.Trim('\0');

            if(request == "REQUEST")
            {
                Logger.Info("[SERVER] Received new request from {0}", client.Client.RemoteEndPoint);
                
                Program.Run();

                byte[] response = Encoding.UTF8.GetBytes(Program.LatestResult);
                stream.Write(response, 0, response.Length);
            }

            stream.Flush();
        }
    }

    public void Reset()
    {
        this.stream?.Dispose();
        this.client?.Close();
        this.listener?.Stop();
    }
}