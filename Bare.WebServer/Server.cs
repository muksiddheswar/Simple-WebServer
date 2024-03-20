using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Bare.Extensions;


namespace Bare.WebServer
{


    public class Server
    {
        public enum ServerError
        {
            OK,
            ExpiredSession,
            NotAuthorized,
            FileNotFound,
            PageNotFound,
            ServerError,
            UnknownType,
            ValidationError,
            AjaxError,
        }
        public Func<Session, string, string, string> PostProcess { get; set; }
        public Func<ServerError, string> OnError { get; set; }
        public Action<Session, HttpListenerContext> OnRequest;
        public int MaxSimultaneousConnections { get; set; }
        public int ExpirationTimeSeconds { get; set; }
        public string ValidationTokenName { get; set; }

        protected string protectedIP = String.Empty;
        protected string validationTokenScript = "@AntiForgeryToken@";
        protected string publicIP = null;


        protected Semaphore sem;
        protected Router router;
        protected SessionManager sessionManager;

        public Server()
        {
            MaxSimultaneousConnections = 20;            // TODO: This needs to be externally settable before initializing the semaphore.
            ExpirationTimeSeconds = 60;                 // default expires in 1 minute.
            ValidationTokenName = "__CSRFToken__";


            sem = new Semaphore(MaxSimultaneousConnections, MaxSimultaneousConnections);
            router = new Router(this);
            sessionManager = new SessionManager(this);
            PostProcess = DefaultPostProcess;
        }

        /// <summary>
		/// Starts the web server.
		/// </summary>
        public void Start(string websitePath)
        {

            router.WebsitePath = websitePath;
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
                //listener.Prefixes.Add("https://" + ip.ToString() + ":8443/");
                listener.Prefixes.Add("http://" + ip.ToString() + "/");

            });

            return listener;
        }

        // Begin listening to connections on a separate worker thread.
        private void Start(HttpListener listener)
        {
            listener.Start();
            Task.Run(() => RunServer(listener));
        }

        /// Start awaiting for connections, up to the "maxSimultaneousConnections" value.
        /// This code runs in a separate thread.
        private void RunServer(HttpListener listener)
        {
            while (true)
            {
                sem.WaitOne();
                StartConnectionListener(listener);
            }
        }

        /// <summary>
		/// Await connections.
		/// </summary>
		private async void StartConnectionListener(HttpListener listener)
        {
            ResponsePacket resp = null;

            // Wait for a connection.  Return to caller while we wait.
            HttpListenerContext context = await listener.GetContextAsync();
            Session session = sessionManager.GetSession(context.Request.RemoteEndPoint);
            OnRequest.IfNotNull(r => r(session, context));

            // Release the semaphore so that another listener can be immediately started up.
            sem.Release();
            Log(context.Request);

            HttpListenerRequest request = context.Request;

            try
            {
                string path = request.RawUrl.LeftOfChar('?');               // Only the path, not any of the parameters
                string verb = request.HttpMethod;                           // get, post, delete, etc.
                string parms = request.RawUrl.RightOfChar('?');             // Params on the URL itself follow the URL and are separated by a ?
                Dictionary<string, object> kvParams = GetKeyValues(parms);  // Extract into key-value entries.
                string data = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding).ReadToEnd();
                GetKeyValues(data, kvParams);
                Log(kvParams);

                //if (!VerifyCsrf(session, verb, kvParams))
                //{
                //    Console.WriteLine("CSRF did not match.  Terminating connection.");
                //    context.Response.OutputStream.Close();
                //}
                //else
                {
                    resp = router.Route(session, verb, path, kvParams);

                    // Update session last connection after getting the response, as the router itself validates session expiration only on pages requiring authentication.
                    session.UpdateLastConnectionTime();

                    if (resp.Error != ServerError.OK)
                    {
                        resp.Redirect = OnError(resp.Error);
                    }

                    // TODO: Nested exception: is this best?

                    try
                    {
                        Respond(request, context.Response, resp);
                    }
                    catch (Exception reallyBadException)
                    {
                        // The response failed!
                        // TODO: We need to put in some decent logging!
                        Console.WriteLine(reallyBadException.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                resp = new ResponsePacket() { Redirect = OnError(ServerError.ServerError) };
            }
        }

        private void Respond(HttpListenerRequest request, HttpListenerResponse response, ResponsePacket resp)
        {
            response.ContentType = resp.ContentType;
            response.ContentLength64 = resp.Data.Length;
            response.OutputStream.Write(resp.Data, 0, resp.Data.Length);
            response.ContentEncoding = resp.Encoding;
            response.StatusCode = (int)HttpStatusCode.OK;
            response.OutputStream.Close();
        }


        // Log requests.
        public void Log(HttpListenerRequest request)
        {
            //Console.WriteLine(request.RemoteEndPoint + " " + request.HttpMethod + " /" + request.Url.AbsoluteUri.RightOf('/', 3));
            Console.WriteLine(request.RemoteEndPoint + " " + request.HttpMethod + " /" + request.Url.AbsoluteUri);

        }

        /// <summary>
        /// Log parameters.
        /// </summary>
        private void Log(Dictionary<string, object> kv)
        {
            kv.ForEach(kvp => Console.WriteLine(kvp.Key + " : " + Uri.UnescapeDataString(kvp.Value.ToString())));
        }


        /// <summary>
        /// Separate out key-value pairs, delimited by & and into individual key-value instances, separated by =
        /// Ex input: username=abc&password=123
        /// </summary>
        private static Dictionary<string, object> GetKeyValues(string data, Dictionary<string, object> kv = null)
        {
            kv.IfNull(() => kv = new Dictionary<string, object>());
            data.If(d => d.Length > 0, (d) => d.Split('&').ForEach(keyValue => kv[keyValue.LeftOfChar('=')] = System.Uri.UnescapeDataString(keyValue.RightOfChar('='))));

            return kv;
        }


        /// <summary>
        /// Callable by the application for default handling, therefore must be public.
        /// </summary>
        // TODO: Implement this as interface with a base class so the app can call the base class default behavior.
        public string DefaultPostProcess(Session session, string fileName, string html)
        {
            string ret = html.Replace(validationTokenScript, "<input name=" + ValidationTokenName.SingleQuote() +
                " type='hidden' value=" + session[ValidationTokenName].ToString().SingleQuote() +
                " id='__csrf__'/>");

            // For when the CSRF is in a knockout model or other JSON that is being posted back to the server.
            ret = ret.Replace("@CSRF@", session[ValidationTokenName].ToString().SingleQuote());

            ret = ret.Replace("@CSRFValue@", session[ValidationTokenName].ToString());

            return ret;
        }

    }
}
