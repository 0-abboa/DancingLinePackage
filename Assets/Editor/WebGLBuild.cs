using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class WebGLBuild
{
    private const string RequiredBeatmap = "Assets/StreamingAssets/1543646563.mc";

    public static void Build()
    {
        if (!File.Exists(RequiredBeatmap))
        {
            throw new InvalidOperationException(
                "The startup scene references Malody chart 1543646563.mc, but it is not in this repository. " +
                "Add the chart at " + RequiredBeatmap + " before building a playable WebGL package.");
        }

        List<string> scenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled && File.Exists(scene.path))
                scenes.Add(scene.path);
        }

        if (scenes.Count == 0)
            throw new InvalidOperationException("No enabled scenes are configured in Editor Build Settings.");

        string outputPath = Environment.GetEnvironmentVariable("WEBGL_BUILD_PATH");
        if (string.IsNullOrEmpty(outputPath))
            outputPath = Path.Combine(Directory.GetCurrentDirectory(), "Build", "WebGL");

        Directory.CreateDirectory(outputPath);
        string error = BuildPipeline.BuildPlayer(
            scenes.ToArray(),
            outputPath,
            BuildTarget.WebGL,
            BuildOptions.None);

        if (!string.IsNullOrEmpty(error))
            throw new InvalidOperationException("WebGL build failed: " + error);
    }
}
