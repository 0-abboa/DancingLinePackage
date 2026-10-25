# WebGL test build

This is a test port of the existing Unity project. The project started on Unity **5.6.0f3** and this branch targets Unity **2019.4.40f1** so the hosted build pipeline can use a supported editor. Opening it upgrades the legacy project data.

## Test mode without the Malody chart

The chart `1543646563.mc` is absent from the repository. This test build can still start the main scene and play its assigned audio clip without that chart. The player can move and turn manually, but every beat-synchronized trigger is disabled, so level timing and effects may be incomplete.

If the chart is added later at `Assets/StreamingAssets/1543646563.mc`, `MusicPlayer` loads it and restores beat-trigger behavior. The chart is optional for this test build.

This project currently has one main gameplay scene; it is a test of whether this port remains usable without the chart, not a large level library.

## Build locally

With Unity **2019.4.40f1** and the WebGL Build Support module installed, run from the repository root:

```sh
Unity -quit -batchmode -nographics -projectPath . -executeMethod WebGLBuild.Build -logFile -
```

The output goes to `Build/WebGL`. Set `WEBGL_BUILD_PATH` to use another output directory. Serve the generated files over HTTP(S); Unity WebGL does not run correctly from `file://`.

## Hosted build

The GitHub Actions workflow builds WebGL and uploads a downloadable test artifact. GameCI requires a Unity license configured in the repository's Actions secrets (`UNITY_LICENSE`, `UNITY_EMAIL`, and `UNITY_PASSWORD`).