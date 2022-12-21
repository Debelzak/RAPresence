using System;
using System.IO;
using System.Text;
using System.Net;
using System.Threading.Tasks;

class HttpServer
{
    public static HttpListener listener = new HttpListener();
    public static int pageViews = 0;
    public static int requestCount = 0;

    public static async Task HandleIncomingConnections()
    {
        bool runServer = true;

        while (runServer)
        {
            // Will wait here until we hear from a connection
            HttpListenerContext ctx = await listener.GetContextAsync();

            // Peel out the requests and response objects
            HttpListenerRequest? req = ctx.Request;
            HttpListenerResponse resp = ctx.Response;

            // Make sure we don't increment the page views counter if `favicon.ico` is requested
            if (req?.Url?.AbsolutePath != "/favicon.ico")
                pageViews += 1;

            // Write the response info
            string disableSubmit = !runServer ? "disabled" : "";
            byte[] data = Encoding.UTF8.GetBytes(Program.LatestResult);
            resp.ContentType = "application/json";
            resp.ContentEncoding = Encoding.UTF8;
            resp.ContentLength64 = data.LongLength;

            // Write out to the response stream (asynchronously), then close it
            await resp.OutputStream.WriteAsync(data, 0, data.Length);
            resp.Close();
        }
    }

    public static void Start()
    {
        // Create a Http server and start listening for incoming connections
        listener.Prefixes.Add("http://+:8000/");
        listener.Start();
        Logger.Info("[HTTPServer] Listening for connections on 8000");

        // Handle requests
        Task listenTask = HandleIncomingConnections();
        listenTask.GetAwaiter().GetResult();

        listener.Close();
    }
}