using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using ReverseR.Common.Code;
using System.Text.RegularExpressions;

namespace AntlrParser
{
    public class AntlrParser : IClassParser
    {
        protected void PreParse(ref string content)//deletes anything wrapped in braces
        {
            int index = 0;
            Regex regex = new Regex("^import\\s+(\\w+\\.)*\\w+;[\\r\\n]", RegexOptions.Multiline);
            content = regex.Replace(content, match => new string(' ', match.Length));
            regex= new Regex("^package\\s+(\\w+\\.)*\\w+;[\\r\\n]", RegexOptions.Multiline);
            content = regex.Replace(content, match => new string(' ', match.Length));
            char[] str = content.ToCharArray();
            while (index<str.Length&&((index=content.IndexOf(')', index)) != -1))
            {
                int i = index + 1;
                bool CodeBody = false;//including method body, initializer body
                for (; str[i] != '\n'&&str[i]!='\r'; i++)
                {
                    if(str[i]=='{'||str[i]=='t')//throws ...
                    {
                        CodeBody = true;
                    }
                    if (str[i] == ';')
                    {
                        CodeBody = false;
                    }
                }
                if (CodeBody)
                {
                    int braceCount = 0;
                    int start = 0;
                    int end = 0;
                    for(int j = index; j<str.Length&&braceCount > 0 || j < i; j++)
                    {
                        if (str[j] == '{')
                        {
                            if (braceCount == 0)
                            {
                                start = j;
                            }
                            braceCount++;
                        }
                        if (str[j] == '}')
                        {
                            braceCount--;
                            if (braceCount == 0)
                            {
                                end = j;
                            }
                        }
                    }
                    for (int k = start + 1; k < end; k++)
                    {
                        if (!char.IsWhiteSpace(str[k]))
                            str[k] = ' ';
                    }
                    if (index < end)
                    {
                        index = end;
                    }
                }
                index = index + 1;
            }
            content = new string(str);
        }
        public IEnumerable<IClassParser.ParseTreeNode> Parse(string content)
        {
            PreParse(ref content);
            Java8Lexer lexer = new Java8Lexer(CharStreams.fromstring(content));
            Java8Parser parser = new Java8Parser(new CommonTokenStream(lexer));

            JavaClassVisitor visitor = new JavaClassVisitor();

            return visitor.Visit(parser.compilationUnit()).Children;
        }

        public IEnumerable<IClassParser.ParseTreeNode> Parse(Stream stream)
        {
            string content = new StreamReader(stream).ReadToEnd();
            return Parse(content);
        }
    }
}
