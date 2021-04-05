using UnityEngine;

namespace CsprojToAsmdef
{
    public static class AsmdefTools
    {
        public static void GenerateAllAsmdef()
        {
            foreach (var filePath in BuildTools.GetAllCsprojFiles())
            {
                Dotnet.CreateAsmdefForProject(filePath);

                Debug.Log($"Created asmdef for project: {filePath}");
            }
        }
    }
}
