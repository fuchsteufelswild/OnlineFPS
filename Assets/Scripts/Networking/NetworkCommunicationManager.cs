using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Text;

namespace OnlineFPS
{
    public abstract class NetworkCommunicationManager : MonoBehaviour
    {
        public static NetworkCommunicationManager Instance;

        protected int nextSequenceNumber;
        protected int lastAckedPackageNumber;

        protected Dictionary<int, FPSController> playerControllers;

        protected FPSController ownedPlayerController;

        protected StringBuilder cachedStringBuilder; // Used to construct packets, cached to prevent unnecessary allocations

        // Temporary
        [SerializeField] HitDecal hitDecalPrefab;
        [SerializeField] HitDecal[] hitDecals;

        public virtual WorldState[] LastWorldStates => null;
        public int LastAckedPackageNumber => lastAckedPackageNumber;
        public int NextSequenceNumber => nextSequenceNumber;

        public FPSController ControlledPlayerController => ownedPlayerController;

        private void Awake()
        {
            if (Instance != null)
                Destroy(gameObject);

            Instance = this;
        }

        protected virtual IEnumerator Start()
        {
            // Temporary
            hitDecals = new HitDecal[100];
            for (int i = 0; i < 100; ++i)
                hitDecals[i] = Instantiate(hitDecalPrefab, HitDecal.defaultPosition, Quaternion.identity);

            cachedStringBuilder = new StringBuilder(900);
            playerControllers = new Dictionary<int, FPSController>(PhotonNetwork.PlayerList.Length);
            nextSequenceNumber = 0;
            lastAckedPackageNumber = -1;
            yield return null;
        }

        protected void AssignPlayerControllers()
        {
            FPSController[] fpsControllers = FindObjectsOfType<FPSController>();
            for (int i = 0; i < fpsControllers.Length; ++i)
            {
                Player controllingPlayer = fpsControllers[i].PhotonView.Controller;
                playerControllers.Add(controllingPlayer.ActorNumber, fpsControllers[i]);
                if (fpsControllers[i].PhotonView.IsMine)
                    ownedPlayerController = fpsControllers[i];
            }
        }

        public FPSController GetPlayerControllerWithID(int playerNetworkID) =>
            playerControllers[playerNetworkID];

        // Temporary function
        public void InitNextDecalAt(Vector3 position, Vector3 normal)
        {
            for (int i = 0; i < 100; ++i)
            {
                if (!hitDecals[i].IsInUse)
                {
                    hitDecals[i].InitAt(position, normal);
                    break;
                }
            }
            
        }

        #region RPC Callback Prototypes
        public virtual void OnWorldStateReceived(string worldStateInput)
        {

        }
        
        public virtual void OnClientInputReceived(string clientInput)
        {

        }

        public virtual void OnFireSimulationInputReceived(int playerID, Vector3 bulletSpawnPosition, Quaternion bulleSpawnRotation)
        {

        }

        public virtual void OnReloadSimulationInputReceived(int playerID, int activeWeaponIndex)
        {

        }

        public virtual void OnReviveSimulationInputReceived(int playerID, Vector3 revivePosition, Quaternion reviveRotation)
        {

        }

        public virtual void OnFireFeedbackReceived(int hitPlayerID, Vector3 hitPoint, Vector3 hitNormal, bool isHit)
        {

        }

        public virtual void AddCommandIntoBuffer(ClientPacket input)
        {

        }
        #endregion

        #region World Data Types
        [System.Serializable]
        public struct WorldState
        {
            public int packetSequenceNumber;
            public int lastReceivedInputACK;
            public int previousACKBitmap;
            public PlayerState[] playerStates;
        }

        [System.Serializable]
        public struct PlayerState
        {
            public int playerNetworkID;
            public Vector3 playerPosition;
            public Quaternion playerRotation;
            public int playerWeaponID;
            public int playerFirstWeaponAmmo;
            public int playerSecondWeaponAmmo;
            public int playerHealth;

