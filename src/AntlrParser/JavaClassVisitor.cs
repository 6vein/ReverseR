using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using ReverseR.Common.Code;

namespace AntlrParser
{
    internal class JavaClassVisitor:Java8ParserBaseVisitor<IClassParser.ParseTreeNode>
    {
        public override IClassParser.ParseTreeNode VisitCompilationUnit([NotNull] Java8Parser.CompilationUnitContext context)
        {
            var node = new IClassParser.ParseTreeNode();
            List<IClassParser.ParseTreeNode> children= new List<IClassParser.ParseTreeNode>();
            foreach(var child in context.children)
            {
                if(child is Java8Parser.ClassDeclarationContext
                    || child is Java8Parser.InterfaceDeclarationContext
                    || child is Java8Parser.FieldDeclarationContext
                    ||child is Java8Parser.MethodDeclarationContext
                    ||child is Java8Parser.InterfaceMethodDeclarationContext)
                {
                    children.Add(Visit(child));
                }
            }
            node.Children = children;
            return node;
        }
    }
}
