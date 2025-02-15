﻿using Imgeneus.Core.Structures.Configuration;
using Imgeneus.Database;
using Imgeneus.Database.Context;
using Imgeneus.Database.Entities;
using Imgeneus.Database.Preload;
using Imgeneus.GameDefinitions;
using Imgeneus.Logs;
using Imgeneus.Network.Server;
using Imgeneus.Network.Server.Crypto;
using Imgeneus.World.Game;
using Imgeneus.World.Game.AdditionalInfo;
using Imgeneus.World.Game.AI;
using Imgeneus.World.Game.Attack;
using Imgeneus.World.Game.Bank;
using Imgeneus.World.Game.Buffs;
using Imgeneus.World.Game.Chat;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Duel;
using Imgeneus.World.Game.Dyeing;
using Imgeneus.World.Game.Elements;
using Imgeneus.World.Game.Etin;
using Imgeneus.World.Game.Friends;
using Imgeneus.World.Game.Guild;
using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Kills;
using Imgeneus.World.Game.Levelling;
using Imgeneus.World.Game.Linking;
using Imgeneus.World.Game.Monster;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.Notice;
using Imgeneus.World.Game.NPCs;
using Imgeneus.World.Game.PartyAndRaid;
using Imgeneus.World.Game.Player;
using Imgeneus.World.Game.Player.Config;
using Imgeneus.World.Game.Quests;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Shape;
using Imgeneus.World.Game.Shop;
using Imgeneus.World.Game.Skills;
using Imgeneus.World.Game.Speed;
using Imgeneus.World.Game.Stats;
using Imgeneus.World.Game.Stealth;
using Imgeneus.World.Game.Teleport;
using Imgeneus.World.Game.Time;
using Imgeneus.World.Game.Trade;
using Imgeneus.World.Game.Untouchable;
using Imgeneus.World.Game.Vehicle;
using Imgeneus.World.Game.Warehouse;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Game.Zone.MapConfig;
using Imgeneus.World.Game.Zone.Obelisks;
using Imgeneus.World.Packets;
using Imgeneus.World.SelectionScreen;
using InterServer.Client;
using InterServer.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sylver.HandlerInvoker;

namespace Imgeneus.World
{
    public sealed class WorldServerStartup
    {
        /// <inheritdoc />
        public void ConfigureServices(IServiceCollection services)
        {
            // Add options.
            services.AddOptions<ImgeneusServerOptions>()
                .Configure<IConfiguration>((settings, configuration) => configuration.GetSection("TcpServer").Bind(settings));
            services.AddOptions<WorldConfiguration>()
                .Configure<IConfiguration>((settings, configuration) => configuration.GetSection("WorldServer").Bind(settings));
            services.AddOptions<DatabaseConfiguration>()
               .Configure<IConfiguration>((settings, configuration) => configuration.GetSection("Database").Bind(settings));
            services.AddOptions<InterServerConfig>()
               .Configure<IConfiguration>((settings, configuration) => configuration.GetSection("InterServer").Bind(settings));
            services.AddOptions<GuildConfiguration>()
               .Configure<IConfiguration>((settings, configuration) => configuration.GetSection("Game:Guild").Bind(settings));

            services.RegisterDatabaseServices();

            services.AddHandlers();

            services.AddSingleton<IInterServerClient, ISClient>();
            services.AddSingleton<IWorldServer, WorldServer>();
            services.AddSingleton<IGamePacketFactory, GamePacketFactory>();
            services.AddSingleton<IGameWorld, GameWorld>();
            services.AddSingleton<IGameDefinitionsPreloder, GameDefinitionsPreloder>();
            services.AddSingleton<IMapsLoader, MapsLoader>();
            services.AddSingleton<IMapFactory, MapFactory>();
            services.AddSingleton<IMobFactory, MobFactory>();
            services.AddSingleton<INpcFactory, NpcFactory>();
            services.AddSingleton<IObeliskFactory, ObeliskFactory>();
            services.AddSingleton<ICharacterConfiguration, CharacterConfiguration>((x) => CharacterConfiguration.LoadFromConfigFile());
            services.AddSingleton<IGuildConfiguration, GuildConfiguration>((x) => GuildConfiguration.LoadFromConfigFile());
            services.AddSingleton<IGuildHouseConfiguration, GuildHouseConfiguration>((x => GuildHouseConfiguration.LoadFromConfigFile()));
            services.AddSingleton<IItemEnchantConfiguration, ItemEnchantConfiguration>((x => ItemEnchantConfiguration.LoadFromConfigFile()));
            services.AddSingleton<IMoveTownsConfiguration, MoveTownsConfiguration>((x => MoveTownsConfiguration.LoadFromConfigFile()));
            services.AddSingleton<IItemCreateConfiguration, ItemCreateConfiguration>((x => ItemCreateConfiguration.LoadFromConfigFile()));
            services.AddSingleton<IChatManager, ChatManager>();
            services.AddSingleton<INoticeManager, NoticeManager>();
            services.AddSingleton<IGuildRankingManager, GuildRankingManager>();
            services.AddSingleton<IEtinManager, EtinManager>();
            services.AddSingleton<IDatabasePreloader, DatabasePreloader>((x) =>
            {
                using (var scope = x.CreateScope())
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<DatabasePreloader>>();
                    var db = scope.ServiceProvider.GetRequiredService<IDatabase>();
                    return new DatabasePreloader(logger, db);
                }
            });