            public void FillAmmo(int weaponIndex)
            {
                if (weaponIndex == 0)
                    playerFirstWeaponAmmo = 9;
                else if (weaponIndex == 1)
                    playerSecondWeaponAmmo = 30;
            }
        }
        #endregion

        #region Input Packet Types


        public enum ClientInputType
        {
            FIRE = 0,
            MOVE,
            CHANGE_WEAPON,
            RELOAD
        }

        [System.Serializable]
        public struct ClientPacket
        {
            #region Static Construct Methods
            public static void CreateAndAddMovePacket(Vector3 movementAmount, float rotationAmountX, float rotationAmountY)
            {
                NetworkCommunicationManager.ClientPacket clientPacket = new NetworkCommunicationManager.ClientPacket();
                clientPacket.inputType = NetworkCommunicationManager.ClientInputType.MOVE;
                clientPacket.clientInput = new NetworkCommunicationManager.MoveInput(movementAmount, rotationAmountX, rotationAmountY);
                NetworkRoomManager.Instance.CommunicationManager.AddCommandIntoBuffer(clientPacket);
            }

            public static void CreateAndAddFirePacket(Vector3 movementAmount, float rotationAmountX, float rotationAmountY)
            {
                NetworkCommunicationManager.ClientPacket clientPacket = new NetworkCommunicationManager.ClientPacket();
                clientPacket.inputType = NetworkCommunicationManager.ClientInputType.MOVE;
                clientPacket.clientInput = new NetworkCommunicationManager.MoveInput(movementAmount, rotationAmountX, rotationAmountY);
                NetworkRoomManager.Instance.CommunicationManager.AddCommandIntoBuffer(clientPacket);
            }

            public static void CreateAndAddReloadPacket(Vector3 movementAmount, float rotationAmountX, float rotationAmountY)
            {
                NetworkCommunicationManager.ClientPacket clientPacket = new NetworkCommunicationManager.ClientPacket();
                clientPacket.inputType = NetworkCommunicationManager.ClientInputType.MOVE;
                clientPacket.clientInput = new NetworkCommunicationManager.MoveInput(movementAmount, rotationAmountX, rotationAmountY);
                NetworkRoomManager.Instance.CommunicationManager.AddCommandIntoBuffer(clientPacket);
            }

            public static void CreateAndAddChangeWeaponPacket(Vector3 movementAmount, float rotationAmountX, float rotationAmountY)
            {
                NetworkCommunicationManager.ClientPacket clientPacket = new NetworkCommunicationManager.ClientPacket();
                clientPacket.inputType = NetworkCommunicationManager.ClientInputType.MOVE;
                clientPacket.clientInput = new NetworkCommunicationManager.MoveInput(movementAmount, rotationAmountX, rotationAmountY);
                NetworkRoomManager.Instance.CommunicationManager.AddCommandIntoBuffer(clientPacket);
            }
            #endregion

            public int clientNetworkID;
            public int packetSequenceNumber;
            public int lastReceivedWorldStateNumber;

            public ClientInputType inputType;

            public ClientInput clientInput;

            public void ApplyInput(FPSController fpsController) =>
                clientInput?.ApplyInput(fpsController);

            public PlayerState ApplyInput(PlayerState playerState) =>
                clientInput.ApplyInput(playerState);

            public void RunSimulation()
            {
                if(clientInput != null)
                {
                    clientInput.RunSimulationOn(clientNetworkID);
                }
            }
        }


        [System.Serializable]
        public abstract class ClientInput
        {
            public static ClientInput DeserializeFrom(ClientInputType packetType, string st, ref int startFrom, char delimiter = '/')
            {
                switch (packetType)
                {
                    case ClientInputType.FIRE:
                        return FireInput.DeserializeFrom(st, ref startFrom);
                    case ClientInputType.MOVE:
                        return MoveInput.DeserializeFrom(st, ref startFrom);
                    case ClientInputType.CHANGE_WEAPON:
                        return ChangeWeaponInput.DeserializeFrom(st, ref startFrom);
                    case ClientInputType.RELOAD:
                        return ReloadInput.DeserializeFrom(st, ref startFrom);
                }

                return null;
            }

