using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace CsprojToAsmdef
{
    [InitializeOnLoad]
    public static class Dotnet
    {
        private static readonly string DataPath;

        static Dotnet() => DataPath = Application.dataPath;

        public static void Build(string projectPath)
        {
            var devDllPath = Path.GetFullPath(Path.Combine(DataPath, "..", "..", nameof(CsprojToAsmdef) + ".Cli", "bin", "Debug", "net5.0", "CsprojToAsmdef.Cli.dll"));

            var args = File.Exists(devDllPath)
                ? $"build \"{projectPath}\" /p:AsmdefToolAccess=\"dotnet {devDllPath}\""
                : $"build \"{projectPath}\"";

            Execute(args);
        }

        public static void Execute(string args)
        {
            using (var process = new Process
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
            })
            {
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
}
