﻿using Imgeneus.World.Game.Player;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Imgeneus.World.Game.Speed
{
    public class SpeedManager : ISpeedManager
    {
        private readonly ILogger<SpeedManager> _logger;

        protected int _ownerId;

        public SpeedManager(ILogger<SpeedManager> logger)
        {
            _logger = logger;
#if DEBUG
            _logger.LogDebug("SpeedManager {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~SpeedManager()
        {
            _logger.LogDebug("SpeedManager {hashcode} collected by GC", GetHashCode());
        }
#endif

        #region Init

        public void Init(int ownerId)
        {
            _ownerId = ownerId;
        }

        #endregion

        #region Attack speed

        public Dictionary<byte, byte> WeaponSpeedPassiveSkillModificator { get; init; } = new Dictionary<byte, byte>();

        private int _constAttackSpeed = 0;
        public int ConstAttackSpeed { get => _constAttackSpeed; set { _constAttackSpeed = value; RaiseMoveAndAttackSpeed(); } }

        private int _extraAttackSpeed;
        public int ExtraAttackSpeed { get => _extraAttackSpeed; set { _extraAttackSpeed = value; RaiseMoveAndAttackSpeed(); } }

        public AttackSpeed TotalAttackSpeed
        {
            get
            {

                if (ConstAttackSpeed == 0)
                    return AttackSpeed.None;

                var finalSpeed = ConstAttackSpeed + ExtraAttackSpeed;

                if (finalSpeed < 0)
                    return AttackSpeed.ExteremelySlow;

                if (finalSpeed > 9)
                    return AttackSpeed.ExteremelyFast;

                return (AttackSpeed)finalSpeed;
            }
        }

        #endregion

        #region Move speed

        private int _constMoveSpeed = 2; // 2 == normal by default.
        public int ConstMoveSpeed { get => _constMoveSpeed; set { _constMoveSpeed = value; RaiseMoveAndAttackSpeed(); } }

        private int _extraMoveSpeed;
        public int ExtraMoveSpeed { get => _extraMoveSpeed; set { _extraMoveSpeed = value; RaiseMoveAndAttackSpeed(); } }

        private bool _immobilize;
        public bool Immobilize { get => _immobilize; set { _immobilize = value; RaiseMoveAndAttackSpeed(); } }

        public MoveSpeed TotalMoveSpeed
        {
            get
            {
                if (Immobilize)
                    return MoveSpeed.CanNotMove;

                var finalSpeed = ConstMoveSpeed + ExtraMoveSpeed;

                if (finalSpeed < 0)
                    return MoveSpeed.VerySlow;

                if (finalSpeed > 4)
                    return MoveSpeed.VeryFast;

                return (MoveSpeed)finalSpeed;
            }
        }

        #endregion

        #region Evenets

        public event Action<int, AttackSpeed, MoveSpeed> OnAttackOrMoveChanged;
        public event Action<byte, byte, bool> OnPassiveModificatorChanged;

        public void RaiseMoveAndAttackSpeed()
        {
            OnAttackOrMoveChanged?.Invoke(_ownerId, TotalAttackSpeed, TotalMoveSpeed);
        }

        public void RaisePassiveModificatorChanged(byte weaponType, byte passiveSkillModifier, bool shouldAdd)
        {
            OnPassiveModificatorChanged?.Invoke(weaponType, passiveSkillModifier, shouldAdd);
        }

        #endregion
    }
}
