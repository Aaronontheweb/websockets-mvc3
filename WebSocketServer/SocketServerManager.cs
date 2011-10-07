using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SuperWebSocket;

namespace TerminalSocketServer
{
    public class SocketServerManager
    {
        private List<WebSocketSession> m_Sessions = new List<WebSocketSession>();
        private List<WebSocketSession> m_SecureSessions = new List<WebSocketSession>();
        private object m_SessionSyncRoot = new object();
        private object m_SecureSessionSyncRoot = new object();
        private Timer m_SecureSocketPushTimer;
    }
}
