using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Prism.Ioc;
using Prism.Modularity;
using ReverseR.Common;
using ReverseR.Properties;

namespace ReverseR.Internal.Modularity
{
    public class DefaultModuleCatalog : ModuleCatalog
    {
        /// <summary>
        /// Directory containing modules to search for.
        /// </summary>
        internal List<string> ModulePaths { get; set; }

        /// <summary>
        /// Drives the main logic of building the child domain and searching for the assemblies.
        /// </summary>
        protected override void InnerLoad()
        {
            ModulePaths= new List<string>();
            foreach (var modulePath in GlobalUtils.GlobalConfig.ModuleInfos
                .Where(info=>info.Enabled)
                .Select(info=>info.Path))
            {
                if (!File.Exists(modulePath))
                {
                    //when relative path
                    string name = Path.Combine(GlobalUtils.GlobalConfig.ModuleDirectory, modulePath);
                    if (File.Exists(name))
                    {
                        ModulePaths.Add(name);
                    }
                    else
                    {
                        //when no extension name
                        name = Path.Combine(GlobalUtils.GlobalConfig.ModuleDirectory, modulePath + ".dll");
                        if (File.Exists(name))
                        {
                            ModulePaths.Add(name);
                        }
                    }
                }
                else
                {
                    ModulePaths.Add(modulePath);
                }
            }

            AppDomain childDomain = BuildChildDomain(AppDomain.CurrentDomain);

            try
            {
                List<string> loadedAssemblies = new List<string>();

                var assemblies = (
                                     from Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()
                                     where !(assembly is System.Reflection.Emit.AssemblyBuilder)
                                        && assembly.GetType().FullName != "System.Reflection.Emit.InternalAssemblyBuilder"
                                        // TODO: Do this in a less hacky way... probably never gonna happen
                                        && !assembly.GetName().Name.StartsWith("xunit")
                                        && !string.IsNullOrEmpty(assembly.Location)
                                     select assembly.Location
                                 );

                loadedAssemblies.AddRange(assemblies);

                Type loaderType = typeof(InnerModuleInfoLoader);

                if (loaderType.Assembly != null)
                {
                    var loader =
                        (InnerModuleInfoLoader)
                        childDomain.CreateInstanceFrom(loaderType.Assembly.Location, loaderType.FullName).Unwrap();
                    loader.LoadAssemblies(loadedAssemblies);
                    this.Items.AddRange(loader.GetModuleInfos(ModulePaths.ToArray()));
                }
            }
            finally
            {
                AppDomain.Unload(childDomain);
            }
        }


        /// <summary>
        /// Creates a new child domain and copies the evidence from a parent domain.
        /// </summary>
        /// <param name="parentDomain">The parent domain.</param>
        /// <returns>The new child domain.</returns>
        /// <remarks>
        /// Grabs the <paramref name="parentDomain"/> evidence and uses it to construct the new
        /// <see cref="AppDomain"/> because in a ClickOnce execution environment, creating an
        /// <see cref="AppDomain"/> will by default pick up the partial trust environment of
        /// the AppLaunch.exe, which was the root executable. The AppLaunch.exe does a
        /// create domain and applies the evidence from the ClickOnce manifests to
        /// create the domain that the application is actually executing in. This will
        /// need to be Full Trust for Prism applications.
        /// </remarks>
        /// <exception cref="ArgumentNullException">An <see cref="ArgumentNullException"/> is thrown if <paramref name="parentDomain"/> is null.</exception>
        internal static AppDomain BuildChildDomain(AppDomain parentDomain)
        {
            if (parentDomain == null)
                throw new ArgumentNullException(nameof(parentDomain));

            Evidence evidence = new Evidence(parentDomain.Evidence);
            AppDomainSetup setup = parentDomain.SetupInformation;
            return AppDomain.CreateDomain("DiscoveryRegion", evidence, setup);
        }

        internal class InnerModuleInfoLoader : MarshalByRefObject
        {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
            internal ModuleInfo[] GetModuleInfos(string[] paths)
            {
                var array = new List<ModuleInfo>();
                foreach(string path in paths)
                {
                    DirectoryInfo directory = new DirectoryInfo(path);

                    ResolveEventHandler resolveEventHandler =
                        delegate (object sender, ResolveEventArgs args) { return OnReflectionOnlyResolve(args, directory); };

                    AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += resolveEventHandler;

                    Assembly moduleReflectionOnlyAssembly =
                        AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies().First(
                            asm => asm.FullName == typeof(IModule).Assembly.FullName);
                    Type IModuleType = moduleReflectionOnlyAssembly.GetType(typeof(IModule).FullName);

                    IEnumerable<ModuleInfo> modules = GetNotAlreadyLoadedModuleInfos(path, IModuleType);

                    array.AddRange(modules);
                    AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= resolveEventHandler;
                }
                return array.ToArray();
            }

