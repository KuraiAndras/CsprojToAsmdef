using CsprojToAsmdef.Extensions;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CsprojToAsmdef
{
    public static class BuildTools
    {
        public static void BuildAllCsproj()
        {
            Debug.Log("Started restoring projects");

            GetAllCsprojFiles()
                .ForEach(Dotnet.Build);

            Debug.Log("Finished restoring projects");
        }

        public static IEnumerable<string> GetAllCsprojFiles() =>
            Directory
                .EnumerateFiles(Application.dataPath, "*.csproj", SearchOption.AllDirectories);
    }
}