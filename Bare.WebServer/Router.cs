﻿using Bare.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Bare.WebServer
{

    public class ResponsePacket
    {
        public string Redirect { get; set; }
        public byte[] Data { get; set; }
        public string ContentType { get; set; }
        public Encoding Encoding { get; set; }
        public Server.ServerError Error { get; set; }
        public HttpStatusCode StatusCode { get; set; }

        public ResponsePacket()
        {
            Error = Server.ServerError.OK;
            StatusCode = HttpStatusCode.OK;
        }
    }


    public class Route
    {
        public string Verb { get; set; }
        public string Path { get; set; }

        /// <summary>
        /// Usually a route has a single handler, so we can just set the Handler property.
        /// </summary>
        public RouteHandler Handler { get; set; }

        /// <summary>
        /// Sometimes we need a collection of handlers for a single route because the handler is determined by the data, not the url.
        /// </summary>
        public List<RouteHandler> Handlers { get; set; }

        public Func<Session, Dictionary<string, object>, string, string> PostProcess { get; set; }

        public Route()
        {
            Handlers = new List<RouteHandler>();
        }
    }

    public class ExtensionInfo
    {
        public string ContentType { get; set; }
        public Func<Route, Session, Dictionary<string, object>, string, string, ExtensionInfo, ResponsePacket> Loader { get; set; }
    }


    public class Router
    {
        public string WebsitePath { get; set; }

        public const string POST = "post";
        public const string GET = "get";
        public const string PUT = "put";
        public const string DELETE = "delete";


        protected Dictionary<string, ExtensionInfo> extFolderMap;
        protected List<Route> routes;
        protected Server server;

        public Router(Server serve)
        {
            extFolderMap = new Dictionary<string, ExtensionInfo>()
            {
              {"ico", new ExtensionInfo() {Loader=ImageLoader, ContentType="image/ico"}},
              {"png", new ExtensionInfo() {Loader=ImageLoader, ContentType="image/png"}},
              {"jpg", new ExtensionInfo() {Loader=ImageLoader, ContentType="image/jpg"}},
              {"gif", new ExtensionInfo() {Loader=ImageLoader, ContentType="image/gif"}},
              {"bmp", new ExtensionInfo() {Loader=ImageLoader, ContentType="image/bmp"}},
              {"html", new ExtensionInfo() {Loader=PageLoader, ContentType="text/html"}},
              {"css", new ExtensionInfo() {Loader=FileLoader, ContentType="text/css"}},
              {"js", new ExtensionInfo() {Loader=FileLoader, ContentType="text/javascript"}},
              {"", new ExtensionInfo() {Loader=PageLoader, ContentType="text/html"}},
            };
        }


        public ResponsePacket Route(Session session, string verb, string path, Dictionary<string, object> kvParams)
        {
            string ext = path.RightOfRightmostOf('.');
            ExtensionInfo extInfo;
            ResponsePacket ret = null;
            verb = verb.ToLower();
            path = path.ToLower();

            if (extFolderMap.TryGetValue(ext, out extInfo))
            {
                string wpath = path.Substring(1).Replace('/', '\\');            // Strip off leading '/' and reformat as with windows path separator.
                string fullPath = Path.Combine(WebsitePath, wpath);

                Route routeHandler = routes.SingleOrDefault(r => verb == r.Verb.ToLower() && path == r.Path.ToLower());

                if (routeHandler != null)
                {
                    // Application has a handler for this route.
                    ResponsePacket handlerResponse = null;

                    // If a handler exists:
                    routeHandler.Handler.IfNotNull((h) => handlerResponse = h.Handle(session, kvParams));

                    // If multiple handlers exist, see which one, if any, is willing to handle the request.
                    // We stop after the first handler.
                    // This behavior is useful for web services or other types of routes where the data determines the route, not the URL.
                    if (routeHandler.Handlers.Count > 0)
                    {
                        foreach (RouteHandler h in routeHandler.Handlers)
                        {
                            if (h.CanHandle(session, kvParams))
                            {
                                handlerResponse = h.Handle(session, kvParams);
                                break;
                            }
                        }
                    }

                    if (handlerResponse == null)
                    {
                        // Respond with default content loader.
                        ret = extInfo.Loader(routeHandler, session, kvParams, fullPath, ext, extInfo);
                    }
                    else
                    {
                        // Respond with redirect.
                        ret = handlerResponse;
                    }
                }
                else
                {
                    // Attempt default behavior
                    ret = extInfo.Loader(null, session, kvParams, fullPath, ext, extInfo);
                }
            }
            else
            {
                ret = new ResponsePacket() { Error = Server.ServerError.UnknownType };
            }

            return ret;
        }



        /// <summary>
        /// Read in an image file and returns a ResponsePacket with the raw data.
        /// </summary>
        protected ResponsePacket ImageLoader(Route routeHandler, Session session, Dictionary<string, object> kvParams, string fullPath, string ext, ExtensionInfo extInfo)
        {
            ResponsePacket ret;

            if (!File.Exists(fullPath))
            {
                ret = new ResponsePacket() { Error = Server.ServerError.FileNotFound };
                Console.WriteLine("!!! File not found: " + fullPath);
            }
            else
            {
                FileStream fStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
                BinaryReader br = new BinaryReader(fStream);
                ret = new ResponsePacket() { Data = br.ReadBytes((int)fStream.Length), ContentType = extInfo.ContentType };
                br.Close();
                fStream.Close();
            }

            return ret;
        }

        /// <summary>
		/// Load an HTML file, taking into account missing extensions and a file-less IP/domain, which should default to index.html.
		/// </summary>
		protected ResponsePacket PageLoader(Route routeHandler, Session session, Dictionary<string, object> kvParams, string fullPath, string ext, ExtensionInfo extInfo)
        {
            ResponsePacket ret;

            if (fullPath == WebsitePath)        // If nothing follows the domain name or IP, then default to loading index.html.
            {
                ret = Route(session, GET, "/index.html", null);
            }
            else
            {
                if (String.IsNullOrEmpty(ext))
                {
                    // No extension, so we make it ".html"
                    fullPath = fullPath + ".html";
                }

                // Inject the "Pages" folder into the path
                fullPath = WebsitePath + "\\Pages" + fullPath.RightOfChar(WebsitePath);

                // Custom, for page not found error.
                if (!File.Exists(fullPath))
                {
                    ret = new ResponsePacket() { Error = Server.ServerError.PageNotFound };
                    Console.WriteLine("!!! File not found: " + fullPath);
                }
                else
                {
                    string text = File.ReadAllText(fullPath);

                    // TODO: We put the route custom post process last because of how content is merged in the application's process,
                    // but this might cause problems if the route post processor adds something that the app's post processor needs to replace.
                    // How do we handle this?  A before/after process?  CSRF tokens are a great example!

                    // Do the application global post process replacement.
                    text = server.PostProcess(session, fullPath, text);

                    // If a custom post process callback exists, call it.
                    routeHandler.IfNotNull((r) => r.PostProcess.IfNotNull((p) => text = p(session, kvParams, text)));

                    // Do our default post process to catch any final CSRF stuff in the fully merged document.
                    text = server.DefaultPostProcess(session, fullPath, text);


                    ret = new ResponsePacket() { Data = Encoding.UTF8.GetBytes(text), ContentType = extInfo.ContentType, Encoding = Encoding.UTF8 };
                }
            }

            return ret;
        }

        /// <summary>
        /// Read in what is basically a text file and return a ResponsePacket with the text UTF8 encoded.
        /// </summary>
        protected ResponsePacket FileLoader(Route routeHandler, Session session, Dictionary<string, object> kvParams, string fullPath, string ext, ExtensionInfo extInfo)
        {
            ResponsePacket ret;

            if (!File.Exists(fullPath))
            {
                ret = new ResponsePacket() { Error = Server.ServerError.FileNotFound };
                Console.WriteLine("!!! File not found: " + fullPath);
            }
            else
            {
                string text = File.ReadAllText(fullPath);
                ret = new ResponsePacket() { Data = Encoding.UTF8.GetBytes(text), ContentType = extInfo.ContentType, Encoding = Encoding.UTF8 };
            }

            return ret;
        }

    }
}
