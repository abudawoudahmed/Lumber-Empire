using UnityEditor;
using System.IO;
public static class WebGLBuild { public static void Build(){ CreatePrototypeScene.Create(); Directory.CreateDirectory("Build/WebGL"); BuildPipeline.BuildPlayer(EditorBuildSettings.scenes,"Build/WebGL",BuildTarget.WebGL,BuildOptions.None); } }
