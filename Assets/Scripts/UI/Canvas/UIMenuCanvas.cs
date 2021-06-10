using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace OnlineFPS
{
    public class UIMenuCanvas : UICanvas
    {
        [SerializeField] TextMeshProUGUI playerName;

        public void ChangePlayerNickNameText(string newNickName) =>
            playerName.text = newNickName;
    }
}