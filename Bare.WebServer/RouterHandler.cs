using Bare.Extensions;

namespace Bare.WebServer
{
    /// <summary>
    /// The base class for route handlers.  If not for being abstract, this would be the equivalent of an anonymous handler,
    /// but we want to enforce an explicit declaration of that so the developer doesn't accidentally use RouteHandler without
    /// realizing that it's an anonymous, unauthenticated, no session timeout check, handler.  Defensive Programming!
    /// </summary>
    public abstract class RouteHandler
    {
        protected Server server;
        protected Func<Session, Dictionary<string, object>, ResponsePacket> handler;

        public RouteHandler(Server server, Func<Session, Dictionary<string, object>, ResponsePacket> handler)
        {
            this.server = server;
            this.handler = handler;
        }

        public virtual ResponsePacket Handle(Session session, Dictionary<string, object> parms)
        {
            return InvokeHandler(session, parms);
        }

        /// <summary>
        /// CanHandle is used only for determining which handler, in a multiple handler for a single route, can actually handle to session and params for that route.
        /// </summary>
        public virtual bool CanHandle(Session session, Dictionary<string, object> parms) { return true; }

        protected ResponsePacket InvokeHandler(Session session, Dictionary<string, object> parms)
        {
            ResponsePacket? ret = null;
            handler.IfNotNull((h) => ret = h(session, parms));

            return ret;
        }
    }
}
