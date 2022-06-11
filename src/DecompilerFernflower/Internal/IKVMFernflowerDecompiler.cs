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

namespace DecompilerFernflower.Decompile.Internal
{
    internal class IKVMFernflowerDecompiler:FernflowerDecompiler
    {
        public IKVMFernflowerDecompiler(IContainerProvider containerProvider) : base(containerProvider) { }
        protected override IDecompileResult RunDecompiler(string path, Action<string> msgSetter, CancellationToken? token = null, params string[] referlib)
        {
            string output = null;
            IDecompileResult result = Container.Resolve<IDecompileResult>();
            //check if path is a directory
            if (Directory.Exists(path))
            {
                output = Path.Combine(path, "decompiled");
                path += "\\\\";//add backslash for fernflower
                Directory.CreateDirectory(output);
            }
            else if (File.Exists(path))
            {
                output = Path.Combine(Path.GetDirectoryName(path), "decompiled");
                Directory.CreateDirectory(output);
            }
            else
            {
                throw new ArgumentException($"Error:\"{path}\" is not a valid path!");
            }
            string executePath = Options.GetDecompilerPath();
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
                process.StartInfo.Arguments = Options.GetArgumentsString(path, output, referlib);
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
                        catch (Exception) { }
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
                    result.OutputDir = output;
                    result.ResultCode = result.HasError ? DecompileResultEnum.WithJavaException : DecompileResultEnum.Success;
                    result.ReturnCode = process.ExitCode;
                    return result;
                }
            }
        }
    }
}
