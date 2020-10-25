using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;
using ReverseR.Common;
using ReverseR.Common.DecompUtilities;
using Newtonsoft.Json;
using System.ComponentModel;
using Prism.Ioc;

namespace DecompilerFernflower.Decompile
{
    /// <summary>
    /// Fernflower decompiler,please use <see cref="Prism.Ioc.IContainerProvider"/> to create a instance
    /// Please pack the classes into a zip file before decompiling them.
    /// </summary>
    public abstract class FernflowerDecompiler : CommonDecompiler
    {
        protected IContainerProvider Container { get; set; }
        public FernflowerDecompiler(IContainerProvider containerProvider)
        {
            Container = containerProvider;
        }
    }
}
