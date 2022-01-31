using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReverseR.Common.DecompUtilities
{
    public interface IDecompilerResolver
    {
        void Register(string id, Type jvmDecompiler, Type ikvmDecompiler);
        /// <summary>
        /// Resolve the decompiler with the given name
        /// </summary>
        /// <exception cref="Prism.Ioc.ContainerResolutionException">
        /// when the decompiler of the given cannot run in the default <see cref="ICommonPreferences.RunType"/> or the container itself is currupted
        /// </exception>
        /// <param name="id"></param>
        /// <returns></returns>
        object Resolve(string id);
    }

    public static class DecompilerResolverExtensions
    {
        public static void Register<TJVMDecompiler, TIKVMDecompiler>(this IDecompilerResolver resolver,string id) 
            where TIKVMDecompiler:CommonDecompiler where TJVMDecompiler:CommonDecompiler
        {
            resolver.Register(id, typeof(TJVMDecompiler), typeof(TIKVMDecompiler));
        }
        public static T Resolve<T>(this IDecompilerResolver resolver, string id) where T : CommonDecompiler
        {
            return resolver.Resolve(id) as T;
        }
    }
}
