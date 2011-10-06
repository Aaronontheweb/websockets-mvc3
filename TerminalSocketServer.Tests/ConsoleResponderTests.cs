using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace TerminalSocketServer.Tests
{
    [TestFixture(Description ="Used to validate that our test responder works properly")]
    public class ConsoleResponderTests
    {
        [Test(Description = "Does our Help generation function work properly?")]
        public void Can_Get_Help_Info_Back()
        {
            var responder = new ConsoleResponder();

            var helpResponse = responder.Help();

            Assert.IsNotNull(helpResponse);
            Assert.IsNotNull(helpResponse.Messages);
            Assert.IsTrue(helpResponse.Messages.Count() > 1);
        }
    }
}
