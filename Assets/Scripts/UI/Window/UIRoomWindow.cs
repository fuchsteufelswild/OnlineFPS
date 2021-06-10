using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;
using Photon.Pun;

namespace OnlineFPS
{
    public class UIRoomWindow : UIWindow
    {
        [SerializeField] TextMeshProUGUI roomName;
        
        [SerializeField] Transform playerListLayout;
        [SerializeField] List<UIRoomPlayerSlot> playerSlots;
        [SerializeField] UIRoomPlayerSlot playerSlotPrefab;
        [SerializeField] int maxPlayerCount;

        [SerializeField] GameObject startButton;

        public override void Init()
        {
            base.Init();

            playerSlots = new List<UIRoomPlayerSlot>(maxPlayerCount);
            for (int i = 0; i < maxPlayerCount; ++i)
            {
                playerSlots.Add(Instantiate(playerSlotPrefab, playerListLayout));
                playerSlots[i].gameObject.SetActive(false);
            }

            NetworkManager.Instance.NewPlayerEnteredRoom += OnNewPlayerEnteredRoom;
            NetworkManager.Instance.HostLeftRoom += () => startButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        }

        public void LeaveRoom() =>
            NetworkManager.Instance.LeaveRoom();

        public void StartGame() =>
            NetworkManager.Instance.StartGame();

        protected override void PrepareToOpen()
        {
            base.PrepareToOpen();

            roomName.text = NetworkManager.Instance.CurrentRoomName;

            Player[] allPlayers = PhotonNetwork.PlayerList;
            for (int i = 0; i < allPlayers.Length; i++)
                OnNewPlayerEnteredRoom(allPlayers[i]);

            startButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        }

        public void OnNewPlayerEnteredRoom(Player player)
        {
            for (int i = 0; i < playerSlots.Count; ++i)
            {
                if(!playerSlots[i].gameObject.activeSelf)
                {
                    playerSlots[i].Init(player);
                    playerSlots[i].gameObject.SetActive(true);
                    break;
                }
            }
        }

        protected override void PrepareToClose()
        {
            base.PrepareToClose();

            for (int i = 0; i < maxPlayerCount; ++i)
            {
                playerSlots[i].gameObject.SetActive(false);
            }
        }
    } 
}
