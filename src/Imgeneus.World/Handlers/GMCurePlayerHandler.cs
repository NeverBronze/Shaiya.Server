﻿using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;
using System.Linq;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class GMCurePlayerHandler : BaseHandler
    {
        private readonly IGameWorld _gameWorld;

        public GMCurePlayerHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IGameWorld gameWorld) : base(packetFactory, gameSession)
        {
            _gameWorld = gameWorld;
        }

        [HandlerAction(PacketType.GM_CURE_PLAYER)]
        public void HandleOriginal(WorldClient client, GMCurePlayerPacket packet)
        {
            if (!_gameSession.IsAdmin)
                return;

            var target = _gameWorld.Players.FirstOrDefault(p => p.Value.Name == packet.Name).Value;

            if (target == null)
            {
                _packetFactory.SendGmCommandError(client, PacketType.GM_CURE_PLAYER);
                return;
            }

            target?.HealthManager.FullRecover();

            _packetFactory.SendGmCommandSuccess(client);
        }

        [HandlerAction(PacketType.GM_SHAIYA_US_CURE_PLAYER)]
        public void HandleUs(WorldClient client, GMCurePlayerPacket packet)
        {
            if (!_gameSession.IsAdmin)
                return;

            var target = _gameWorld.Players.FirstOrDefault(p => p.Value.Name == packet.Name).Value;

            if (target == null)
            {
                _packetFactory.SendGmCommandError(client, PacketType.GM_SHAIYA_US_CURE_PLAYER);
                return;
            }

            target?.HealthManager.FullRecover();

            _packetFactory.SendGmCommandSuccess(client);
        }
    }
}
