using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReverseR.Common.IO
{
    public static class FileUtilities
    {
        public static void SafeDeleteDirectory(string path)
        {
            string realPath = Path.GetFullPath(Environment.ExpandEnvironmentVariables(path));
            if(!Directory.Exists(realPath))
            {
                throw new InvalidOperationException("Target path does not exist!\nPath:" + realPath);
            }
            bool isSafePath = false;
            DirectoryInfo allowedDirectory = new DirectoryInfo(GlobalUtils.GlobalConfig.CachePath);
            DirectoryInfo targetDirectory = new DirectoryInfo(realPath);
            while(targetDirectory.Parent!=null)
            {
                if (targetDirectory.Parent.FullName == allowedDirectory.FullName)
                {
                    isSafePath = true;
                    break;
                }
                else targetDirectory = targetDirectory.Parent;
            }
            if(isSafePath)
            {
                Directory.Delete(realPath, true);
            }
            else
            {
                throw new InvalidOperationException("Trying to delete unauthorized directory\nPath:" + realPath);
            }
        }
    }
}
