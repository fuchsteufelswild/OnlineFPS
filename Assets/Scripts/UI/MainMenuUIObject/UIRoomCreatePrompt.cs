using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace OnlineFPS
{
    public class UIRoomCreatePrompt : MonoBehaviour
    {
        [SerializeField] Canvas canvas;

        [SerializeField] TMP_InputField roomNameField;

        public void OnCreateButtonClicked()
        {
            // Call someone to create room

            NetworkManager.Instance.CreateRoomWithName(roomNameField.text);
            canvas.enabled = false;
        }

        public void Open()
        {
            roomNameField.text = "";
            canvas.enabled = true;
        }
    }
}