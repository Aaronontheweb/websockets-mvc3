using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace TerminalSocketServer
{
    /// <summary>
    /// Data object used to serialize / deserialize commands from end-users
    /// </summary>
    [Serializable]
    [DataContract]
    public class ConsoleCommand
    {
        [DataMember]
        public string CommandText { get; set; }

        [DataMember]
        public string When { get; set; }
    }

    /// <summary>
    /// Data object used to send responses back to the caller
    /// </summary>
    [Serializable]
    [DataContract]
    public class ConsoleCommandResponse
    {
        private IList<string> _messages;
        public ConsoleCommandResponse()
        {
            _messages = new List<string>();
        }

        /// <summary>
        /// Appends a message to the back of the list
        /// </summary>
        /// <param name="m">The message to be added</param>
        public void AppendMessage(string m)
        {
            _messages.Add(m);
        }

        /// <summary>
        /// Removes a message from the list
        /// </summary>
        /// <param name="m">The message to be removed</param>
        public void RemoveMessage(string m)
        {
            _messages.Remove(m);
        }

        /// <summary>
        /// Adds a message to the front of the list as opposed to the back
        /// </summary>
        /// <param name="m">The message to be added</param>
        public void PrependMessage(string m)
        {
            _messages.Insert(0, m);
        }

        [DataMember]
        public string[] Messages { get { return _messages.ToArray(); } set { _messages = value.ToList(); } }
    }

    public static class ConsoleIOHelper
    {
        public static string Serialize(this ConsoleCommand command)
        {
            return JsonConvert.SerializeObject(command);
        }

        public static string Serialize(this ConsoleCommandResponse response)
        {
            return JsonConvert.SerializeObject(response);
        }
    }
}
