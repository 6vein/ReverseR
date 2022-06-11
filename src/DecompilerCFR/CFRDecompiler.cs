using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using ReverseR.Common;
using ReverseR.Common.DecompUtilities;
using Newtonsoft.Json;
using System.ComponentModel;
using Prism.Ioc;
using ICSharpCode.SharpZipLib.Zip;

namespace DecompilerCFR.Decompile
{
    public abstract class CFRDecompiler : CommonDecompiler
    {
        protected IContainerProvider Container { get; set; }
        public override string Id => $"org.benf.cfr";
        public CFRDecompiler(IContainerProvider containerProvider)
        {
            Container = containerProvider;
            Options = GlobalUtils.Decompilers.First(info => info.Id == Id).Options;
        }
        protected string tempJarPath;
        protected void PreProcessFiles(string path)
        {
            FastZip zip = new FastZip();
            var tempPath = Path.GetTempFileName();
            File.Delete(tempPath);
            Directory.CreateDirectory(tempPath);
            tempJarPath = Path.Combine(tempPath, "temp.jar");
            zip.CreateZip(tempJarPath, path, true, "");
        }
        protected void PostProcessFiles(string path,string target)
        {
            string[] fileNames = Directory.GetFiles(path, "*.java", SearchOption.AllDirectories);
            foreach (string file in fileNames)
            {
                File.Copy(file, Path.Combine(target, Path.GetFileName(file)));
            }
            Directory.Delete(Path.GetDirectoryName(tempJarPath), true);
            tempJarPath = "";
        }
    }
}
