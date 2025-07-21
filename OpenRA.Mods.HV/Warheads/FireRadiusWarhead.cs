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

using System.Linq;
using OpenRA.GameRules;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Warheads
{
	[Desc("Fires a defined amount of weapons with their maximum range in a wave pattern.")]
	public class FireRadiusWarhead : ValidateTriggerWarhead, IRulesetLoaded<WeaponInfo>
	{
		[WeaponReference]
		[FieldLoader.Require]
		[Desc("Has to be defined in weapons.yaml as well.")]
		public readonly string Weapon = null;

		[Desc("Amount of weapons fired.")]
		public readonly int[] Amount = [1];

		[Desc("Should the weapons be fired around the intended target or at the explosion's epicenter.")]
		public readonly bool AroundTarget = false;

		[Desc("Should the weapons be fired towards the same altitude of the original explosion.")]
		public readonly bool TransferAltitude = false;

		WeaponInfo weapon;

		public void RulesetLoaded(Ruleset rules, WeaponInfo info)
		{
			if (!rules.Weapons.TryGetValue(Weapon.ToLowerInvariant(), out weapon))
				throw new YamlException($"Weapons Ruleset does not contain an entry '{Weapon.ToLowerInvariant()}'");
		}

		public override void DoImpact(in Target target, WarheadArgs args)
		{
			if (!ValidateTrigger(target, args))
				return;

			var firedBy = args.SourceActor;
			var world = firedBy.World;
			var map = world.Map;
			var targetPosition = target.CenterPosition;

			var epicenter = AroundTarget && args.WeaponTarget.Type != TargetType.Invalid
				? args.WeaponTarget.CenterPosition
				: targetPosition;

			var amount = Common.Util.RandomInRange(world.SharedRandom, Amount);

			var offset = 256 / amount;

			for (var i = 0; i < amount; i++)
			{
				var rotation = WRot.FromFacing(i * offset);
				var radiusTargetPosition = epicenter + new WVec(weapon.Range.Length, 0, 0).Rotate(rotation);

				// Allow maintaining Z offset in chained-airburst/rangelimit situations.
				var radiusTargetAltitude = TransferAltitude
					? map.CenterOfCell(map.CellContaining(radiusTargetPosition)).Z + map.DistanceAboveTerrain(targetPosition).Length
					: map.CenterOfCell(map.CellContaining(radiusTargetPosition)).Z;

				var radiusTarget = Target.FromPos(new WPos(radiusTargetPosition.X, radiusTargetPosition.Y, radiusTargetAltitude));

				if (!weapon.IsValidAgainst(radiusTarget, firedBy.World, firedBy))
					continue;

				var projectileArgs = new ProjectileArgs
				{
					Weapon = weapon,
					Facing = (radiusTarget.CenterPosition - targetPosition).Yaw,
					CurrentMuzzleFacing = () => (radiusTarget.CenterPosition - targetPosition).Yaw,

					DamageModifiers = args.DamageModifiers,

					InaccuracyModifiers = !firedBy.IsDead ? firedBy.TraitsImplementing<IInaccuracyModifier>()
						.Select(a => a.GetInaccuracyModifier()).ToArray() : [],

					RangeModifiers = !firedBy.IsDead ? firedBy.TraitsImplementing<IRangeModifier>()
						.Select(a => a.GetRangeModifier()).ToArray() : [],

					Source = targetPosition,
					CurrentSource = () => targetPosition,
					SourceActor = firedBy,
					GuidedTarget = radiusTarget,
					PassiveTarget = radiusTarget.CenterPosition
				};

				if (projectileArgs.Weapon.Projectile != null)
				{
					var projectile = projectileArgs.Weapon.Projectile.Create(projectileArgs);
					if (projectile != null)
						firedBy.World.AddFrameEndTask(w => w.Add(projectile));

					if (projectileArgs.Weapon.Report != null && projectileArgs.Weapon.Report.Length > 0)
						Game.Sound.Play(SoundType.World, projectileArgs.Weapon.Report.Random(firedBy.World.SharedRandom), target.CenterPosition);
				}
			}
		}
	}
}
