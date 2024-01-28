using System;
using Bare.WebServer;
using Bare.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ConsoleWebServer
{
    class Program
    {
        static void Main(string[] args)
        {
            string websitePath = GetWebsitePath();
            Console.WriteLine("Hello, World!");

            //server.AddRoute(new Route() { Verb = Router.POST, Path = "/demo/redirect", Handler = new AuthenticatedExpirableRouteHandler(server, RedirectMe) });
            //server.AddRoute(new Route() { Verb = Router.PUT, Path = "/demo/ajax", Handler = new AnonymousRouteHandler(server, AjaxResponder) });
            //server.AddRoute(new Route() { Verb = Router.GET, Path = "/demo/ajax", Handler = new AnonymousRouteHandler(server, AjaxGetResponder) });

            Server.Start(websitePath);
            Console.ReadLine();
            
        }

        public static string GetWebsitePath()
        {
            // Path of our exe.
            string websitePath = Assembly.GetExecutingAssembly().Location;
            websitePath = websitePath.LeftOfRightmostOf('\\').LeftOfRightmostOf('\\').LeftOfRightmostOf('\\') + "\\Website";

            return websitePath;
        }
    }
}