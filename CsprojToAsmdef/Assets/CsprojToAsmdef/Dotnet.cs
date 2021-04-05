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

        public static void Build(string args) => Execute($"build {args}");
        public static void Restore(string args) => Execute($"restore {args}");

        public static void CreateAsmdefForProject(string csprojPath) => RunTool($"{nameof(CreateAsmdefForProject)} {csprojPath}");

        public static void RunTool(string args)
        {
            var devDirPath = Path.Combine(DataPath, "..", "..", nameof(CsprojToAsmdef) + ".Cli");

            if (Directory.Exists(devDirPath))
            {
                Execute($"run --project {devDirPath} -- {args}");
            }
            else
            {
                throw new NotImplementedException();
            }
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
