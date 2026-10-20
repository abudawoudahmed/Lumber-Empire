using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CreatePrototypeScene
{
    [MenuItem("Lumber Empire/Create Prototype Scene")]
    public static void Create()
    {
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        new GameObject("Lumber Empire Bootstrap").AddComponent<LumberPrototypeBootstrap>();
        System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene,"Assets/Scenes/LumberPrototype.unity");
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/LumberPrototype.unity",true)};
    }
}
