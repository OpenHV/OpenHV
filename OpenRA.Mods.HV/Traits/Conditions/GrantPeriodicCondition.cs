#region Copyright & License Information
/*
 * Copyright 2015-2024 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Periodically grants and revokes a condition.")]
	public class GrantPeriodicConditionInfo : PausableConditionalTraitInfo
	{
		[GrantedConditionReference]
		[FieldLoader.Require]
		[Desc("The condition to grant.")]
		public readonly string Condition = null;

		[Desc("The range of time (in ticks) with the condition being disabled. Two values imply a range.")]
		public readonly int[] CooldownDuration = { 1000 };

		[Desc("The range of time (in ticks) with the condition being enabled. Two values imply a range.")]
		public readonly int[] ActiveDuration = { 100 };

		public readonly bool StartsGranted = false;

		public readonly bool ShowSelectionBar = false;
		public readonly Color CooldownColor = Color.DarkRed;
		public readonly Color ActiveColor = Color.DarkMagenta;

		public override object Create(ActorInitializer init) { return new GrantPeriodicCondition(init, this); }
	}

	public class GrantPeriodicCondition : PausableConditionalTrait<GrantPeriodicConditionInfo>, ISelectionBar, ITick, ISync
	{
		readonly Actor self;
		readonly GrantPeriodicConditionInfo info;

		[Sync]
		int ticks;

		int cooldown, active;
		bool isSuspended;
		int token = Actor.InvalidConditionToken;

		bool IsEnabled { get { return token != Actor.InvalidConditionToken; } }

		public GrantPeriodicCondition(ActorInitializer init, GrantPeriodicConditionInfo info)
			: base(info)
		{
			self = init.Self;
			this.info = info;
		}

		void SetDefaultState()
		{
			if (info.StartsGranted)
			{
				ticks = Common.Util.RandomInRange(self.World.SharedRandom, info.ActiveDuration);
				active = ticks;
				if (info.StartsGranted != IsEnabled)
					EnableCondition();
			}
			else
			{
				ticks = Common.Util.RandomInRange(self.World.SharedRandom, info.CooldownDuration);
				cooldown = ticks;
				if (info.StartsGranted != IsEnabled)
					DisableCondition();
			}

			isSuspended = false;
		}

		protected override void Created(Actor self)
		{
			if (!IsTraitDisabled)
				SetDefaultState();

			base.Created(self);
		}

		void ITick.Tick(Actor self)
		{
			if (!IsTraitDisabled && !IsTraitPaused && --ticks < 0)
			{
				if (IsEnabled)
				{
					ticks = Common.Util.RandomInRange(self.World.SharedRandom, info.CooldownDuration);
					cooldown = ticks;
					DisableCondition();
				}
				else
				{
					ticks = Common.Util.RandomInRange(self.World.SharedRandom, info.ActiveDuration);
					active = ticks;
					EnableCondition();
				}
			}
		}

		protected override void TraitEnabled(Actor self)
		{
			SetDefaultState();
		}

		protected override void TraitDisabled(Actor self)
		{
			if (IsEnabled)
				DisableCondition();
		}

		protected override void TraitPaused(Actor self)
		{
			if (IsEnabled)
			{
				DisableCondition();
				isSuspended = true;
			}
		}

		protected override void TraitResumed(Actor self)
		{
			if (isSuspended)
			{
				EnableCondition();
				isSuspended = false;
			}
		}

		void EnableCondition()
		{
			if (token == Actor.InvalidConditionToken)
				token = self.GrantCondition(info.Condition);
		}

		void DisableCondition()
		{
			if (token != Actor.InvalidConditionToken)
				token = self.RevokeCondition(token);
		}

		float ISelectionBar.GetValue()
		{
			if (!info.ShowSelectionBar)
				return 0f;

			return IsEnabled
				? (float)(active - ticks) / active
				: (float)ticks / cooldown;
		}

		bool ISelectionBar.DisplayWhenEmpty { get { return info.ShowSelectionBar; } }

		Color ISelectionBar.GetColor() { return IsEnabled ? info.ActiveColor : info.CooldownColor; }
	}
}
