using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OnlineFPS
{
    using Photon.Pun;
    using static NetworkCommunicationManager;
    public class ClientProxy
    {
        private static int BufferSize = 15000;

        int lastReceivedSequenceNumber;

        List<ClientPacket> unprocessedPackets;
        int clientID;

        public List<ClientPacket> UnprocessedPackets => unprocessedPackets;
        public int ClientID => clientID;

        public int LastReceivedSequenceNumber => lastReceivedSequenceNumber;

        // Might be implemented in future to get delta packets from server
        public int PreviousACKNumberBitmap => 0;

        public ClientProxy(int id)
        {
            lastReceivedSequenceNumber = 0;
            this.clientID = id;
            unprocessedPackets = new List<ClientPacket>(BufferSize);
        }

        public void ReceivedPacketFromClient(ClientPacket packet)
        {
            // Is received packet is old or the buffer is full
            if (packet.packetSequenceNumber <= lastReceivedSequenceNumber ||
                unprocessedPackets.Count == BufferSize)
                return;

            lastReceivedSequenceNumber = packet.packetSequenceNumber;
            unprocessedPackets.Add(packet);
        }
    }
}
