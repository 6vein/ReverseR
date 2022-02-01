using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ReverseR.Common.DecompUtilities;

namespace ReverseR.Common.Code
{
    public interface IClassParser
    {
        public enum ItemType
        {
            Directory,
            CompilationUnit,
            Others,
            Class,Interface,Field,Constructor,Method,InterfaceMethod
                ,Enum,EnumConstant,__InternalPlaceHolder=-1
        }
        public enum ModifierType
        {
            Public,Protected,Private,Default,Static,Final,Abstract,StrictFP,
            Synchronized,Native,Transient,Volatile
        }
        public IClassParser SetBasePath(IJPath basePath);
        public IEnumerable<ParseTreeNode> Parse(string content);
    }

    public class ParseTreeNode : ICloneable,IJPath
    {
        #region AST
        public IClassParser.ItemType ItemType { get; set; } = IClassParser.ItemType.__InternalPlaceHolder;
        public IList<IClassParser.ModifierType> Modifiers { get; set; } = new List<IClassParser.ModifierType>();
        /// <summary>
        /// Id,also performs as identfier in classpath and in AST tree
        /// </summary>
        public string Id { get; set; } = "";
        public string Content { get; set; } = "";
        public int Start { get; set; }
        public int End { get; set; }
        public IList<ParseTreeNode> Children { get; set; } = new List<ParseTreeNode>();
        #endregion
        #region File
        public bool HasInnerClasses { get; private set; } = false;
        /// <summary>
        /// Path of the file,such as ...\ClassPathHelper.class
        /// </summary>
        public string Path { get; private set; } = "";
        /// <summary>
        /// Path inside the jar
        /// </summary>
        public string ClassPath { get; private set; } = "";
        /// <summary>
        /// Collection of inner classes,such as ...\ClassPathHelper$1.class
        /// </summary>
        public IEnumerable<string> InnerClassPaths { get; private set; } = new List<string>();
        #endregion
        public ParseTreeNode() { }
        public ParseTreeNode(ParseTreeNode other):this()
        {
            if (other != null)
            {
                Children = new List<ParseTreeNode>(other.Children);
                ItemType = other.ItemType;
                Modifiers = new List<IClassParser.ModifierType>(other.Modifiers);
                Content = string.Copy(other.Content);
                Start = other.Start;
                End = other.End;
                HasInnerClasses = other.HasInnerClasses;
                Path = other.Path;
                ClassPath = other.ClassPath;
                InnerClassPaths = other.InnerClassPaths;
            }
        }
        public ParseTreeNode(IJPath jpath):this()
        {
            HasInnerClasses = jpath.HasInnerClasses;
            Path = jpath.Path;
            ClassPath = jpath.ClassPath;
            InnerClassPaths = (jpath.InnerClassPaths == null) ? new List<string>() : new List<string>(InnerClassPaths);
        }
        public object Clone()
        {
            return new ParseTreeNode()
            {
                Children = new List<ParseTreeNode>(this.Children),
                ItemType = this.ItemType,
                Modifiers = new List<IClassParser.ModifierType>(this.Modifiers),
                Content = string.Copy(this.Content),
                Start = this.Start,
                End = this.End,
                HasInnerClasses = this.HasInnerClasses,
                Path = this.Path,
                ClassPath = this.ClassPath,
                InnerClassPaths = new List<string>(this.InnerClassPaths)
            };
        }

        public void SetPaths(string path,string classPath)
        {
            Path = path;
            ClassPath = classPath.Replace('\\', '/');
        }

        public void SetInnerClasses(IEnumerable<string> inners)
        {
            if (inners != null)
            {
                ClassPath = ClassPath.Remove(ClassPath.LastIndexOf('$'));
                InnerClassPaths = new List<string>(inners);
                HasInnerClasses = true;
            }
        }
    }
}
