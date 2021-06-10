using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OnlineFPS
{
    public class NetworkCommunicationManagerClient : NetworkCommunicationManager
    {
        public const bool IsLagSimulated = false;

        private const int backupBufferSize = 30000;

        bool IsInit = false;

        WaitForSecondsRealtime waiterTick = new WaitForSecondsRealtime(1f / 60);

        List<ClientPacket> packetBuffers = new List<ClientPacket>(backupBufferSize);
        ClientPacket[] backUpPackets = new ClientPacket[backupBufferSize];

        // Used for interpolation
        List<Vector3> mostRecentArrivedPlayerPositions;
        List<Quaternion> mostRecentArrivedPlayerRotations;
        
        // Used for lag simulation
        // List<string> queuedToSent = new List<string>(backupBufferSize);
        // List<double> queuedTimeStamps = new List<double>(backupBufferSize);
        // public static double LagSimulationAmount = 0.250;

        

        protected override IEnumerator Start()
        {
            yield return base.Start();

            // Lag Simulation
            //for(int i = 0; i < backupBufferSize; ++i)
            //{
            //    queuedTimeStamps.Add(-1);
            //    queuedToSent.Add(null);
            //}

            while (!NetworkRoomManager.IsInit)
                yield return null;

            AssignPlayerControllers();

            mostRecentArrivedPlayerPositions = new List<Vector3>(playerControllers.Count);
            mostRecentArrivedPlayerRotations = new List<Quaternion>(playerControllers.Count);

            for (int i = 1; i < playerControllers.Count + 1; ++i)
            {
                mostRecentArrivedPlayerPositions.Add(playerControllers[i].transform.position);
                mostRecentArrivedPlayerRotations.Add(playerControllers[i].transform.rotation);
            }

            StartCoroutine(ClientTickRoutine());
            StartCoroutine(InterpolationRoutine());
            if(IsLagSimulated)
                StartCoroutine(LagSimulatedSendRoutine());

            IsInit = true;
        }

        /*
         * Interpolates all players' current position/rotation to
         * lastly retrieved one
         */ 
        private IEnumerator InterpolationRoutine()
        {
            // Tweak interpolation speed for smoother correction
            float interpolationSpeed = 10f;

            while(true)
            {
                for(int i = 1; i < playerControllers.Count + 1; ++i)
                {
                    FPSController playerController = playerControllers[i];

                    if(playerController.IsInterpolatingPosition ||
                       playerController.IsInterpolatingRotation)
                    {
                        if(playerController.IsInterpolatingPosition && playerController.transform.position.Approximately(mostRecentArrivedPlayerPositions[i - 1], 0.2f))
                            playerController.IsInterpolatingPosition = false;
                        if(playerController.IsInterpolatingRotation && playerController.transform.rotation.Approximately(mostRecentArrivedPlayerRotations[i - 1], 0.04f))
                            playerController.IsInterpolatingRotation = false;

                        if(playerController.IsInterpolatingPosition)
                            playerController.transform.position = Vector3.Lerp(playerController.transform.position, mostRecentArrivedPlayerPositions[i - 1], Time.deltaTime * interpolationSpeed);
                        if(playerController.IsInterpolatingRotation)
                            playerController.transform.rotation = Quaternion.Lerp(playerController.transform.rotation, mostRecentArrivedPlayerRotations[i - 1], Time.deltaTime * interpolationSpeed);
                    }
                }

                yield return null;
            }
        }

        private IEnumerator ClientTickRoutine()
        {
            while (true)
            {
                yield return waiterTick;
                
                // Send all packets stored in the buffer
                for (int i = 0; i < packetBuffers.Count; ++i)
                {
                    ClientPacket input = packetBuffers[i];

                    input.lastReceivedWorldStateNumber = lastAckedPackageNumber;

                    cachedStringBuilder.Clear();
                    NetworkParser.AddClientPacket(input, cachedStringBuilder);

                    if (IsLagSimulated)
                    {
                        //queuedTimeStamps[input.packetSequenceNumber % backupBufferSize] = PhotonNetwork.Time;
                        //queuedToSent[input.packetSequenceNumber % backupBufferSize] = cachedStringBuilder.ToString();
                    }
                    else
                    {
                        ownedPlayerController.PhotonView.RPC("RPC_ClientInputPacket", RpcTarget.MasterClient, cachedStringBuilder.ToString());
                        ownedPlayerController.PhotonView.RPC("RPC_ClientInputPacket", RpcTarget.MasterClient, cachedStringBuilder.ToString());
                        ownedPlayerController.PhotonView.RPC("RPC_ClientInputPacket", RpcTarget.MasterClient, cachedStringBuilder.ToString());
                    }
                }

                packetBuffers.Clear();
            }
        }

        /*
         * Lag simulation test routine. When test is enabled
         * this routine runs. Sends packet at (their issued time + given lag amount)
         */ 
        private IEnumerator LagSimulatedSendRoutine()
        {
            yield break;
            //int lastId = 0;
            //WaitForSecondsRealtime waitForSecondsRealtime = new WaitForSecondsRealtime((float)LagSimulationAmount);
            //while(true)
            //{
            //    yield return waitForSecondsRealtime;

            //    bool circle = true;

            //    for(int i = lastId; i < queuedToSent.Count; ++i)
            //    {
            //        if(queuedTimeStamps[i] != -1)
            //        {
            //            if (queuedTimeStamps[i] + LagSimulationAmount < PhotonNetwork.Time)
            //            {
            //                ownedPlayerController.PhotonView.RPC("RPC_ClientInputPacket", RpcTarget.MasterClient, queuedToSent[i]);
            //                ownedPlayerController.PhotonView.RPC("RPC_ClientInputPacket", RpcTarget.MasterClient, queuedToSent[i]);
            //                ownedPlayerController.PhotonView.RPC("RPC_ClientInputPacket", RpcTarget.MasterClient, queuedToSent[i]);
            //            }
            //            else
            //            {
            //                lastId = i;
            //                circle = false;
            //                break;
            //            }
            //        }
            //        else
            //        {
            //            lastId = i;
            //            circle = false;
            //            break;
            //        }
            //    }

            //    if(circle)
            //    {
            //        for (int i = 0; i < queuedToSent.Count; ++i)
            //        {
            //            if (queuedTimeStamps[i] == -2)
            //                continue;

            //            if (queuedTimeStamps[i] + LagSimulationAmount < PhotonNetwork.Time)
            //            {
            //                ownedPlayerController.PhotonView.RPC("RPC_ClientInputPacket", RpcTarget.MasterClient, queuedToSent[i]);
            //                ownedPlayerController.PhotonView.RPC("RPC_ClientInputPacket", RpcTarget.MasterClient, queuedToSent[i]);
            //                ownedPlayerController.PhotonView.RPC("RPC_ClientInputPacket", RpcTarget.MasterClient, queuedToSent[i]);
            //            }
            //            else
            //            {
            //                lastId = i;
            //                break;
            //            }
            //        }
            //    }
            //}
        }

        public override void AddCommandIntoBuffer(ClientPacket input)
        {
            base.AddCommandIntoBuffer(input);
            
            if (packetBuffers.Count < backupBufferSize)
            {
                input.clientNetworkID = PhotonNetwork.LocalPlayer.ActorNumber;
                input.packetSequenceNumber = nextSequenceNumber++;
                packetBuffers.Add(input);
                backUpPackets[input.packetSequenceNumber % backupBufferSize] = input;
            }
        }

        /*
         * Updates interpolation positions/rotations of all players, 
         * and all other player state (ammo, health etc.)
         */ 
        public override void OnWorldStateReceived(string worldStateInput)
        {
            base.OnWorldStateReceived(worldStateInput);

            if (!IsInit) return;

            int startIndex = 0;
            WorldState worldState = NetworkParser.ParseWorldState(worldStateInput, ref startIndex);
            if (worldState.packetSequenceNumber > lastAckedPackageNumber)
            {
                lastAckedPackageNumber = worldState.packetSequenceNumber;
                int serverLastInputReceive = worldState.lastReceivedInputACK;
                int serverPreviousInputReceive = worldState.previousACKBitmap;

                PlayerState[] playerStates = worldState.playerStates;

                for (int i = 0; i < playerStates.Length; ++i)
                {
                    PlayerState playerState = playerStates[i];
                    if (playerState.playerNetworkID == PhotonNetwork.LocalPlayer.ActorNumber)
                        ProcessOwnedPlayerState(ref playerState, worldState.lastReceivedInputACK);
                    else
                        ProcessOtherPlayerState(ref playerState, playerState.playerNetworkID);
                }
            }
        }

        private void ProcessOwnedPlayerState(ref PlayerState playerState, int lastAckedPlayerInputSequenceNumber)
        {
            PredictOwnedPlayerState(ref playerState, lastAckedPlayerInputSequenceNumber, nextSequenceNumber);

            UpdateNonInterpolatedPlayerControllerVariables(ref playerState, ownedPlayerController);
        }
        
        private void PredictOwnedPlayerState(ref PlayerState playerState, int lastAckedPlayerInputSequenceNumber, int nextSequenceNumberToSend)
        {
            int start = (lastAckedPlayerInputSequenceNumber % backupBufferSize) + 1;
            int end = (nextSequenceNumber) % backupBufferSize;
            if (start < end)
            {
                for (int k = start; k < end; ++k)
                {
                    if (backUpPackets[k].clientInput != null)
                        playerState = backUpPackets[k].ApplyInput(playerState);
                }
            }
            else // Circle back to beginning [ 0...end.....start...Count-1]
            {
                for (int k = start; k < packetBuffers.Count; ++k)
                {
                    if (backUpPackets[k].clientInput != null)
                        playerState = backUpPackets[k].ApplyInput(playerState);
                }
                for (int k = 0; k < end; ++k)
                {
                    if (backUpPackets[k].clientInput != null)
                        playerState = backUpPackets[k].ApplyInput(playerState);
                }
            }

            Vector3 currentPosition = ownedPlayerController.transform.position;
            if (!currentPosition.Approximately(playerState.playerPosition, 0.1f))
            {
                mostRecentArrivedPlayerPositions[PhotonNetwork.LocalPlayer.ActorNumber - 1] = playerState.playerPosition;
                ownedPlayerController.IsInterpolatingPosition = true;
            }

            Vector3 currentRotation = ownedPlayerController.transform.rotation.eulerAngles;
            if (!currentRotation.Approximately(playerState.playerRotation.eulerAngles, 0.1f))
                ownedPlayerController.transform.rotation = playerState.playerRotation;
        }

        private void ProcessOtherPlayerState(ref PlayerState playerState, int playerNetworkID)
        {
            FPSController playerController = playerControllers[playerState.playerNetworkID];

            if (!playerController.transform.position.Approximately(playerState.playerPosition, 0.1f))
            {
                mostRecentArrivedPlayerPositions[playerController.PhotonView.Controller.ActorNumber - 1] = playerState.playerPosition;
                playerController.IsInterpolatingPosition = true;
            }
            if (!playerController.transform.rotation.Approximately(playerState.playerRotation, 0.0004f))
            {
                // mostRecentArrivedPlayerRotations[playerController.PhotonView.Controller.ActorNumber - 1] = playerState.playerRotation;
                // playerController.IsInterpolatingRotation = true;
                playerController.transform.rotation = playerState.playerRotation;
            }

            UpdateNonInterpolatedPlayerControllerVariables(ref playerState, playerController);
            playerController.MovementComponent.OnNewStateArrived(playerState.playerPosition, 0, 0);
        }

        private void UpdateNonInterpolatedPlayerControllerVariables(ref PlayerState playerState, FPSController playerController)
        {
            playerController.SetHealth(playerState.playerHealth);
            playerController.SetFirstWeaponAmmo(playerState.playerFirstWeaponAmmo);
            playerController.SetSecondWeaponAmmo(playerState.playerSecondWeaponAmmo);
            playerController.EquipWeapon(playerState.playerWeaponID);

            if (playerController.IsAlive)
                playerController.Revive();
            else
                playerController.Die();

            playerController.AmmoUpdate();
        }

        #region Immediate Feedback RPC Functionality
        public override void OnFireSimulationInputReceived(int playerID, Vector3 bulletSpawnPosition, Quaternion bulleSpawnRotation)
        {
            if(playerID != PhotonNetwork.LocalPlayer.ActorNumber)
                playerControllers[playerID].SimulateFire(bulletSpawnPosition, bulleSpawnRotation);
        }

        public override void OnReloadSimulationInputReceived(int playerID, int activeWeaponIndex)
        {
            if (playerID != PhotonNetwork.LocalPlayer.ActorNumber)
                playerControllers[playerID].SimulateReload();
        }

        public override void OnReviveSimulationInputReceived(int playerID, Vector3 revivePosition, Quaternion reviveRotation)
        {
            playerControllers[playerID].SimulateReviveAt(revivePosition, reviveRotation);
        }

        public override void OnFireFeedbackReceived(int hitPlayerID, Vector3 hitPoint, Vector3 hitNormal, bool isHit)
        {
            if (isHit)
            {
                if(hitPlayerID != -1)
                {
                    playerControllers[hitPlayerID].TakeDamage(1);
                }

                // Will be retrieved from pool later on
                InitNextDecalAt(hitPoint, hitNormal.normalized);
            }
        }
        #endregion
    }
}