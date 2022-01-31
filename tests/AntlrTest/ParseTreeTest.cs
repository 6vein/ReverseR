using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using AntlrParser;
using System.IO;

namespace AntlrTest
{
    [TestClass]
    public class ParseTreeTest
    {
        [TestMethod]
        public void TestVisitor()
        {
            AntlrParser.AntlrParser parser = new AntlrParser.AntlrParser();
            Stream stream = File.OpenRead("Program.java");
            var nodes = parser.Parse(stream);
            stream.Close();
        }
    }
}
