#region Copyright & License Information
/*
 * Copyright 2019-2020 The OpenHV Developers (see CREDITS)
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
	public class FireRadiusWarhead : WarheadHV, IRulesetLoaded<WeaponInfo>
	{
		[WeaponReference]
		[FieldLoader.Require]
		[Desc("Has to be defined in weapons.yaml as well.")]
		public readonly string Weapon = null;

		[Desc("Amount of weapons fired.")]
		public readonly int[] Amount = { 1 };

		[Desc("Should the weapons be fired around the intended target or at the explosion's epicenter.")]
		public readonly bool AroundTarget = false;

		WeaponInfo weapon;

		public void RulesetLoaded(Ruleset rules, WeaponInfo info)
		{
			if (!rules.Weapons.TryGetValue(Weapon.ToLowerInvariant(), out weapon))
				throw new YamlException("Weapons Ruleset does not contain an entry '{0}'".F(Weapon.ToLowerInvariant()));
		}

		public override void DoImpact(Target target, WarheadArgs args)
		{
			var firedBy = args.SourceActor;
			if (!target.IsValidFor(firedBy))
				return;

			var world = firedBy.World;
			var map = world.Map;

			if (target.Type == TargetType.Invalid)
				return;

			var pos = target.CenterPosition;
			var actorAtImpact = ImpactActors ? ActorTypeAtImpact(world, pos, firedBy) : ImpactActorType.None;

			// If there's either a) an invalid actor, or b) no actor and invalid terrain, we don't trigger the effect(s).
			if (actorAtImpact == ImpactActorType.Invalid)
				return;
			else if (actorAtImpact == ImpactActorType.None && !IsValidAgainstTerrain(world, pos))
				return;

			var epicenter = AroundTarget && args.WeaponTarget.Type != TargetType.Invalid
				? args.WeaponTarget.CenterPosition
				: target.CenterPosition;

			var amount = Amount.Length == 2
					? world.SharedRandom.Next(Amount[0], Amount[1])
					: Amount[0];

			var offset = 256 / amount;

			for (var i = 0; i < amount; i++)
			{
				Target radiusTarget = Target.Invalid;

				var rotation = WRot.FromFacing(i * offset);
				var targetpos = epicenter + new WVec(weapon.Range.Length, 0, 0).Rotate(rotation);
				var tpos = Target.FromPos(new WPos(targetpos.X, targetpos.Y, map.CenterOfCell(map.CellContaining(targetpos)).Z));
				if (weapon.IsValidAgainst(tpos, firedBy.World, firedBy))
					radiusTarget = tpos;

				if (radiusTarget.Type == TargetType.Invalid)
					continue;

				var projectileArgs = new ProjectileArgs
				{
					Weapon = weapon,
					Facing = (radiusTarget.CenterPosition - target.CenterPosition).Yaw,
					CurrentMuzzleFacing = () => (radiusTarget.CenterPosition - target.CenterPosition).Yaw,

					DamageModifiers = args.DamageModifiers,

					InaccuracyModifiers = !firedBy.IsDead ? firedBy.TraitsImplementing<IInaccuracyModifier>()
						.Select(a => a.GetInaccuracyModifier()).ToArray() : new int[0],

					RangeModifiers = !firedBy.IsDead ? firedBy.TraitsImplementing<IRangeModifier>()
						.Select(a => a.GetRangeModifier()).ToArray() : new int[0],

					Source = target.CenterPosition,
					CurrentSource = () => target.CenterPosition,
					SourceActor = firedBy,
					GuidedTarget = radiusTarget,
					PassiveTarget = radiusTarget.CenterPosition
				};

				if (projectileArgs.Weapon.Projectile != null)
				{
					var projectile = projectileArgs.Weapon.Projectile.Create(projectileArgs);
					if (projectile != null)
						firedBy.World.AddFrameEndTask(w => w.Add(projectile));

					if (projectileArgs.Weapon.Report != null && projectileArgs.Weapon.Report.Any())
						Game.Sound.Play(SoundType.World, projectileArgs.Weapon.Report.Random(firedBy.World.SharedRandom), target.CenterPosition);
				}
			}
		}
	}
}
