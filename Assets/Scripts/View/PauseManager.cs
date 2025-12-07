using UnityEngine;
using UnityEngine.SceneManagement;

namespace View
{
    public class PauseManager : MonoBehaviour
    {
        [Header("Pause Menu")]
        [SerializeField] private GameObject pauseMenuUI;
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        
        private bool isPaused = false;

        private void Start()
        {
            if (pauseMenuUI)
            {
                pauseMenuUI.SetActive(false);
            }
        }
        
        public bool OnPause()
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }  
            return isPaused;
        }

        public void Pause()
        {
            if (pauseMenuUI)
            {
                pauseMenuUI.SetActive(true);
            }
            
            Time.timeScale = 0f;
            isPaused = true;
            
            // Unlock and show cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Resume()
        {
            if (pauseMenuUI)
            {
                pauseMenuUI.SetActive(false);
            }
            
            Time.timeScale = 1f;
            isPaused = false;
            
            // Lock and hide cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void QuitToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
        }

        public void QuitGame()
        {
            Time.timeScale = 1f;
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
}