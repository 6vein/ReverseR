using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReverseR.Common.DecompUtilities
{
    public interface IDecompilerResolver
    {
        void Register(string friendlyName, Type jvmDecompiler, Type embeddedDecompiler);
        /// <summary>
        /// Resolve the decompiler with the given name
        /// </summary>
        /// <exception cref="Prism.Ioc.ContainerResolutionException">
        /// when the decompiler of the given cannot run in the default <see cref="ICommonPreferences.RunType"/> or the container itself is currupted
        /// </exception>
        /// <param name="friendlyName"></param>
        /// <returns></returns>
        object Resolve(string friendlyName);
    }

    public static class DecompilerResolverExtensions
    {
        public static void Register<TJVMDecompiler, TEmbeddedDecompiler>(this IDecompilerResolver resolver,string friendlyName) 
            where TEmbeddedDecompiler:CommonDecompiler where TJVMDecompiler:CommonDecompiler
        {
            resolver.Register(friendlyName, typeof(TJVMDecompiler), typeof(TEmbeddedDecompiler));
        }
        public static T Resolve<T>(this IDecompilerResolver resolver, string friendlyName) where T : CommonDecompiler
        {
            return resolver.Resolve(friendlyName) as T;
        }
    }
}
