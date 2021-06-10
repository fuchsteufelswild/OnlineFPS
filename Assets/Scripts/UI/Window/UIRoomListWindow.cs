using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OnlineFPS
{
    public class UIRoomListWindow : UIWindow
    {
        [SerializeField] Transform roomListLayout;
        [SerializeField] List<UIRoomListSlot> roomSlots;
        [SerializeField] UIRoomListSlot roomSlotPrefab;
        [SerializeField] int maxRoomCount;

        public override void Init()
        {
            base.Init();

            roomSlots = new List<UIRoomListSlot>(maxRoomCount);
            for (int i = 0; i < maxRoomCount; ++i)
            {
                roomSlots.Add(Instantiate(roomSlotPrefab, roomListLayout));
                roomSlots[i].gameObject.SetActive(false);
            }

            NetworkManager.Instance.RoomListUpdated += FillRoomList;
        }

        protected override void PrepareToOpen()
        {
            base.PrepareToOpen();

            
        }

        public void FillRoomList(List<RoomInfo> roomList)
        {
            for (int i = 0; i < roomSlots.Count; ++i)
                roomSlots[i].gameObject.SetActive(false);

            for(int i = 0; i < roomList.Count; ++i)
            {
                if (!roomList[i].RemovedFromList)
                {
                    roomSlots[i].Init(roomList[i]);
                    roomSlots[i].gameObject.SetActive(true);
                }
            }
        }

        
    }

}