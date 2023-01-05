using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReverseR.Common;
using ReverseR.Common.DecompUtilities;
using Newtonsoft.Json;
using System.IO;

namespace DecompilerCFR.Decompile
{
    [JsonObject]
    internal class CFRArguments : ICommonPreferences.IArgument
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string[] AvailableValues { get; private set; }
        public int ValueIndex { get; set; }
        public string GetArgument()
        {
            if (AvailableValues.Count() == 0)
            {
                return "--" + Name + " " + ValueIndex + " ";
            }
            else if (ValueIndex == -1)
            {
                if (string.IsNullOrEmpty(AvailableValues[0]))
                {
                    return string.Empty;
                }
                else
                {
                    return "--" + Name + " \"" + AvailableValues[0] + "\" ";
                }
            }
            else if (AvailableValues[0] == "Default")
            {
                return string.Empty;
            }
            return "--" + Name + ' ' + AvailableValues[ValueIndex] + ' ';
        }
        [JsonConstructor]
        public CFRArguments()
        {
            Name = "";
            Description = "";
            AvailableValues = new string[0];
            ValueIndex = 0;
        }
        public CFRArguments(string name, string descr, string[] values, int index)
        {
            Name = name;
            Description = descr;
            AvailableValues = values ?? new string[0];
            ValueIndex = index;
        }
    }
    public class CFRPreferences : ICommonPreferences
    {
        [JsonProperty(PropertyName = "Path")]
        protected string DecompilerBasePath { get; set; }
        [JsonProperty(PropertyName = "Args")]
        protected ICommonPreferences.IArgument[] Arguments { get; set; }
        protected string DecompilerPath => DecompilerBasePath +
            (RunType == ICommonPreferences.RunTypes.JVM ? "cfr-0.152.jar\"" : "cfr-0.152.exe\"");
        public ICommonPreferences.RunTypes RunType { get; set; }
        /// <summary>
        /// Constructor of <see cref="CFRPreferences"/>,needs %JAVA_HOME% to be set.
        /// </summary>
        /// <remarks>
        /// JAVA_HOME is the base directory of the JDK/JRE
        /// </remarks>
        /// <exception cref="DirectoryNotFoundException"></exception>
        [JsonConstructor]
        public CFRPreferences()
        {
            RunType = ICommonPreferences.RunTypes.JVM;
            DecompilerBasePath = $"\"{Path.GetDirectoryName(GetType().Assembly.Location)}\\{nameof(DecompilerCFR)}\\";
        }
        public string GetDecompilerPath()
        {
            if (!File.Exists(DecompilerPath.Replace("\"", "")))
            {
                return null;
            }
            return DecompilerPath;
        }
        public ICommonPreferences.IArgument[] GetDefaultArguments()
        {
            var arguments = new List<ICommonPreferences.IArgument>();
            arguments.Add(new CFRArguments("aexagg", "", new string[] { "Default", "False", "True" }, 0));
            arguments.Add(new CFRArguments("aexagg2", "", new string[] { "Default", "False", "True" }, 0));
            arguments.Add(new CFRArguments("aggressivedocopy", "", new string[0], 0));
            arguments.Add(new CFRArguments("aggressivedoextension", "", new string[] { "Default", "False", "True" }, 0));
            arguments.Add(new CFRArguments("aggressiveduff", "", new string[] { "Default", "False", "True" }, 0));
            arguments.Add(new CFRArguments("aggressivesizethreshold", "", new string[0], 13000));
            arguments.Add(new CFRArguments("allowcorrecting", "", new string[] { "False", "True" }, 1));
            arguments.Add(new CFRArguments("allowmalformedswitch", "", new string[] { "Default", "False", "True" }, 0));
            arguments.Add(new CFRArguments("antiobf", "", new string[] { "False", "True" }, 0));
            arguments.Add(new CFRArguments("caseinsensitivefs", "", new string[] { "False", "True" }, 1));
            arguments.Add(new CFRArguments("clobber", "", new string[] { "Default", "False", "True" }, 0));
            arguments.Add(new CFRArguments("commentmonitors", "", new string[] { "False", "True" }, 0));
            arguments.Add(new CFRArguments("comments", "", new string[] { "False", "True" }, 1));
            arguments.Add(new CFRArguments("decodefinally", "", new string[] { "False", "True" }, 1));
            arguments.Add(new CFRArguments("dumpclasspath", "", new string[] { "False", "True" }, 0));
            arguments.Add(new CFRArguments("eclipse", "", new string[] { "False", "True" }, 1));
            arguments.Add(new CFRArguments("elidescala", "", new string[] { "False", "True" }, 0));
            arguments.Add(new CFRArguments("extraclasspath", "", new string[] { "" }, -1));
            arguments.Add(new CFRArguments("forbidanonymousclasses", "", new string[] { "False", "True" }, 0));
            arguments.Add(new CFRArguments("forbidmethodscopedclasses", "", new string[] { "False", "True" }, 0));
            arguments.Add(new CFRArguments("forceclassfilever", "specifying either java version as 'j6', 'j1.0', or classfile as '56', '56.65535'", new string[] { "" }, -1));
            arguments.Add(new CFRArguments("forcecondpropagate", "", new string[] { "Default", "False", "True" }, 0));
            arguments.Add(new CFRArguments("forceexceptionprune", "", new string[] { "Default", "False", "True" }, 0));
            arguments.Add(new CFRArguments("forcereturningifs", "", new string[] { "Default", "False", "True" }, 0));
            arguments.Add(new CFRArguments("forcetopsort", "", new string[] { "Default", "False", "True" }, 0));
            arguments.Add(new CFRArguments("forcetopsortaggress", "", new string[] { "Default", "False", "True" }, 0));
            arguments.Add(new CFRArguments("forcetopsortnopull", "", new string[] { "Default", "False", "True" }, 0));
            arguments.Add(new CFRArguments("forloopaggcapture", "", new string[] { "Default", "False", "True" }, 0));
            arguments.Add(new CFRArguments("hidelangimports", "", new string[] { "False", "True" }, 1));
            arguments.Add(new CFRArguments("hidelongstrings", "", new string[] { "False", "True" }, 0));
            arguments.Add(new CFRArguments("hideutf", "", new string[] { "False", "True" }, 1));
            arguments.Add(new CFRArguments("ignoreexceptions", "", new string[] { "False", "True" }, 0));
            arguments.Add(new CFRArguments("ignoreexceptionsalways", "", new string[] { "False", "True" }, 0));
            arguments.Add(new CFRArguments("importfilter", "", new string[] { "" }, -1));
            arguments.Add(new CFRArguments("innerclasses", "", new string[] { "False", "True" }, 1));
            arguments.Add(new CFRArguments("jarfilter", "", new string[] { "" }, -1));
            arguments.Add(new CFRArguments("labelledblocks", "", new string[] { "False", "True" }, 1));
            arguments.Add(new CFRArguments("lenient", "", new string[] { "False", "True" }, 0));
            arguments.Add(new CFRArguments("liftconstructorinit", "", new string[] { "False", "True" }, 1));
            arguments.Add(new CFRArguments("lomem", "", new string[] { "False", "True" }, 0));
            arguments.Add(new CFRArguments("methodname", "", new string[] { "" }, -1));
            arguments.Add(new CFRArguments("obfuscationpath", "", new string[] { "" }, -1));
            arguments.Add(new CFRArguments("outputdir", "", new string[] { "" }, -1));
            arguments.Add(new CFRArguments("outputencoding", "", new string[] { "" }, -1));
            arguments.Add(new CFRArguments("outputpath", "", new string[] { "" }, -1));
            arguments.Add(new CFRArguments("previewfeatures", "", new string[] { "False", "True" }, 1));
            arguments.Add(new CFRArguments("pullcodecase", "", new string[] { "False", "True" }, 0));
            arguments.Add(new CFRArguments("recover", "", new string[] { "False", "True" }, 1));
            arguments.Add(new CFRArguments("recovertypeclash", "", new string[] { "Default", "False", "True" }, 0));
            arguments.Add(new CFRArguments("recovertypehints", "", new string[] { "Default", "False", "True" }, 0));
            arguments.Add(new CFRArguments("reducecondscope", "", new string[] { "Default", "False", "True" }, 0));
            arguments.Add(new CFRArguments("relinkconst", "", new string[] { "False", "True" }, 1));
            arguments.Add(new CFRArguments("removebadgenerics", "", new string[] { "False", "True" }, 1));
            arguments.Add(new CFRArguments("removeboilerplate", "", new string[] { "False", "True" }, 1));
            arguments.Add(new CFRArguments("removedeadconditionals", "", new string[] { "Default", "False", "True" }, 0));
            arguments.Add(new CFRArguments("removedeadmethods", "", new string[] { "False", "True" }, 1));
            arguments.Add(new CFRArguments("removeinnerclasssynthetics", "", new string[] { "False", "True" }, 1));
            arguments.Add(new CFRArguments("rename", "", new string[] { "False", "True" }, 0));
            arguments.Add(new CFRArguments("renamesmallmembers", "", new string[0], 0));
            arguments.Add(new CFRArguments("showversion", "", new string[] { "False", "True" }, 1));
            arguments.Add(new CFRArguments("silent", "", new string[] { "False", "True" }, 0));
            arguments.Add(new CFRArguments("skipbatchinnerclasses", "", new string[] { "False", "True" }, 1));
            arguments.Add(new CFRArguments("staticinitreturn", "", new string[] { "False", "True" }, 1));
            arguments.Add(new CFRArguments("sugarasserts", "", new string[] { "False", "True" }, 1));
            arguments.Add(new CFRArguments("sugarboxing", "", new string[] { "False", "True" }, 1));
            arguments.Add(new CFRArguments("sugarretrolambda", "", new string[] { "False", "True" }, 0));
            arguments.Add(new CFRArguments("tidymonitors", "", new string[] { "False", "True" }, 1));
            arguments.Add(new CFRArguments("trackbytecodeloc", "", new string[] { "False", "True" }, 0));
            arguments.Add(new CFRArguments("usenametable", "", new string[] { "False", "True" }, 1));
            arguments.Add(new CFRArguments("usesignatures", "", new string[] { "False", "True" }, 1));
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
                Arguments = arguments.Select(it => new CFRArguments(it.Name, it.Description, it.AvailableValues, it.ValueIndex))
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
                    output = Path.Combine(path, "decompiled");
                    path += "\\";//add backslash for fernflower
                    Directory.CreateDirectory(output);
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
            arg += $" \"{path}\" ";
            foreach (var argument in Arguments)
            {
                arg += argument.GetArgument();
            }
            foreach (string item in referlib)
            {
                arg += $"--extraclasspath \"{item}\" ";
            }
            arg += $"--outputdir \"{outpath}\"";
            return arg;
        }

        public string SerializePart()
        {
            return JsonConvert.SerializeObject(Arguments);
        }

        public void DeserializePart(string value)
        {
            Arguments = JsonConvert.DeserializeObject<CFRArguments[]>(value) ?? GetDefaultArguments();
            if (this.GetInvalidArguments(Arguments).Any())
            {
                this.MergeInvalidArguments(Arguments);
            }
        }
    }
}
