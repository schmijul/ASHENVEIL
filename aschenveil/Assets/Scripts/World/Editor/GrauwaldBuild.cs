using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Ashenveil.World.Editor
{
    /// <summary>
    /// Produces a standalone Linux player of the Ashenveil demo. Run with
    /// -executeMethod Ashenveil.World.Editor.GrauwaldBuild.BuildLinux. Output dir can be
    /// overridden with the ASHENVEIL_BUILD_DIR env var.
    /// </summary>
    public static class GrauwaldBuild
    {
        public static void BuildLinux()
        {
            string outDir = System.Environment.GetEnvironmentVariable("ASHENVEIL_BUILD_DIR");
            if (string.IsNullOrEmpty(outDir))
            {
                outDir = "/tmp/ashenveil-build";
            }

            Directory.CreateDirectory(outDir);
            string exe = Path.Combine(outDir, "Ashenveil.x86_64");

            var options = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/Grauwald.unity" },
                locationPathName = exe,
                target = BuildTarget.StandaloneLinux64,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;
            Debug.Log($"[GrauwaldBuild] result={summary.result} size={summary.totalSize} errors={summary.totalErrors} time={summary.totalTime} -> {exe}");

            if (summary.result != BuildResult.Succeeded)
            {
                EditorApplication.Exit(1);
            }
        }
    }
}
