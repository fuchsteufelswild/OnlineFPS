using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using FPSControllerLPFP;
using Photon.Pun;
using Photon;
// Spawn, When dead prevent movement, on start prevenet every action, refactor this class
namespace OnlineFPS
{
    using static NetworkCommunicationManager;

    public class FPSController : MonoBehaviour
    {
        // Activate/Deactivate to compability with Low Poly FPS sample
        [SerializeField] Camera[] cameras;
        [SerializeField] AudioListener[] audioListeners;
        [SerializeField] AudioReverbZone[] audioReverbZones;
        [SerializeField] GameObject[] canvases;
        [SerializeField] GameObject handGunHands;
        [SerializeField] GameObject automaticGunHands;

        // Low Poly FPS weapon implementations
        [SerializeField] HandgunScriptLPFP handGun;
        [SerializeField] AutomaticGunScriptLPFP automaticGun;
        [SerializeField] Camera[] weaponCameras;

        // 
        [SerializeField] CharacterController characterController;
        [SerializeField] MeshRenderer capsule;
        [SerializeField] GameObject[] availableWeapons;
        [SerializeField] int activeWeaponIndex = 0;

        Camera shootCamera;
        NetworkGameplayManager owner;
        FPSMovement fpsMovementController;
        PhotonView photonView;
        bool shouldUpdate;

        int health = 1;

        public bool IsInterpolatingPosition { get; set; } = false;
        public bool IsInterpolatingRotation { get; set; } = false;

        public PhotonView PhotonView => photonView;
        public FPSMovement MovementComponent => fpsMovementController;

        public int Health => health;
        public int ActiveWeaponIndex => activeWeaponIndex;
        
        public int FirstWeaponAmmo => handGun.currentAmmo;
        public int SecondWeaponAmmo => automaticGun.currentAmmo;
        
        public bool CanFire =>
            health > 0 && (ActiveWeaponIndex == 0 ? handGun.currentAmmo > 0 : automaticGun.currentAmmo > 0);

        public bool CanReload(int weaponIndex) =>
            health > 0 && ActiveWeaponIndex == weaponIndex;

        public bool CanChangeWeapon =>
            health > 0;

        public bool IsAlive =>
            health > 0;
        
        public Vector3 BulletSpawnPosition
        {
            get
            {
                if(ActiveWeaponIndex == 0)
                    return handGun.Spawnpoints.bulletSpawnPoint.position;

                return automaticGun.Spawnpoints.bulletSpawnPoint.position;
            }
        }

        public Quaternion BulletSpawnRotation
        {
            get
            {
                if (ActiveWeaponIndex == 0)
                    return handGun.Spawnpoints.bulletSpawnPoint.rotation;

                return automaticGun.Spawnpoints.bulletSpawnPoint.rotation;
            }
        }
        
        public void SetManager(NetworkGameplayManager manager) =>
            owner = manager;

        private void Awake()
        {
            photonView = GetComponent<PhotonView>();
        }

        private void Start()
        {
            shootCamera = weaponCameras[0];
            health = 1;
            fpsMovementController = GetComponent<FPSMovement>();

            shouldUpdate = photonView.IsMine;

            if(!shouldUpdate)
            {
                for (int i = 0; i < cameras.Length; ++i)
                {
                    cameras[i].gameObject.SetActive(false);
                }

                for (int i = 0; i < audioListeners.Length; ++i)
                {
                    audioListeners[i].gameObject.SetActive(false);
                    audioListeners[i].gameObject.SetActive(false);
                    canvases[i].gameObject.SetActive(false);
                }
            }

            if (photonView.IsMine)
            {
                ExitGames.Client.Photon.Hashtable hashTable = new ExitGames.Client.Photon.Hashtable();
                hashTable.Add("isSpawned", true);
                PhotonNetwork.LocalPlayer.SetCustomProperties(hashTable);
                // Listen to fire, reload, change weapon events

#if UNITY_ANDROID
                Cursor.visible = false;
#else
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
#endif
            }

            Debug.Log("Player count " + PhotonNetwork.PlayerList.Length);


        }

        public void SetHealth(int newHealth) =>
            health = newHealth;

        public void Reload(int weaponIndex)
        {
            if (weaponIndex == 0)
                handGun.currentAmmo = 9;
            else
                automaticGun.currentAmmo = 30;
        }

        public void SimulateFire(Vector3 bulletSpawnPoint, Quaternion bulletSpawnRotation)
        {
            if (ActiveWeaponIndex == 0)
                handGun.SimulateFire(bulletSpawnPoint, bulletSpawnRotation);
            else
                automaticGun.SimulateFire(bulletSpawnPoint, bulletSpawnRotation);
        }

        public void SimulateReload()
        {
            if (ActiveWeaponIndex == 0)
                handGun.SimulateReload();
            else
                automaticGun.SimulateReload();
        }

