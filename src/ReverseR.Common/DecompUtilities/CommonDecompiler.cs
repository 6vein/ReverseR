using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReverseR.Common.DecompUtilities
{
    //For some reasons,it had to be implemented as class
    public abstract class CommonDecompiler
    {
        public ICommonPreferences Preferences { get; set; }
        public Action PreDecompileCallback { get; set; }
        public Action AfterDecompileCallback { get; set; }

        public enum RunTypes
        {
            JVM,
            Embedded
        }
        public RunTypes RunType { get; set; } = RunTypes.JVM;

        public IDecompileResult Decompile(string path, Action<string> msgSetter, CancellationToken? token = null, params string[] referlib)
        {
            PreDecompileCallback?.Invoke();
            IDecompileResult result = RunDecompiler(path, msgSetter, token, referlib);
            AfterDecompileCallback?.Invoke();
            return result;
        }
        protected abstract IDecompileResult RunDecompiler(string path, Action<string> msgSetter, CancellationToken? token, params string[] referlib);
    }
}
