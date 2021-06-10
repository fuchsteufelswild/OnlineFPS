using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


namespace OnlineFPS
{
    using static NetworkCommunicationManager;

    public class NetworkManager : MonoBehaviourPunCallbacks
    {
        public static NetworkManager Instance;
        public static bool IsInit = false;
        public static int CachedNetworkID;

        public string CurrentRoomName =>
            PhotonNetwork.CurrentRoom != null ? PhotonNetwork.CurrentRoom.Name : "";

        public event System.Action<List<RoomInfo>> RoomListUpdated;
        public event System.Action<Player> NewPlayerEnteredRoom;
        public event System.Action HostLeftRoom;

        private void Awake()
        {
            if (Instance != null)
                Destroy(gameObject);

            Instance = this;
        }

        private void Start()
        {
            PhotonNetwork.ConnectUsingSettings();

            Application.targetFrameRate = 60;
        }

        public override void OnConnectedToMaster()
        {
            PhotonNetwork.JoinLobby();
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        public override void OnJoinedLobby()
        {
            UIManager.Instance.CloseLoadingWindow();
        }

        public void CreateRoomWithName(string roomName)
        {
            if (!string.IsNullOrEmpty(roomName))
            {
                UIManager.Instance.OpenLoadingWindow();
                PhotonNetwork.CreateRoom(roomName);
            }
        }

        public override void OnJoinedRoom()
        {
            if (CheckPlayerWithSameNameAlreadyInRoom())
            {
                UIManager.Instance.OpenErrorWindow("Player with the same nickname already inside room!", LeaveRoom);
                UIManager.Instance.CloseLoadingWindow();
            }
            else
            {
                UIManager.Instance.PerformWindowTransition(WindowSignature.ROOM);
                UIManager.Instance.CloseLoadingWindow();
            }
        }

        private bool CheckPlayerWithSameNameAlreadyInRoom()
        {
            Player[] players = PhotonNetwork.PlayerList;

            string currentNickname = PhotonNetwork.NickName;

            for (int i = 0; i < players.Length; ++i)
                if (players[i] != PhotonNetwork.LocalPlayer && players[i].NickName == currentNickname)
                    return true;

            return false;
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            base.OnCreateRoomFailed(returnCode, message);

            UIManager.Instance.PerformWindowTransition(WindowSignature.MENU);
            UIManager.Instance.CloseLoadingWindow();

            Debug.Log("Failed to join room for: " + message);
        }

        public void LeaveRoom()
        {
            UIManager.Instance.OpenLoadingWindow();
            PhotonNetwork.LeaveRoom();
        }

        public override void OnLeftRoom()
        {
            UIManager.Instance.PerformWindowTransition(WindowSignature.MENU);
            UIManager.Instance.CloseLoadingWindow();
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            RoomListUpdated?.Invoke(roomList);
        }

        public void JoinRoom(RoomInfo info)
        {
            UIManager.Instance.OpenLoadingWindow();
            PhotonNetwork.JoinRoom(info.Name);
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            NewPlayerEnteredRoom?.Invoke(newPlayer);
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            HostLeftRoom?.Invoke();
        }

        public void StartGame()
        {
            PhotonNetwork.LoadLevel(1);
        }

        public void OnPlayerNickNameChanged(string newNickName)
        {
            PhotonNetwork.NickName = newNickName;
            CachedNetworkID = newNickName.GetHashCode();
        }
    }
}