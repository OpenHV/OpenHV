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
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Warheads;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Warheads
{
	public class FireShrapnelWarhead : ImpactAirWarhead, IRulesetLoaded<WeaponInfo>
	{
		[WeaponReference]
		[FieldLoader.Require]
		[Desc("Has to be defined in weapons.yaml as well.")]
		public readonly string Weapon = null;

		[Desc("Amount of shrapnels thrown.")]
		public readonly int[] Amount = { 1 };

		[Desc("The percentage of aiming this shrapnel to a suitable target actor.")]
		public readonly int AimChance = 0;

		[Desc("What diplomatic stances can be targeted by the shrapnel.")]
		public readonly Stance AimTargetStances = Stance.Ally | Stance.Neutral | Stance.Enemy;

		[Desc("Allow this shrapnel to be thrown randomly when no targets found.")]
		public readonly bool ThrowWithoutTarget = true;

		[Desc("Should the shrapnel hit the direct target?")]
		public readonly bool AllowDirectHit = false;

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

			var directActors = world.FindActorsOnCircle(epicenter, WDist.Zero)
				.Where(a =>
				{
					var activeShapes = a.TraitsImplementing<HitShape>().Where(Exts.IsTraitEnabled);
					if (!activeShapes.Any())
						return false;

					var distance = activeShapes.Min(t => t.DistanceFromEdge(a, epicenter));

					if (distance != WDist.Zero)
						return false;

					return true;
				});

			var availableTargetActors = world.FindActorsOnCircle(epicenter, weapon.Range)
				.Where(x => (AllowDirectHit || !directActors.Contains(x))
					&& weapon.IsValidAgainst(Target.FromActor(x), firedBy.World, firedBy)
					&& AimTargetStances.HasStance(firedBy.Owner.Stances[x.Owner]))
				.Where(x =>
				{
					var activeShapes = x.TraitsImplementing<HitShape>().Where(Exts.IsTraitEnabled);
					if (!activeShapes.Any())
						return false;

					var distance = activeShapes.Min(t => t.DistanceFromEdge(x, epicenter));

					if (distance < weapon.Range)
						return true;

					return false;
				})
				.Shuffle(world.SharedRandom);

			var targetActor = availableTargetActors.GetEnumerator();

			var amount = Amount.Length == 2
					? world.SharedRandom.Next(Amount[0], Amount[1])
					: Amount[0];

			for (var i = 0; i < amount; i++)
			{
				var shrapnelTarget = Target.Invalid;

				if (world.SharedRandom.Next(100) < AimChance && targetActor.MoveNext())
					shrapnelTarget = Target.FromActor(targetActor.Current);

				if (ThrowWithoutTarget && shrapnelTarget.Type == TargetType.Invalid)
				{
					var rotation = WRot.FromFacing(world.SharedRandom.Next(256));
					var range = world.SharedRandom.Next(weapon.MinRange.Length, weapon.Range.Length);
					var targetpos = epicenter + new WVec(range, 0, 0).Rotate(rotation);
					var tpos = Target.FromPos(new WPos(targetpos.X, targetpos.Y, map.CenterOfCell(map.CellContaining(targetpos)).Z));
					if (weapon.IsValidAgainst(tpos, firedBy.World, firedBy))
						shrapnelTarget = tpos;
				}

				if (shrapnelTarget.Type == TargetType.Invalid)
					continue;

				var shrapnelFacing = (shrapnelTarget.CenterPosition - epicenter).Yaw;

				var projectileArgs = new ProjectileArgs
				{
					Weapon = weapon,
					Facing = shrapnelFacing,
					CurrentMuzzleFacing = () => shrapnelFacing,

					DamageModifiers = !firedBy.IsDead ? firedBy.TraitsImplementing<IFirepowerModifier>()
						.Select(a => a.GetFirepowerModifier()).ToArray() : new int[0],

					InaccuracyModifiers = !firedBy.IsDead ? firedBy.TraitsImplementing<IInaccuracyModifier>()
						.Select(a => a.GetInaccuracyModifier()).ToArray() : new int[0],

					RangeModifiers = !firedBy.IsDead ? firedBy.TraitsImplementing<IRangeModifier>()
						.Select(a => a.GetRangeModifier()).ToArray() : new int[0],

					Source = target.CenterPosition,
					CurrentSource = () => target.CenterPosition,
					SourceActor = firedBy,
					GuidedTarget = shrapnelTarget,
					PassiveTarget = shrapnelTarget.CenterPosition
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
