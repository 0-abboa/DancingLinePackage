# WebGL packaging

This project uses Unity **5.6.0f3**. Build it with that editor and its WebGL support module so Unity can import the project using its original serialization and scripting APIs.

## Required game data

The startup scene stores a Windows-only Malody chart path ending in `1543646563.mc`. The chart is not present in the repository. A playable build needs that chart at:

```text
Assets/StreamingAssets/1543646563.mc
```

`MusicPlayer` now resolves the old absolute path to `StreamingAssets` and uses `UnityWebRequest` in WebGL. The build entry point checks for the chart and stops with a specific error if it is missing, instead of producing an artifact that fails when launched.

## Build

With Unity 5.6.0f3 and WebGL support installed, run from the repository root:

```sh
Unity -quit -batchmode -nographics -projectPath . -executeMethod WebGLBuild.Build -logFile -
```

The output goes to `Build/WebGL`. Set `WEBGL_BUILD_PATH` to use another output directory.

The generated folder is a static web app and must be served over HTTP(S); opening its `index.html` directly with `file://` is not supported by Unity WebGL.

## Automation limitation

The maintained GameCI version pipeline starts at Unity 2017, while Unity Build Automation currently lists versions starting at 2019.4. Neither service provides a supported Unity 5.6.0f3 build image. A hosted workflow would need a custom legacy-editor installation and a Unity license file; it cannot use the standard Unity build actions as-is.

- [GameCI versioning backend](https://github.com/game-ci/versioning-backend) filters stable editor builds to 2017 and newer.
- [Unity Build Automation supported versions](https://docs.unity.com/en-us/build-automation/reference/supported-unity-versions)
