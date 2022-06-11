using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using ReverseR.Common.Code;
using System.Text.RegularExpressions;
using ReverseR.Common.DecompUtilities;

namespace AntlrParser
{
    public class AntlrParser : IClassParser
    {
        protected void PreParse(ref string content)//deletes anything wrapped in braces
        {
            int index = 0;
            Regex regex = new Regex("^import\\s+(\\w+\\.)*\\w+;", RegexOptions.Multiline);
            content = regex.Replace(content, match => new string(' ', match.Length));
            regex= new Regex("^package\\s+(\\w+\\.)*\\w+;", RegexOptions.Multiline);
            content = regex.Replace(content, match => new string(' ', match.Length));
            char[] str = content.ToCharArray();
            index = 0;
            while (index < str.Length && ((index = content.IndexOf('\"', index+1)) != -1))//clear string values
            {
                int start = index;
                int counter = 0;
                for (int i = index; i >= 0 && str[i] == '\\'; i--)
                    counter++;
                if (counter % 2 != 0)
                {
                    for (int j = index - counter + 1; j <= index; j++)
                    {
                        str[j] = ' ';
                    }
                    continue;
                }
                else
                {
                    counter = 0;
                    int end = 0;
                    while (index < str.Length && ((index = content.IndexOf('\"', index+1)) != -1))
                    {
                        for (int i = index; i >= 0 && str[i] == '\\'; i--)
                            counter++;
                        if (counter % 2 != 0)
                        {
                            for (int j = index - counter + 1; j <= index; j++)
                            {
                                str[j] = ' ';
                            }
                            continue;
                        }
                        else
                        {
                            end = index;
                            break;
                        }
                    }
                    for(int j = start; j <= index; j++)
                    {
                        str[j] = ' ';
                    }
                }
            }
            content = new string(str);
            index = 0;
            while (index < str.Length && ((index = content.IndexOf('\'', index+1)) != -1))//clear char values
            {
                if(str[index+3]=='\'')// '\*'
                {
                    str[index] = str[index + 1] = str[index + 2] = str[index + 3] = ' ';
                }
                if (str[index + 2] == '\'')
                {
                    str[index] = str[index + 1] = str[index + 2] = ' ';
                }
            }
            content = new string(str);
            index = 0;
            while (index < str.Length && ((index = content.IndexOf(')', index+1)) != -1)) 
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
                    for (int j = index; j < str.Length && braceCount > 0 || j < i; j++)
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
                    string tmp = new string(str);
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
            }
            content = new string(str);
        }
        public IEnumerable<ParseTreeNode> Parse(string content)
        {
            string tmp = content;
            PreParse(ref tmp);
            Java8Lexer lexer = new Java8Lexer(CharStreams.fromString(tmp));
            Java8Parser parser = new Java8Parser(new CommonTokenStream(lexer));

            JavaClassVisitor visitor = new JavaClassVisitor() { baseClassPath=_basePath.ClassPath,filePath=_basePath.Path };

            return visitor.Visit(parser.compilationUnit()).Children;
        }
        IJPath _basePath;
        public IClassParser SetBasePath(IJPath basePath)
        {
            _basePath = basePath;
            return this;
        }
    }
}
