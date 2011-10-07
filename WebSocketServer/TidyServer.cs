using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketEngine;
using SuperSocket.SocketEngine.Configuration;
using SuperWebSocket;

namespace TerminalSocketServer
{
    public class TidyServer
    {
        private List<WebSocketSession> m_Sessions = new List<WebSocketSession>();
        private List<WebSocketSession> m_SecureSessions = new List<WebSocketSession>();
        private object m_SessionSyncRoot = new object();
        private object m_SecureSessionSyncRoot = new object();
        private Timer m_SecureSocketPushTimer;

        public int SecureSocketServerPort { get; set; }
        public int SocketServerPort { get; set; }

        public TidyServer()
        {
            
        }

        public void Init()
        {
            LogUtil.Setup();
            StartSuperWebSocketByConfig();

            var ts = new TimeSpan(0, 0, 5);
            m_SecureSocketPushTimer = new Timer(OnSecureSocketPushTimerCallback, new object(), ts, ts);
        }

        void StartSuperWebSocketByConfig()
        {
            var serverConfig = ConfigurationManager.GetSection("socketServer") as SocketServiceConfig;
            if (!SocketServerManager.Initialize(serverConfig))
                return;

            var socketServer = SocketServerManager.GetServerByName("SuperWebSocket") as WebSocketServer;
            var secureSocketServer = SocketServerManager.GetServerByName("SecureSuperWebSocket") as WebSocketServer;

            SocketServerPort = socketServer.Config.Port;
            SecureSocketServerPort = secureSocketServer.Config.Port;

            socketServer.NewMessageReceived += new SessionEventHandler<WebSocketSession, string>(socketServer_NewMessageReceived);
            socketServer.NewSessionConnected += new SessionEventHandler<WebSocketSession>(socketServer_NewSessionConnected);
            socketServer.SessionClosed += new SessionEventHandler<WebSocketSession, CloseReason>(socketServer_SessionClosed);

            secureSocketServer.NewSessionConnected += new SessionEventHandler<WebSocketSession>(secureSocketServer_NewSessionConnected);
            secureSocketServer.SessionClosed += new SessionEventHandler<WebSocketSession, CloseReason>(secureSocketServer_SessionClosed);

            if (!SocketServerManager.Start())
                SocketServerManager.Stop();
        }

        void OnSecureSocketPushTimerCallback(object state)
        {
            lock (m_SecureSessionSyncRoot)
            {
                m_SecureSessions.ForEach(s => s.SendResponseAsync("Push data from SecureWebSocket. Current Time: " + DateTime.Now));
            }
        }

        void socketServer_NewMessageReceived(WebSocketSession session, string e)
        {
            var command = JsonConvert.DeserializeObject<ConsoleCommand>(e);

            try
            {
                IConsoleResponder r = new ConsoleResponder(SendToAll);
                r.ProcessResponse(command, session);
            }
            catch (Exception ex)
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

        public void SendToAll(string message)
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

        public void Kill()
        {
            m_SecureSocketPushTimer.Change(Timeout.Infinite, Timeout.Infinite);
            m_SecureSocketPushTimer.Dispose();
            SocketServerManager.Stop();
        }
    }
}
