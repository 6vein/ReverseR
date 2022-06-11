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
    [JsonObject]
    internal class FernflowerArguments : ICommonPreferences.IArgument
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string[] AvailableValues { get; private set; }
        public int ValueIndex { get; set; }
        public string GetArgument()
        {
            if (AvailableValues.Count() == 0)
            {
                return "-" + Name + "=" + ValueIndex + " ";
            }
            else if (ValueIndex == -1)
            {
                if (AvailableValues == null 
                    || AvailableValues.Length == 0 
                    || string.IsNullOrEmpty(AvailableValues[0]))
                {
                    return string.Empty;
                }
                else
                {
                    return "-" + Name + "=\"" + AvailableValues[0] + "\" ";
                }
            }
            return "-" + Name + '=' + AvailableValues[ValueIndex] + ' ';
        }
        public FernflowerArguments(string name, string descr, string[] values,int index)
        {
            Name = name;
            Description = descr;
            AvailableValues = values;
            ValueIndex = index;
        }
    }
    public class FernflowerPreferences : ICommonPreferences
    {
        [JsonProperty(PropertyName ="Path")]
        protected string DecompilerBasePath { get; set; }
        [JsonProperty(PropertyName ="Args")]
        protected ICommonPreferences.IArgument[] Arguments { get; set; }
        protected string DecompilerPath => DecompilerBasePath +
            (RunType == ICommonPreferences.RunTypes.JVM ? "fernflower.jar\"" : "fernflower.exe\"");
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
            RunType = ICommonPreferences.RunTypes.JVM;
            DecompilerBasePath = $"\"{Path.GetDirectoryName(GetType().Assembly.Location)}\\{nameof(DecompilerFernflower)}\\";
        }
        public string GetDecompilerPath()
        {
            if (!File.Exists(DecompilerPath.Replace("\"","")))
            {
                return null;
            }
            return DecompilerPath;
        }
        public ICommonPreferences.IArgument[] GetDefaultArguments()
        {
            var arguments = new List<ICommonPreferences.IArgument>();
            arguments.Add(new FernflowerArguments("rbr", "Hide bridge methods", new string[] { "False", "True" }, 1));
            arguments.Add(new FernflowerArguments("rsy", "Hide synthetic class members", new string[] { "False", "True" }, 0));
            arguments.Add(new FernflowerArguments("din", "Decompile inner classes", new string[] { "False", "True" }, 1));
            arguments.Add(new FernflowerArguments("dc4", "Collapse 1.4 class references", new string[] { "False", "True" }, 1));
            arguments.Add(new FernflowerArguments("das", "Decompile assertions", new string[] { "False", "True" }, 1));
            arguments.Add(new FernflowerArguments("hes", "Hide empty super invocation", new string[] { "False", "True" }, 1));
            arguments.Add(new FernflowerArguments("hdc", "Hide empty default constructor", new string[] { "False", "True" }, 1));
            arguments.Add(new FernflowerArguments("dgs", "Decompile generic signatures", new string[] { "False", "True" }, 1));
            arguments.Add(new FernflowerArguments("ner", "Assume return not throwing exceptions", new string[] { "False", "True" }, 1));
            arguments.Add(new FernflowerArguments("den", "Decompile enumerations", new string[] { "False", "True" }, 1));
            arguments.Add(new FernflowerArguments("rgn", "Remove getClass() invocation, when it is part of a qualified new statement", new string[] { "False", "True" }, 1));
            arguments.Add(new FernflowerArguments("lit", "Output numeric literals \"as-is\"", new string[] { "False", "True" }, 0));
            arguments.Add(new FernflowerArguments("asc", "Encode non-ASCII characters in string and character literals as Unicode escapes", new string[] { "False", "True" }, 0));
            arguments.Add(new FernflowerArguments("bto", "Interpret int 1 as boolean true (workaround to a compiler bug)", new string[] { "False", "True" }, 1));
            arguments.Add(new FernflowerArguments("nns", "Allow for not set synthetic attribute(workaround to a compiler bug)", new string[] { "False", "True" }, 0));
            arguments.Add(new FernflowerArguments("uto", "Consider nameless types as java.lang.Object(workaround to a compiler architecture flaw)", new string[] { "False", "True" }, 1));
            arguments.Add(new FernflowerArguments("udv", "Reconstruct variable names from debug information, if present", new string[] { "False", "True" }, 1));
            arguments.Add(new FernflowerArguments("ump", "Reconstruct parameter names from corresponding attributes, if present", new string[] { "False", "True" }, 1));
            arguments.Add(new FernflowerArguments("rer", "Remove empty exception ranges", new string[] { "False", "True" }, 1));
            arguments.Add(new FernflowerArguments("fdi", "De-inline finally structures", new string[] { "False", "True" }, 1));
            arguments.Add(new FernflowerArguments("mpm", "Maximum allowed processing time per decompiled method, in seconds. 0 means no upper limit", new string[] { "False", "True" }, 0));
            arguments.Add(new FernflowerArguments("ren", "Rename ambiguous(resp.obfuscated) classes and class elements", new string[] { "False", "True" }, 0));
            arguments.Add(new FernflowerArguments("urc", "Full name of a user-supplied class implementing IIdentifierRenamer interface. It is used to determine which class identifiers should be renamed and provides new identifier names(see \"Renaming identifiers\")", new string[] { "" }, -1));
            arguments.Add(new FernflowerArguments("inn", "Check for IntelliJ IDEA-specific @NotNull annotation and remove inserted code if found", new string[] { "False", "True" }, 1));
            arguments.Add(new FernflowerArguments("lac", "Decompile lambda expressions to anonymous classes", new string[] { "False", "True" }, 0));
            arguments.Add(new FernflowerArguments("nls", "Define new line character to be used for output. 0 - '\\r\\n' (Windows), 1 - '\\n' (Unix), default is OS-dependent", new string[] { "False", "True" }, 0));
            arguments.Add(new FernflowerArguments("ind", "Indetation string", new string[] { "   " }, -1));
            arguments.Add(new FernflowerArguments("log", "A logging level, possible values are TRACE, INFO, WARN, ERROR", new string[] { "TRACE", "INFO", "WARN", "ERROR" }, 1));
            return arguments.ToArray();
        }
        public IEnumerable<ICommonPreferences.IArgument> GetArguments()
        {
            if (Arguments == null || Arguments.Length == 0)
            {
                Arguments = GetDefaultArguments();
            }
            return Arguments;
        }
        public bool SetArguments(IEnumerable<ICommonPreferences.IArgument> arguments)
        {
            if (arguments.Count() == Arguments.Count())
            {
                Arguments = arguments.Select(it => new FernflowerArguments(it.Name, it.Description, it.AvailableValues, it.ValueIndex))
                    .ToArray();
                return true;
            }
            return false;
        }
        public string GetArgumentsString(string path, string output = null, params string[] referlib)
        {
            if (Arguments == null || Arguments.Length == 0)
            {
                Arguments = GetDefaultArguments();
            }
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
            string arg = " ";
            foreach (var argument in Arguments)
            {
                arg += argument.GetArgument();
            }
            arg += $" \"{path}\" ";
            foreach (string item in referlib)
            {
                arg += $"\"-e={item}\" ";
            }
            arg += $"\"{outpath}\\\\\"";
            return arg;
        }

        public string SerializePart()
        {
            return JsonConvert.SerializeObject(Arguments);
        }

        public void DeserializePart(string value)
        {
            Arguments = JsonConvert.DeserializeObject<FernflowerArguments[]>(value);
        }
    }
}
