#region Copyright & License Information
/*
 * Copyright 2021-2025 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Linq;
using OpenRA.GameRules;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Warheads;
using OpenRA.Mods.HV.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Warheads
{
	[Desc("A simplified " + nameof(SpreadDamageWarhead) + " that interacts with " + nameof(ForestLayer))]
	public class TreeDamageWarhead : DamageWarhead, IRulesetLoaded<WeaponInfo>
	{
		[Desc("Range between falloff steps.")]
		public readonly WDist Spread = new(43);

		[Desc("Damage percentage at each range step")]
		public readonly int[] Falloff = [100, 37, 14, 5, 0];

		[Desc("Ranges at which each Falloff step is defined. Overrides Spread.")]
		public WDist[] Range = null;

		[Desc("How much damage to apply to wood armor.")]
		public int Percentage = 100;

		void IRulesetLoaded<WeaponInfo>.RulesetLoaded(Ruleset rules, WeaponInfo info)
		{
			if (Range != null)
			{
				if (Range.Length != 1 && Range.Length != Falloff.Length)
					throw new YamlException("Number of range values must be 1 or equal to the number of Falloff values.");

				for (var i = 0; i < Range.Length - 1; i++)
					if (Range[i] > Range[i + 1])
						throw new YamlException("Range values must be specified in an increasing order.");
			}
			else
				Range = Exts.MakeArray(Falloff.Length, i => i * Spread);
		}

		protected override void DoImpact(WPos pos, Actor firedBy, WarheadArgs args)
		{
			var layer = firedBy.World.WorldActor.Trait<ForestLayer>();
			var minRange = WDist.Zero.Length / 1024;
			var maxRange = Range[^1].Length / 1024;
			var impactCell = firedBy.World.Map.CellContaining(pos);
			foreach (var cell in firedBy.World.Map.FindTilesInAnnulus(impactCell, minRange, maxRange))
			{
				var falloffDistance = (firedBy.World.Map.CenterOfCell(cell) - pos).Length;

				// The range to target is more than the range the warhead covers,
				// so GetDamageFalloff() is going to give us 0 and we're going to do 0 damage anyway, so bail early.
				if (falloffDistance > Range[^1].Length)
					continue;

				var localModifiers = args.DamageModifiers.Append(GetDamageFalloff(falloffDistance)).ToArray();
				var damage = Util.ApplyPercentageModifiers(Damage, localModifiers.Append(Percentage));
				layer.Hit(cell, damage);
			}
		}

		int GetDamageFalloff(int distance)
		{
			var inner = Range[0].Length;
			for (var i = 1; i < Range.Length; i++)
			{
				var outer = Range[i].Length;
				if (outer > distance)
					return int2.Lerp(Falloff[i - 1], Falloff[i], distance - inner, outer - inner);

				inner = outer;
			}

			return 0;
		}
	}
}
