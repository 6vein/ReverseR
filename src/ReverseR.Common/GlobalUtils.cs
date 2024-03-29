﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Prism.Ioc;
using ReverseR.Common.DecompUtilities;
using ReverseR.Common.ViewUtilities;
using System.Linq.Expressions;
using System.Windows.Markup;

namespace ReverseR.Common
{
    public static class GlobalUtils
    {
        [Serializable]
        public class ConfigStorage
        {
            public string PreferredDecompilerId { get; set; } = "";
            /// <summary>
            /// Determines the base directory of the config files
            /// </summary>
            public string ConfigPrefix { get; set; } = "";

            /// <summary>
            /// Determines where the cache stays.
            /// </summary>
            public string CachePath { get; set; } = "";
            /// <summary>
            /// Enables storing decompiled files in the cache
            /// </summary>
            [DefaultValue(false)]
            public bool CacheDecompiledFiles { get; set; } = false;
            /// <summary>
            /// Enables extracting jar files into cache
            /// </summary>
            [DefaultValue(true)]
            public bool CacheExtractedFiles { get; set; } = true;
            /// <summary>
            /// Decompile the whole file whenever a class based on <see cref="DecompileViewModelBase"/> opened a file.
            /// </summary>
            public bool DecompileWhole { get; set; }
            /// <summary>
            /// Path of java.exe to run jars
            /// </summary>
            public string JavaPath { get; set; } = "";
            /// <summary>
            /// Path of modules
            /// </summary>
            public string ModuleDirectory { get; set; } = "";
            /// <summary>
            /// Infos of modules to load
            /// </summary>
            public RRModuleInfo[] ModuleInfos { get; set; }
            public Dictionary<string, string> DecompilerConfigs { get; set; } = new Dictionary<string, string>();
            public ICommonPreferences.RunTypes RunType { get; set; }
        }
        public static ConfigStorage GlobalConfig { get; set; }
        public static string SaveToString()
        {
            foreach(var decompiler in Decompilers)
            {
                GlobalConfig.DecompilerConfigs[decompiler.Id] = decompiler.Options.SerializePart();
            }
            return JsonConvert.SerializeObject(GlobalConfig);
        }
        public static void LoadFromString(string data)
        {
            if (!string.IsNullOrEmpty(data))
            {
                try
                {
                    GlobalConfig = JsonConvert.DeserializeObject(data, typeof(ConfigStorage)) as ConfigStorage;
                }
                catch { }
                //check valid and set default values
                if (GlobalConfig != null)
                {
                    if (!File.Exists(Environment.ExpandEnvironmentVariables(GlobalConfig.ConfigPrefix)))
                    {
                        GlobalConfig.ConfigPrefix = Environment.ExpandEnvironmentVariables("%UserProfile%\\.ReverseR");
                    }
                    if (string.IsNullOrEmpty(GlobalConfig.JavaPath)|| !File.Exists(Environment.ExpandEnvironmentVariables(GlobalConfig.JavaPath)))
                    {
                        if (Environment.GetEnvironmentVariable("JAVA_HOME") == null)
                        {
                            GlobalConfig.JavaPath = null;
                            GlobalConfig.RunType = ICommonPreferences.RunTypes.IKVM;
                        }
                        else
                        {
                            GlobalConfig.JavaPath = Environment.GetEnvironmentVariable("JAVA_HOME") + "\\bin\\java.exe";
                        }
                    }
                    if (!File.Exists(Environment.ExpandEnvironmentVariables(GlobalConfig.CachePath)))
                    {
                        GlobalConfig.CachePath = Environment.ExpandEnvironmentVariables("%UserProfile%\\.ReverseR\\Cache");
                        Directory.CreateDirectory(GlobalConfig.CachePath);
                    }
                    if (!File.Exists(Environment.ExpandEnvironmentVariables(GlobalConfig.ModuleDirectory)))
                    {
                        GlobalConfig.ModuleDirectory = AppDomain.CurrentDomain.BaseDirectory + "Plugins\\";
                    }
                    if (GlobalConfig.ModuleInfos == null || GlobalConfig.ModuleInfos.Length == 0)
                    {
                        GlobalConfig.ModuleInfos = new string[] { "AntlrParser", "BasicCodeCompletion", "DecompilerCFR" ,"DecompilerFernflower", "PluginSourceControl" }
                        .Select(path => new RRModuleInfo() { Id = $"6168218c.{path}", Path = path, Enabled = true }).ToArray();
                    }
                }
            }
            if (GlobalConfig == null)
            {
                GlobalConfig = new ConfigStorage();
                GlobalConfig.ConfigPrefix = Environment.ExpandEnvironmentVariables("%UserProfile%\\.ReverseR");
                if (Environment.GetEnvironmentVariable("JAVA_HOME") == null)
                {
                    GlobalConfig.JavaPath = null;
                }
                else
                {
                    GlobalConfig.JavaPath = Environment.ExpandEnvironmentVariables("%JAVA_HOME%\\bin\\java.exe");
                }
                GlobalConfig.CachePath = Environment.ExpandEnvironmentVariables("%UserProfile%\\.ReverseR\\Cache");
                GlobalConfig.ModuleDirectory = AppDomain.CurrentDomain.BaseDirectory + "Plugins\\";
                GlobalConfig.ModuleInfos = new string[] { "AntlrParser", "BasicCodeCompletion","DecompilerCFR", "DecompilerFernflower", "PluginSourceControl" }
                .Select(path => new RRModuleInfo() { Id=$"6168218c.{path}", Path = path, Enabled = true }).ToArray();
                Directory.CreateDirectory(GlobalConfig.CachePath);
            }
        }
        public static void Save<T>(T holder,Expression<Func<T,string>> prop)
        {
            MemberExpression memberExpression = prop.Body as MemberExpression;
            var property = typeof(T).GetProperties().First(l => l.Name == memberExpression.Member.Name);
            property.SetValue(holder, SaveToString());

            //File.WriteAllText("config.json", JsonConvert.SerializeObject(GlobalConfig));
        }
        public static void Load<T>(T holder,Expression<Func<T,string>> prop)
        {
            MemberExpression memberExpression = prop.Body as MemberExpression;
            var property = typeof(T).GetProperties().First(l => l.Name == memberExpression.Member.Name);
#if DEBUG
            object str = property?.GetValue(holder);
#endif
            LoadFromString(property?.GetValue(holder) as string);
        }
        public class RRModuleInfo
        {
            public string Id { get; set; }
            [JsonIgnore]
            public string Name { get; set; }
            [JsonIgnore]
            public string Description { get; set; }
            [JsonIgnore]
            public bool Loaded { get; set; }
            public string Path { get; set; }
            public bool Enabled { get; set; }
            public RRModuleInfo() { }
            public RRModuleInfo(RRModuleInfo other)
            {
                Id = other.Id;
                Name = other.Name;
                Description = other.Description;
                Path = other.Path;
                Enabled = other.Enabled;
            }
        }
        public struct DecompilerInfo
        {
            public string Id { get; set; }
            public string FriendlyName { get; set; }
            public Type ViewType { get; set; }
            public (Type jvmDecompiler, Type embeddedDecompiler) DecompilerTypes { get; set; }
            public ICommonPreferences Options { get; set; }
        }
        public struct DockablePluginInfo
        {
            public Type PluginType { get; set; }
            public string JsonConfigPath { get; set; }
        }
        public static IEnumerable<RRModuleInfo> Modules => GlobalConfig.ModuleInfos;
        public static List<DecompilerInfo> Decompilers { get; private set; } = new List<DecompilerInfo>();
        public static List<DockablePluginInfo> DockablePlugins { get; set; } = new List<DockablePluginInfo>();
        public static DecompilerInfo? PreferredDecompiler
        {
            get
            {
                if(string.IsNullOrEmpty(GlobalConfig.PreferredDecompilerId))
                {
                    GlobalConfig.PreferredDecompilerId = Decompilers.First().Id;
                    return Decompilers.First();
                }
                var query = Decompilers.Where(info => info.Id == GlobalConfig.PreferredDecompilerId);
                if (query.Any())
                {
                    GlobalConfig.PreferredDecompilerId = Decompilers.First().Id;
                    return query.First();
                }
                else throw new InvalidOperationException();
            }
            set
            {
                if (!value.HasValue)
                {
                    GlobalConfig.PreferredDecompilerId = GlobalConfig.PreferredDecompilerId = Decompilers.First().Id; ;
                }
                if (Decompilers.Contains(value.Value))
                {
                    GlobalConfig.PreferredDecompilerId = value.Value.Id;
                }
                else
                {
                    throw new InvalidOperationException("Illegal Decompiler info!");
                }
            }
        }
        /// <summary>
        /// Register a decompiler
        /// <para>
        /// IMPORTANT:this does NOT register the decompiler with the container,users should register them themselves
        /// </para>
        /// </summary>
        /// <param name="name">the friendly name of your decompiler</param>
        /// <param name="pref">default options</param>
        /// <param name="viewtype">your own implementation to view the decompiled file,specify null to use the default one</param>
        public static void RegisterDecompiler(string id,string name, ICommonPreferences pref, 
            (Type jvmDecompiler, Type embeddedDecompiler) decompilers, Type viewtype = null)
        {
            if(!GlobalConfig.DecompilerConfigs.ContainsKey(id))
            {
                GlobalConfig.DecompilerConfigs.Add(id,"");//empty config, so we add one
            }
            pref.DeserializePart(GlobalConfig.DecompilerConfigs[id]);
            Decompilers.Add(new DecompilerInfo { Id = id, FriendlyName = name,
                DecompilerTypes=decompilers,
                ViewType = viewtype, Options = pref });
            (System.Windows.Application.Current as Prism.PrismApplicationBase)
                .Container
                .Resolve<IDecompilerResolver>()
                .Register(id, decompilers.jvmDecompiler, decompilers.embeddedDecompiler);
            /*System.Xml.XmlDocument xmlDocument = new System.Xml.XmlDocument();
            //Do not move the configuration file
            xmlDocument.Load(AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName + ".config");
            System.Xml.XmlNamespaceManager manager = new System.Xml.XmlNamespaceManager(xmlDocument.NameTable);
            manager.AddNamespace("ass", "urn:schemas-microsoft-com:asm.v1");

            var probing = xmlDocument.SelectSingleNode("//runtime/ass:assemblyBinding/ass:probing", manager) as System.Xml.XmlElement;
            if(probing!=null)
            {
                string privatepath = probing.GetAttribute("privatePath");
                probing.SetAttribute("privatePath", privatepath + $";Plugins\\Module{name}");
            }
            xmlDocument.Save(AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName + ".config");
            ConfigurationManager.RefreshSection("runtime");*/
        }
        public static void RegisterDockablePlugin(Type type)
        {
            DockablePlugins.Add(new DockablePluginInfo() { PluginType = type });
        }
        public static object ResolveViewByIndex(int index)
        {
            var container = (System.Windows.Application.Current as Prism.PrismApplicationBase).Container;
            object view = null;
            if(Decompilers[index].ViewType == null)
            {
                var defaultView = container.Resolve<IDefaultView>();
                defaultView.SetDecompiler(Decompilers[index].Id);
                view = defaultView;
            }
            else
            {
                view= container.Resolve(Decompilers[index].ViewType);
            }

            return view;
        }
        public static Type GetViewTypeByIndex(int index)
        {
            return Decompilers[index].ViewType;
        }
        public static string GetTempDirectoryPath()
        {
            string file = Path.GetTempFileName();
            string name = Path.GetFileName(file);
            File.Delete(file);
            return Path.Combine(GlobalConfig.CachePath, "Temp", name);
        }
    }
}
