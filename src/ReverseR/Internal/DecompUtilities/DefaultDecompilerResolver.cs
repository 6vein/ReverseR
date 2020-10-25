using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Ioc;
using ReverseR.Common;
using ReverseR.Common.DecompUtilities;

namespace ReverseR.Internal.DecompUtilities
{
    class DefaultDecompilerResolver : IDecompilerResolver
    {
        private IContainerExtension Container { get; }
        public DefaultDecompilerResolver()
        {
            Container = ContainerLocator.Container as IContainerExtension;
        }
        public void Register(string friendlyName, Type jvmDecompiler, Type embeddedDecompiler)
        {
            if (jvmDecompiler != null && jvmDecompiler.IsSubclassOf(typeof(CommonDecompiler)))
            {
                Container.Register(typeof(CommonDecompiler), jvmDecompiler,
                    $"{friendlyName}.{ICommonPreferences.RunTypes.JVM}");
            }
            if (embeddedDecompiler != null && embeddedDecompiler.IsSubclassOf(typeof(CommonDecompiler)))
            {
                Container.Register(typeof(CommonDecompiler), embeddedDecompiler,
                    $"{friendlyName}.{ICommonPreferences.RunTypes.Embedded}");
            }
        }

        public object Resolve(string friendlyName)
        {
            var list = GlobalUtils.Decompilers.Where(info => info.Name == friendlyName);
            if (list.Count()!=1)
            {
                throw new ContainerResolutionException(typeof(ICommonPreferences), friendlyName, 
                    new Exception("Decompiler not registered in the global catalog or registered twice or more!"));
            }
            var runtypeString = list.First().DefaultPreference.RunType.ToString();
            return Container.Resolve<CommonDecompiler>($"{friendlyName}.{runtypeString}");
        }
    }
}
