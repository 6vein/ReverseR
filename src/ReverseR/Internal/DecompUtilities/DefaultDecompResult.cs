using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReverseR.Common.DecompUtilities;

namespace ReverseR.Internal.DecompUtilities
{
    class DefaultDecompResult:IDecompileResult
    {
        /// <summary>
        /// The code JVM returns,if it is using embedded decompiler,return 1 when error occours
        /// </summary>
        public int ReturnCode { get; set; } = 0;
        /// <summary>
        /// The result,for more info,see <see cref="DecompileResultEnum"/>
        /// </summary>
        public DecompileResultEnum ResultCode { get; set; } = DecompileResultEnum.Error;
        /// <summary>
        /// Base directory of the output path
        /// </summary>
        public string OutputDir { get; set; } = "";
        /// <summary>
        /// Check if any Java exception(s) occoured
        /// </summary>
        public bool HasError { get; set; } = false;
    }
}
