﻿using Imgeneus.Database.Constants;
using Imgeneus.World.Game.Buffs;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Elements;
using Imgeneus.World.Game.Levelling;
using Imgeneus.World.Game.Monster;
using Imgeneus.World.Game.Player;
using Imgeneus.World.Game.Skills;
using Imgeneus.World.Game.Speed;
using Imgeneus.World.Game.Stats;
using Imgeneus.World.Game.Stealth;
using Microsoft.Extensions.Logging;
using System;

namespace Imgeneus.World.Game.Attack
{
    public class AttackManager : IAttackManager
    {
        private readonly ILogger<AttackManager> _logger;
        private readonly IStatsManager _statsManager;
        private readonly ILevelProvider _levelProvider;
        private readonly IElementProvider _elementManager;
        private readonly ICountryProvider _countryProvider;
        private readonly ISpeedManager _speedManager;
        private readonly IStealthManager _stealthManager;
        private int _ownerId;

        public AttackManager(ILogger<AttackManager> logger, IStatsManager statsManager, ILevelProvider levelProvider, IElementProvider elementManager, ICountryProvider countryProvider, ISpeedManager speedManager, IStealthManager stealthManager)
        {
            _logger = logger;
            _statsManager = statsManager;
            _levelProvider = levelProvider;
            _elementManager = elementManager;
            _countryProvider = countryProvider;
            _speedManager = speedManager;
            _stealthManager = stealthManager;

#if DEBUG
            _logger.LogDebug("AttackManager {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~AttackManager()
        {
            _logger.LogDebug("AttackManager {hashcode} collected by GC", GetHashCode());
        }
#endif

        #region Init

        public void Init(int ownerId)
        {
            _ownerId = ownerId;
        }

        #endregion

        #region Target

        public event Action<IKillable> OnTargetChanged;

        private IKillable _target;
        public IKillable Target
        {
            get => _target;
            set
            {
                if (_target != null)
                {
                    _target.BuffsManager.OnBuffAdded -= Target_OnBuffAdded;
                    _target.BuffsManager.OnBuffRemoved -= Target_OnBuffRemoved;
                }

                _target = value;

                if (_target != null)
                {
                    _target.BuffsManager.OnBuffAdded += Target_OnBuffAdded;
                    _target.BuffsManager.OnBuffRemoved += Target_OnBuffRemoved;
                }

                OnTargetChanged?.Invoke(_target);
            }
        }

        public event Action<IKillable, Buff> TargetOnBuffAdded;

        private void Target_OnBuffAdded(int senderId, Buff buff)
        {
            TargetOnBuffAdded?.Invoke(Target, buff);
        }

        public event Action<IKillable, Buff> TargetOnBuffRemoved;
        private void Target_OnBuffRemoved(int senderId, Buff buff)
        {
            TargetOnBuffRemoved?.Invoke(Target, buff);
        }

        #endregion

        /// <summary>
        /// I'm not sure how exactly in original server next attack time was implemented.
        /// For now, I'm implementing it as usual date time and increase it based on attack speed and casting time.
        /// </summary>
        private DateTime _nextAttackTime;

        private int NextAttackTime
        {
            get
            {
                switch (_speedManager.TotalAttackSpeed)
                {
                    case AttackSpeed.ExteremelySlow:
                        return 4000;

                    case AttackSpeed.VerySlow:
                        return 3750;

                    case AttackSpeed.Slow:
                        return 3500;

                    case AttackSpeed.ABitSlow:
                        return 3250;

                    case AttackSpeed.Normal:
                        return 3000;

                    case AttackSpeed.ABitFast:
                        return 2750;

                    case AttackSpeed.Fast:
                        return 2500;

                    case AttackSpeed.VeryFast:
                        return 2250;

                    case AttackSpeed.ExteremelyFast:
                        return 2000;

                    default:
                        return 2000;
                }
            }
        }

        public void StartAttack()
        {
            _nextAttackTime = DateTime.UtcNow.AddMilliseconds(NextAttackTime);
            OnStartAttack?.Invoke();
        }

        public event Action OnStartAttack;

        public event Action<int, IKillable, AttackResult> OnAttack;

        public bool IsWeaponAvailable { get; set; } = true;

        public byte WeaponType { get; set; }

        public bool IsShieldAvailable { get; set; } = true;

        public bool IsAbleToAttack { get; set; } = true;

        public bool CanAttack(byte skillNumber, IKillable target, out AttackSuccess success)
        {
            if (!IsWeaponAvailable)
            {
                success = AttackSuccess.WrongEquipment;
                return false;
            }

            if (skillNumber == IAttackManager.AUTO_ATTACK_NUMBER && DateTime.UtcNow < _nextAttackTime)
            {
                // TODO: send not enough elapsed time?
                //_logger.Log(LogLevel.Debug, "Too fast attack.");
                success = AttackSuccess.CanNotAttack;
                return false;
            }

            if (DateTime.UtcNow < _nextAttackTime)
            {
                success = AttackSuccess.CooldownNotOver;
                return false;
            }

            if (target is null || target.HealthManager.IsDead || !target.HealthManager.IsAttackable)
            {
                success = AttackSuccess.WrongTarget;
                return false;
            }

            if (skillNumber == IAttackManager.AUTO_ATTACK_NUMBER && Target.CountryProvider.Country == _countryProvider.Country && (Target is Character && ((Character)Target).DuelManager.OpponentId != _ownerId))
            {
                success = AttackSuccess.WrongTarget;
                return false;
            }

            if (skillNumber == IAttackManager.AUTO_ATTACK_NUMBER && !IsAbleToAttack)
            {
                success = AttackSuccess.CanNotAttack;
                return false;
            }

            if (skillNumber != IAttackManager.AUTO_ATTACK_NUMBER && skillNumber != ISkillsManager.ITEM_SKILL_NUMBER)
            {
                success = AttackSuccess.Normal;
                return true;
            }

            success = AttackSuccess.Normal;
            return true;
        }

        /// <summary>
        /// Usual physical attack, "auto attack".
        /// </summary>
        public void AutoAttack(IKiller sender)
        {
            StartAttack();

            AttackResult result;
            if (!AttackSuccessRate(Target, TypeAttack.PhysicalAttack))
            {
                result = new AttackResult(AttackSuccess.Miss, new Damage());
                OnAttack?.Invoke(_ownerId, Target, result);
                return;
            }

            result = CalculateDamage(Target, TypeAttack.PhysicalAttack,
                                             _elementManager.AttackElement,
                                             _statsManager.MinAttack,
                                             _statsManager.MaxAttack,
                                             _statsManager.MinMagicAttack,
                                             _statsManager.MaxMagicAttack);

            OnAttack?.Invoke(_ownerId, Target, result); // Event should go first, otherwise AI manager will clear target and it will be null.
            Target.HealthManager.DecreaseHP(result.Damage.HP, sender);

            // In AI manager, if target is killed, it's cleared via setting to null.
            // That's why after decreasing HP we must check if target is still presented, otherwise null exception is thrown.
            if (Target != null)
            {
                Target.HealthManager.CurrentSP -= result.Damage.SP;
                Target.HealthManager.CurrentMP -= result.Damage.MP;
            }
        }

        public bool AttackSuccessRate(IKillable target, TypeAttack typeAttack, Skill skill = null)
        {
            // Uncomment this code, if you want to always hit target.
            // return true;
            if (target.IsUntouchable)
                return false;

            if (skill != null && (skill.StateType == StateType.FlatDamage || skill.StateType == StateType.DeathTouch))
                return true;

            if (skill != null && skill.UseSuccessValue)
                return new Random().Next(1, 101) < skill.SuccessValue;


            double levelDifference;
            double result;

            // Starting from here there might be not clear code.
            // This code is not my invention, it's raw implementation of ep 4 calculations.
            // You're free to change it to whatever you think fits better your server.
            switch (typeAttack)
            {
                case TypeAttack.PhysicalAttack:
                case TypeAttack.ShootingAttack:
                    levelDifference = _levelProvider.Level * 1.0 / (target.LevelProvider.Level + _levelProvider.Level);
                    var targetAttackPercent = target.StatsManager.PhysicalHittingChance / (target.StatsManager.PhysicalHittingChance + _statsManager.PhysicalEvasionChance);
                    var myAttackPercent = _statsManager.PhysicalHittingChance / (_statsManager.PhysicalHittingChance + target.StatsManager.PhysicalEvasionChance);
                    var attackPercent = targetAttackPercent * 100 - myAttackPercent * 100;
                    result = levelDifference * 160 - attackPercent;
                    if (result >= 20)
                    {
                        if (result > 99)
                            result = 99;
                    }
                    else
                    {
                        if (target is Mob)
                            result = 20;
                        else
                            result = 1;
                    }

                    return new Random().Next(1, 101) < result;

                case TypeAttack.MagicAttack:
                    levelDifference = ((target.LevelProvider.Level - _levelProvider.Level - 2) * 100 + target.LevelProvider.Level) / (target.LevelProvider.Level + _levelProvider.Level) * 1.1;
                    var fxDef = levelDifference + target.StatsManager.MagicEvasionChance;
                    if (fxDef >= 1)
                    {
                        if (fxDef > 70)
                            fxDef = 70;
                    }
                    else
                    {
                        fxDef = 1;
                    }

                    var wisDifference = (11 * target.StatsManager.TotalWis - 10 * _statsManager.TotalWis) / (target.StatsManager.TotalWis + _statsManager.TotalWis) * 3.9000001;
                    var nAttackTypea = wisDifference + _statsManager.MagicHittingChance;
                    if (nAttackTypea >= 1)
                    {
                        if (nAttackTypea > 70)
                            nAttackTypea = 70;
                    }
                    else
                    {
                        nAttackTypea = 1;
                    }

                    result = nAttackTypea + fxDef;
                    if (result >= 1)
                    {
                        if (result > 90)
                            result = 90;
                    }
                    else
                    {
                        result = 1;
                    }
                    return new Random().Next(1, 101) < result;
            }
            return true;
        }

        public AttackResult CalculateAttackResult(Skill skill, IKillable target, Element element, int minAttack, int maxAttack, int minMagicAttack, int maxMagicAttack)
        {
            switch (skill.DamageType)
            {
                case DamageType.FixedDamage:
                    return new AttackResult(AttackSuccess.Normal, new Damage(skill.DamageHP, skill.DamageMP, skill.DamageSP));

                case DamageType.PlusExtraDamage:
                    return CalculateDamage(target,
                                           skill.TypeAttack,
                                           element,
                                           minAttack,
                                           maxAttack,
                                           minMagicAttack,
                                           maxMagicAttack,
                                           skill);

                default:
                    throw new NotImplementedException("Not implemented damage type.");
            }
        }

        public AttackResult CalculateDamage(
            IKillable target,
            TypeAttack typeAttack,
            Element attackElement,
            int minAttack,
            int maxAttack,
            int minMagicAttack,
            int maxMagicAttack,
            Skill skill = null)
        {
            double damage = 0;

            // First, calculate damage, that is made of stats, weapon and buffs.
            switch (typeAttack)
            {
                case TypeAttack.PhysicalAttack:
                    damage = new Random().Next(minAttack, maxAttack);
                    if (skill != null)
                    {
                        damage += skill.DamageHP;
                    }
                    damage -= target.StatsManager.TotalDefense;
                    if (damage < 0)
                        damage = 1;
                    damage = damage * 1.5;
                    break;

                case TypeAttack.ShootingAttack:
                    damage = new Random().Next(minAttack, maxAttack);
                    if (skill != null)
                    {
                        damage += skill.DamageHP;
                    }
                    damage -= target.StatsManager.TotalDefense;
                    if (damage < 0)
                        damage = 1;
                    // TODO: multiply by range to the target.
                    damage = damage * 1.5; // * 0.7 if target is too close.
                    break;

                case TypeAttack.MagicAttack:
                    damage = new Random().Next(minMagicAttack, maxMagicAttack);
                    if (skill != null)
                    {
                        damage += skill.DamageHP;
                    }
                    damage -= target.StatsManager.TotalResistance;
                    if (damage < 0)
                        damage = 1;
                    damage = damage * 1.5;
                    break;
            }

            // Second, add element calculation.
            Element element = skill != null && skill.Element != Element.None ? skill.Element : attackElement;
            var elementFactor = GetElementFactor(element, target.ElementProvider.DefenceElement);
            damage = damage * elementFactor;

            // Third, calculate if critical damage should be added.
            var criticalDamage = false;
            if (new Random().Next(1, 101) < CriticalSuccessRate(target))
            {
                criticalDamage = true;
                damage += Convert.ToInt32(_statsManager.TotalLuc * new Random().NextDouble() * 1.5);
            }

            if (damage > 30000)
                damage = 30000;

            // Forth, subtract absorption value;
            ushort absorb = 0;
            if (target.Absorption < damage)
            {
                damage -= target.Absorption;
                absorb = target.Absorption;
            }
            else
            {
                absorb = Convert.ToUInt16(damage);
                damage = 0;
            }

            if (criticalDamage)
                return new AttackResult(AttackSuccess.Critical, new Damage(Convert.ToUInt16(damage), 0, 0), absorb);
            else
                return new AttackResult(AttackSuccess.Normal, new Damage(Convert.ToUInt16(damage), 0, 0), absorb);
        }

        /// <summary>
        /// Calculates critical rate or possibility to make critical hit.
        /// Can be only more then 5 and less than 99.
        /// </summary>
        private int CriticalSuccessRate(IKillable target)
        {
            var result = Convert.ToInt32(_statsManager.CriticalHittingChance - (target.StatsManager.TotalLuc * 0.034000002));

            if (result < 5)
                result = 5;

            if (result > 99)
                result = 99;

            return result;
        }

        public double GetElementFactor(Element attackElement, Element defenceElement)
        {
            if (attackElement == defenceElement)
                return 1;

            if (attackElement != Element.None && defenceElement == Element.None)
            {
                if (attackElement == Element.Fire1 || attackElement == Element.Earth1 || attackElement == Element.Water1 || attackElement == Element.Wind1)
                    return 1.2;
                if (attackElement == Element.Fire2 || attackElement == Element.Earth2 || attackElement == Element.Water2 || attackElement == Element.Wind2)
                    return 1.3;
            }

            if (attackElement == Element.None && defenceElement != Element.None)
            {
                if (defenceElement == Element.Fire1 || defenceElement == Element.Earth1 || defenceElement == Element.Water1 || defenceElement == Element.Wind1)
                    return 0.8;
                if (defenceElement == Element.Fire2 || defenceElement == Element.Earth2 || defenceElement == Element.Water2 || defenceElement == Element.Wind2)
                    return 0.7;
            }

            if (attackElement == Element.Water1)
            {
                if (defenceElement == Element.Fire1)
                    return 1.4;
                if (defenceElement == Element.Fire2)
                    return 1.3;

                if (defenceElement == Element.Earth1)
                    return 0.5;
                if (defenceElement == Element.Earth2)
                    return 0.4;

                return 1; // wind or water
            }

            if (attackElement == Element.Fire1)
            {
                if (defenceElement == Element.Wind1)
                    return 1.4;
                if (defenceElement == Element.Wind2)
                    return 1.3;

                if (defenceElement == Element.Water1)
                    return 0.5;
                if (defenceElement == Element.Water2)
                    return 0.4;

                return 1; // earth or fire
            }

            if (attackElement == Element.Wind1)
            {
                if (defenceElement == Element.Earth1)
                    return 1.4;
                if (defenceElement == Element.Earth2)
                    return 1.3;

                if (defenceElement == Element.Fire1)
                    return 0.5;
                if (defenceElement == Element.Fire2)
                    return 0.4;

                return 1; // wind or water
            }

            if (attackElement == Element.Earth1)
            {
                if (defenceElement == Element.Water1)
                    return 1.4;
                if (defenceElement == Element.Water2)
                    return 1.3;

                if (defenceElement == Element.Wind1)
                    return 0.5;
                if (defenceElement == Element.Wind2)
                    return 0.4;

                return 1; // earth or fire
            }

            if (attackElement == Element.Water2)
            {
                if (defenceElement == Element.Fire1)
                    return 1.6;
                if (defenceElement == Element.Fire2)
                    return 1.4;

                if (defenceElement == Element.Earth1)
                    return 0.5;
                if (defenceElement == Element.Earth2)
                    return 0.5;

                return 1; // wind or water
            }

            if (attackElement == Element.Fire2)
            {
                if (defenceElement == Element.Wind1)
                    return 1.6;
                if (defenceElement == Element.Wind2)
                    return 1.4;

                if (defenceElement == Element.Water1)
                    return 0.5;
                if (defenceElement == Element.Water2)
                    return 0.5;

                return 1; // earth or fire
            }

            if (attackElement == Element.Wind2)
            {
                if (defenceElement == Element.Earth1)
                    return 1.6;
                if (defenceElement == Element.Earth2)
                    return 1.4;

                if (defenceElement == Element.Fire1)
                    return 0.5;
                if (defenceElement == Element.Fire2)
                    return 0.5;

                return 1; // wind or water
            }

            if (attackElement == Element.Earth2)
            {
                if (defenceElement == Element.Water1)
                    return 1.6;
                if (defenceElement == Element.Water2)
                    return 1.4;

                if (defenceElement == Element.Wind1)
                    return 0.5;
                if (defenceElement == Element.Wind2)
                    return 0.5;

                return 1; // earth or fire
            }

            return 1;
        }
    }
}
