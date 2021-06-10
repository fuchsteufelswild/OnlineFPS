using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace OnlineFPS
{
    public class NetworkGameplayManager : MonoBehaviour
    {
        public Transform[] spawnPoints;

        public Transform RandomSpawnPoint =>
            spawnPoints[Random.Range(0, spawnPoints.Length)];

        PhotonView photonView;

        private void Awake()
        {
            photonView = GetComponent<PhotonView>();
        }

        void Start()
        {
            if (photonView.IsMine)
                CreateFPSController();
        }

        private void CreateFPSController()
        {
            GameObject go = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "FPSController"), Vector3.one * 2, Quaternion.identity);
            go.GetComponent<FPSController>().SetManager(this);
        }
    }
}