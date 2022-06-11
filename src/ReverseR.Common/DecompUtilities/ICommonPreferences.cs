using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReverseR.Common.Serialization;

namespace ReverseR.Common.DecompUtilities
{
    /// <summary>
    /// Interface that represents decompiler options,such as arguments
    /// </summary>
    public interface ICommonPreferences:IPartialSerializable
    {
        public interface IArgument
        {
            public string Name { get; set; }
            public string Description { get; set; }
            /// <summary>
            /// List of available values, 
            /// <para>if <see cref="ValueIndex"/> == -1,
            /// then the argument is of type 'string',</para>
            /// <para>if <see cref="AvailableValues"/> is of 0 length
            /// then the argument is of type 'int'</para>
            /// which also indicates the first element of the item is the selected value
            /// </summary>
            public string[] AvailableValues { get; }
            public int ValueIndex { get; set; }
            public string GetArgument();
        }

        public enum RunTypes
        {
            JVM,
            IKVM
        }
        public RunTypes RunType { get; set; }
        /// <summary>
        /// Get arguments for the decompiler
        /// </summary>
        /// <param name="path"></param>
        /// <param name="outpath">the output path,in most cases,you should create it beforehand</param>
        /// <param name="referlib"></param>
        /// <returns></returns>
        string GetArgumentsString(string path, string outpath = null, params string[] referlib);
        IEnumerable<IArgument> GetArguments();
        bool SetArguments(IEnumerable<IArgument> arguments);
        /// <summary>
        /// Get path of the executable jar file for decompiling,but can also return null or throw <see cref="NotImplementedException"/>
        /// </summary>
        /// <returns></returns>
        string GetDecompilerPath();
    }
    public static class PreferenceHelper
    {
        internal class ArgumentValidComparer : IEqualityComparer<ICommonPreferences.IArgument>
        {
            public bool Equals(ICommonPreferences.IArgument x, ICommonPreferences.IArgument y)
            {
                if (x.AvailableValues.Count() == 1 && y.AvailableValues.Count() == 1)
                {
                    return x.ValueIndex == -1 && y.ValueIndex == -1;
                }
                else if(x.AvailableValues.Count() == 0 && y.AvailableValues.Count() == 0)
                {
                    return true;
                }
                else
                {
                    return x.AvailableValues.SequenceEqual(y.AvailableValues);
                }
            }

            public int GetHashCode(ICommonPreferences.IArgument obj)
            {
                return obj.AvailableValues.GetHashCode();
            }
        }
        public static IEnumerable<ICommonPreferences.IArgument> GetInvalidArguments(this ICommonPreferences preferences,IEnumerable<ICommonPreferences.IArgument> arguments)
        {
            return arguments.Except(preferences.GetArguments(), new ArgumentValidComparer());
        }
        public static bool IsStringArgument(this ICommonPreferences.IArgument argument)
        {
            return argument.AvailableValues.Count() == 1 && argument.ValueIndex == -1;
        }
        public static bool IsIntArgument(this ICommonPreferences.IArgument argument)
        {
            return argument.AvailableValues.Count() == 0;
        }
        public static bool IsSelectionArgument(this ICommonPreferences.IArgument argument)
        {
            return argument.AvailableValues.Count() > 0 && argument.ValueIndex >= 0;
        }
    }
}