            public ClientInputType inputType;

            public abstract void SerializeInto(StringBuilder sb, char delimiter = '/');

            public abstract void ApplyInput(FPSController fpsController);
            public abstract void RevertInput(FPSController fpsController);

            public abstract PlayerState ApplyInput(PlayerState playerState);

            public virtual void RunSimulationOn(int clientNetworkID)
            {

            }
        }

        [System.Serializable]
        public class ReloadInput : ClientInput
        {
            public int activeWeaponIndex;

            public ReloadInput(int activeWeaponIndex)
            {
                this.activeWeaponIndex = activeWeaponIndex;

                inputType = ClientInputType.RELOAD;
            }

            public override void ApplyInput(FPSController fpsController)
            {
                if (fpsController.CanReload(activeWeaponIndex))
                {
                    fpsController.Reload(activeWeaponIndex);

                    Debug.Log($"Weapon Reloaded On Server by: {fpsController.PhotonView.Controller.ActorNumber}, weapon Reloaded: {activeWeaponIndex}");
                }
            }

            public static ClientInput DeserializeFrom(string st, ref int startFrom, char delimiter = '/')
            {
                int activeIndex = NetworkParser.ParseInt(st, ref startFrom, delimiter);

                return new ReloadInput(activeIndex);
            }

            public override void SerializeInto(StringBuilder sb, char delimiter = '/')
            {
                NetworkParser.AddInt((int)ClientInputType.RELOAD, sb, delimiter);
                NetworkParser.AddInt(activeWeaponIndex, sb, delimiter);
            }

            public override void RevertInput(FPSController fpsController)
            {
                throw new System.NotImplementedException();
            }

            public override PlayerState ApplyInput(PlayerState playerState)
            {
                if (playerState.playerHealth <= 0)
                    return playerState;

                if (activeWeaponIndex == 0)
                    playerState.playerFirstWeaponAmmo = 9;
                else if (activeWeaponIndex == 1)
                    playerState.playerSecondWeaponAmmo = 30;

                return playerState;
            }

            public override void RunSimulationOn(int clientNetworkID)
            {
                FPSController playerController = NetworkRoomManager.Instance.CommunicationManager.GetPlayerControllerWithID(clientNetworkID);

                if (playerController.IsAlive)
                {
                    playerController.SimulateReload();

                    playerController.PhotonView.RPC("RPC_ClientReloadSimulation", RpcTarget.All, clientNetworkID, playerController.ActiveWeaponIndex);
                }
            }
        }

        [System.Serializable]
        public class ChangeWeaponInput : ClientInput
        {
            public int newActiveIndex;

            public ChangeWeaponInput(int newActiveIndex)
            {
                this.newActiveIndex = newActiveIndex;
                inputType = ClientInputType.CHANGE_WEAPON;
            }

            public override void ApplyInput(FPSController fpsController)
            {
                if (fpsController.CanChangeWeapon)
                {
                    Debug.Log($"Weapon Changed On Server by: {fpsController.PhotonView.Controller.ActorNumber}, newWeapon: {newActiveIndex}");

                    fpsController.EquipWeapon(newActiveIndex);
                }
            }

            public static ClientInput DeserializeFrom(string st, ref int startFrom, char delimiter = '/')
            {
                int activeIndex = NetworkParser.ParseInt(st, ref startFrom, delimiter);

                return new ChangeWeaponInput(activeIndex);
            }

            public override void SerializeInto(StringBuilder sb, char delimiter = '/')
            {
                NetworkParser.AddInt((int)ClientInputType.CHANGE_WEAPON, sb, delimiter);

                NetworkParser.AddInt(newActiveIndex, sb, delimiter);
            }

            public override void RevertInput(FPSController fpsController)
            {
                throw new System.NotImplementedException();
            }

            public override PlayerState ApplyInput(PlayerState playerState)
            {
                if (playerState.playerHealth <= 0)
                    return playerState;
                playerState.playerWeaponID = newActiveIndex;

                return playerState;
            }
        }