            private static IEnumerable<ModuleInfo> GetNotAlreadyLoadedModuleInfos(string path, Type IModuleType)
            {
                List<FileInfo> validAssemblies = new List<FileInfo>();
                Assembly[] alreadyLoadedAssemblies = AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies();

                if(path.EndsWith(".dll",StringComparison.OrdinalIgnoreCase)&&!alreadyLoadedAssemblies
                    .Any(assembly=>string.Compare(Path.GetFileName(assembly.Location)
                    , path, StringComparison.OrdinalIgnoreCase) == 0))
                {
                    try
                    {
                        Assembly.ReflectionOnlyLoadFrom(path);
                        validAssemblies.Add(new FileInfo(path));
                    }
                    catch (BadImageFormatException)
                    {
                        // skip non-.NET Dlls
                    }
                }


                return validAssemblies.SelectMany(file => Assembly.ReflectionOnlyLoadFrom(file.FullName)
                                            .GetExportedTypes()
                                            .Where(IModuleType.IsAssignableFrom)
                                            .Where(t => t != IModuleType)
                                            .Where(t => !t.IsAbstract)
                                            .Select(type => CreateModuleInfo(type)));
            }

            private static Assembly OnReflectionOnlyResolve(ResolveEventArgs args, DirectoryInfo directory)
            {
                Assembly loadedAssembly = AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies().FirstOrDefault(
                    asm => string.Equals(asm.FullName, args.Name, StringComparison.OrdinalIgnoreCase));
                if (loadedAssembly != null)
                {
                    return loadedAssembly;
                }
                AssemblyName assemblyName = new AssemblyName(args.Name);
                string dependentAssemblyFilename = Path.Combine(directory.FullName, assemblyName.Name + ".dll");
                if (File.Exists(dependentAssemblyFilename))
                {
                    return Assembly.ReflectionOnlyLoadFrom(dependentAssemblyFilename);
                }
                return Assembly.ReflectionOnlyLoad(args.Name);
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
            internal void LoadAssemblies(IEnumerable<string> assemblies)
            {
                foreach (string assemblyPath in assemblies)
                {
                    try
                    {
                        Assembly.ReflectionOnlyLoadFrom(assemblyPath);
                    }
                    catch (FileNotFoundException)
                    {
                        // Continue loading assemblies even if an assembly can not be loaded in the new AppDomain
                    }
                }
            }

            private static ModuleInfo CreateModuleInfo(Type type)
            {
                string moduleName = type.Name;
                List<string> dependsOn = new List<string>();
                bool onDemand = false;
                var moduleAttribute =
                    CustomAttributeData.GetCustomAttributes(type).FirstOrDefault(
                        cad => cad.Constructor.DeclaringType.FullName == typeof(ModuleAttribute).FullName);

                if (moduleAttribute != null)
                {
                    foreach (CustomAttributeNamedArgument argument in moduleAttribute.NamedArguments)
                    {
                        string argumentName = argument.MemberInfo.Name;
                        switch (argumentName)
                        {
                            case "ModuleName":
                                moduleName = (string)argument.TypedValue.Value;
                                break;

                            case "OnDemand":
                                onDemand = (bool)argument.TypedValue.Value;
                                break;

                            case "StartupLoaded":
                                onDemand = !((bool)argument.TypedValue.Value);
                                break;
                        }
                    }
                }

                var moduleDependencyAttributes =
                    CustomAttributeData.GetCustomAttributes(type).Where(
                        cad => cad.Constructor.DeclaringType.FullName == typeof(ModuleDependencyAttribute).FullName);

                foreach (CustomAttributeData cad in moduleDependencyAttributes)
                {
                    dependsOn.Add((string)cad.ConstructorArguments[0].Value);
                }

                ModuleInfo moduleInfo = new ModuleInfo(moduleName, type.AssemblyQualifiedName)
                {
                    InitializationMode =
                                                    onDemand
                                                        ? InitializationMode.OnDemand
                                                        : InitializationMode.WhenAvailable,
                    Ref = type.Assembly.EscapedCodeBase,
                };
                moduleInfo.DependsOn.AddRange(dependsOn);
                return moduleInfo;
            }
            public string GetModuleId(ModuleInfo moduleInfo)
            {
                if (Type.GetType(moduleInfo.ModuleType) != null)
                {
                    IModule module = Activator.CreateInstance(Type.GetType(moduleInfo.ModuleType)) as IModule;
                    if (module is ReverseR.Common.Modularity.ModuleBase baseModule)
                    {
                        return baseModule.Id;
                    }
                }
                return null;
            }
        }
    }
}
