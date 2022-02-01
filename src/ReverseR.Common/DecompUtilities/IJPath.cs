using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReverseR.Common.DecompUtilities
{
    public interface IJPath
    {
        public bool HasInnerClasses { get; }
        /// <summary>
        /// Path of the file,such as ...\ClassPathHelper.class
        /// If there is inner classes, Path is actually fake,that you cannot find the real file.
        /// </summary>
        public string Path { get; }
        /// <summary>
        /// Path inside the jar
        /// </summary>
        public string ClassPath { get; }
        /// <summary>
        /// Collection of inner classes,such as ...\ClassPathHelper$1.class
        /// </summary>
        public IEnumerable<string> InnerClassPaths { get; }
        public void SetPaths(string path, string classPath);
        public void SetInnerClasses(IEnumerable<string> innerClasses);
    }
}
