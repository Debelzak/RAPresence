using System;
using System.Text;
using System.Net.Sockets;

class TcpClient
{
    public void Send(string serverIp, int port, byte[] data)
    {
        connection:
        try 
        {
            Logger.Info("[CLIENT] Sending feedback to {0}:{1}...", serverIp, port);
            System.Net.Sockets.TcpClient client = new System.Net.Sockets.TcpClient();
            
            if(!client.ConnectAsync(serverIp, port).Wait(5000))
            {
                Logger.Info("Connection timeout, retrying...");
                client.Dispose();
                goto connection;
            }
            
            NetworkStream stream = client.GetStream();
            
            stream.Write(data, 0, data.Length);

            byte[] responseBuffer = new byte[client.ReceiveBufferSize];
            int bytesReceived = stream.Read(responseBuffer, 0, client.ReceiveBufferSize);

            string response = Encoding.UTF8.GetString(responseBuffer);
            response = response.Trim('\0');

            if(response == "SUCCESS")
            {
                Logger.Info("[CLIENT] The server responded with: {0}", response);
            }

            stream.Close();
        }
        catch (Exception ex)
        {
            Logger.Info("[CLIENT] Failed to send feedback: {0}", ex.Message);
        }
    }
}