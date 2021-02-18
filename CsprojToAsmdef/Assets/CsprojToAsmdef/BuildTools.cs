using CsprojToAsmdef.Extensions;
using System.IO;
using UnityEngine;

namespace CsprojToAsmdef
{
    public static class BuildTools
    {
        public static void BuildAllCsproj()
        {
            Debug.Log("Started restoring projects");

            Directory
                .EnumerateFiles(Application.dataPath, "*.csproj", SearchOption.AllDirectories)
                .ForEach(Dotnet.Build);

            Debug.Log("Finished restoring projects");
        }
    }
}