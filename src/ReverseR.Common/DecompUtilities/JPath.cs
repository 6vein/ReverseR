using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReverseR.Common.DecompUtilities
{
    public class JPath
    {
        public bool HasInnerClasses { get; set; }
        /// <summary>
        /// Path of the file,such as ...\ClassPathHelper.class
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// Collection of inner classes,such as ...\ClassPathHelper$1.class
        /// </summary>
        public IEnumerable<string> InnerClassPaths { get; set; }
        public JPath() { }
        public JPath(string path) { Path = path;HasInnerClasses = false; }
        public JPath(string path,IEnumerable<string> inners) 
        { 
            Path = path;
            if (inners != null) 
            {
                InnerClassPaths = inners;
                HasInnerClasses = true;
            }
        }

        public static implicit operator JPath(string v)
        {
            return new JPath(v);
        }
    }
}
