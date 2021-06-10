using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;
using Photon.Pun;

namespace OnlineFPS
{
    public class UIRoomPlayerSlot : MonoBehaviourPunCallbacks
    {
        [SerializeField] TextMeshProUGUI playerName;

        Player storedPlayer;

        public void Init(Player player)
        {
            storedPlayer = player;
            playerName.text = player.NickName;
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            if (otherPlayer == storedPlayer)
                gameObject.SetActive(false);
        }
    }
}