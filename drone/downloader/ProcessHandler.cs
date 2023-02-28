using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Constellation.Drone.Downloader
{
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

        public void Run(string exe, string arguments, string workingDirectory)
        {
            ProcessStartInfo processStartInfo = CreateProcessInfo(exe, workingDirectory);
            processStartInfo.Arguments = arguments;

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

                    if (process.ExitCode != 0)
                    {
                        Console.WriteLine("Unable to download! ahghgh1");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to download! ahghgh1");
            }
        }
    }
}
