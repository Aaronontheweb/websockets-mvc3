using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace TerminalSocketServer.Tests
{
    [TestFixture(Description = "Tests for making sure that our DTOs function properly")]
    public class CommandDTOTests
    {
        [Test(Description = "Test to see if we can append messages to the front of a messaging array")]
        public void Can_Prepend_Message_To_Command_Response()
        {
            var commandResponse = new ConsoleCommandResponse();

            commandResponse.AppendMessage("Testing?!");
            Assert.IsTrue(commandResponse.Messages.Count() == 1);
            Assert.AreEqual("Testing?!", commandResponse.Messages[0], string.Format("Expected Testing?!, not {0}", commandResponse.Messages[0]));

            commandResponse.PrependMessage("Hi");
            Assert.IsTrue(commandResponse.Messages.Count() == 2);
            Assert.AreEqual("Hi", commandResponse.Messages[0], string.Format("Expected Hi, not {0}", commandResponse.Messages[0]));
        }
    }
}
