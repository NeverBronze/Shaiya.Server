﻿using Imgeneus.Database.Constants;
using Imgeneus.World.Game.Buffs;
using Imgeneus.World.Game.Skills;
using System;

namespace Imgeneus.World.Game.Attack
{
    public interface IAttackManager
    {
        /// <summary>
        /// Number for autoattack.
        /// </summary>
        public const byte AUTO_ATTACK_NUMBER = 255;

        /// <summary>
        /// Inits attack manager.
        /// </summary>
        void Init(int ownerId);

        /// <summary>
        /// Updates the last date, when attack was called.
        /// </summary>
        void StartAttack();

        /// <summary>
        /// Event, that is fired when <see cref="Target"/> changes.
        /// </summary>
        event Action<IKillable> OnTargetChanged;

        /// <summary>
        /// Current enemy in target.
        /// </summary>
        IKillable Target { get; set; }

        event Action<IKillable, Buff> TargetOnBuffAdded;

        event Action<IKillable, Buff> TargetOnBuffRemoved;

        /// <summary>
        /// Set by inventory weapon.
        /// </summary>
        bool IsWeaponAvailable { get; set; }

        /// <summary>
        /// Set by inventory weapon.
        /// </summary>
        byte WeaponType { get; set; }

        /// <summary>
        /// Set by inventory shield.
        /// </summary>
        bool IsShieldAvailable { get; set; }

        /// <summary>
        /// Checks if it's possible to attack target. (or use skill)
        /// </summary>
        bool CanAttack(byte skillNumber, IKillable target, out AttackSuccess success);

        /// <summary>
        /// Usual physical attack, "auto attack".
        /// </summary>
        void AutoAttack(IKiller sender);

        /// <summary>
        /// Event before each attack.
        /// </summary>
        event Action OnStartAttack;

        /// <summary>
        /// Is it possible to make attack?
        /// </summary>
        bool IsAbleToAttack { get; set; }

        /// <summary>
        /// Event, that is fired, when melee attack.
        /// </summary>
        event Action<int, IKillable, AttackResult> OnAttack;

        /// <summary>
        /// Calculates attack result based on skill type and target.
        /// </summary>
        AttackResult CalculateAttackResult(Skill skill, IKillable target, Element element, int minAttack, int maxAttack, int minMagicAttack, int maxMagicAttack);

        /// <summary>
        /// Calculates damage based on player stats and target stats.
        /// </summary>
        AttackResult CalculateDamage(IKillable target, TypeAttack typeAttack, Element attackElement, int minAttack, int maxAttack, int minMagicAttack, int maxMagicAttack, Skill skill = null);

        /// <summary>
        /// The calculation of the attack success.
        /// </summary>
        /// <param name="target">target</param>
        /// <param name="typeAttack">type of attack</param>
        /// <param name="skill">skill if any</param>
        /// <returns>true if attack hits target, otherwise false</returns>
        bool AttackSuccessRate(IKillable target, TypeAttack typeAttack, Skill skill = null);

        /// <summary>
        /// Calculates element multiplier based on attack and defence elements.
        /// </summary>
        double GetElementFactor(Element attackElement, Element defenceElement);
    }
}