        [System.Serializable]
        public class FireInput : ClientInput
        {
            Vector3 origin;
            Vector3 direction;
            int worldStateIndex;

            public FireInput(Vector3 origin, Vector3 direction, int worldStateIndex)
            {
                this.origin = origin;
                this.direction = direction;
                this.worldStateIndex = worldStateIndex;

                inputType = ClientInputType.FIRE;
            }

            /*
             * Fire ray with give input. Test for the current world state (this way test untracked objects)
             * Then fire ray in the world state given by "worldStateIndex" variable. 
             * Array of states are in the server, with their data test ray intersection.
             * If the hit player is closer then previous intersection test then take it as HIT.
             * Propagate hit feedback to all players so that they may simulate hit and show decal.
             */ 
            public override void ApplyInput(FPSController fpsController)
            {
                int weaponAmmo = fpsController.ActiveWeaponIndex == 0 ? fpsController.FirstWeaponAmmo : fpsController.SecondWeaponAmmo;

                if (!fpsController.CanFire)
                    return;

                float distance = 1e9f;

                PlayerState[] snapShot = NetworkRoomManager.Instance.CommunicationManager.LastWorldStates[worldStateIndex % 1000].playerStates; // Player states in the given world state

                
                RaycastHit hit;
                if(Physics.Raycast(origin, direction, out hit, Mathf.Infinity, 1 << 0, QueryTriggerInteraction.Collide))
                {
                    distance = Vector3.Distance(hit.point, origin);
                }

                int hitPlayerID = -1;
                Ray r = new Ray(origin, direction * 100f);
                Bounds playerBounds = new Bounds(Vector3.zero, Vector3.zero);
                Debug.DrawRay(origin, direction * 100, Color.black, 10f);
                for (int i = 0; i < snapShot.Length; ++i)
                {
                    PlayerState playerStateInSnapshot = snapShot[i];

                    if (NetworkRoomManager.Instance.CommunicationManager.playerControllers[playerStateInSnapshot.playerNetworkID] != fpsController &&
                        playerStateInSnapshot.playerHealth > 0)
                    {
                        playerBounds.center = playerStateInSnapshot.playerPosition;
                        playerBounds.size = new Vector3(1f, 2f, 1f);

                        if (playerBounds.IntersectRay(r, out float dist))
                        {
                            Debug.Log($"Intersects with bound {playerStateInSnapshot.playerNetworkID} dist: {dist}");
                            if(dist < distance)
                            {
                                hitPlayerID = playerStateInSnapshot.playerNetworkID;
                                distance = dist;
                            }
                        }
                    }
                }

                if(hitPlayerID != -1)
                    NetworkRoomManager.Instance.CommunicationManager.playerControllers[hitPlayerID].TakeDamage(1);
                
                fpsController.Fire();

                Vector3 hitPoint = (hitPlayerID != -1) ? (r.origin + r.direction * distance) : (hit.collider != null ? hit.point : Vector3.zero);
                bool isHit = hitPlayerID != -1 || hit.collider != null;
                Vector3 hitNormal = (hitPlayerID != -1) ? (r.origin - hitPoint) : (hit.collider != null ? hit.normal : Vector3.zero);

                fpsController.PhotonView.RPC("RPC_ClientFireFeedback", RpcTarget.All, hitPlayerID, hitPoint, hitNormal, isHit);
                Debug.Log($"Weapon Fired On Server by: {fpsController.PhotonView.Controller.ActorNumber}, hit: {hitPlayerID}, distance: {distance}, remaining Ammo: {fpsController.FirstWeaponAmmo}");

                if (isHit)
                    NetworkRoomManager.Instance.CommunicationManager.InitNextDecalAt(hitPoint, hitNormal);
                // Send fire callback
            }

            public static ClientInput DeserializeFrom(string st, ref int startFrom, char delimiter = '/')
            {
                Vector3 origin = NetworkParser.ParseVector3(st, ref startFrom, delimiter);
                Vector3 direction = NetworkParser.ParseVector3(st, ref startFrom, delimiter);
                int worldStateIndex = NetworkParser.ParseInt(st, ref startFrom, delimiter);

                return new FireInput(origin, direction, worldStateIndex);
            }

