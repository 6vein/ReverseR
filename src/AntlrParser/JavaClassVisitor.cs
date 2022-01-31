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
        public override IClassParser.ParseTreeNode VisitNormalClassDeclaration([NotNull] Java8Parser.NormalClassDeclarationContext context)
        {
            var node = VisitChildren(context) ?? new IClassParser.ParseTreeNode();
            node.ItemType = IClassParser.ItemType.Class;
            foreach(var modifier in context.classModifier())
            {
                if (modifier.PUBLIC() != null) node.Modifiers.Add(IClassParser.ModifierType.Public);
                if (modifier.PROTECTED() != null) node.Modifiers.Add(IClassParser.ModifierType.Protected);
                if (modifier.PRIVATE() != null) node.Modifiers.Add(IClassParser.ModifierType.Private);
                if (modifier.FINAL() != null) node.Modifiers.Add(IClassParser.ModifierType.Final);
                if (modifier.STATIC() != null) node.Modifiers.Add(IClassParser.ModifierType.Static);
                if (modifier.ABSTRACT() != null) node.Modifiers.Add(IClassParser.ModifierType.Abstract);
                if (modifier.STRICTFP() != null) node.Modifiers.Add(IClassParser.ModifierType.StrictFP);
            }
            node.Id = node.Content = context.Identifier().GetText();
            node.Start = context.Identifier().Symbol.StartIndex;
            node.End = context.Identifier().Symbol.StopIndex;
            return node;
        }
        public override IClassParser.ParseTreeNode VisitNormalInterfaceDeclaration([NotNull] Java8Parser.NormalInterfaceDeclarationContext context)
        {
            var node = VisitChildren(context) ?? new IClassParser.ParseTreeNode();
            node.ItemType = IClassParser.ItemType.Interface;
            foreach (var modifier in context.interfaceModifier())
            {
                if (modifier.PUBLIC() != null) node.Modifiers.Add(IClassParser.ModifierType.Public);
                if (modifier.PROTECTED() != null) node.Modifiers.Add(IClassParser.ModifierType.Protected);
                if (modifier.PRIVATE() != null) node.Modifiers.Add(IClassParser.ModifierType.Private);
                if (modifier.STATIC() != null) node.Modifiers.Add(IClassParser.ModifierType.Static);
                if (modifier.ABSTRACT() != null) node.Modifiers.Add(IClassParser.ModifierType.Abstract);
                if (modifier.STRICTFP() != null) node.Modifiers.Add(IClassParser.ModifierType.StrictFP);
            }
            node.Id=node.Content = context.Identifier().GetText();
            node.Start = context.Identifier().Symbol.StartIndex;
            node.End = context.Identifier().Symbol.StopIndex;
            return node;
        }
        public override IClassParser.ParseTreeNode VisitEnumDeclaration([NotNull] Java8Parser.EnumDeclarationContext context)
        {
            var node = VisitChildren(context) ?? new IClassParser.ParseTreeNode();
            node.ItemType = IClassParser.ItemType.Enum;
            foreach (var modifier in context.classModifier())
            {
                if (modifier.PUBLIC() != null) node.Modifiers.Add(IClassParser.ModifierType.Public);
                if (modifier.PROTECTED() != null) node.Modifiers.Add(IClassParser.ModifierType.Protected);
                if (modifier.PRIVATE() != null) node.Modifiers.Add(IClassParser.ModifierType.Private);
                if (modifier.FINAL() != null) node.Modifiers.Add(IClassParser.ModifierType.Final);
                if (modifier.STATIC() != null) node.Modifiers.Add(IClassParser.ModifierType.Static);
                if (modifier.ABSTRACT() != null) node.Modifiers.Add(IClassParser.ModifierType.Abstract);
                if (modifier.STRICTFP() != null) node.Modifiers.Add(IClassParser.ModifierType.StrictFP);
            }
            node.Id=node.Content = context.Identifier().GetText();
            node.Start = context.Identifier().Symbol.StartIndex;
            node.End = context.Identifier().Symbol.StopIndex;
            return node;
        }
        public override IClassParser.ParseTreeNode VisitConstructorDeclaration([NotNull] Java8Parser.ConstructorDeclarationContext context)
        {
            var node = VisitChildren(context) ?? new IClassParser.ParseTreeNode();
            node.ItemType = IClassParser.ItemType.Constructor;
            foreach (var modifier in context.constructorModifier())
            {
                if (modifier.PUBLIC() != null) node.Modifiers.Add(IClassParser.ModifierType.Public);
                if (modifier.PROTECTED() != null) node.Modifiers.Add(IClassParser.ModifierType.Protected);
                if (modifier.PRIVATE() != null) node.Modifiers.Add(IClassParser.ModifierType.Private);
            }
            var paraList = context.constructorDeclarator().formalParameterList();
            node.Id = context.constructorDeclarator().simpleTypeName().Identifier().GetText();
            if(paraList != null)
            {
                node.Content = context.constructorDeclarator().simpleTypeName().Identifier().GetText()
                + "(" + paraList.Start.InputStream.GetText(new Interval(paraList.Start.StartIndex, paraList.Stop.StopIndex)) + "):Constructor";
            }
            else
            {
                node.Content = context.constructorDeclarator().simpleTypeName().Identifier().GetText()
                + "():Constructor";
            }
            node.Start = context.constructorDeclarator().simpleTypeName().Identifier().Symbol.StartIndex;
            node.End = context.constructorDeclarator().simpleTypeName().Identifier().Symbol.StopIndex;
            return node;
        }
        public override IClassParser.ParseTreeNode VisitMethodDeclaration([NotNull] Java8Parser.MethodDeclarationContext context)
        {
            var node = VisitChildren(context) ?? new IClassParser.ParseTreeNode();
            node.ItemType = IClassParser.ItemType.Method;
            foreach (var modifier in context.methodModifier())
            {
                if (modifier.PUBLIC() != null) node.Modifiers.Add(IClassParser.ModifierType.Public);
                if (modifier.PROTECTED() != null) node.Modifiers.Add(IClassParser.ModifierType.Protected);
                if (modifier.PRIVATE() != null) node.Modifiers.Add(IClassParser.ModifierType.Private);
                if (modifier.STATIC() != null) node.Modifiers.Add(IClassParser.ModifierType.Static);
                if (modifier.FINAL() != null) node.Modifiers.Add(IClassParser.ModifierType.Final);
                if (modifier.SYNCHRONIZED() != null) node.Modifiers.Add(IClassParser.ModifierType.Synchronized);
                if (modifier.ABSTRACT() != null) node.Modifiers.Add(IClassParser.ModifierType.Abstract);
                if (modifier.STRICTFP() != null) node.Modifiers.Add(IClassParser.ModifierType.StrictFP);
                if (modifier.NATIVE() != null) node.Modifiers.Add(IClassParser.ModifierType.Native);
            }
            var paraList = context.methodHeader().methodDeclarator().formalParameterList();
            node.Id = context.methodHeader().methodDeclarator().Identifier().GetText();
            if (paraList != null)
            {
                node.Content = context.methodHeader().methodDeclarator().Identifier().GetText()
                + "(" + paraList.Start.InputStream.GetText(new Interval(paraList.Start.StartIndex, paraList.Stop.StopIndex)) + "):"
                + context.methodHeader().result().GetText();
            }
            else
            {
                node.Content = context.methodHeader().methodDeclarator().Identifier().GetText()
                + "():"
                + context.methodHeader().result().GetText();
            }
            node.Start = context.methodHeader().methodDeclarator().Identifier().Symbol.StartIndex;
            node.End = context.methodHeader().methodDeclarator().Identifier().Symbol.StopIndex;
            return node;
        }
        public override IClassParser.ParseTreeNode VisitFieldDeclaration([NotNull] Java8Parser.FieldDeclarationContext context)
        {
            var node = VisitChildren(context) ?? new IClassParser.ParseTreeNode();
            node.ItemType = IClassParser.ItemType.Field;
            foreach (var modifier in context.fieldModifier())
            {
                if (modifier.PUBLIC() != null) node.Modifiers.Add(IClassParser.ModifierType.Public);
                if (modifier.PROTECTED() != null) node.Modifiers.Add(IClassParser.ModifierType.Protected);
                if (modifier.PRIVATE() != null) node.Modifiers.Add(IClassParser.ModifierType.Private);
                if (modifier.STATIC() != null) node.Modifiers.Add(IClassParser.ModifierType.Static);
                if (modifier.FINAL() != null) node.Modifiers.Add(IClassParser.ModifierType.Synchronized);
            }
            foreach(var defination in context.variableDeclaratorList().variableDeclarator())
            {
                var subNode = (IClassParser.ParseTreeNode)node.Clone();
                subNode.Id = defination.variableDeclaratorId().Identifier().GetText();
                subNode.Content = defination.variableDeclaratorId().Identifier().GetText() + ":"
                    + context.unannType().GetText();
                subNode.Start = defination.variableDeclaratorId().Identifier().Symbol.StartIndex;
                subNode.End = defination.variableDeclaratorId().Identifier().Symbol.StopIndex;
                node.Children.Add(subNode);
            }
            return node;
        }
        public override IClassParser.ParseTreeNode VisitInterfaceMethodDeclaration([NotNull] Java8Parser.InterfaceMethodDeclarationContext context)
        {
            var node = VisitChildren(context) ?? new IClassParser.ParseTreeNode();
            node.ItemType = IClassParser.ItemType.InterfaceMethod;
            foreach (var modifier in context.interfaceMethodModifier())
            {
                if (modifier.PUBLIC() != null) node.Modifiers.Add(IClassParser.ModifierType.Public);
                if (modifier.STATIC() != null) node.Modifiers.Add(IClassParser.ModifierType.Static);
                if (modifier.DEFAULT() != null) node.Modifiers.Add(IClassParser.ModifierType.Default);
                if (modifier.ABSTRACT() != null) node.Modifiers.Add(IClassParser.ModifierType.Abstract);
                if (modifier.STRICTFP() != null) node.Modifiers.Add(IClassParser.ModifierType.StrictFP);
            }
            var paraList = context.methodHeader().methodDeclarator().formalParameterList();
            node.Id = context.methodHeader().methodDeclarator().Identifier().GetText();
            if (paraList != null)
            {
                node.Content = context.methodHeader().methodDeclarator().Identifier().GetText()
                + "(" + paraList.Start.InputStream.GetText(new Interval(paraList.Start.StartIndex, paraList.Stop.StopIndex)) + "):"
                + context.methodHeader().result().GetText();
            }
            else
            {
                node.Content = context.methodHeader().methodDeclarator().Identifier().GetText()
                + "():"
                + context.methodHeader().result().GetText();
            }
            node.Start = context.methodHeader().methodDeclarator().Identifier().Symbol.StartIndex;
            node.End = context.methodHeader().methodDeclarator().Identifier().Symbol.StopIndex;
            return node;
        }
        public override IClassParser.ParseTreeNode VisitEnumConstantList([NotNull] Java8Parser.EnumConstantListContext context)
        {
            var node = VisitChildren(context) ?? new IClassParser.ParseTreeNode();
            node.ItemType = IClassParser.ItemType.EnumConstant;
            foreach (var defination in context.enumConstant())
            {
                var subNode = (IClassParser.ParseTreeNode)node.Clone();
                subNode.Id=subNode.Content = defination.Identifier().GetText();
                subNode.Start = defination.Identifier().Symbol.StartIndex;
                subNode.End = defination.Identifier().Symbol.StopIndex;
                node.Children.Add(subNode);
            }
            return node;
        }
        public override IClassParser.ParseTreeNode VisitChildren(IRuleNode node)
        {
            IClassParser.ParseTreeNode result = new IClassParser.ParseTreeNode();
            /*if(node.RuleContext is Java8Parser.CompilationUnitContext
                ||node.RuleContext is Java8Parser.NormalClassDeclarationContext
                ||node.RuleContext is Java8Parser.FieldDeclarationContext
                ||node.RuleContext is Java8Parser.MethodDeclarationContext
                ||node.RuleContext is Java8Parser.InterfaceDeclarationContext
                ||node.RuleContext is Java8Parser.InterfaceMethodDeclarationContext)
            {
                result = DefaultResult;
            }*/
            int n = node.ChildCount;
            for (int i = 0; i < n; i++)
            {
                if (!ShouldVisitNextChild(node, result))
                {
                    break;
                }
                IParseTree c = node.GetChild(i);
                IClassParser.ParseTreeNode childResult = c.Accept(this);
                //result = AggregateResult(result, childResult);
                if (childResult != null)
                {
                    if (childResult.ItemType == IClassParser.ItemType._PlaceHolder
                        ||childResult.ItemType==IClassParser.ItemType.EnumConstant
                        || childResult.ItemType == IClassParser.ItemType.Field)//expand sub items
                    {
                        result.Children = result.Children.Concat(childResult.Children).ToList();
                    }
                    else
                    {
                        result.Children.Add(childResult);
                    }
                }
            }
            return result;
        }
    }
}
