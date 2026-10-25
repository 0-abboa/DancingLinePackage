# WebGL test build

This branch tests the one gameplay scene present in the archived Unity project. It is not a complete Dancing Line level library.

## Scene and music inventory

| Scene | Role | Music and chart evidence |
| --- | --- | --- |
| `Assets/Level/BeginningOld/Level.unity` | The only gameplay scene in the repository; this is the scene selected by `WebGLBuild.Build`. | `Assets/Level/BeginningOld/The Beginning (old).mp3` is in the matching level folder, so it is the strongest likely pairing. The scene's old chart path names `1579349376.mc`, but the original chart is absent. |
| `Assets/Scene/GreenBackgroundDL.unity` | Placeholder, not a music level. | Its old local path mentions `1543646563.mc`; that path is not evidence of a song match. No chart is supplied for this placeholder. |
| `Assets/Test/DLTest.unity` | Test scene. | References a local temporary chart named `1550817018.mc`; no matching music track is established. |
| `Assets/Test/AnimationTest.unity` | Animation test scene. | References a local chart named `1546327363.mc`; no matching music track is established. |

Music files in the repository:

- `Assets/Level/BeginningOld/The Beginning (old).mp3`
- `Assets/Test/Music/Toby Fox - MEGALOVANIA.mp3`
- `Assets/Test/Music/丑小鸭.ogg`
- `Assets/Test/Music/赛车.ogg`

The three files under `Assets/Test/Music` are test assets; the repository does not contain corresponding production scenes. The upstream repository and its commit history contain no additional scene files. The default Unity build settings point to `GreenBackgroundDL`; this branch's build method explicitly selects `Level.unity`.

The generated beat grids previously placed in `Assets/StreamingAssets` were removed because they were not recovered or verified charts. With a missing chart, the modified `MusicPlayer` falls back to audio-only playback and disables beat-synchronized triggers. The audio clip assignment still needs verification in a built runtime.

## Build locally

With Unity **2019.4.40f1** and the WebGL Build Support module installed, run from the repository root:

```sh
Unity -quit -batchmode -nographics -projectPath . -executeMethod WebGLBuild.Build -logFile -
```

The output goes to `Build/WebGL`. Set `WEBGL_BUILD_PATH` to use another output directory. Serve the generated files over HTTP(S); Unity WebGL does not run correctly from `file://`.

## Hosted build

The GitHub Actions workflow builds WebGL and uploads a downloadable test artifact. GameCI requires a Unity license configured in the repository's Actions secrets (`UNITY_LICENSE`, `UNITY_EMAIL`, and `UNITY_PASSWORD`).
