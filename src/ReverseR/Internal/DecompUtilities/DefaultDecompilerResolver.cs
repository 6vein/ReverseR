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
        public void Register(string id, Type jvmDecompiler, Type embeddedDecompiler)
        {
            if (jvmDecompiler != null && jvmDecompiler.IsSubclassOf(typeof(CommonDecompiler)))
            {
                Container.Register(typeof(CommonDecompiler), jvmDecompiler,
                    $"{id}.{ICommonPreferences.RunTypes.JVM}");
            }
            if (embeddedDecompiler != null && embeddedDecompiler.IsSubclassOf(typeof(CommonDecompiler)))
            {
                Container.Register(typeof(CommonDecompiler), embeddedDecompiler,
                    $"{id}.{ICommonPreferences.RunTypes.IKVM}");
            }
        }

        public object Resolve(string id)
        {
            var list = GlobalUtils.Decompilers.Where(info => info.Id == id);
            if (list.Count()!=1)
            {
                throw new ContainerResolutionException(typeof(ICommonPreferences), id, 
                    new Exception("Decompiler not registered in the global catalog or registered twice or more!"));
            }
            var runtypeString = list.First().Options.RunType.ToString();
            return Container.Resolve<CommonDecompiler>($"{id}.{runtypeString}");
        }
    }
}
