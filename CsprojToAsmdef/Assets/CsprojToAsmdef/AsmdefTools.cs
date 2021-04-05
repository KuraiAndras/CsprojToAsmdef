using System.Diagnostics;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;

namespace CsprojToAsmdef
{
    public static class AsmdefTools
    {
        public static async void GenerateAllAsmdef() =>
            await MethodLocker.RunLockedMethod(async () =>
            {
                var mainStopwatch = Stopwatch.StartNew();
                foreach (var filePath in BuildTools.GetAllCsprojFiles())
                {
                    var projectStopWatch = Stopwatch.StartNew();

                    Debug.Log($"Starting asmdef generation for project: {filePath}");

                    await Task.Run(() => Dotnet.CreateAsmdefForProject(filePath)).ConfigureAwait(false);

                    projectStopWatch.Stop();
                    Debug.Log($"Created asmdef for project: {filePath} Generation took: {projectStopWatch.Elapsed}");
                }

                mainStopwatch.Stop();
                Debug.Log($"Generating all project files took: {mainStopwatch.Elapsed}");
            });
    }
}
