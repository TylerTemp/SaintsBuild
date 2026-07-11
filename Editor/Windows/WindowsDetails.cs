using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor.Build.Reporting;
using Debug = UnityEngine.Debug;

namespace SaintsBuild.Editor.Windows
{
    public class WindowsDetails
    {
        private readonly BuildReport _report;

        public string ProductName;
        public string ProductVersion;
        public string FileDescription;
        public string LegalCopyright;
        public string FileVersion;

        public WindowsDetails(BuildReport report)
        {
            _report = report;
        }

        // public static WindowsDetails CreateDefault(string fileDescription = null) => new WindowsDetails(
        //     productName: Application.productName,
        //     productVersion: Application.version,
        //     fileDescription: fileDescription,
        //     legalCopyright: $"Copyright (c) {DateTime.Today.Year} {Application.companyName}",
        //     fileVersion: Application.version
        // );

        public WindowsDetails SetProductName(string productName = null)
        {
            ProductName = productName ?? Application.productName;
            return this;
        }

        public WindowsDetails SetProductVersion(string productVersion = null)
        {
            ProductVersion = productVersion ?? Application.version;
            return this;
        }

        public WindowsDetails SetFileDescription(string fileDescription)
        {
            FileDescription = fileDescription;
            return this;
        }

        public WindowsDetails SetLegalCopyright(string legalCopyright = null)
        {
            LegalCopyright = legalCopyright ?? $"Copyright (c) {DateTime.Today.Year} {Application.companyName}. All rights reserved.";
            return this;
        }

        public WindowsDetails SetFileVersion(string fileVersion = null)
        {
            FileVersion = fileVersion ?? Application.version;
            return this;
        }

        public void Apply()
        {
            (bool isVsExport, string exportPath) = BuildType(_report.summary.outputPath);
            if (isVsExport)
            {
                ApplyVsExport(exportPath);
            }
            else
            {
                ApplyRcedit(exportPath);
            }
        }

        private static (bool isVsExport, string exportPath) BuildType(string outputPath)
        {
            if (outputPath.EndsWith(".exe") && File.Exists(outputPath))
            {
                return (false, outputPath);
            }

            return (true, Path.GetDirectoryName(outputPath));
        }

        private void ApplyVsExport(string outputPath)
        {
            string[] rcFiles = Directory
                .GetFiles(outputPath, "*.rc", SearchOption.AllDirectories)
                .Where(path => File.ReadAllText(path).Contains("VS_VERSION_INFO"))
                .ToArray();

            if (rcFiles.Length == 0)
            {
                Debug.LogWarning(
                    $"Visual Studio export detected, but no version resource (*.rc with VS_VERSION_INFO) found in: {outputPath}");
                return;
            }

            string fileVersionCsv = FileVersion?.Replace('.', ',');
            string productVersionCsv = FileVersion?.Replace('.', ',');

            foreach (string rcFile in rcFiles)
            {
                string content = File.ReadAllText(rcFile);
                bool changed = false;
                if (fileVersionCsv != null)
                {
                    content = ReplaceRcNumeric(content, "FILEVERSION", fileVersionCsv);
                    Debug.Log($"Replace FILEVERSION to {fileVersionCsv}");
                    changed = true;
                }

                if (productVersionCsv != null)
                {
                    content = ReplaceRcNumeric(content, "PRODUCTVERSION", productVersionCsv);
                    Debug.Log($"Replace PRODUCTVERSION to {productVersionCsv}");
                    changed = true;
                }

                if (FileVersion != null)
                {
                    content = ReplaceRcValue(content, "FileVersion", FileVersion);
                    Debug.Log($"Replace FileVersion to {FileVersion}");
                    changed = true;
                }

                if (FileDescription != null)
                {
                    content = ReplaceRcValue(content, "FileDescription", FileDescription);
                    Debug.Log($"Replace FileDescription to {FileDescription}");
                    changed = true;
                }

                if (ProductName != null)
                {
                    content = ReplaceRcValue(content, "ProductName", ProductName);
                    Debug.Log($"Replace ProductName to {ProductName}");
                    changed = true;
                }

                if (ProductVersion != null)
                {
                    content = ReplaceRcValue(content, "ProductVersion", ProductVersion);
                    Debug.Log($"Replace ProductVersion to {ProductVersion}");
                    changed = true;
                }

                if (LegalCopyright != null)
                {
                    content = ReplaceRcValue(content, "LegalCopyright", LegalCopyright);
                    Debug.Log($"Replace LegalCopyright to {LegalCopyright}");
                    changed = true;
                }

                // ReSharper disable once InvertIf
                if (changed)
                {
                    File.WriteAllText(rcFile, content);
                    Debug.Log($"Updated Windows version info in Visual Studio export: {rcFile}");
                }
            }
        }

