using UnityEngine;

namespace Minecraft
{
    public class QuitGameHandler : MonoBehaviour
    {
        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void SaveAndQuit()
        {
            // Save the game
            QuitGame();
        }
    }

}
