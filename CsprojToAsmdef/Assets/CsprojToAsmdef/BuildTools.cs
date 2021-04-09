using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CsprojToAsmdef
{
    public static class BuildTools
    {
        public static async void BuildAllCsproj() =>
            await MethodLocker.RunLockedMethod(async () =>
            {
                var mainStopwatch = Stopwatch.StartNew();

                Debug.Log("Started fixing up all projects");

                var dataPath = Application.dataPath;
                var nuGetFolder = Path.Combine(Application.dataPath, "NuGet");

                await Task.Run(() =>
                {
                    Directory.EnumerateFiles(nuGetFolder, "*", SearchOption.AllDirectories)
                        .Where(f =>
                        {
                            var fileName = Path.GetFileName(f);
                            return fileName != ".gitignore" && Path.GetExtension(fileName) != "*.meta";
                        })
                        .ToList()
                        .ForEach(File.Delete);
                }).ConfigureAwait(false);

                foreach (var filePath in Directory
                    .EnumerateFiles(dataPath, "*.csproj", SearchOption.AllDirectories)
                    .Select(Path.GetFullPath))
                {
                    var projectStopWatch = Stopwatch.StartNew();

                    Debug.Log($"Starting fix up for project: {filePath}");

                    await Task.Run(() => Dotnet.Build(filePath)).ConfigureAwait(false);

                    projectStopWatch.Stop();
                    Debug.Log($"Finished fix up project: {filePath} Generation took: {projectStopWatch.Elapsed}");
                }

                mainStopwatch.Stop();
                Debug.Log($"Generating all project files took: {mainStopwatch.Elapsed}");
            });
    }
}
