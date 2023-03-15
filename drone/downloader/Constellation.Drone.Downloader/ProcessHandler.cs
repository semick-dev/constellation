using Constellation.Drone.Downloader.DownloadClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Constellation.Drone.Downloader
{
    public class ProcessResult
    {
        public string StdOut { get; set; }
        public string StdErr { get; set; }
        public int ExitCode { get; set; }

        public ProcessResult()
        {
            StdOut = string.Empty;
            StdErr = string.Empty;
            ExitCode = -1;
        }

        public ProcessResult(string? stdOut, string? stdErr, int exitCode)
        {
            StdOut = stdOut ?? string.Empty;
            StdErr = stdErr ?? string.Empty;
            ExitCode = exitCode;
        }
    }

    public class ProcessHandler
    {
        public virtual ProcessStartInfo CreateProcessInfo(string exe, string workingDirectory)
        {
            var startInfo = new ProcessStartInfo(exe)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = workingDirectory,
            };

            startInfo.EnvironmentVariables["PATH"] = Environment.GetEnvironmentVariable("PATH");

            return startInfo;
        }

        public ProcessResult Run(string exe, string arguments, string workingDirectory)
        {
            ProcessStartInfo processStartInfo = CreateProcessInfo(exe, workingDirectory);
            processStartInfo.Arguments = arguments;
            ProcessResult result = new ProcessResult();

            try
            {
                Console.WriteLine($"{exe} {arguments}");

                var output = new List<string>();
                var error = new List<string>();

                using (var process = new Process())
                {
                    process.StartInfo = processStartInfo;

                    process.OutputDataReceived += (s, e) =>
                    {
                        lock (output)
                        {
                            if (e.Data != null)
                            {
                                output.Add(e.Data);
                            }
                        }
                    };

                    process.ErrorDataReceived += (s, e) =>
                    {
                        lock (error)
                        {
                            if (e.Data != null)
                            {
                                error.Add(e.Data);
                            }
                        }
                    };

                    process.Start();
                    process.BeginErrorReadLine();
                    process.BeginOutputReadLine();
                    process.WaitForExit();

                    int returnCode = process.ExitCode;
                    var stdOut = string.Join(Environment.NewLine, output);
                    var stdError = string.Join(Environment.NewLine, error);

                    Console.WriteLine($"StdOut: {stdOut}");
                    Console.WriteLine($"StdErr: {stdError}");
                    Console.WriteLine($"ExitCode: {process.ExitCode}");

                    result = new ProcessResult(stdOut, stdError, returnCode);

                    if (process.ExitCode != 0)
                    {
                        throw new RetriableMessageException($"Ran into a non-zero exitcode attempting to download: {process.ExitCode}. Enqueuing message for later work");
                    }
                }
            }
            catch (Exception e)
            {
                throw new RetriableMessageException("Ran into an unexpected exception while starting the youtube-dl process. " + e.Message + " Enqueueing message for later work.");
            }

            return result;
        }
    }
}
