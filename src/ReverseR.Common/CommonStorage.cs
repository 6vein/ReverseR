using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Prism.Ioc;
using ReverseR.Common.DecompUtilities;

namespace ReverseR.Common
{
    public static class CommonStorage
    {
        public class ConfigStorage
        {
            /// <summary>
            /// Determines the base directory of the config files
            /// </summary>
            public string ConfigPrefix { get; set; }

            /// <summary>
            /// Determines where the cache stays.
            /// </summary>
            public string CachePath { get; set; }

            /// <summary>
            /// Decompile the whole file whenever a class based on <see cref="DecompileViewModelBase"/> opened a file.
            /// </summary>
            public bool DecompileWhole { get; set; }
            /// <summary>
            /// Path of java.exe to run jars
            /// </summary>
            public string JavaPath { get; set; }
            public CommonDecompiler.RunTypes RunType { get; set; }
        }
        public static ConfigStorage GlobalConfig { get; set; }
        public static void Save()
        {
            Properties.Settings.Default.Properties["config"].DefaultValue = JsonConvert.SerializeObject(GlobalConfig);

            //File.WriteAllText("config.json", JsonConvert.SerializeObject(GlobalConfig));
        }
        public static void Load()
        {
            if (Properties.Settings.Default.Properties["config"] != null && Properties.Settings.Default.Properties["config"].DefaultValue != null) 
            {
                GlobalConfig = JsonConvert.DeserializeObject(Properties.Settings.Default.Properties["config"].DefaultValue as string, typeof(ConfigStorage)) as ConfigStorage;
                //check valid and set default values
                if (!File.Exists(Environment.ExpandEnvironmentVariables(GlobalConfig.JavaPath)))
                {
                    if (Environment.GetEnvironmentVariable("JAVA_HOME") == null) 
                    {
                        GlobalConfig.JavaPath = null;
                        GlobalConfig.RunType = CommonDecompiler.RunTypes.Embedded;
                    }
                    else
                    {
                        GlobalConfig.JavaPath = Environment.GetEnvironmentVariable("JAVA_HOME") + "\\bin\\java.exe";
                    }
                }
                if(!File.Exists(Environment.ExpandEnvironmentVariables(GlobalConfig.CachePath)))
                {
                    GlobalConfig.CachePath = Environment.ExpandEnvironmentVariables("%UserProfile%\\.ReverseR\\Cache");
                    Directory.CreateDirectory(GlobalConfig.CachePath);
                }
            }
            else
            {
                if (Environment.GetEnvironmentVariable("JAVA_HOME") == null)
                {
                    GlobalConfig.JavaPath = null;
                    GlobalConfig.RunType = CommonDecompiler.RunTypes.Embedded;
                }
                else
                {
                    GlobalConfig = new ConfigStorage();
                    GlobalConfig.JavaPath = Environment.ExpandEnvironmentVariables("%JAVA_HOME%\\bin\\java.exe");
                    GlobalConfig.CachePath = Environment.ExpandEnvironmentVariables("%UserProfile%\\.ReverseR\\Cache");
                    Directory.CreateDirectory(GlobalConfig.CachePath);
                }
            }
        }
        public struct DecompilerInfo
        {
            public string Name { get; set; }
            public Type ViewType { get; set; }
            public ICommonPreferences DefaultPreference { get; set; }
        }
        public struct DockablePluginInfo
        {
            public Type PluginType { get; set; }
        }
        public static List<DecompilerInfo> Decompilers { get; set; } = new List<DecompilerInfo>();
        public static List<DockablePluginInfo> DockablePlugins { get; set; } = new List<DockablePluginInfo>();
        public static DecompilerInfo PreferredDecompiler { get; set; }
        public static void RegisterDecompiler(Type viewtype,ICommonPreferences pref, string name)
        {
            Decompilers.Add(new DecompilerInfo { Name = name, ViewType = viewtype,DefaultPreference=pref });
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
        public static dynamic ResolveViewByIndex(int index)
        {
            var container = (System.Windows.Application.Current as Prism.PrismApplicationBase).Container;
            return container.Resolve(Decompilers[index].ViewType);
        }
        public static Type GetViewTypeByIndex(int index)
        {
            return Decompilers[index].ViewType;
        }
    }
}
