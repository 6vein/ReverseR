using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using ReverseR.Common.Code;

namespace AntlrParser
{
    public class AntlrParser : IClassParser
    {
        public IEnumerable<IClassParser.ParseTreeNode> Parse(string content)
        {
            Java8Lexer lexer = new Java8Lexer(CharStreams.fromstring(content));
            Java8Parser parser = new Java8Parser(new CommonTokenStream(lexer));

            JavaClassVisitor visitor = new JavaClassVisitor();

            return visitor.Visit(parser.compilationUnit()).Children;
        }

        public IEnumerable<IClassParser.ParseTreeNode> Parse(Stream stream)
        {
            Java8Lexer lexer = new Java8Lexer(CharStreams.fromStream(stream));
            Java8Parser parser = new Java8Parser(new CommonTokenStream(lexer));

            JavaClassVisitor visitor = new JavaClassVisitor();
            parser.TrimParseTree = true;
            return visitor.Visit(parser.compilationUnit()).Children;
        }
    }
}