            services.AddScoped<ICharacterFactory, CharacterFactory>();
            services.AddScoped<ISelectionScreenManager, SelectionScreenManager>();
            services.AddScoped<IGameSession, GameSession>();
            services.AddScoped<IStatsManager, StatsManager>();
            services.AddScoped<ICountryProvider, CountryProvider>();
            services.AddScoped<ILevelProvider, LevelProvider>();
            services.AddScoped<ILevelingManager, LevelingManager>();
            services.AddScoped<IHealthManager, HealthManager>();
            services.AddScoped<ISpeedManager, SpeedManager>();
            services.AddScoped<IAttackManager, AttackManager>();
            services.AddScoped<ISkillsManager, SkillsManager>();
            services.AddScoped<IBuffsManager, BuffsManager>();
            services.AddScoped<IElementProvider, ElementProvider>();
            services.AddScoped<IInventoryManager, InventoryManager>();
            services.AddScoped<IStealthManager, StealthManager>();
            services.AddScoped<IKillsManager, KillsManager>();
            services.AddScoped<IVehicleManager, VehicleManager>();
            services.AddScoped<IShapeManager, ShapeManager>();
            services.AddScoped<IMovementManager, MovementManager>();
            services.AddScoped<ILinkingManager, LinkingManager>();
            services.AddScoped<IAdditionalInfoManager, AdditionalInfoManager>();
            services.AddScoped<IMapProvider, MapProvider>();
            services.AddScoped<ITeleportationManager, TeleportationManager>();
            services.AddScoped<IDyeingManager, DyeingManager>();
            services.AddScoped<IPartyManager, PartyManager>();
            services.AddScoped<ITradeManager, TradeManager>();
            services.AddScoped<IFriendsManager, FriendsManager>();
            services.AddScoped<IDuelManager, DuelManager>();
            services.AddScoped<IGuildManager, GuildManager>();
            services.AddScoped<IBankManager, BankManager>();
            services.AddScoped<IQuestsManager, QuestsManager>();
            services.AddScoped<IUntouchableManager, UntouchableManager>();
            services.AddScoped<IWarehouseManager, WarehouseManager>();
            services.AddScoped<IAIManager, AIManager>();
            services.AddScoped<IShopManager, ShopManager>();

            services.AddTransient<ICryptoManager, CryptoManager>();
            services.AddTransient<ILogsDatabase, LogsDbContext>();
            services.AddTransient<ITimeService, TimeService>();

            // Add admin website
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddDefaultIdentity<DbUser>(options =>
                {
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequiredLength = 1;
                })
                .AddRoles<DbRole>()
                .AddEntityFrameworkStores<DatabaseContext>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IWorldServer worldServer, ILogsDatabase logsDb, IDatabase mainDb, IGameDefinitionsPreloder definitionsPreloder, RoleManager<DbRole> roleManager)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });


            roleManager.CreateAsync(new DbRole() { Name = DbRole.SUPER_ADMIN }).Wait();
            roleManager.CreateAsync(new DbRole() { Name = DbRole.ADMIN }).Wait();
            roleManager.CreateAsync(new DbRole() { Name = DbRole.USER }).Wait();

            mainDb.Migrate();
            logsDb.Migrate();
            definitionsPreloder.Preload();
            worldServer.Start();
        }
    }
}
