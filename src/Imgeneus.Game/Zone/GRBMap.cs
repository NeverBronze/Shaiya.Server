﻿using Imgeneus.Database.Preload;
using Imgeneus.World.Game.Guild;
using Imgeneus.World.Game.Monster;
using Imgeneus.World.Game.NPCs;
using Imgeneus.World.Game.Time;
using Imgeneus.World.Game.Zone.MapConfig;
using Imgeneus.World.Game.Zone.Obelisks;
using Imgeneus.World.Packets;
using Microsoft.Extensions.Logging;
using Parsec.Shaiya.Svmap;
using System.Collections.Generic;
using System.Linq;

namespace Imgeneus.World.Game.Zone
{
    public class GRBMap : GuildMap, IGRBMap
    {
        public GRBMap(int guildId, IGuildRankingManager guildRankingManager, ushort id, MapDefinition definition, Svmap config, ILogger<Map> logger, IGamePacketFactory packetFactory, IDatabasePreloader databasePreloader, IMobFactory mobFactory, INpcFactory npcFactory, IObeliskFactory obeliskFactory, ITimeService timeService)
            : base(guildId, guildRankingManager, id, definition, config, logger, packetFactory, databasePreloader, mobFactory, npcFactory, obeliskFactory, timeService)
        {
            _guildRankingManager.OnPointsChanged += GuildRankingManager_OnPointsChanged;
        }

        private void GuildRankingManager_OnPointsChanged(int guildId, int points)
        {
            var topGuild = _guildRankingManager.GetTopGuild();
            var topGuildPoints = _guildRankingManager.GetGuildPoints(topGuild);
            var myPoints = _guildRankingManager.GetGuildPoints(GuildId);

            foreach (var player in Players.Values.ToList())
                player.SendGBRPoints(myPoints, topGuildPoints, topGuild);
        }

        public void AddPoints(short points)
        {
            _guildRankingManager.AddPoints(GuildId, points);
        }

        public override bool LoadPlayer(Player.Character character)
        {
            _guildRankingManager.ParticipatedPlayers.Add(character.Id);
            return base.LoadPlayer(character);
        }
    }
}
