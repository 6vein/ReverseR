using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReverseR.Common.DecompUtilities
{
    /// <summary>
    /// Interface that represents decompiler options,such as arguments
    /// </summary>
    public interface ICommonPreferences
    {
        public interface IArgument
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public bool IsSet { get; set; }
            public string GetArgument();
        }

        public enum RunTypes
        {
            JVM,
            Embedded
        }
        public RunTypes RunType { get; set; }
        /// <summary>
        /// Get arguments for the decompiler
        /// </summary>
        /// <param name="path"></param>
        /// <param name="outpath">in most cases,you should create it beforehand</param>
        /// <param name="referlib"></param>
        /// <returns></returns>
        string GetArgumentsString(string path, string outpath = null, params string[] referlib);
        /// <summary>
        /// Get path of the executable jar file for decompiling,but can also return null or throw <see cref="NotImplementedException"/>
        /// </summary>
        /// <returns></returns>
        string GetDecompilerPath();
    }
}
