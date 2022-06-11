using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ReverseR.Common;
using ReverseR.Common.Services;
using System.IO;
using Prism.Ioc;
using Prism.Services.Dialogs;
using System.ComponentModel;
using ReverseR.Common.DecompUtilities;

namespace DecompilerCFR.Decompile.Internal
{
    internal class IKVMCFRDecompiler:CFRDecompiler
    {
        public IKVMCFRDecompiler(IContainerProvider containerProvider) : base(containerProvider) { }
        protected override IDecompileResult RunDecompiler(string folder, Action<string> msgSetter, CancellationToken? token = null, params string[] referlib)
        {
            string output = null;
            string output0 = null;
            IDecompileResult result = Container.Resolve<IDecompileResult>();
            PreProcessFiles(folder);
            //check if path is a directory
            if (Directory.Exists(folder))
            {
                output0 = Path.Combine(folder, "decompiled");
                Directory.CreateDirectory(output0);
            }
            else if (File.Exists(folder))
            {
                output0 = Path.Combine(Path.GetDirectoryName(folder), "decompiled");
                Directory.CreateDirectory(output0);
            }
            else
            {
                throw new ArgumentException($"Error:\"{folder}\" is not a valid path!");
            }
            if (Directory.Exists(tempJarPath))
            {
                output = Path.Combine(tempJarPath, "decompiled");
                Directory.CreateDirectory(output);
            }
            else if (File.Exists(tempJarPath))
            {
                output = Path.Combine(Path.GetDirectoryName(tempJarPath), "decompiled");
                Directory.CreateDirectory(output);
            }
            else
            {
                throw new ArgumentException($"Error:\"{tempJarPath}\" is not a valid path!");
            }
            string executePath = GlobalUtils.GlobalConfig.JavaPath;
            using (Process process = new Process())
            {
                bool IsCancelled() => token.HasValue && token.Value.IsCancellationRequested == true;
                process.StartInfo.FileName = executePath;
                process.StartInfo.ErrorDialog = true;
                process.StartInfo.UseShellExecute = false;
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    var interop = new System.Windows.Interop.WindowInteropHelper(System.Windows.Application.Current.MainWindow);
                    process.StartInfo.ErrorDialogParentHandle = interop.Handle;
                });
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.Arguments = Options.GetArgumentsString(folder, output, referlib);
#if DEBUG
                process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
#else
                process.StartInfo.CreateNoWindow = true;
#endif
                process.OutputDataReceived += (s, e) =>
                {
                    if (!IsCancelled())
                        msgSetter.Invoke(e.Data);
                    else
                    {
                        process.Kill();
                    }
                };
                process.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        result.HasError = true;
                    }
                    if (!IsCancelled())
                    {
                        if (!string.IsNullOrWhiteSpace(e.Data))
                            msgSetter.Invoke(e.Data);
                    }
                    else
                    {
                        process.Kill();
                    }
                };
                process.Start();
                try
                {
                    if (!IsCancelled())
                    {
                        try
                        {
                            process.ProcessorAffinity = (IntPtr)1;
                        }
                        catch (Exception)
                        {

                        }
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();
                        process.WaitForExit();
                    }
                    else
                    {
                        process.Kill();
                    }
                }
                catch (Exception e)
                {
                    if (!process.HasExited)
                    {
                        throw e;
                    }
                }
                if (IsCancelled())
                {
                    throw new OperationCanceledException();
                }
                else
                {
                    PostProcessFiles(output, output0);
                    result.OutputDir = output0;
                    result.ResultCode = result.HasError ? DecompileResultEnum.WithJavaException : DecompileResultEnum.Success;
                    result.ReturnCode = process.ExitCode;
                    return result;
                }
            }
        }
    }
}
