using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using SuperSocket.Common;

namespace TerminalSocketServer
{
    public class CommandSet : List<CommandArgument>
    {
        public CommandSet() { }

        public CommandSet(IEnumerable<CommandArgument> arguments)
            : base(arguments)
        {
        }

        public void Parse(string commands)
        {
            var commandArray = commands.SplitCommandLine().ToArray();
            Parse(commandArray);
        }

        public void Parse(IList<string> commands, bool isSubset = false)
        {
            //Raise an exception if there are no commands to test...
            if (commands.Count == 0)
            {
                if(isSubset) //Bail if it's a subset...
                {
                    return;
                }
                throw new MissingCommandArgumentException("There were no command arguments sent to the parser (you didn't supply any.)");
            }

            var testArg = commands[0];
            var commandOptions = ExtractCommandOptions(commands);

            var foundMatch = false;
            foreach (var arg in this)
            {
                if (arg.IsMatch(testArg))
                {
                    foundMatch = true;
                    arg.Invoke(commandOptions);
                    if(arg.SubSet.Count > 0) //If this argument takes sub-arguments...
                        arg.SubSet.Parse(commandOptions, true); //Parse down the rest of the tree
                    break; //Exit the for-loop
                }
            }

            //If we have not found a match, throw an exception
            if (!foundMatch)
            {
                throw new MissingCommandArgumentException(string.Format("No matching commands were found for {0}", testArg));
            }
        }

        private static string[] ExtractCommandOptions(ICollection<string> commands)
        {
            var commandOptions = new List<string>();
            if (commands.Count > 1)
            {
                //Copy the contents of the array into our new options container
                commandOptions = commands.BinaryClone().ToList();
                commandOptions.RemoveAt(0);
            }
            return commandOptions.ToArray();
        }
    }

    public class MissingCommandArgumentException : Exception
    {
        public MissingCommandArgumentException(string message) : base(message) { }
        public MissingCommandArgumentException(string message, Exception innerException) : base(message, innerException) { }
        public MissingCommandArgumentException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    public class CommandArgumentException : Exception
    {
        public CommandArgumentException(string message) : base(message) { }
        public CommandArgumentException(string message, Exception innerException) : base(message, innerException) { }
        public CommandArgumentException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    public class CommandArgument
    {
        private readonly Regex _r;

        public CommandArgument(string commandSignature, string description, Action<string[]> executor) :
            this(commandSignature, description, executor, new CommandSet())
        {
        }

        public CommandArgument(string commandSignature, string description, Action<string[]> executor, CommandSet subArguments)
        {
            _r = new Regex(commandSignature, RegexOptions.IgnoreCase);
            CommandRegex = commandSignature;
            Description = description;
            Executor = executor;
            SubSet = subArguments;
        }

        public CommandArgument(string commandSignature, Action<string[]> executor)
            : this(commandSignature, string.Empty, executor, new CommandSet())
        {

        }

        /// <summary>
        /// Each command can have nested commands, as it should be
        /// </summary>
        public CommandSet SubSet { get; set; }

        /// <summary>
        /// Regular expression used to match the argument, aka "echo|--echo|e"
        /// </summary>
        public string CommandRegex { get; set; }

        /// <summary>
        /// Description of what the command does - will be printed ;)
        /// </summary>
        public string Description { get; set; }

        private Action<string[]> Executor { get; set; }

        public void Invoke(string[] args)
        {
            Executor.Invoke(args);
        }

        /// <summary>
        /// Tests a string argument to see if it matches the signature of the command
        /// </summary>
        /// <param name="arg">The string to test</param>
        /// <returns>True if the argument matches</returns>
        public bool IsMatch(string arg)
        {
            return _r.IsMatch(arg);
        }
    }
}