        public int TakeDamage(int damage)
        {
            if (health <= 0)
                return health;
            health--;

            if (health <= 0)
            {
                Die();
                if (PhotonNetwork.MasterClient == PhotonNetwork.LocalPlayer)
                    Invoke("Respawn", 1.5f);
            }

            return health;
        }

        public void Die()
        {
            capsule.enabled = false;
            handGunHands.gameObject.SetActive(false);
            automaticGunHands.gameObject.SetActive(false);
        }

        public void Respawn()
        {
            Transform spawnPoint = NetworkRoomManager.Instance.CommunicationManager.ControlledPlayerController.owner.RandomSpawnPoint;
            characterController.enabled = false;
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;

            health = 1;

            PhotonView.RPC("RPC_ClientRevive", RpcTarget.All, PhotonView.Controller.ActorNumber, spawnPoint.position, spawnPoint.rotation);

            characterController.enabled = true;

            Revive();
        }

        public void MoveToRandomSpawnPoint()
        {
            Transform spawnPoint = NetworkRoomManager.Instance.CommunicationManager.ControlledPlayerController.owner.RandomSpawnPoint;
            characterController.enabled = false;
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
            characterController.enabled = true;
        }

        public void SimulateReviveAt(Vector3 pos, Quaternion rot)
        {
            transform.position = pos;
            transform.rotation = rot;
            health = 1;
            Revive();
        }

        public void Revive()
        {
            capsule.enabled = true;
            handGunHands.gameObject.SetActive(true);
            automaticGunHands.gameObject.SetActive(true);
        }

        public void AmmoUpdate()
        {
            if (activeWeaponIndex == 0)
                handGun.AmmoUpdate();
            else if (activeWeaponIndex == 1)
                automaticGun.AmmoUpdate();
        }

        public void Fire()
        {
            if (PhotonView.Controller.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
            {
                if (activeWeaponIndex == 0)
                    handGun.currentAmmo--;
                else if (activeWeaponIndex == 1)
                    automaticGun.currentAmmo--;
            }
        }

        public void SetFirstWeaponAmmo(int ammo) =>
            handGun.currentAmmo = ammo;
        public void SetSecondWeaponAmmo(int ammo) =>
            automaticGun.currentAmmo = ammo;

        public void EquipWeapon(int equipWeaponIndex)
        {
            if (activeWeaponIndex != equipWeaponIndex)
            {
                availableWeapons[activeWeaponIndex].gameObject.SetActive(false);
                availableWeapons[equipWeaponIndex].gameObject.SetActive(true);

                activeWeaponIndex = equipWeaponIndex;

                shootCamera = weaponCameras[equipWeaponIndex];
            }
        }

        private void Update()
        {
            if (shouldUpdate && NetworkRoomManager.IsInit)
            {
                if (IsAlive)
                {
                    fpsMovementController.UpdateMovement();
                    if (NetworkRoomManager.IsInit && !PhotonNetwork.IsMasterClient)
                    {
                        if (NetworkRoomManager.Instance.CommunicationManager != null)
                        {
                            ProcessClientInput();
                        }
                    }
                    else if (NetworkRoomManager.IsInit && PhotonNetwork.IsMasterClient)
                    {
                        ProcessServerInput();   
                    }
                }
            }
            else
            {
                if (!IsInterpolatingPosition)
                {
                    fpsMovementController.ApplyDeadReckoning();
                }
            }
            
        }

        // TODO: 
        // InputProcessor class is needed
        // Create client and server versions and override input processor functions
#region Input Processing

        private bool CanExecuteAction() =>
            shouldUpdate && IsAlive && NetworkRoomManager.IsInit && NetworkRoomManager.Instance.CommunicationManager != null;

        public void OnFireButtonClickedOnUI()
        {
            if(CanExecuteAction() && CanFire)
            {
                if (PhotonNetwork.IsMasterClient)
                    ProcessServerFireInput();
                else
                    ProcessClientFireInput();
            }
        }

        public void OnReloadButtonClickedOnUI()
        {
            if (CanExecuteAction() && CanReload(activeWeaponIndex))
            {
                if (PhotonNetwork.IsMasterClient)
                    ProcessServerReloadInput();
                else
                    ProcessClientReloadInput();
            }
        }

        public void OnWeaponIconClickedOnUI(int weaponIndex)
        {
            if(CanExecuteAction())
            {
                if (PhotonNetwork.IsMasterClient)
                    ProcessServerChangeWeaponInput(weaponIndex);
                else
                    ProcessClientChangeWeaponInput(weaponIndex);
            }
        }

        private void ProcessClientInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                ProcessClientFireInput();
            }
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                ProcessClientChangeWeaponInput(0);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                ProcessClientChangeWeaponInput(1);
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                ProcessClientReloadInput();
            }
        }

        public void ProcessClientFireInput()
        {
            NetworkCommunicationManager.ClientPacket clientPacket = new NetworkCommunicationManager.ClientPacket();
            clientPacket.inputType = NetworkCommunicationManager.ClientInputType.FIRE;
            Ray r = shootCamera.ViewportPointToRay(Vector3.one * 0.5f);
            clientPacket.clientInput = new NetworkCommunicationManager.FireInput(r.origin, r.direction.normalized, NetworkRoomManager.Instance.CommunicationManager.LastAckedPackageNumber);
            NetworkRoomManager.Instance.CommunicationManager.AddCommandIntoBuffer(clientPacket);
        }

        public void ProcessClientReloadInput()
        {
            NetworkCommunicationManager.ClientPacket clientPacket = new NetworkCommunicationManager.ClientPacket();
            clientPacket.inputType = NetworkCommunicationManager.ClientInputType.RELOAD;
            clientPacket.clientInput = new NetworkCommunicationManager.ReloadInput(ActiveWeaponIndex);
            NetworkRoomManager.Instance.CommunicationManager.AddCommandIntoBuffer(clientPacket);
        }

        public void ProcessClientChangeWeaponInput(int weaponIndex)
        {
            NetworkCommunicationManager.ClientPacket clientPacket = new NetworkCommunicationManager.ClientPacket();
            clientPacket.inputType = NetworkCommunicationManager.ClientInputType.CHANGE_WEAPON;
            clientPacket.clientInput = new NetworkCommunicationManager.ChangeWeaponInput(weaponIndex);
            NetworkRoomManager.Instance.CommunicationManager.AddCommandIntoBuffer(clientPacket);

            EquipWeapon(weaponIndex);
        }

        private void ProcessServerInput()
        {
            if (CanFire && Input.GetMouseButtonDown(0))
            {
                ProcessServerFireInput();
            }
            if (CanReload(activeWeaponIndex) && Input.GetKeyDown(KeyCode.R))
            {
                ProcessServerReloadInput();
            }
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                ProcessServerChangeWeaponInput(0);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                ProcessServerChangeWeaponInput(1);
            }
        }

        public void ProcessServerFireInput()
        {
            Ray r = shootCamera.ViewportPointToRay(Vector3.one * 0.5f);
            FireInput fireInput = new NetworkCommunicationManager.FireInput(r.origin, r.direction.normalized, NetworkRoomManager.Instance.CommunicationManager.NextSequenceNumber - 1);
            fireInput.ApplyInput(this);

            PhotonView.RPC("RPC_ClientFireSimulation", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, BulletSpawnPosition, BulletSpawnRotation);
        }

        public void ProcessServerReloadInput()
        {
            ReloadInput reloadInput = new NetworkCommunicationManager.ReloadInput(ActiveWeaponIndex);
            reloadInput.ApplyInput(this);

            PhotonView.RPC("RPC_ClientReloadSimulation", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, ActiveWeaponIndex);
        }

        public void ProcessServerChangeWeaponInput(int weaponIndex)
        {
            EquipWeapon(weaponIndex);
        }

