﻿using System;

namespace Imgeneus.World.Game.Vehicle
{
    public interface IVehicleManager: IDisposable
    {
        void Init(int ownerId);

        #region Summoning

        /// <summary>
        /// Event, that is fired, when the player starts summoning mount.
        /// </summary>
        event Action<int> OnStartSummonVehicle;

        /// <summary>
        /// Event, that is fired, when summoning mount is finished.
        /// </summary>
        event Action<bool, bool> OnUsedVehicle;

        /// <summary>
        /// Is player currently summoning vehicle?
        /// </summary>
        bool IsSummmoningVehicle { get; }

        /// <summary>
        /// In milliseconds.
        /// </summary>
        int SummoningTime { get; set; }

        /// <summary>
        /// Stops summon timer.
        /// </summary>
        void CancelVehicleSummon();

        #endregion

        #region Vehicle

        /// <summary>
        /// Event, that is fired, when the player change vehicle status.
        /// </summary>
        event Action<int, bool> OnVehicleChange;

        /// <summary>
        /// Indicator if character is on mount now.
        /// </summary>
        bool IsOnVehicle { get; }

        /// <summary>
        /// Tries to summon vehicle(mount).
        /// </summary>
        /// <param name="skipSummoning">Indicates whether the summon casting time should be skipped or not.</param>
        /// <returns>true if ok</returns>
        bool CallVehicle(bool skipSummoning = false);

        /// <summary>
        /// Unmounts vehicle(mount).
        /// </summary>
        /// <returns>true if ok</returns>
        bool RemoveVehicle();

        #endregion

        #region Passenger

        /// <summary>
        /// Id of player, that is vehicle owner (2 places mount).
        /// </summary>
        int Vehicle2CharacterID { get; set; }

        /// <summary>
        /// Event, that is fired, when 2 vehicle character changes.
        /// </summary>
        event Action<int, int> OnVehiclePassengerChanged;

        /// <summary>
        /// Id of player, who has sent vehicle request.
        /// </summary>
        int VehicleRequesterID { get; set; }

        #endregion
    }
}
