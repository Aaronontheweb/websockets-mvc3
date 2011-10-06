using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TerminalSocketServer
{
    /*
     *  Gratitiously stolen (and by that I mean licensed under StackOverflow Creative Commons) from Daniel Earwicker,
     *  a better parsemaster than I
     *  http://stackoverflow.com/questions/298830/split-string-containing-command-line-parameters-into-string-in-c/298990#298990
     */
    public static class CommandLineParser
    {
        public static string TrimMatchingQuotes(this string input, char quote)
        {
            if ((input.Length >= 2) &&
                (input[0] == quote) && (input[input.Length - 1] == quote))
                return input.Substring(1, input.Length - 2);

            return input;
        }

        public static IEnumerable<string> Split(this string str,
                                            Func<char, bool> controller)
        {
            var nextPiece = 0;

            for (var c = 0; c < str.Length; c++)
            {
                if (controller(str[c]))
                {
                    yield return str.Substring(nextPiece, c - nextPiece);
                    nextPiece = c + 1;
                }
            }

            yield return str.Substring(nextPiece);
        }

        public static IEnumerable<string> SplitCommandLine(this string commandLine)
        {
            bool inQuotes = false;

            return commandLine.Split(c =>
            {
                if (c == '\"')
                    inQuotes = !inQuotes;

                return !inQuotes && c == ' ';
            })
                              .Select(arg => arg.Trim().TrimMatchingQuotes('\"'))
                              .Where(arg => !string.IsNullOrEmpty(arg));
        }
    }
}
