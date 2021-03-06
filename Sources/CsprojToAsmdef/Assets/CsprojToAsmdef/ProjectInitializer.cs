﻿using CsprojToAsmdef.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;

namespace CsprojToAsmdef
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1203:Constants should appear before fields", Justification = "This style makes more sense here")]
    public static class ProjectInitializer
    {
        private const string PropsContent = "<Project>\r\n\r\n  <PropertyGroup>\r\n    <BaseIntermediateOutputPath>.obj\\</BaseIntermediateOutputPath>\r\n    <BaseOutputPath>.bin\\</BaseOutputPath>\r\n    <LangVersion>7.3</LangVersion>\r\n    <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>\r\n  </PropertyGroup>\r\n\r\n  <PropertyGroup>\r\n    <UnityProjectPath>$(MSBuildThisFileDirectory)\\..</UnityProjectPath>\r\n    <AssetsFolder>$(MSBuildThisFileDirectory)</AssetsFolder>\r\n  </PropertyGroup>\r\n\r\n  <PropertyGroup Condition=\" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' \">\r\n    <DefineConstants>DEBUG;TRACE;UNITY_2020_2_3;UNITY_2020_2;UNITY_2020;UNITY_5_3_OR_NEWER;UNITY_5_4_OR_NEWER;UNITY_5_5_OR_NEWER;UNITY_5_6_OR_NEWER;UNITY_2017_1_OR_NEWER;UNITY_2017_2_OR_NEWER;UNITY_2017_3_OR_NEWER;UNITY_2017_4_OR_NEWER;UNITY_2018_1_OR_NEWER;UNITY_2018_2_OR_NEWER;UNITY_2018_3_OR_NEWER;UNITY_2018_4_OR_NEWER;UNITY_2019_1_OR_NEWER;UNITY_2019_2_OR_NEWER;UNITY_2019_3_OR_NEWER;UNITY_2019_4_OR_NEWER;UNITY_2020_1_OR_NEWER;UNITY_2020_2_OR_NEWER;UNITY_INCLUDE_TESTS;USE_SEARCH_ENGINE_API;SCENE_TEMPLATE_MODULE;ENABLE_AR;ENABLE_AUDIO;ENABLE_CACHING;ENABLE_CLOTH;ENABLE_EVENT_QUEUE;ENABLE_MICROPHONE;ENABLE_MULTIPLE_DISPLAYS;ENABLE_PHYSICS;ENABLE_TEXTURE_STREAMING;ENABLE_VIRTUALTEXTURING;ENABLE_UNET;ENABLE_LZMA;ENABLE_UNITYEVENTS;ENABLE_VR;ENABLE_WEBCAM;ENABLE_UNITYWEBREQUEST;ENABLE_WWW;ENABLE_CLOUD_SERVICES;ENABLE_CLOUD_SERVICES_COLLAB;ENABLE_CLOUD_SERVICES_COLLAB_SOFTLOCKS;ENABLE_CLOUD_SERVICES_ADS;ENABLE_CLOUD_SERVICES_USE_WEBREQUEST;ENABLE_CLOUD_SERVICES_CRASH_REPORTING;ENABLE_CLOUD_SERVICES_PURCHASING;ENABLE_CLOUD_SERVICES_ANALYTICS;ENABLE_CLOUD_SERVICES_UNET;ENABLE_CLOUD_SERVICES_BUILD;ENABLE_CLOUD_LICENSE;ENABLE_EDITOR_HUB_LICENSE;ENABLE_WEBSOCKET_CLIENT;ENABLE_DIRECTOR_AUDIO;ENABLE_DIRECTOR_TEXTURE;ENABLE_MANAGED_JOBS;ENABLE_MANAGED_TRANSFORM_JOBS;ENABLE_MANAGED_ANIMATION_JOBS;ENABLE_MANAGED_AUDIO_JOBS;INCLUDE_DYNAMIC_GI;ENABLE_MONO_BDWGC;ENABLE_SCRIPTING_GC_WBARRIERS;PLATFORM_SUPPORTS_MONO;RENDER_SOFTWARE_CURSOR;ENABLE_VIDEO;PLATFORM_STANDALONE;PLATFORM_STANDALONE_WIN;UNITY_STANDALONE_WIN;UNITY_STANDALONE;ENABLE_RUNTIME_GI;ENABLE_MOVIES;ENABLE_NETWORK;ENABLE_CRUNCH_TEXTURE_COMPRESSION;ENABLE_OUT_OF_PROCESS_CRASH_HANDLER;ENABLE_CLUSTER_SYNC;ENABLE_CLUSTERINPUT;PLATFORM_UPDATES_TIME_OUTSIDE_OF_PLAYER_LOOP;GFXDEVICE_WAITFOREVENT_MESSAGEPUMP;ENABLE_WEBSOCKET_HOST;ENABLE_MONO;NET_STANDARD_2_0;ENABLE_PROFILER;UNITY_ASSERTIONS;UNITY_EDITOR;UNITY_EDITOR_64;UNITY_EDITOR_WIN;ENABLE_UNITY_COLLECTIONS_CHECKS;ENABLE_BURST_AOT;UNITY_TEAM_LICENSE;ENABLE_CUSTOM_RENDER_TEXTURE;ENABLE_DIRECTOR;ENABLE_LOCALIZATION;ENABLE_SPRITES;ENABLE_TERRAIN;ENABLE_TILEMAP;ENABLE_TIMELINE;ENABLE_LEGACY_INPUT_MANAGER;CSHARP_7_OR_LATER;CSHARP_7_3_OR_NEWER</DefineConstants>\r\n  </PropertyGroup>\r\n\r\n  <PropertyGroup>\r\n    <UnityVersion>2019.2.0f1</UnityVersion>\r\n    <ProjectTypeGuids>{E097FAD1-6243-4DAD-9C02-E9B9EFC3FFC1};{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}</ProjectTypeGuids>\r\n    <UnityProjectGenerator>Package</UnityProjectGenerator>\r\n    <UnityProjectGeneratorVersion>1.0.11</UnityProjectGeneratorVersion>\r\n    <UnityProjectType>Game:1</UnityProjectType>\r\n    <UnityBuildTarget>StandaloneWindows:5</UnityBuildTarget>\r\n  </PropertyGroup>\r\n\r\n  <ItemGroup>\r\n    <PackageReference Include=\"Unity3D\" Version=\"1.7.0\" />\r\n  </ItemGroup>\r\n\r\n  <ItemGroup>\r\n    <PackageReference Include=\"Microsoft.Unity.Analyzers\" Version=\"1.10.0\" PrivateAssets=\"all\" />\r\n  </ItemGroup>\r\n\r\n</Project>\r\n";
        private static readonly string PropsPath = Path.GetFullPath(Path.Combine(Application.dataPath, "Directory.Build.props"));

        private const string TargetsContent = "<Project>\r\n\r\n  <ItemGroup>\r\n    <None Remove=\"**/**/*.meta\" />\r\n  </ItemGroup>\r\n\r\n  <PropertyGroup>\r\n    <AsmdefToolAccess>asmdef-tool</AsmdefToolAccess>\r\n  </PropertyGroup>\r\n\r\n  <Target Name=\"FixUpProject\" AfterTargets=\"AfterBuild\">\r\n      <Exec Command=\"$(AsmdefToolAccess) FixUpProject $(MSBuildProjectFullPath)\"/>\r\n  </Target>\r\n\r\n</Project>\r\n";
        private static readonly string TargetsPath = Path.GetFullPath(Path.Combine(Application.dataPath, "Directory.Build.targets"));

        private const string GitIgnoreContent = "!*.csproj\r\n";
        private static readonly string GitIgnorePath = Path.GetFullPath(Path.Combine(Application.dataPath, ".gitignore"));

        private const string NuGetGitIgnoreContent = "*.dll\r\n";
        private static readonly string NuGetGitIgnorePath = Path.GetFullPath(Path.Combine(Application.dataPath, "NuGet", ".gitignore"));

        public static void InitializeProject(bool ignoreDll)
        {
            InitBuildFile(PropsPath, PropsContent);
            InitBuildFile(TargetsPath, TargetsContent);
            InitBuildFile(GitIgnorePath, GitIgnoreContent);
            if (ignoreDll) InitBuildFile(NuGetGitIgnorePath, NuGetGitIgnoreContent);

            SetCurrentUnityVersionInPropsFile();
            SetCurrentLanguageVersion();
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

        [InitializeOnLoadMethod]
        private static void SetCurrentUnityVersionInPropsFile()
        {
            if (!File.Exists(PropsPath)) return;

            var propsDocument = LoadNiceXDocument(PropsPath);

            var unityVersionElement = propsDocument
                .Descendants()
                .FirstOrDefault(d => d.Name == "UnityVersion");

            if (unityVersionElement is null) return;
            if (unityVersionElement.Value == Application.unityVersion) return;

            unityVersionElement.Value = Application.unityVersion;

            WriteNiceXDocument(propsDocument, PropsPath);
        }

        private static void SetCurrentLanguageVersion()
        {
            var propsDocument = LoadNiceXDocument(PropsPath);

            var langVersionElement = propsDocument
                .Descendants()
                .FirstOrDefault(d => d.Name == "LangVersion");

            if (langVersionElement is null) return;

            var languageVersion = GetSuggestedLanguageVersionFromUnityVersion();

            if (langVersionElement.Value == languageVersion) return;

            langVersionElement.Value = languageVersion;

            WriteNiceXDocument(propsDocument, PropsPath);
        }

        private static XDocument LoadNiceXDocument(string filePath) =>
            new XmlDocument { PreserveWhitespace = true, }
                .LoadFluent(filePath)
                .ToXDocument();

        private static void WriteNiceXDocument(XDocument propsDocument, string filePath)
        {
            using (var writer = XmlWriter.Create(filePath, new XmlWriterSettings { OmitXmlDeclaration = true, }))
            {
                propsDocument.Save(writer);
            }
        }

        private static string GetSuggestedLanguageVersionFromUnityVersion()
        {
            var currentUnityVersion = new Version(Regex.Match(Application.unityVersion, @"(\d+\.)(\d+\.)(\d+)").Value);

            if (currentUnityVersion.CompareTo(new Version("2021.2")) >= 0) return "9.0";
            if (currentUnityVersion.CompareTo(new Version("2020.2")) >= 0) return "8.0";
            return "7.3";
        }
    }
}
