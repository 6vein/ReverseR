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
        /// Path inside the jar
        /// </summary>
        public string ClassPath { get; set; }
        /// <summary>
        /// Collection of inner classes,such as ...\ClassPathHelper$1.class
        /// </summary>
        public IEnumerable<string> InnerClassPaths { get; set; }
        public JPath() { }
        public JPath(string path,string classPath) { Path = path; ClassPath =classPath.Replace('\\','/') ;HasInnerClasses = false; }
        public JPath(string path,string classPath , IEnumerable<string> inners = null)
        { 
            Path = path;
            ClassPath = classPath.Replace('\\', '/');
            if (inners != null) 
            {
                InnerClassPaths = inners;
                HasInnerClasses = true;
            }
        }
    }
}