            public override void SerializeInto(StringBuilder sb, char delimiter = '/')
            {
                NetworkParser.AddInt((int)ClientInputType.FIRE, sb, delimiter);

                NetworkParser.AddVector3(origin, sb, delimiter);
                NetworkParser.AddVector3(direction, sb, delimiter);
                NetworkParser.AddInt(worldStateIndex, sb, delimiter);
            }

            public override void RevertInput(FPSController fpsController)
            {
                throw new System.NotImplementedException();
            }

            public override PlayerState ApplyInput(PlayerState playerState)
            {
                if (playerState.playerHealth <= 0)
                    return playerState;

                int i = playerState.playerWeaponID;
                if(i == 0)
                    playerState.playerFirstWeaponAmmo--;
                else if(i == 1)
                    playerState.playerSecondWeaponAmmo--;

                return playerState;
            }

            public override void RunSimulationOn(int clientNetworkID)
            {
                FPSController playerController = NetworkRoomManager.Instance.CommunicationManager.GetPlayerControllerWithID(clientNetworkID);

                if (playerController.CanFire)
                {
                    playerController.SimulateFire(playerController.BulletSpawnPosition, playerController.BulletSpawnRotation);

                    playerController.PhotonView.RPC("RPC_ClientFireSimulation", RpcTarget.All, clientNetworkID, playerController.BulletSpawnPosition, playerController.BulletSpawnRotation);
                }
            }
        }

        [System.Serializable]
        public class MoveInput : ClientInput
        {
            public Vector3 movementDirection;
            public float rotXAmount;
            public float rotYAmount;

            public MoveInput(Vector3 movementDirection, 
                             float rotXAmount, float rotYAmount)
            {
                this.movementDirection = movementDirection;
                this.rotXAmount = rotXAmount;
                this.rotYAmount = rotYAmount;

                inputType = ClientInputType.MOVE;
            }

            public override void ApplyInput(FPSController fpsController)
            {
                if (!fpsController.IsAlive)
                    return;

                fpsController.MovementComponent.ApplyRotation(rotXAmount, rotYAmount);
                fpsController.MovementComponent.ApplyMovement(movementDirection);
            }

            public override void RevertInput(FPSController fpsController)
            {
                fpsController.MovementComponent.ApplyRotation(-rotXAmount, -rotYAmount);
                fpsController.MovementComponent.ApplyMovement(-movementDirection);
            }

            public override void SerializeInto(StringBuilder sb, char delimiter = '/')
            {
                NetworkParser.AddInt((int)ClientInputType.MOVE, sb, delimiter);

                NetworkParser.AddVector3(movementDirection, sb, delimiter);
                NetworkParser.AddFloat(rotXAmount, sb, delimiter);
                NetworkParser.AddFloat(rotYAmount, sb, delimiter);
            }

            public static ClientInput DeserializeFrom(string st, ref int startFrom, char delimiter = '/')
            {
                Vector3 direction = NetworkParser.ParseVector3(st, ref startFrom, delimiter);
                float rotX = NetworkParser.ParseFloat(st, ref startFrom, delimiter);
                float rotY = NetworkParser.ParseFloat(st, ref startFrom, delimiter);

                return new MoveInput(direction, rotX, rotY);
            }

            public override PlayerState ApplyInput(PlayerState playerState)
            {
                if (playerState.playerHealth <= 0)
                    return playerState;
                playerState.playerPosition += movementDirection;
                Vector3 rotAmount = playerState.playerRotation.eulerAngles;
                rotAmount.x += rotXAmount;
                rotAmount.y += rotYAmount;

                rotAmount.x = Mathf.Clamp(rotAmount.x, -45, 45);

                playerState.playerRotation.eulerAngles = rotAmount;

                return playerState;
            }

            public override string ToString()
            {
                return $"Movement Direction: {movementDirection}, RotationX: {rotXAmount}, RotationY: {rotYAmount}";
            }
        }
    #endregion
    }
}