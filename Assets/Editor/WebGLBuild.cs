using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class WebGLBuild
{
    public static void Build()
    {
        List<string> scenes = new List<string>();
        // Build the real level for this WebGL test. The configured editor scene is a blank
        // green-background test scene and does not exercise gameplay.
        const string gameplayScene = "Assets/Level/BeginningOld/Level.unity";
        if (!File.Exists(gameplayScene))
            throw new InvalidOperationException("The Beginning Old gameplay scene was not found: " + gameplayScene);

        scenes.Add(gameplayScene);

        string outputPath = Environment.GetEnvironmentVariable("WEBGL_BUILD_PATH");
        if (string.IsNullOrEmpty(outputPath))
            outputPath = Path.Combine(Directory.GetCurrentDirectory(), "Build", "WebGL");

        Directory.CreateDirectory(outputPath);
        BuildReport report = BuildPipeline.BuildPlayer(
            scenes.ToArray(),
            outputPath,
            BuildTarget.WebGL,
            BuildOptions.None);

        if (report == null || report.summary.result != BuildResult.Succeeded)
        {
            string result = report == null ? "no build report was returned" :
                report.summary.result + " (" + report.summary.totalErrors + " errors)";
            throw new InvalidOperationException("WebGL build failed: " + result);
        }
    }
}
