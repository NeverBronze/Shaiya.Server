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

namespace Imgeneus.World.Game.Zone
{
    public abstract class GuildMap : Map, IGuildMap
    {
        protected readonly int _guildId;
        protected readonly IGuildRankingManager _guildRankingManager;

        public int GuildId
        {
            get
            {
                return _guildId;
            }
        }

        public GuildMap(int guildId, IGuildRankingManager guildRankingManager, ushort id, MapDefinition definition, Svmap config, ILogger<Map> logger, IGamePacketFactory packetFactory, IDatabasePreloader databasePreloader, IMobFactory mobFactory, INpcFactory npcFactory, IObeliskFactory obeliskFactory, ITimeService timeService)
            : base(id, definition, config, new List<ObeliskConfiguration>(), logger, packetFactory, databasePreloader, mobFactory, npcFactory, obeliskFactory, timeService)
        {
            _guildId = guildId;
            _guildRankingManager = guildRankingManager;
        }

    }
}
