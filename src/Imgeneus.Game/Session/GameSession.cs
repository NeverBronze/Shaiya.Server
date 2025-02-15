﻿using Imgeneus.World.Packets;
using Imgeneus.World.SelectionScreen;
using Microsoft.Extensions.Logging;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Imgeneus.World.Game.Session
{
    public class GameSession : IGameSession, IDisposable
    {
        private readonly Timer _logoutTimer = new Timer()
        {
            AutoReset = false,
            Interval = 10000 // 10 sec
        };

        private readonly ILogger _logger;
        private readonly IGamePacketFactory _packetFactory;
        private readonly IGameWorld _gameWorld;
        private readonly ISelectionScreenManager _selectionScreenManager;

        public int CharId { get; set; }

        public IWorldClient Client { get; set; }

        public bool IsLoggingOff { get; private set; }
        public bool IsAdmin { get; set; }

        private bool _quitGame;

        public GameSession(ILogger<GameSession> logger, IGamePacketFactory packetFactory, IGameWorld gameWorld, ISelectionScreenManager selectionScreenManager)
        {
            _logger = logger;
            _packetFactory = packetFactory;
            _gameWorld = gameWorld;
            _selectionScreenManager = selectionScreenManager;

            _logoutTimer.Elapsed += LogoutTimer_Elapsed;
#if DEBUG
            _logger.LogDebug("GameSession {hashcode} created", GetHashCode());
#endif
        }

        public void Dispose()
        {
            _logoutTimer.Elapsed -= LogoutTimer_Elapsed;
        }

#if DEBUG
        ~GameSession()
        {
            _logger.LogDebug("GameSession {hashcode} collected by GC", GetHashCode());
        }
#endif

        public void StartLogOff(bool quitGame = false)
        {
            if (IsLoggingOff || CharId == 0)
                return;

            IsLoggingOff = true;
            _quitGame = quitGame;
            _logoutTimer.Start();
        }

        public void StopLogOff()
        {
            IsLoggingOff = false;
            _logoutTimer.Stop();
        }

        private async void LogoutTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // First, remove player from game world.
            // Because some managers like Guild Manager provide GuildId and player can be found in guild map ONLY if this id is presented.
            _gameWorld.RemovePlayer(CharId);

            try
            {
                // ONLY after player is unloaded from game world we can clear session servises.
                await Client.ClearSession(_quitGame);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed clear session for {characterId}. Reason: {message}. Stack trace: {trace}", CharId, ex.Message, ex.StackTrace);
            }

            CharId = 0;
            IsLoggingOff = false;

            if (_quitGame)
            {
                _packetFactory.SendQuitGame(Client);
            }
            else
            {
                _packetFactory.SendLogout(Client);
                Client.CryptoManager.UseExpandedKey = false;

                _packetFactory.SendCharacterList(Client, await _selectionScreenManager.GetCharacters(Client.UserId));
                _packetFactory.SendFaction(Client, await _selectionScreenManager.GetFaction(Client.UserId), await _selectionScreenManager.GetMaxMode(Client.UserId));
            }
        }
    }
}
