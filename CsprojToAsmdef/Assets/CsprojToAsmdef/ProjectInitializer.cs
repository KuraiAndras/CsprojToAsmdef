using System;
using System.IO;
using UnityEngine;

namespace CsprojToAsmdef
{
    public static class ProjectInitializer
    {
        private static readonly string PropsContent =
            "<Project>\r\n\r\n  <PropertyGroup>\r\n\r\n    <BaseIntermediateOutputPath>.obj\\</BaseIntermediateOutputPath>\r\n    <LangVersion>8.0</LangVersion>\r\n    <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>\r\n\r\n  </PropertyGroup>\r\n\r\n  <PropertyGroup Condition=\" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' \">\r\n    <DefineConstants>DEBUG;TRACE;UNITY_2020_2_3;UNITY_2020_2;UNITY_2020;UNITY_5_3_OR_NEWER;UNITY_5_4_OR_NEWER;UNITY_5_5_OR_NEWER;UNITY_5_6_OR_NEWER;UNITY_2017_1_OR_NEWER;UNITY_2017_2_OR_NEWER;UNITY_2017_3_OR_NEWER;UNITY_2017_4_OR_NEWER;UNITY_2018_1_OR_NEWER;UNITY_2018_2_OR_NEWER;UNITY_2018_3_OR_NEWER;UNITY_2018_4_OR_NEWER;UNITY_2019_1_OR_NEWER;UNITY_2019_2_OR_NEWER;UNITY_2019_3_OR_NEWER;UNITY_2019_4_OR_NEWER;UNITY_2020_1_OR_NEWER;UNITY_2020_2_OR_NEWER;UNITY_INCLUDE_TESTS;USE_SEARCH_ENGINE_API;SCENE_TEMPLATE_MODULE;ENABLE_AR;ENABLE_AUDIO;ENABLE_CACHING;ENABLE_CLOTH;ENABLE_EVENT_QUEUE;ENABLE_MICROPHONE;ENABLE_MULTIPLE_DISPLAYS;ENABLE_PHYSICS;ENABLE_TEXTURE_STREAMING;ENABLE_VIRTUALTEXTURING;ENABLE_UNET;ENABLE_LZMA;ENABLE_UNITYEVENTS;ENABLE_VR;ENABLE_WEBCAM;ENABLE_UNITYWEBREQUEST;ENABLE_WWW;ENABLE_CLOUD_SERVICES;ENABLE_CLOUD_SERVICES_COLLAB;ENABLE_CLOUD_SERVICES_COLLAB_SOFTLOCKS;ENABLE_CLOUD_SERVICES_ADS;ENABLE_CLOUD_SERVICES_USE_WEBREQUEST;ENABLE_CLOUD_SERVICES_CRASH_REPORTING;ENABLE_CLOUD_SERVICES_PURCHASING;ENABLE_CLOUD_SERVICES_ANALYTICS;ENABLE_CLOUD_SERVICES_UNET;ENABLE_CLOUD_SERVICES_BUILD;ENABLE_CLOUD_LICENSE;ENABLE_EDITOR_HUB_LICENSE;ENABLE_WEBSOCKET_CLIENT;ENABLE_DIRECTOR_AUDIO;ENABLE_DIRECTOR_TEXTURE;ENABLE_MANAGED_JOBS;ENABLE_MANAGED_TRANSFORM_JOBS;ENABLE_MANAGED_ANIMATION_JOBS;ENABLE_MANAGED_AUDIO_JOBS;INCLUDE_DYNAMIC_GI;ENABLE_MONO_BDWGC;ENABLE_SCRIPTING_GC_WBARRIERS;PLATFORM_SUPPORTS_MONO;RENDER_SOFTWARE_CURSOR;ENABLE_VIDEO;PLATFORM_STANDALONE;PLATFORM_STANDALONE_WIN;UNITY_STANDALONE_WIN;UNITY_STANDALONE;ENABLE_RUNTIME_GI;ENABLE_MOVIES;ENABLE_NETWORK;ENABLE_CRUNCH_TEXTURE_COMPRESSION;ENABLE_OUT_OF_PROCESS_CRASH_HANDLER;ENABLE_CLUSTER_SYNC;ENABLE_CLUSTERINPUT;PLATFORM_UPDATES_TIME_OUTSIDE_OF_PLAYER_LOOP;GFXDEVICE_WAITFOREVENT_MESSAGEPUMP;ENABLE_WEBSOCKET_HOST;ENABLE_MONO;NET_STANDARD_2_0;ENABLE_PROFILER;UNITY_ASSERTIONS;UNITY_EDITOR;UNITY_EDITOR_64;UNITY_EDITOR_WIN;ENABLE_UNITY_COLLECTIONS_CHECKS;ENABLE_BURST_AOT;UNITY_TEAM_LICENSE;ENABLE_CUSTOM_RENDER_TEXTURE;ENABLE_DIRECTOR;ENABLE_LOCALIZATION;ENABLE_SPRITES;ENABLE_TERRAIN;ENABLE_TILEMAP;ENABLE_TIMELINE;ENABLE_LEGACY_INPUT_MANAGER;CSHARP_7_OR_LATER;CSHARP_7_3_OR_NEWER</DefineConstants>\r\n  </PropertyGroup>\r\n\r\n  <PropertyGroup>\r\n    <UnityVersion>2020.2.3f1</UnityVersion>\r\n    <ProjectTypeGuids>{E097FAD1-6243-4DAD-9C02-E9B9EFC3FFC1};{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}</ProjectTypeGuids>\r\n    <UnityProjectGenerator>Package</UnityProjectGenerator>\r\n    <UnityProjectGeneratorVersion>2.0.7</UnityProjectGeneratorVersion>\r\n    <UnityProjectType>Game:1</UnityProjectType>\r\n    <UnityBuildTarget>StandaloneWindows:5</UnityBuildTarget>\r\n  </PropertyGroup>\r\n\r\n  <ItemGroup>\r\n    <PackageReference Include=\"Microsoft.Unity.Analyzers\" Version=\"1.10.0\" PrivateAssets=\"all\"/>\r\n  </ItemGroup>\r\n\r\n  <ItemGroup>\r\n    <PackageReference Include=\"Unity3D\" Version=\"1.7.0\" />\r\n  </ItemGroup>\r\n\r\n</Project>";

