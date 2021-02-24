using System;
using System.Diagnostics;
using System.Text;

namespace CsprojToAsmdef
{
    public static class Dotnet
    {
        public static void Build(string args) => Run($"build {args}");
        public static void Restore(string args) => Run($"restore {args}");

        public static void Run(string args)
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            void WriteProcessOutput(object sender, DataReceivedEventArgs e)
            {
                if (!string.IsNullOrWhiteSpace(e.Data)) outputBuilder.AppendLine(e.Data);
            }

            void WriteProcessError(object sender, DataReceivedEventArgs e)
            {
                if (!string.IsNullOrWhiteSpace(e.Data)) errorBuilder.AppendLine(e.Data);
            }

            process.OutputDataReceived += WriteProcessOutput;
            process.ErrorDataReceived += WriteProcessError;

            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }

            process.OutputDataReceived -= WriteProcessOutput;
            process.ErrorDataReceived -= WriteProcessError;

            var outputs = outputBuilder.ToString();
            var errors = errorBuilder.ToString();

            if (!string.IsNullOrWhiteSpace(outputs)) UnityEngine.Debug.Log(outputs);
            if (!string.IsNullOrWhiteSpace(errors)) UnityEngine.Debug.LogError(errors);
        }
    }
}