using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.IO;
using System.Threading;
using Photon.Realtime;

namespace OnlineFPS
{
    public class NetworkRoomManager : MonoBehaviourPunCallbacks
    {
        public static NetworkRoomManager Instance;
        public static bool IsInit;
        
        [SerializeField] NetworkCommunicationManagerServer serverCommunicatorPrefab;
        [SerializeField] NetworkCommunicationManagerClient clientCommunicatorPrefab;

        int spawnedCount = 0;
        Mutex spawnIncrementMutex;

        NetworkCommunicationManager networkCommunicationManager;

        public NetworkCommunicationManager CommunicationManager => networkCommunicationManager;

        private void Awake()
        {
            if(Instance)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(this);
            Instance = this;
            IsInit = false;
            spawnIncrementMutex = new Mutex();

            Application.targetFrameRate = 60;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public override void OnDisable()
        {
            base.OnDisable();
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if(scene.buildIndex == 1)
            {
                PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "NetworkGameplayManager"), Vector3.zero, Quaternion.identity);
            }
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
        {
            if (changedProps.TryGetValue("isSpawned", out object result))
            {
                spawnIncrementMutex.WaitOne();
                if (++spawnedCount == PhotonNetwork.PlayerList.Length - 1)
                {
                    Invoke("SpawnCommunicationManager", 5.0f);
                }
                spawnIncrementMutex.ReleaseMutex();
            }
            else
            {
                Debug.Log("Wrong key");
            }
        }

        public NetworkCommunicationManager SpawnCommunicationManager()
        {
            Debug.Log("Spawning communication manager");

            if (PhotonNetwork.IsMasterClient)
                networkCommunicationManager = Instantiate(serverCommunicatorPrefab, Vector3.zero, Quaternion.identity);
            else
                networkCommunicationManager = Instantiate(clientCommunicatorPrefab, Vector3.zero, Quaternion.identity);

            IsInit = true;

            return networkCommunicationManager;
        }
    }
}