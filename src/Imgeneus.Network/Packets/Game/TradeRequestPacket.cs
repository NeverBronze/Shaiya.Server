﻿using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record TradeRequestPacket : IPacketDeserializer
    {
        public int TradeToWhomId { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            TradeToWhomId = packetStream.Read<int>();
        }
    }
}
