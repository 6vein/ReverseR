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
            parser.SetBasePath(new ReverseR.Common.Code.ParseTreeNode());
            Stream stream = File.OpenRead("Program.java");
            var nodes = parser.Parse((new StreamReader(stream)).ReadToEnd());
            stream.Close();
        }
    }
}
