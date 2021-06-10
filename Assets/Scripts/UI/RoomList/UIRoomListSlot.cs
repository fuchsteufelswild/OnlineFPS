using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;

namespace OnlineFPS
{
    public class UIRoomListSlot : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI roomName;

        RoomInfo storedInfo;

        public void Init(RoomInfo roomInfo)
        {
            roomName.text = roomInfo.Name;
            storedInfo = roomInfo;
        }

        public void OnClicked()
        {
            NetworkManager.Instance.JoinRoom(storedInfo);
        }
    }

}