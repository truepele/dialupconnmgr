using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SEAppBuilder.Common
{
    class ProcessHelper
    {
        public static async Task<int> ExecuteCommand(string commandPath, DirectoryInfo workingDirectory,
           string arguments, Action<    string> logFunc = null, CancellationTokenSource ct = null)
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = commandPath,
                    WorkingDirectory = workingDirectory.FullName,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                }
            };

            DataReceivedEventHandler outputDataRecivedAction = (sender, args) => ProcessOnOutputDataReceived(args.Data, logFunc);
            process.OutputDataReceived += outputDataRecivedAction;
            process.Start();
            process.BeginOutputReadLine();

            await Task.Run(async () =>
            {
                try
                {
                    while (!process.HasExited)
                    {
                        process.WaitForExit(500);
                        if (ct != null)
                            ct.Token.ThrowIfCancellationRequested();

                    }
                }
                catch (OperationCanceledException)
                {
                    process.Kill();
                    throw;
                }

            });

            process.OutputDataReceived -= outputDataRecivedAction;

            int exitcode = process.ExitCode;
            process.Close();

            return exitcode;
        }

        private static async void ProcessOnOutputDataReceived(string data, Action<string> logFunction = null)
        {
            if (logFunction != null)
            {
                lock (logFunction)
                {
                    logFunction(data);
                }
            }
        }
    }
}
