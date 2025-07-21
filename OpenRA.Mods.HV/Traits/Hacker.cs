#region Copyright & License Information
/*
 * Copyright 2024-2025 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("This actor can subvert other actors.")]
	public class HackerInfo : PausableConditionalTraitInfo, Requires<ArmamentInfo>, Requires<HealthInfo>
	{
		[Desc("Name of the armaments that grant this condition.")]
		public readonly HashSet<string> ArmamentNames = ["primary"];

		[Desc("Up to how many units can this unit control?",
			"Use 0 or negative numbers for infinite.")]
		public readonly int Capacity = 1;

		[Desc("If the capacity is reached, discard the oldest controlled unit and control the new one",
			"If false, controlling new units is forbidden after capacity is reached.")]
		public readonly bool DiscardOldest = true;

		[Desc("Condition to grant to self when controlling actors. Can stack up by the number of affected actors.")]
		[GrantedConditionReference]
		[FieldLoader.Require]
		public readonly string ControllingCondition = null;

		[Desc("The sound played when the unit is hacked.")]
		public readonly string[] Sounds = [];

		public override object Create(ActorInitializer init) { return new Hacker(this); }
	}

	public class Hacker : PausableConditionalTrait<HackerInfo>, INotifyAttack, INotifyKilled, INotifyActorDisposing
	{
		readonly List<Actor> victims = [];
		readonly Stack<int> controllingTokens = [];

		public IEnumerable<Actor> Victims => victims;

		public Hacker(HackerInfo info)
			: base(info) { }

		void StackControllingCondition(Actor self, string condition)
		{
			if (string.IsNullOrEmpty(condition))
				return;

			controllingTokens.Push(self.GrantCondition(condition));
		}

		void UnstackControllingCondition(Actor self, string condition)
		{
			if (string.IsNullOrEmpty(condition))
				return;

			self.RevokeCondition(controllingTokens.Pop());
		}

		public void DisconnectVictim(Actor self, Actor victim)
		{
			if (victims.Remove(victim))
				UnstackControllingCondition(self, Info.ControllingCondition);
		}

		void INotifyAttack.PreparingAttack(Actor self, in Target target, Armament a, Barrel barrel) { }

		void INotifyAttack.Attacking(Actor self, in Target target, Armament a, Barrel barrel)
		{
			if (IsTraitDisabled || IsTraitPaused)
				return;

			if (!Info.ArmamentNames.Contains(a.Info.Name))
				return;

			if (target.Actor == null || !target.IsValidFor(self))
				return;

			if (self.Owner.RelationshipWith(target.Actor.Owner) == PlayerRelationship.Ally)
				return;

			var hackable = target.Actor.TraitOrDefault<Hackable>();

			if (hackable == null)
				throw new InvalidOperationException(
					$"`{self.Info.Name}` tried to hack `{target.Actor.Info.Name}`, but the latter does not have the necessary trait!");

			if (hackable.IsTraitDisabled || hackable.IsTraitPaused)
				return;

			if (Info.Capacity > 0 && !Info.DiscardOldest && victims.Count >= Info.Capacity)
				return;

			victims.Add(target.Actor);
			StackControllingCondition(self, Info.ControllingCondition);
			hackable.LinkAttacker(target.Actor, self);

			if (Info.Sounds.Length > 0)
				Game.Sound.Play(SoundType.World, Info.Sounds.Random(self.World.SharedRandom), self.CenterPosition);

			if (Info.Capacity > 0 && Info.DiscardOldest && victims.Count > Info.Capacity)
				victims[0].Trait<Hackable>().RevokeHack(victims[0]);
		}

		void DisconnectVictims(Actor self)
		{
			foreach (var victim in victims)
			{
				if (victim.IsDead || victim.Disposed)
					continue;

				victim.Trait<Hackable>().RevokeHack(victim);
			}

			victims.Clear();
			while (controllingTokens.Count > 0)
				UnstackControllingCondition(self, Info.ControllingCondition);
		}

		void INotifyKilled.Killed(Actor self, AttackInfo e)
		{
			DisconnectVictims(self);
		}

		void INotifyActorDisposing.Disposing(Actor self)
		{
			DisconnectVictims(self);
		}

		protected override void TraitDisabled(Actor self)
		{
			DisconnectVictims(self);
		}
	}
}
