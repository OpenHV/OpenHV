#region Copyright & License Information
/*
 * Copyright 2024 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.HV.Activities;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Can be teleported via " + nameof(TeleportPower))]
	public class TeleportableInfo : ConditionalTraitInfo
	{
		public readonly string TeleportSound = null;

		public override void RulesetLoaded(Ruleset rules, ActorInfo ai)
		{
			if (!ai.HasTraitInfo<MobileInfo>() && !ai.HasTraitInfo<HuskInfo>())
				throw new YamlException(nameof(Teleportable) + " requires actors to have the " + nameof(Mobile) + " or " + nameof(Husk) + " traits.");
		}

		public override object Create(ActorInitializer init) { return new Teleportable(init, this); }
	}

	public class Teleportable : ConditionalTrait<TeleportableInfo>
	{
		IPositionable positionable;

		public Teleportable(ActorInitializer init, TeleportableInfo info)
			: base(info) { }

		protected override void Created(Actor self)
		{
			positionable = self.OccupiesSpace as IPositionable;
			base.Created(self);
		}

		public virtual bool CanTeleportTo(Actor self, CPos targetLocation)
		{
			// TODO: Allow enemy units to be teleported into bad terrain to kill them
			return !IsTraitDisabled && positionable != null && positionable.CanEnterCell(targetLocation);
		}

		public virtual bool Teleport(Actor self, CPos targetLocation, Actor teleporter)
		{
			if (IsTraitDisabled)
				return false;

			self.QueueActivity(false, new Teleport(teleporter, targetLocation, null, true, Info.TeleportSound));
			return true;
		}
	}
}
