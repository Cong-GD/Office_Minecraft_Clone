using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public class GameRunner : MonoBehaviour
{
    static GameRunner()
    {
        EditorSceneManager.playModeStartScene
            = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/MainMenu.unity");
    }
}