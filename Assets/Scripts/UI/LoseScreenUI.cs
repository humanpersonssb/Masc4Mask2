using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MasqueradeGame.UI
{
    public class LoseScreenUI : MonoBehaviour
    {
        public Button ReloadButton;

        private void OnEnable()
        {
            ReloadButton.onClick.AddListener(HandleClickReload);
        }

        private void OnDisable()
        {
            ReloadButton.onClick.RemoveAllListeners();
        }
        
        private void HandleClickReload()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}