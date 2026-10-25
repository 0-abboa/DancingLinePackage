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
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled && File.Exists(scene.path))
                scenes.Add(scene.path);
        }

        if (scenes.Count == 0)
        {
            const string mainScene = "Assets/Scene/GreenBackgroundDL.unity";
            if (File.Exists(mainScene))
                scenes.Add(mainScene);
        }

        if (scenes.Count == 0)
            throw new InvalidOperationException("No enabled scenes are configured in Editor Build Settings, and the main scene was not found.");

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
