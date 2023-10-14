#region Copyright & License Information
/*
 * Copyright 2023 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Collections.Generic;
using System.Linq;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Plays a sound at actor location when it is being captured.")]
	sealed class CapturableProgressBeepInfo : ConditionalTraitInfo, Requires<CapturableInfo>
	{
		[Desc("Number of ticks to wait between beeps.")]
		public readonly int Interval = 50;

		[FieldLoader.Require]
		[Desc("Sounds to play.")]
		public readonly string Sound = "";

		public override object Create(ActorInitializer init) { return new CapturableProgressBeep(this); }
	}

	sealed class CapturableProgressBeep : ConditionalTrait<CapturableProgressBeepInfo>, ITick, ICaptureProgressWatcher
	{
		readonly List<Player> captorOwners = new();
		readonly HashSet<Actor> captors = new();
		int tick = 0;

		public CapturableProgressBeep(CapturableProgressBeepInfo info)
			: base(info) { }

		void ICaptureProgressWatcher.Update(Actor self, Actor captor, Actor target, int current, int total)
		{
			if (self != target)
				return;

			if (total == 0)
			{
				captors.Remove(captor);
				if (!captors.Any(c => c.Owner == captor.Owner))
					captorOwners.Remove(captor.Owner);
			}
			else if (captors.Add(captor) && !captorOwners.Contains(captor.Owner))
				captorOwners.Add(captor.Owner);
		}

		void ITick.Tick(Actor self)
		{
			if (IsTraitDisabled)
				return;

			if (captorOwners.Count == 0)
			{
				tick = 0;
				return;
			}

			if (tick / 4 < captorOwners.Count && tick % 4 == 0)
				Game.Sound.Play(SoundType.World, Info.Sound, self.CenterPosition);

			if (++tick >= Info.Interval)
				tick = 0;
		}
	}
}