        private static string ReplaceRcNumeric(string content, string key, string value)
        {
            string pattern = $@"(?m)^(\s*{Regex.Escape(key)}\s+).*$";
            return Regex.Replace(content, pattern, match => $"{match.Groups[1].Value}{value}");
        }

        private static string ReplaceRcValue(string content, string key, string value)
        {
            string escapedValue = value.Replace("\\", "\\\\").Replace("\"", "\\\"");
            string pattern = $@"(?m)^(\s*VALUE\s+""{Regex.Escape(key)}"",\s*)(?:""[^""]*""|[^\r\n]+)(\s*)$";
            return Regex.Replace(content, pattern,
                match => $"{match.Groups[1].Value}\"{escapedValue}\"{match.Groups[2].Value}");
        }

        private string _overrideRceditPath;

        private void ApplyRcedit(string exportPath)
        {
            string rceditPath = _overrideRceditPath;
            if (string.IsNullOrEmpty(rceditPath))
            {
                rceditPath = FindRceditPath();
            }

            if (string.IsNullOrEmpty(rceditPath))
            {
                Debug.LogError("Failed to find rcedit path, please download https://github.com/electron/rcedit and use `SetRcedit(path)` to manually set it");
                return;
            }

            StringBuilder execStringBuilder = new StringBuilder($"\"{exportPath}\" ");
            bool changed = false;
            if (FileDescription != null)
            {
                execStringBuilder.Append($"--set-version-string \"FileDescription\" \"{FileDescription}\" ");
                changed = true;
                Debug.Log($"Set FileDescription to {FileDescription}");
            }

            if (ProductName != null)
            {
                execStringBuilder.Append($"--set-version-string \"ProductName\" \"{ProductName}\" ");
                changed = true;
                Debug.Log($"Set ProductName to {ProductName}");
            }

            if (ProductVersion != null)
            {
                execStringBuilder.Append($"--set-version-string \"ProductVersion\" \"{ProductVersion}\" ");
                changed = true;
                Debug.Log($"Set ProductVersion to {ProductVersion}");
            }

            if (LegalCopyright != null)
            {
                execStringBuilder.Append($"--set-version-string \"LegalCopyright\" \"{LegalCopyright}\" ");
                changed = true;
                Debug.Log($"Set LegalCopyright to {LegalCopyright}");
            }

            if (FileVersion != null)
            {
                execStringBuilder.Append($"--set-file-version \"{FileVersion}\" ");
                execStringBuilder.Append($"--set-product-version \"{FileVersion}\" ");
                changed = true;
                Debug.Log($"Set FileVersion to {FileVersion}");
            }

            if (!changed)
            {
                return;
            }

            string execString = execStringBuilder.ToString().Trim();
            Debug.Log($"{rceditPath} {execString}");
            using Process p = Process.Start(new ProcessStartInfo
            {
                FileName = rceditPath,
                Arguments = execString,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            });

            if (p == null)
            {
                Debug.LogError($"Failed to start rcedit {rceditPath}");
                return;
            }

            p.WaitForExit();

            if (p.ExitCode != 0)
            {
                Debug.LogError(p.StandardError.ReadToEnd());
            }
            else
            {
                Debug.Log($"Successfully updated Windows version info in {exportPath}");
            }
        }

        private static readonly string[] ResourceSearchFolder = {
            "Assets/Editor Default Resources/SaintsBuild",
            // this is readonly, put it to last so user  can easily override it
            "Packages/today.comes.saintsbuild/Editor/Editor Default Resources/SaintsBuild", // Unity UPM
        };

        private static string FindRceditPath()
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (string folder in  ResourceSearchFolder)
            {
                string path = folder + "/rcedit-x64.exe";

                // Resolve it to an absolute OS-compliant path
                // Unity automatically remaps this to the correct Library/PackageCache/today.comes.saintsbuild@X.Y.Z/... directory
                string absolutePath = Path.GetFullPath(path);
                if (File.Exists(absolutePath))
                {
                    return absolutePath;
                }
            }

            return null;
        }



        // ReSharper disable once UnusedMember.Global
        public void SetRcedit(string rceditPath) => _overrideRceditPath = rceditPath;

    }
}
