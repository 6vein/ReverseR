using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReverseR.Common.DecompUtilities
{
    //For some reasons,it had to be implemented as class
    public abstract class CommonDecompiler
    {
        public abstract string Id { get; }
        public ICommonPreferences Options { get; protected set; }
        public Action PreDecompileCallback { get; set; }
        public Action AfterDecompileCallback { get; set; }
        public IDecompileResult Decompile(string path, Action<string> msgSetter, CancellationToken? token = null, params string[] referlib)
        {
            PreDecompileCallback?.Invoke();
            IDecompileResult result = RunDecompiler(path, msgSetter, token, referlib);
            AfterDecompileCallback?.Invoke();
            return result;
        }
        public GlobalUtils.DecompilerInfo GetDecompilerInfo()
        {
            return GlobalUtils.Decompilers.First(item => item.Id == Id);
        }
        protected abstract IDecompileResult RunDecompiler(string path, Action<string> msgSetter, CancellationToken? token, params string[] referlib);
    }
}
