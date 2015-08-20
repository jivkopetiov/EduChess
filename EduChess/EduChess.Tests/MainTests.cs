using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EduChess.Tests
{
    [TestFixture]
    public class MainTests
    {
        public void ParsePgn()
        {
            var board = new PgnParser().Parse(@"..\..\pgn\2.pgn");
            board.PrintDump();
            
        }
    }
}
