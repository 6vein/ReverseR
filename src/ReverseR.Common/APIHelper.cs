using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Media;
using Prism.Ioc;

namespace ReverseR.Common
{
    public static class APIHelper
    {
        [DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
        public static extern Boolean SetProcessPreferredUILanguages(
            UInt32 dwFlags,
            String pwszLanguagesBuffer,
            ref UInt32 pulNumLanguages
            );
        public const UInt32 MUI_LANGUAGE_NAME = 0x8;
        public static IContainerProvider GetIContainer(this Prism.Mvvm.BindableBase obj)
        {
            return (System.Windows.Application.Current as Prism.PrismApplicationBase).Container;
        }
        public static IContainerProvider GetIContainer(this ViewUtilities.IDockablePlugin obj)
        {
            return (System.Windows.Application.Current as Prism.PrismApplicationBase).Container;
        }
        [Obsolete]
        public static IContainerProvider GetIContainer()
        {
            return (System.Windows.Application.Current as Prism.PrismApplicationBase).Container;
        }

        public static string GetMd5Of(string path)
        {
            var md5obj = new System.Security.Cryptography.MD5CryptoServiceProvider();
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            byte[] buffer = md5obj.ComputeHash(fs);
            fs.Close();
            string md5 = BitConverter.ToString(buffer);
            md5 = md5.Replace("-", "");
            return md5;
        }

        public static string GetMd5OfText(string text)
        {
            var md5obj = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] buffer = md5obj.ComputeHash(Encoding.Default.GetBytes(text));
            string md5 = BitConverter.ToString(buffer);
            md5 = md5.Replace("-", "");
            return md5;
        }

        /// <summary>
        /// Find a parent of T type in the visual tree
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="i_dp"></param>
        /// <returns></returns>
        public static T FindParent<T>(this DependencyObject i_dp) where T : DependencyObject
        {
            DependencyObject dobj = (DependencyObject)VisualTreeHelper.GetParent(i_dp);
            if (dobj != null)
            {
                if (dobj is T)
                {
                    return (T)dobj;
                }
                else
                {
                    dobj = FindParent<T>(dobj);
                    if (dobj != null && dobj is T)
                    {
                        return (T)dobj;
                    }
                }
            }
            return null;
        }
    }
}
