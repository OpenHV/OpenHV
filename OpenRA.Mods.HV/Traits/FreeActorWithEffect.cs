#region Copyright & License Information
/*
 * Copyright 2019-2025 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Effects;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Player receives a unit for free once the building is placed with a sprite effect.",
		"If you want more than one unit to be spawned, copy this section and assign IDs like FreeActorWithDelay@2, ...")]
	public class FreeActorWithEffectInfo : FreeActorInfo
	{
		public readonly string Image = null;

		[SequenceReference(nameof(Image))]
		[Desc("Sequence to use for overlay animation.")]
		public readonly string Sequence = null;

		[PaletteReference]
		[Desc("Custom palette name")]
		public readonly string Palette = null;

		[Desc("Start after this duration in ticks.")]
		public readonly int Delay = 0;

		public override object Create(ActorInitializer init) { return new FreeActorWithEffect(init, this); }
	}

	public class FreeActorWithEffect : FreeActor, ITick, ISync
	{
		readonly FreeActorWithEffectInfo info;

		[VerifySync]
		int delay;

		public FreeActorWithEffect(ActorInitializer init, FreeActorWithEffectInfo info)
			: base(init, info)
		{
			this.info = info;
		}

		protected override void TraitEnabled(Actor self)
		{
			delay = info.Delay;
		}

		void ITick.Tick(Actor self)
		{
			if (!allowSpawn)
				return;

			if (--delay < 0)
			{
				SpawnActor(self);
				allowSpawn = info.AllowRespawn;
			}
		}

		void SpawnActor(Actor self)
		{
			var location = self.Location + Info.SpawnOffset;
			var position = self.World.Map.CenterOfCell(location);

			self.World.AddFrameEndTask(w => w.Add(new SpriteEffect(position, w, info.Image, info.Sequence, info.Palette)));
			self.World.AddFrameEndTask(w =>
			{
				w.CreateActor(Info.Actor,
				[
					new ParentActorInit(self),
					new LocationInit(location),
					new OwnerInit(self.Owner),
					new FacingInit(Info.Facing),
				]);
			});
		}
	}
}
