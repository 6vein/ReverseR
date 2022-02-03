using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReverseR.Common.Serialization;

namespace ReverseR.Common.DecompUtilities
{
    /// <summary>
    /// Interface that represents decompiler options,such as arguments
    /// </summary>
    public interface ICommonPreferences:IPartialSerializable
    {
        public interface IArgument
        {
            public string Name { get; set; }
            public string Description { get; set; }
            /// <summary>
            /// List of available values, if <see cref="ValueIndex"/> == -1,
            /// then the first element of the item is the selected value,
            /// which also indicates the argument is of type 'string'
            /// </summary>
            public string[] AvailableValues { get; }
            public int ValueIndex { get; set; }
            public string GetArgument();
        }

        public enum RunTypes
        {
            JVM,
            IKVM
        }
        public RunTypes RunType { get; set; }
        /// <summary>
        /// Get arguments for the decompiler
        /// </summary>
        /// <param name="path"></param>
        /// <param name="outpath">the output path,in most cases,you should create it beforehand</param>
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
