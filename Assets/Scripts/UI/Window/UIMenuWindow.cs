using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace OnlineFPS
{
    public class UIMenuWindow : UIWindow
    {
        [SerializeField] UIRoomCreatePrompt roomCreatePrompt;
        [SerializeField] UIEnterPlayerNamePrompt enterPlayerNamePrompt;

        public void OpenRoomCreatorPrompt() =>
            roomCreatePrompt.Open();

        public void OpenPlayerEnterNamePrompt() =>
            enterPlayerNamePrompt.Open();

        public void QuitGame() =>
            Application.Quit();
    }
}
