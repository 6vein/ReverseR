using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReverseR.Common;
using ReverseR.Common.DecompUtilities;
using Newtonsoft.Json;
using System.IO;

namespace DecompilerFernflower.Decompile
{
    [JsonObject(MemberSerialization.OptIn)]
    public class FernflowerPreferences : ICommonPreferences
    {
        [JsonProperty(PropertyName ="Path")]
        protected string DecompilerPath { get; set; }
        public ICommonPreferences.RunTypes RunType { get; set; }
        /// <summary>
        /// Constructor of <see cref="FernflowerPreferences"/>,needs %JAVA_HOME% to be set.
        /// </summary>
        /// <remarks>
        /// JAVA_HOME is the base directory of the JDK/JRE
        /// </remarks>
        /// <exception cref="DirectoryNotFoundException"></exception>
        [JsonConstructor]
        public FernflowerPreferences()
        {
            DecompilerPath = $"\"{AppDomain.CurrentDomain.BaseDirectory}Plugins\\Jars\\fernflower.jar\"";
            RunType = ICommonPreferences.RunTypes.JVM;
        }
        public string GetDecompilerPath()
        {
            if (!File.Exists(DecompilerPath.Replace("\"","")))
            {
                throw new InvalidOperationException($"Decompiler at {DecompilerPath} does not exist!");
            }
            return DecompilerPath;
        }

        public string GetArgumentsString(string path, string output = null, params string[] referlib)
        {
            //Todo...
            string outpath = output;
            if (outpath == null) 
            {
                if (Directory.Exists(path))
                {
                    outpath = Path.Combine(path, "decompiled");
                    Directory.CreateDirectory(outpath);
                }
                else if (File.Exists(path))
                {
                    outpath = Path.Combine(Path.GetDirectoryName(path), "decompiled");
                    Directory.CreateDirectory(outpath);
                }
                else
                {
                    throw new ArgumentException($"Error:\"{path}\" is not a valid path!");
                }
            }
            string arg = $" \"{path}\" ";
            foreach(string item in referlib)
            {
                arg += $"\"-e={item}\" ";
            }
            arg += $"\"{outpath}\\\\\"";
            return arg;
        }
    }
}