#endregion

        public void FillPlayerState(ref PlayerState playerState)
        {
            playerState.playerPosition = transform.position;
            playerState.playerRotation = transform.rotation;
            playerState.playerWeaponID = ActiveWeaponIndex;
            playerState.playerFirstWeaponAmmo = FirstWeaponAmmo;
            playerState.playerSecondWeaponAmmo = SecondWeaponAmmo;
            playerState.playerHealth = health;
        }

#region RPC Calls
        [PunRPC]
        public void RPC_WorldStatePacket(string worldState)
        {
            NetworkRoomManager.Instance.CommunicationManager?.OnWorldStateReceived(worldState);
        }

        [PunRPC]
        public void RPC_ClientInputPacket(string input)
        {
            NetworkRoomManager.Instance.CommunicationManager?.OnClientInputReceived(input);
        }

        [PunRPC]
        public void RPC_ClientFireSimulation(int playerID, Vector3 bulletSpawnPosition, Quaternion bulletSpawnRotation)
        {
            NetworkRoomManager.Instance.CommunicationManager?.OnFireSimulationInputReceived(playerID, bulletSpawnPosition, bulletSpawnRotation);
        }

        [PunRPC]
        public void RPC_ClientReloadSimulation(int playerID, int activeWeaponIndex)
        {
            NetworkRoomManager.Instance.CommunicationManager?.OnReloadSimulationInputReceived(playerID, activeWeaponIndex);
        }

        [PunRPC]
        public void RPC_ClientRevive(int playerID, Vector3 revivePosition, Quaternion reviveRotation)
        {
            NetworkRoomManager.Instance.CommunicationManager?.OnReviveSimulationInputReceived(playerID, revivePosition, reviveRotation);
        }

        [PunRPC]
        public void RPC_ClientFireFeedback(int hitPlayerID, Vector3 hitPoint, Vector3 hitNormal, bool isHit)
        {
            NetworkRoomManager.Instance.CommunicationManager?.OnFireFeedbackReceived(hitPlayerID, hitPoint, hitNormal, isHit);
        }
#endregion
    }
}