using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Text;

namespace OnlineFPS
{
    public class NetworkCommunicationManagerServer : NetworkCommunicationManager
    {
        public const int lastWorldStateBufferSize = 5000;

        WaitForSecondsRealtime waiterTick = new WaitForSecondsRealtime(1f / 58); // Not 60 to sync with the client (client sends packets every 1/60th of a second)
        WaitForSecondsRealtime waiterWorldStateUpdate = new WaitForSecondsRealtime(1f / 20);

        WorldState[] lastWorldStates = new WorldState[lastWorldStateBufferSize];

        Dictionary<int, ClientProxy> clientProxies; // Keeps track of the state of the network client (last acked ids, buffer, etc.)

        // Used to prevent reallocation on the stack when preparing the world data to be sent to clients
        WorldState reusedWorldState;
        PlayerState[] reusedPlayerStates;

        int[] clientIDs;

        public override WorldState[] LastWorldStates => lastWorldStates;

        bool IsInit = false;

        protected override IEnumerator Start()
        {
            yield return base.Start();

            Player[] allPlayers = PhotonNetwork.PlayerList;
            reusedPlayerStates = new PlayerState[allPlayers.Length];
            clientProxies = new Dictionary<int, ClientProxy>(allPlayers.Length);
            clientIDs = new int[allPlayers.Length - 1];

            int count = 0;
            for (int i = 0; i < allPlayers.Length; ++i)
            {
                if (allPlayers[i] != PhotonNetwork.LocalPlayer)
                {
                    int clientID = allPlayers[i].ActorNumber;
                    clientProxies.Add(allPlayers[i].ActorNumber, new ClientProxy(clientID));
                    clientIDs[count++] = clientID;
                }
            }

            while (!NetworkRoomManager.IsInit)
                yield return null;

            AssignPlayerControllers();

            for (int i = 1; i < playerControllers.Count + 1; ++i)
                playerControllers[i].MoveToRandomSpawnPoint();

            //for (int i = 0; i < clientIDs.Length; ++i)
            //    Debug.Log($"Client ID at {i}: {clientIDs[i]}");

            //foreach (var pair in playerControllers)
            //    Debug.Log($"Controller ID: {pair.Key}, {pair.Value.PhotonView.Controller.ActorNumber}");

            //foreach (var pair in clientProxies)
            //    Debug.Log($"Proxy ID: {pair.Key}, {pair.Value.ClientID}");

            StartCoroutine(ServerTickRoutine());
            StartCoroutine(ServerSendWorldStateRoutine());

            IsInit = true;
        }

        public override void OnClientInputReceived(string clientInput)
        {
            base.OnClientInputReceived(clientInput);

            if (!IsInit)
                return;

            int startIndex = 0;
            ClientPacket clientPacket = NetworkParser.ParseClientPacket(clientInput, ref startIndex);

            clientPacket.RunSimulation();

            clientProxies[clientPacket.clientNetworkID].ReceivedPacketFromClient(clientPacket);
        }

        /*
         * Execute unprocessed packet buffers kept in proxies
         */ 
        private IEnumerator ServerTickRoutine()
        {
            while (true)
            {
                yield return waiterTick;
                for(int i = 0; i < clientIDs.Length; ++i)
                {
                    ClientProxy clientProxy = clientProxies[clientIDs[i]];

                    List<ClientPacket> clientInputs = clientProxy.UnprocessedPackets;
                    if (clientInputs.Count > 0)
                    {
                        int clientID = clientProxy.ClientID;

                        FPSController clientController = playerControllers[clientID];
                        for(int j = 0; j < clientInputs.Count; ++j)
                            clientInputs[j].ApplyInput(clientController);

                        clientProxy.UnprocessedPackets.Clear();
                        clientInputs.Clear();
                    }
                }
            }
        }

        private IEnumerator ServerSendWorldStateRoutine()
        {
            while (true)
            {
                yield return waiterWorldStateUpdate;

                // Player Infos
                int i = 0;
                for (; i < clientIDs.Length; ++i)
                {
                    int playerNetworkID = clientIDs[i];
                    reusedPlayerStates[i].playerNetworkID = playerNetworkID;
                    playerControllers[playerNetworkID].FillPlayerState(ref reusedPlayerStates[i]);
                }
                reusedPlayerStates[i].playerNetworkID = PhotonNetwork.LocalPlayer.ActorNumber;
                ownedPlayerController.FillPlayerState(ref reusedPlayerStates[i]);

                reusedWorldState.playerStates = reusedPlayerStates;
                reusedWorldState.packetSequenceNumber = nextSequenceNumber++;
                
                lastWorldStates[(nextSequenceNumber - 1) % 1000] = reusedWorldState; // Keep it in the buffer

                // Fill client specific infos then fire packets
                // Each client have some specific variables such as last acked input sequence number
                for (i = 0; i < clientIDs.Length; ++i)
                {
                    ClientProxy clientProxy = clientProxies[clientIDs[i]];
                    reusedWorldState.lastReceivedInputACK = clientProxy.LastReceivedSequenceNumber;
                    reusedWorldState.previousACKBitmap = clientProxy.PreviousACKNumberBitmap;

                    cachedStringBuilder.Clear();
                    NetworkParser.AddWorldState(reusedWorldState, cachedStringBuilder);

                    ownedPlayerController.PhotonView.RPC("RPC_WorldStatePacket", playerControllers[clientProxy.ClientID].PhotonView.Controller, cachedStringBuilder.ToString());
                    ownedPlayerController.PhotonView.RPC("RPC_WorldStatePacket", playerControllers[clientProxy.ClientID].PhotonView.Controller, cachedStringBuilder.ToString());
                    ownedPlayerController.PhotonView.RPC("RPC_WorldStatePacket", playerControllers[clientProxy.ClientID].PhotonView.Controller, cachedStringBuilder.ToString());
                }

            }
        }
    }
}