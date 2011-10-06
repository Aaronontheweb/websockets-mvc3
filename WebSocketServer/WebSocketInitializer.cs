using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.Mvc;
using System.Web.Routing;
using Newtonsoft.Json;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketEngine;
using SuperSocket.SocketEngine.Configuration;
using SuperWebSocket;

namespace TerminalSocketServer
{
    public class WebSocketInitializer : System.Web.HttpApplication
    {


        private List<WebSocketSession> m_Sessions = new List<WebSocketSession>();
        private List<WebSocketSession> m_SecureSessions = new List<WebSocketSession>();
        private object m_SessionSyncRoot = new object();
        private object m_SecureSessionSyncRoot = new object();
        private Timer m_SecureSocketPushTimer;

        void Application_Start(object sender, EventArgs e)
        {
            //Do our MVC magic first...
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            LogUtil.Setup();
            StartSuperWebSocketByConfig();
            var ts = new TimeSpan(0, 0, 5);
            m_SecureSocketPushTimer = new Timer(OnSecureSocketPushTimerCallback, new object(), ts, ts);

        }

        void OnSecureSocketPushTimerCallback(object state)
        {
            lock (m_SecureSessionSyncRoot)
            {
                m_SecureSessions.ForEach(s => s.SendResponseAsync("Push data from SecureWebSocket. Current Time: " + DateTime.Now));
            }
        }

        void StartSuperWebSocketByConfig()
        {
            var serverConfig = ConfigurationManager.GetSection("socketServer") as SocketServiceConfig;
            if (!SocketServerManager.Initialize(serverConfig))
                return;

            var socketServer = SocketServerManager.GetServerByName("SuperWebSocket") as WebSocketServer;
            var secureSocketServer = SocketServerManager.GetServerByName("SecureSuperWebSocket") as WebSocketServer;

            Application["WebSocketPort"] = socketServer.Config.Port;
            Application["SecureWebSocketPort"] = secureSocketServer.Config.Port;

            socketServer.NewMessageReceived += new SessionEventHandler<WebSocketSession, string>(socketServer_NewMessageReceived);
            socketServer.NewSessionConnected += new SessionEventHandler<WebSocketSession>(socketServer_NewSessionConnected);
            socketServer.SessionClosed += new SessionEventHandler<WebSocketSession, CloseReason>(socketServer_SessionClosed);

            secureSocketServer.NewSessionConnected += new SessionEventHandler<WebSocketSession>(secureSocketServer_NewSessionConnected);
            secureSocketServer.SessionClosed += new SessionEventHandler<WebSocketSession, CloseReason>(secureSocketServer_SessionClosed);

            if (!SocketServerManager.Start())
                SocketServerManager.Stop();
        }

        void socketServer_NewMessageReceived(WebSocketSession session, string e)
        {
            var command = JsonConvert.DeserializeObject<ConsoleCommand>(e);
            
            try
            {
                IConsoleResponder r = new ConsoleResponder();
                r.ProcessResponse(command, session);
            }
            catch(Exception ex)
            {
                var response = new ConsoleCommandResponse();
                response.AppendMessage("Server Error! :(");
                response.AppendMessage(ex.Message);
                session.SendResponseAsync(response.Serialize());
            }
            
        }

        void secureSocketServer_SessionClosed(WebSocketSession session, CloseReason reason)
        {
            lock (m_SecureSessionSyncRoot)
            {
                m_SecureSessions.Remove(session);
            }
        }

        void secureSocketServer_NewSessionConnected(WebSocketSession session)
        {
            lock (m_SecureSessionSyncRoot)
            {
                m_SecureSessions.Add(session);
            }
        }

     
        void socketServer_NewSessionConnected(WebSocketSession session)
        {
            lock (m_SessionSyncRoot)
                m_Sessions.Add(session);
        }

        void socketServer_SessionClosed(WebSocketSession session, CloseReason reason)
        {
            lock (m_SessionSyncRoot)
                m_Sessions.Remove(session);

            if (reason == CloseReason.ServerShutdown)
                return;
        }

        void SendToAll(string message)
        {
            lock (m_SessionSyncRoot)
            {
                foreach (var s in m_Sessions)
                {
                    //s.SendResponse(message);
                    s.SendResponseAsync(message);
                }
            }
        }

        void Application_End(object sender, EventArgs e)
        {
            m_SecureSocketPushTimer.Change(Timeout.Infinite, Timeout.Infinite);
            m_SecureSocketPushTimer.Dispose();
            SocketServerManager.Stop();
        }

        void Application_Error(object sender, EventArgs e)
        {
            // Code that runs when an unhandled error occurs

        }

        void Session_Start(object sender, EventArgs e)
        {
            // Code that runs when a new session is started

        }

        void Session_End(object sender, EventArgs e)
        {
            // Code that runs when a session ends. 
            // Note: The Session_End event is raised only when the sessionstate mode
            // is set to InProc in the Web.config file. If session mode is set to StateServer 
            // or SQLServer, the event is not raised.

        }

        #region routing functions
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }
        #endregion
    }
}
