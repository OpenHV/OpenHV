#region Copyright & License Information
/*
 * Copyright 2021 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Collections.Generic;
using System.Linq;
using OpenRA.GameRules;
using OpenRA.Graphics;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Effects
{
	public class Railgun : IProjectile
	{
		readonly Target target;
		readonly Player firedBy;
		readonly WeaponInfo weapon;

		int weaponDelay;
		bool impacted = false;

		public Railgun(Player firedBy, WeaponInfo weapon, World world, WPos launchPos, in Target target, int delay)
		{
			this.target = target;
			this.firedBy = firedBy;
			this.weapon = weapon;
			weaponDelay = delay;

			if (weapon.Report != null && weapon.Report.Any())
				Game.Sound.Play(SoundType.World, weapon.Report, world, launchPos);
		}

		public void Tick(World world)
		{
			if (!impacted && weaponDelay-- <= 0)
			{
				var warheadArgs = new WarheadArgs
				{
					Weapon = weapon,
					Source = target.CenterPosition,
					SourceActor = firedBy.PlayerActor,
					WeaponTarget = target
				};

				weapon.Impact(target, warheadArgs);
				impacted = true;

				world.AddFrameEndTask(w => w.Remove(this));
			}
		}

		public IEnumerable<IRenderable> Render(WorldRenderer wr)
		{
			return Enumerable.Empty<IRenderable>();
		}
	}
}
