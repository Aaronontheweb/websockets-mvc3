using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperWebSocket;

namespace TerminalSocketServer
{
    /// <summary>
    /// Interface used to handle queries from webterminal users
    /// </summary>
    public interface IConsoleResponder
    {
        void ProcessResponse(ConsoleCommand command, WebSocketSession session);
    }

    public class ConsoleResponder : IConsoleResponder
    {
        private ConsoleCommandResponse _response;
        public readonly CommandSet _commands;
        private WebSocketSession _session;

        public ConsoleResponder()
        {
            _commands = new CommandSet()
                              {
                                  new CommandArgument("echo|-e|--echo","Echoes a simple response back from the server. Sample usage:\n\t\t\t" +
                                                                       "$echo \"Hi!\"\n\t\t\t" +
                                                                       "Server: \"Hi!\"", v => _response = Echo(v.FirstOrDefault())),
                                  new CommandArgument("help|-h|--help","{help} explains all of the available commands. " +
                                                                       "Can be used at the end of each individual command (--help) for command-specific instructions.", v =>  _response = Help()),
                              };
        }

        public void ProcessResponse(ConsoleCommand command, WebSocketSession session)
        {
            _session = session;

            try
            {
                var args = command.CommandText.SplitCommandLine().ToArray();
                _commands.Parse(args);
            }
            catch(MissingCommandArgumentException e) //An invalid option was passed
            {
                var response = Help();
                response.PrependMessage(e.Message);
                session.SendResponseAsync(response.Serialize());
            }
        }

        public ConsoleCommandResponse Help()
        {
            var response = new ConsoleCommandResponse();
            foreach (var o in _commands)
            {
                response.AppendMessage(PrettyFormat(o));
            }

            _session.SendResponseAsync(response.Serialize());
            return response;
        }

        public string PrettyFormat(CommandArgument command)
        {
            return string.Format("{0}\t\t{1}", command.CommandRegex, command.Description);
        }

        public ConsoleCommandResponse Echo(string content)
        {
            if(string.IsNullOrEmpty(content)) //Throw an error if we're missing content
            {
                throw new CommandArgumentException("Invalid command - missing argument for echo {phrase_to_be_echoed}");
            }
            var response = new ConsoleCommandResponse();
            response.AppendMessage(content);
            _session.SendResponseAsync(response.Serialize());
            return response;
        }
    }


}
