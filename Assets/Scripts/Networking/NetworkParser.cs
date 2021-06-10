using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace OnlineFPS
{
    using static NetworkCommunicationManager;

    public static class NetworkParser
    {
        public static ClientPacket ParseClientPacket(string st, ref int startFrom, char delimiter = '/')
        {
            ClientPacket clientPacket = new ClientPacket();
            clientPacket.clientNetworkID = ParseInt(st, ref startFrom, delimiter);
            clientPacket.packetSequenceNumber = ParseInt(st, ref startFrom, delimiter);
            clientPacket.lastReceivedWorldStateNumber = ParseInt(st, ref startFrom, delimiter);

            ClientInputType packetType = (ClientInputType)ParseInt(st, ref startFrom, delimiter);
            clientPacket.inputType = packetType;

            clientPacket.clientInput = ClientInput.DeserializeFrom(packetType, st, ref startFrom, delimiter);
            clientPacket.clientInput.inputType = packetType;

            return clientPacket;
        }

        public static WorldState ParseWorldState(string st, ref int startFrom, char delimiter = '/')
        {
            WorldState worldState;
            worldState.packetSequenceNumber = ParseInt(st, ref startFrom, delimiter);
            worldState.lastReceivedInputACK = ParseInt(st, ref startFrom, delimiter);
            worldState.previousACKBitmap = ParseInt(st, ref startFrom, delimiter);

            int playerCount = ParseInt(st, ref startFrom, delimiter);
            worldState.playerStates = new PlayerState[playerCount];

            for(int i = 0; i < playerCount; ++i)
                worldState.playerStates[i] = ParsePlayerState(st, ref startFrom, delimiter);

            return worldState;
        }

        public static PlayerState ParsePlayerState(string st, ref int startFrom, char delimiter = '/')
        {
            PlayerState playerState;
            playerState.playerNetworkID = ParseInt(st, ref startFrom, delimiter);
            playerState.playerPosition = ParseVector3(st, ref startFrom, delimiter);
            playerState.playerRotation = ParseQuaternion(st, ref startFrom, delimiter);
            playerState.playerWeaponID = ParseInt(st, ref startFrom, delimiter);
            playerState.playerFirstWeaponAmmo = ParseInt(st, ref startFrom, delimiter);
            playerState.playerSecondWeaponAmmo = ParseInt(st, ref startFrom, delimiter);
            playerState.playerHealth = ParseInt(st, ref startFrom, delimiter);

            return playerState;
        }

        public static Quaternion ParseQuaternion(string st, ref int startFrom, char delimiter = '/')
        {
            return Quaternion.Euler(ParseVector3(st, ref startFrom, delimiter));
        }

        public static Vector3 ParseVector3(string st, ref int startFrom, char delimiter = '/')
        {
            float x = ParseFloat(st, ref startFrom, '|');
            float y = ParseFloat(st, ref startFrom, '|');
            float z = ParseFloat(st, ref startFrom, delimiter);

            return new Vector3(x, y, z);
        }

        public static float ParseFloat(string st, ref int startFrom, char delimiter = '/')
        {
            int multiplier = GetIsNegative(st, ref startFrom);
            int wholePart = ParseInt(st, ref startFrom, ',');
            float decimalPart = ParseInt(st, ref startFrom, delimiter);

            decimalPart /= 1000;

            return multiplier * (wholePart + decimalPart);
        }

        public static int ParseInt(string st, ref int startFrom, char delimiter = '/')
        {
            int multiplier = GetIsNegative(st, ref startFrom);
            int index = 0;

            while (st[startFrom + index] != delimiter)
                index++;

            (int intForm, bool canParse) = st.Substring(startFrom, index).TryParseToInt();

            startFrom += index + 1;

            return intForm * multiplier;
        }

        public static int GetIsNegative(string st, ref int startFrom)
        {
            if (st[startFrom] == '-')
            {
                startFrom++;
                return -1;
            }

            return 1;
        }

        public static void AddClientPacket(ClientPacket clientPacket, StringBuilder sb, char delimiter = '/')
        {
            AddInt(clientPacket.clientNetworkID, sb, delimiter);
            AddInt(clientPacket.packetSequenceNumber, sb, delimiter);
            AddInt(clientPacket.lastReceivedWorldStateNumber, sb, delimiter);

            clientPacket.clientInput.SerializeInto(sb, delimiter);
        }

        public static void AddWorldState(WorldState worldState, StringBuilder sb, char delimiter = '/')
        {
            AddInt(worldState.packetSequenceNumber, sb);
            AddInt(worldState.lastReceivedInputACK, sb);
            AddInt(worldState.previousACKBitmap, sb);

            AddInt(worldState.playerStates.Length, sb);

            PlayerState[] playerStates = worldState.playerStates;

            for(int i = 0; i < playerStates.Length; ++i)
            {
                AddPlayerState(playerStates[i], sb, delimiter);
            }
        }

        public static void AddPlayerState(PlayerState playerState, StringBuilder sb, char delimiter = '/')
        {
            AddInt(playerState.playerNetworkID, sb);
            AddVector3(playerState.playerPosition, sb);
            AddQuaternion(playerState.playerRotation, sb);
            AddInt(playerState.playerWeaponID, sb);
            AddInt(playerState.playerFirstWeaponAmmo, sb);
            AddInt(playerState.playerSecondWeaponAmmo, sb);
            AddInt(playerState.playerHealth, sb);
        }

        public static void AddQuaternion(Quaternion quaternion, StringBuilder sb, char delimiter = '/') =>
            AddVector3(quaternion.eulerAngles, sb, delimiter);

        public static void AddVector3(Vector3 value, StringBuilder sb, char delimiter = '/')
        {
            AddFloat(value.x, sb, '|');
            AddFloat(value.y, sb, '|');
            AddFloat(value.z, sb, delimiter);
        }

        public static void AddFloat(float value, StringBuilder sb, char delimiter = '/') =>
            sb.Append(value.ToString("F3") + delimiter);

        public static void AddInt(int value, StringBuilder sb, char delimiter = '/') =>
           sb.Append(value.ToString() + delimiter);
    }
}