        private static readonly string PropsPath = Path.Combine(Application.dataPath, "Directory.Build.props");

        private static readonly string TargetsContent = "<Project>\r\n\r\n  <ItemGroup>\r\n    <None Remove=\"**/**/*.meta\" />\r\n  </ItemGroup>\r\n\r\n  <Target Name=\"Cleanup\" AfterTargets=\"AfterBuild\">\r\n\r\n    <Delete Files=\"$(OutDir)/$(AssemblyName).dll\" />\r\n    <Delete Files=\"$(OutDir)/$(AssemblyName).pdb\" />\r\n    <Delete Files=\"$(OutDir)/$(AssemblyName).dll.RoslynCA.json\" />\r\n    <Delete Files=\"$(OutDir)/$(AssemblyName).deps.json\" />\r\n\r\n    <RemoveDir Directories=\"$(MSBuildProjectDirectory)/obj\"/>\r\n    <Delete Files=\"$(MSBuildProjectDirectory)/obj.meta\" />\r\n\r\n  </Target>\r\n\r\n</Project>";

        private static readonly string TargetsPath = Path.Combine(Application.dataPath, "Directory.Build.targets");

        private static readonly string GitIgnoreContent =
            "!*.csproj"
            + Environment.NewLine
            + Environment.NewLine;

        private static readonly string GitIgnorePath = Path.Combine(Application.dataPath, ".gitignore");

        public static void InitializeProject()
        {
            InitBuildFile(PropsPath, PropsContent);
            InitBuildFile(TargetsPath, TargetsContent);
            InitBuildFile(GitIgnorePath, GitIgnoreContent);
        }

        private static void InitBuildFile(string filePath, string fileContent)
        {
            if (File.Exists(filePath))
            {
                Debug.Log($"The build file {filePath} already exists");
                return;
            }

            File.WriteAllText(filePath, fileContent);
            Debug.Log($"Created file {filePath}");
        }
    }
}
