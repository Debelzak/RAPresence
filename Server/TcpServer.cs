using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

class TcpServer
{
    public void Start(string? acceptIp, int port)
    {
        IPAddress listen = (IPAddress.TryParse(acceptIp, out _)) ? IPAddress.Parse(acceptIp) : IPAddress.Any;

        TcpListener listener = new TcpListener(listen, port);
        listener.Start();
        Logger.Info("[SERVER] Listening connections from {0}:{1}", listen.ToString(), port);

        while(true)
        {
            System.Net.Sockets.TcpClient client = listener.AcceptTcpClient();

            NetworkStream stream = client.GetStream();

            byte[] buffer = new byte[client.ReceiveBufferSize];
            stream.Read(buffer, 0, buffer.Length);

            string request = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            request = request.Trim('\0');

            Logger.Info("[SERVER] Received new data from {0}", client.Client.RemoteEndPoint);
            Program.ParseResults(request);
            Logger.Debug(request);

            string response = "SUCCESS";
            stream.Write(Encoding.UTF8.GetBytes(response), 0, response.Length);

            stream.Flush();
        }
    }
}