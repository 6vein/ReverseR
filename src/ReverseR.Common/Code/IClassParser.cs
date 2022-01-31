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
            Class,Interface,Field,Constructor,Method,InterfaceMethod
                ,Enum,EnumConstant,_PlaceHolder=-1
        }
        public enum ModifierType
        {
            Public,Protected,Private,Default,Static,Final,Abstract,StrictFP,
            Synchronized,Native,Transient,Volatile
        }
        public class ParseTreeNode:ICloneable
        {
            public ItemType ItemType { get; set; } = ItemType._PlaceHolder;
            public IList<ModifierType> Modifiers { get; set; }=new List<ModifierType>();
            public string Id { get; set; } = "";
            public string Content { get; set; } = "";
            public int Start { get; set; }
            public int End { get; set; }
            public IList<ParseTreeNode> Children { get; set; } = new List<ParseTreeNode>();

            public object Clone()
            {
                return new ParseTreeNode()
                {
                    Children = new List<ParseTreeNode>(this.Children),
                    ItemType = this.ItemType,
                    Modifiers = new List<ModifierType>(this.Modifiers),
                    Content = string.Copy(this.Content),
                    Start = this.Start,
                    End = this.End
                };
            }
        }

        public IEnumerable<ParseTreeNode> Parse(string content);
        public IEnumerable<ParseTreeNode> Parse(Stream stream);
    }
}
