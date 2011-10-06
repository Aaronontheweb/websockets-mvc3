using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using SuperWebSocket;

namespace TerminalSocketServer.Tests
{
    [TestFixture(Description ="Used to validate that our test responder works properly")]
    public class ConsoleResponderTests
    {
        private ConsoleResponder _responder;
        private Mock<WebSocketSession> _fakeSession;

            /// <summary>
        /// Sets up before each test run
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _responder = new ConsoleResponder();
            _fakeSession = new Mock<WebSocketSession>();
        }

        [Test(Description = "Does our Help generation function work properly?")]
        public void Can_Get_Help_Info_Back()
        {
            var helpResponse = _responder.Help();

            Assert.IsNotNull(helpResponse);
            Assert.IsNotNull(helpResponse.Messages);
            Assert.IsTrue(helpResponse.Messages.Count() > 1);
        }

        [Test(Description = "Are we sure we don't do stuff like get an 'echo' and a 'help' mixed up?")]
        public void Can_Distinguish_Commands_Correctly()
        {
            var commands = _responder._commands;

            //I hate hard-coding with magic numbers, but this was the most expedient way to do this
            Assert.IsFalse(commands[0].IsMatch("help"));
            Assert.IsTrue(commands[1].IsMatch("help"));
        }
    }
}
