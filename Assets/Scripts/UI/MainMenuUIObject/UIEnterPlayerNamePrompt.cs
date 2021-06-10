using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace OnlineFPS
{
    public class UIEnterPlayerNamePrompt : MonoBehaviour
    {
        [SerializeField] Canvas canvas;

        [SerializeField] TMP_InputField playerNameField;

        public event System.Action<string> OnPlayerNameSet;

        private void Start()
        {
            OnPlayerNameSet += FindObjectOfType<UIMenuCanvas>().ChangePlayerNickNameText;
            OnPlayerNameSet += NetworkManager.Instance.OnPlayerNickNameChanged;
            
#if UNITY_ANDROID
            gameObject.SetActive(false);
#endif
        }

        public void OnChangeNickNameButtonClicked()
        {
#if UNITY_ANDROID
            return;
#endif

            if (!string.IsNullOrEmpty(playerNameField.text))
            {
                OnPlayerNameSet?.Invoke(playerNameField.text);
                canvas.enabled = false;
            }
        }

        public void Open()
        {
            playerNameField.text = "";
            canvas.enabled = true;
        }
    }
}