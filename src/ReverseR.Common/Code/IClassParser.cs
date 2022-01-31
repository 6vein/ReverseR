using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ReverseR.Common.Code
{
    public interface IClassParser
    {
        public enum ItemType
        {
            Class,Interface,Field,Method
        }
        public enum ModifierType
        {
            Public,Protected,Private,Default
        }
        public struct ParseTreeNode
        {
            public ItemType ItemType { get; set; }
            public ModifierType ModifierType { get; set; }
            public string Content { get; set; }
            public int Start { get; set; }
            public int End { get; set; }
            public IEnumerable<ParseTreeNode> Children { get; set; }
        }

        public IEnumerable<ParseTreeNode> Parse(string content);
        public IEnumerable<ParseTreeNode> Parse(Stream stream);
    }
}
