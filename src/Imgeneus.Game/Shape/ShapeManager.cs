﻿using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Stealth;
using Imgeneus.World.Game.Vehicle;
using Microsoft.Extensions.Logging;
using System;

namespace Imgeneus.World.Game.Shape
{
    public class ShapeManager : IShapeManager
    {
        private readonly ILogger<ShapeManager> _logger;
        private readonly IStealthManager _stealthManager;
        private readonly IVehicleManager _vehicleManager;
        private readonly IInventoryManager _inventoryManager;
        private int _ownerId;

        public ShapeManager(ILogger<ShapeManager> logger, IStealthManager stealthManager, IVehicleManager vehicleManager, IInventoryManager inventoryManager)
        {
            _logger = logger;
            _stealthManager = stealthManager;
            _vehicleManager = vehicleManager;
            _inventoryManager = inventoryManager;

            _stealthManager.OnStealthChange += StealthManager_OnStealthChange;
            _vehicleManager.OnVehicleChange += VehicleManager_OnVehicleChange;

#if DEBUG
            _logger.LogDebug("ShapeManager {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~ShapeManager()
        {
            _logger.LogDebug("ShapeManager {hashcode} collected by GC", GetHashCode());
        }
#endif
        #region Init & Clear

        public void Init(int ownerId)
        {
            _ownerId = ownerId;
        }

        public void Dispose()
        {
            _stealthManager.OnStealthChange -= StealthManager_OnStealthChange;
            _vehicleManager.OnVehicleChange -= VehicleManager_OnVehicleChange;
        }

        #endregion

        public event Action<int, ShapeEnum, int, int> OnShapeChange;

        public ShapeEnum Shape
        {
            get
            {
                if (_stealthManager.IsStealth)
                    return ShapeEnum.Stealth;

                if (_vehicleManager.IsOnVehicle)
                {
                    var value1 = (byte)_inventoryManager.Mount.Grow >= 2 ? 15 : 14;
                    var value2 = _inventoryManager.Mount.Range < 2 ? _inventoryManager.Mount.Range * 2 : _inventoryManager.Mount.Range + 7;
                    var mountType = value1 + value2;
                    return (ShapeEnum)mountType;
                }

                return ShapeEnum.None;
            }
        }

        private void StealthManager_OnStealthChange(int senderId)
        {
            OnShapeChange?.Invoke(_ownerId, Shape, 0, 0);
        }

        private void VehicleManager_OnVehicleChange(int senderId, bool isOnVehicle)
        {
            var param1 = _inventoryManager.Mount is null ? 0 : _inventoryManager.Mount.Type;
            var param2 = _inventoryManager.Mount is null ? 0 : _inventoryManager.Mount.TypeId;
            OnShapeChange?.Invoke(_ownerId, Shape, param1, param2);
        }
    }
}
