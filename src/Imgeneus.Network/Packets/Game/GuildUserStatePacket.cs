﻿using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record GuildUserStatePacket : IPacketDeserializer
    {
        public bool Demote { get; private set; }

        public int CharacterId { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            Demote = packetStream.Read<bool>();
            CharacterId = packetStream.Read<int>();
        }
    }
}
