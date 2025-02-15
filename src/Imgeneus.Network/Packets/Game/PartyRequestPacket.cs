﻿using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record PartyRequestPacket : IPacketDeserializer
    {
        public int CharacterId { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            CharacterId = packetStream.Read<int>();
        }
    }
}
