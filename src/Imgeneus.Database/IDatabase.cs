﻿using Imgeneus.Database.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Imgeneus.Database
{
    public interface IDatabase : IDisposable
    {
        /// <summary>
        /// Gets or sets the users.
        /// </summary>
        public DbSet<DbUser> Users { get; set; }

        /// <summary>
        /// Gets or sets the characters.
        /// </summary>
        public DbSet<DbCharacter> Characters { get; set; }

        /// <summary>
        /// Gets or sets the character items.
        /// </summary>
        public DbSet<DbCharacterItems> CharacterItems { get; set; }

        /// <summary>
        /// Gets or sets character skills.
        /// </summary>
        public DbSet<DbCharacterSkill> CharacterSkills { get; set; }

        /// <summary>
        /// Gets or sets character quests.
        /// </summary>
        public DbSet<DbCharacterQuest> CharacterQuests { get; set; }

        /// <summary>
        /// Gets or sets character saved positions.
        /// </summary>
        public DbSet<DbCharacterSavePositions> CharacterSavePositions { get; set; }

        /// <summary>
        /// Collection of friend pairs.
        /// </summary>
        public DbSet<DbCharacterFriend> Friends { get; set; }

        /// <summary>
        /// Collection of skills. Taken from original db.
        /// </summary>
        public DbSet<DbSkill> Skills { get; set; }

        /// <summary>
        /// Collection of characters' active buffs.
        /// </summary>
        public DbSet<DbCharacterActiveBuff> ActiveBuffs { get; set; }

        /// <summary>
        /// Collection of items. Taken from original db.
        /// </summary>
        public DbSet<DbItem> Items { get; set; }

        /// <summary>
        /// Collection of mobs. Taken from original db.
        /// </summary>
        public DbSet<DbMob> Mobs { get; set; }

        /// <summary>
        /// Available drop from a monster. Taken from original db.
        /// </summary>
        public DbSet<DbMobItems> MobItems { get; set; }

        /// <summary>
        /// Quick items. E.g. skills on skill bar or motion on skill bar or inventory item on skill bar.
        /// </summary>
        public DbSet<DbQuickSkillBarItem> QuickItems { get; set; }

        /// <summary>
        /// Collection of levels and required experience for them. Taken from original db.
        /// </summary>
        public DbSet<DbLevel> Levels { get; set; }

        /// <summary>
        /// Collection of user's bank items.
        /// </summary>
        public DbSet<DbBankItem> BankItems { get; set; }


        /// <summary>
        /// Collection of user's stored items.
        /// </summary>
        public DbSet<DbWarehouseItem> WarehouseItems { get; set; }

        /// <summary>
        /// Collection of guild's stored items.
        /// </summary>
        public DbSet<DbGuildWarehouseItem> GuildWarehouseItems { get; set; }

        /// <summary>
        /// Collection of guilds.
        /// </summary>
        public DbSet<DbGuild> Guilds { get; set; }

        /// <summary>
        /// Connection between guild and its' npcs.
        /// </summary>
        public DbSet<DbGuildNpcLvl> GuildNpcLvls { get; set; }

        /// <summary>
        /// Saves changes to database.
        /// </summary>
        public int SaveChanges();

        /// <summary>
        /// Saves changes to database.
        /// </summary>
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Migrate database.
        /// </summary>
        public void Migrate();
    }
}
