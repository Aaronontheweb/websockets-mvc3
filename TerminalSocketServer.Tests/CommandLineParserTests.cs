using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace TerminalSocketServer.Tests
{
    [TestFixture(Description = "Used for validating whether or not our command line parser works for valid arguments.")]
    public class CommandLineParserTests
    {
        [Test(Description = "Used to make sure that we can split really simple command line argument strings properly, like echo --help")]
        public void Can_Split_Basic_CommandLine_Argument_Strings()
        {
            var simpleCommandLine = "echo --help";
            var parsedArgs = simpleCommandLine.SplitCommandLine().ToArray();
            Assert.AreEqual("echo", parsedArgs[0], string.Format("Expected echo not {0}", parsedArgs[0]));
            Assert.AreEqual("--help", parsedArgs[1], string.Format("Expected --help not {0}", parsedArgs[1]));
            Assert.IsTrue(parsedArgs.Count() == 2,
                          "There shouldn't be more than 2 command line arguments parsed for  \"echo --help\"");
        }

        [Test(Description = "Used to make sure that we can split more sophisticated command lines, like curl")]
        public void Can_Split_Complex_CommandLine_Argument_Strings()
        {
            var complexCommandLine = "curl -d \"user=1234F5&time=10340343\" -I  -G \"http://www.github.com/Aaronontheweb\"";
            var parsedArgs = complexCommandLine.SplitCommandLine().ToArray();
            Assert.AreEqual("curl", parsedArgs[0], string.Format("Expected curl not {0}", parsedArgs[0]));
            Assert.AreEqual("-d", parsedArgs[1], string.Format("Expected -d not {0}", parsedArgs[1]));
            Assert.AreEqual("user=1234F5&time=10340343", parsedArgs[2], string.Format("Expected user=1234F5&time=10340343 not {0}", parsedArgs[2])); //Notice how we expect the quotes to fall off ;)
            Assert.AreEqual("-I", parsedArgs[3], string.Format("Expected -I not {0}", parsedArgs[3]));
            Assert.AreEqual("-G", parsedArgs[4], string.Format("Expected -G not {0}", parsedArgs[4]));
            Assert.AreEqual("http://www.github.com/Aaronontheweb", parsedArgs[5], string.Format("Expected http://www.github.com/Aaronontheweb not {0}", parsedArgs[5]));
            Assert.IsTrue(parsedArgs.Count() == 6);
        }

        [Test(Description = "Does the parser behave properly when there's whitespace in strings like echo \"Hello World!\" ")]
        public void Can_Split_CommandLine_With_Spaces()
        {
            var whiteSpaceCommandLine = "echo \"Hello World!\"";
            var parsedArgs = whiteSpaceCommandLine.SplitCommandLine().ToArray();
            Assert.AreEqual("echo", parsedArgs[0], string.Format("Expected echo not {0}", parsedArgs[0]));
            Assert.AreEqual("Hello World!", parsedArgs[1], string.Format("Expected \"Hello World!\" not {0}", parsedArgs[1]));
            Assert.IsTrue(parsedArgs.Count() == 2,
                          "There shouldn't be more than 2 command line arguments parsed for  \"echo --help\"");
        }

        [Test(Description = "Does our regex ignore case like how it's supposed to?")]
        public void Can_Match_Command_Regardless_Of_Case()
        {
            var setFinished = false;
            var options = new CommandSet(
                                  new[]{
                                  new CommandArgument("help|--help","Used to display help information", v => setFinished = !setFinished)
                              });

            options.Parse("HELP");
            Assert.IsTrue(setFinished); //This should have been set to true by the lamba expression
        }

        [Test(Description = "Test mostly to make sure that I didn't F-up the constructor")]
        public void Can_Find_Valid_Match_With_Multiple_Signatures()
        {
            var setFinished = false;
            var options = new CommandSet(
                                  new []{
                                  new CommandArgument("help|--help","Used to display help information", v => setFinished = !setFinished)
                              });

            options.Parse("help");
            Assert.IsTrue(setFinished); //This should have been set to true by the lamba expression
            options.Parse("--help");
            Assert.IsFalse(setFinished); //Should have been set to "false" when the options found it ;)
        }
    }
}
