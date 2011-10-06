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
        private readonly CommandSet _commands;

        public ConsoleResponder()
        {
            _commands = new CommandSet()
                              {
                                  new CommandArgument("echo|e|--echo","Echoes a simple response back from the server", v => _response = Echo(v.FirstOrDefault())),
                                  new CommandArgument("help|-h|--help","Explains all of the available commands", v =>  _response = Help()),
                              };
        }

        public void ProcessResponse(ConsoleCommand command, WebSocketSession session)
        {
            try
            {
                var args = command.CommandText.SplitCommandLine().ToArray();
                _commands.Parse(args);

                if(_response == null) //Something went wrong with the parser
                {
                    _response = new ConsoleCommandResponse();
                    _response.AppendMessage("ERROR: Unknown command");
                }

                session.SendResponseAsync(_response.Serialize());
            }
            catch(MissingCommandArgumentException e) //An invalid option was passed
            {
                _response = Help();
                _response.PrependMessage(e.Message);
                session.SendResponseAsync(_response.Serialize());
                return;
            }

            
        }

        public ConsoleCommandResponse Help()
        {
            var response = new ConsoleCommandResponse();
            foreach (var o in _commands)
            {
                response.AppendMessage(o.Description);
            }
            return response;
        }

        public ConsoleCommandResponse Echo(string content)
        {
            var response = new ConsoleCommandResponse();
            response.AppendMessage(content);
            return response;
        }
    }


}
