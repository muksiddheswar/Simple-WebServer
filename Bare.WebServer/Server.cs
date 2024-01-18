using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;



namespace Bare.WebServer
{
    public static class Server
    {
        public static int maxSimultaneousConnections = 20;
        private static Semaphore sem = new Semaphore(maxSimultaneousConnections, maxSimultaneousConnections);


        // Starts the web server.
        //public void Start(string websitePath, int port = 80, bool acquirePublicIP = false)
        public static void Start()
        {
            //OnError.IfNull(() => Console.WriteLine("Warning - the onError callback has not been initialized by the application."));

            //if (acquirePublicIP)
            //{
            //    publicIP = GetExternalIP();
            //    Console.WriteLine("public IP: " + publicIP);
            //}

            //router.WebsitePath = websitePath;
            List<IPAddress> localHostIPs = GetLocalHostIPs();
            //HttpListener listener = InitializeListener(localHostIPs, port);
            HttpListener listener = InitializeListener(localHostIPs);
            Start(listener);
        }

        // Returns list of IP addresses assigned to localhost network devices, such as hardwired ethernet, wireless, etc.
        private static List<IPAddress> GetLocalHostIPs()
        {
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());
            List<IPAddress> ret = host.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToList();

            return ret;
        }

        private static HttpListener InitializeListener(List<IPAddress> localhostIPs)
        {
            HttpListener listener = new HttpListener();
            //listener.Prefixes.Add("http://localhost/");
            //listener.Prefixes.Add("http://localhost:80/");
            listener.Prefixes.Add("http://localhost:4444/");

            // Listen to IP address as well.
            localhostIPs.ForEach(ip =>
            {
                Console.WriteLine("Listening on IP " + "http://" + ip.ToString() + "/");
                //listener.Prefixes.Add("http://" + ip.ToString() + "/");

                // For testing on a different port:
                listener.Prefixes.Add("https://" + ip.ToString() + ":8443/");

            });

            return listener;
        }

        // Begin listening to connections on a separate worker thread.
        private static void Start(HttpListener listener)
        {
            listener.Start();
            Task.Run(() => RunServer(listener));
        }

        /// Start awaiting for connections, up to the "maxSimultaneousConnections" value.
        /// This code runs in a separate thread.
        private static void RunServer(HttpListener listener)
        {
            while (true)
            {
                bool v = sem.WaitOne();
                StartConnectionListener(listener);
            }
        }

        // Await connections.
        private static async void StartConnectionListener(HttpListener listener)
        {
            // Wait for a connection. Return to caller while we wait.
            HttpListenerContext context = await listener.GetContextAsync();

            // Release the semaphore so that another listener can be immediately started up.
            sem.Release();
            Log(context.Request);

            // We have a connection, do something...
            //string response = "Hello Browser!";
            //byte[] encoded = Encoding.UTF8.GetBytes(response);
            //context.Response.ContentLength64 = encoded.Length;
            //context.Response.OutputStream.Write(encoded, 0, encoded.Length);
            //context.Response.OutputStream.Close();

            HttpListenerRequest request = context.Request;
            string path = request.RawUrl.LeftOfChar("?"); // Only the path, not any of the parameters
            string verb = request.HttpMethod; // get, post, delete, etc.
            string parms = request.RawUrl.RightOf("?"); // Params on the URL itself follow the URL and are separated by a ?
            Dictionary<string, string> kvParams = GetKeyValues(parms); // Extract into key-value entries.
        }

        // Log requests.
        public static void Log(HttpListenerRequest request)
        {
            //Console.WriteLine(request.RemoteEndPoint + " " + request.HttpMethod + " /" + request.Url.AbsoluteUri.RightOf('/', 3));
            Console.WriteLine(request.RemoteEndPoint + " " + request.HttpMethod + " /" + request.Url.AbsoluteUri);

        }


    }
}
