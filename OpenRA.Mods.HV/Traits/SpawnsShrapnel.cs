#region Copyright & License Information
/*
 * Copyright 2019-2022 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Linq;
using OpenRA.GameRules;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Spawns shrapnel weapons after a periodic interval.")]
	public class SpawnsShrapnelInfo : PausableConditionalTraitInfo
	{
		[WeaponReference]
		[FieldLoader.Require]
		[Desc("Has to be defined in weapons.yaml as well.")]
		public readonly string Weapon = null;

		[Desc("Amount of shrapnels thrown. Two values indicate a range.")]
		public readonly int[] Amount = { 1 };

		[Desc("Delay between two spawns. Two values indicate a range.")]
		public readonly int[] Delay = { 50 };

		[Desc("The percentage of aiming this shrapnel to a suitable target actor.")]
		public readonly int AimChance = 0;

		[Desc("What diplomatic stances can be targeted by the shrapnel.")]
		public readonly PlayerRelationship AimTargetStances = PlayerRelationship.Ally | PlayerRelationship.Neutral | PlayerRelationship.Enemy;

		[Desc("Allow this shrapnel to be thrown randomly when no targets found.")]
		public readonly bool ThrowWithoutTarget = true;

		[Desc("Should the shrapnel hit the spawner actor?")]
		public readonly bool AllowSelfHit = false;

		[Desc("Shrapnel spawn offset relative to actor's position.")]
		public readonly WVec LocalOffset = WVec.Zero;

		public WeaponInfo WeaponInfo { get; private set; }

		public override object Create(ActorInitializer init) { return new SpawnsShrapnel(init.Self, this); }

		public override void RulesetLoaded(Ruleset rules, ActorInfo ai)
		{
			base.RulesetLoaded(rules, ai);

			var weaponToLower = Weapon.ToLowerInvariant();
			if (!rules.Weapons.TryGetValue(weaponToLower, out WeaponInfo weaponInfo))
				throw new YamlException("Weapons Ruleset does not contain an entry '{0}'".F(weaponToLower));

			WeaponInfo = weaponInfo;
		}
	}

	class SpawnsShrapnel : PausableConditionalTrait<SpawnsShrapnelInfo>, ITick, ISync
	{
		readonly World world;
		readonly BodyOrientation body;

		[Sync]
		int ticks;

		public SpawnsShrapnel(Actor self, SpawnsShrapnelInfo info)
			: base(info)
		{
			world = self.World;
			body = self.TraitOrDefault<BodyOrientation>();
		}

		void ITick.Tick(Actor self)
		{
			if (IsTraitDisabled || IsTraitPaused || !self.IsInWorld || --ticks > 0)
				return;

			ticks = Info.Delay.Length == 2
					? world.SharedRandom.Next(Info.Delay[0], Info.Delay[1])
					: Info.Delay[0];

			var localoffset = body != null
					? body.LocalToWorld(Info.LocalOffset.Rotate(body.QuantizeOrientation(self, self.Orientation)))
					: Info.LocalOffset;

			var position = self.CenterPosition + localoffset;

			var availableTargetActors = world.FindActorsOnCircle(position, Info.WeaponInfo.Range)
				.Where(x => (Info.AllowSelfHit || x != self)
					&& Info.WeaponInfo.IsValidAgainst(Target.FromActor(x), world, self)
					&& Info.AimTargetStances.HasRelationship(self.Owner.RelationshipWith(x.Owner)))
				.Where(x =>
				{
					var activeShapes = x.TraitsImplementing<HitShape>().Where(Exts.IsTraitEnabled);
					if (!activeShapes.Any())
						return false;

					var distance = activeShapes.Min(t => t.DistanceFromEdge(x, position));

					if (distance < Info.WeaponInfo.Range)
						return true;

					return false;
				})
				.Shuffle(world.SharedRandom);

			var targetActor = availableTargetActors.GetEnumerator();

			var amount = Info.Amount.Length == 2
					? world.SharedRandom.Next(Info.Amount[0], Info.Amount[1])
					: Info.Amount[0];

			for (var i = 0; i < amount; i++)
			{
				var shrapnelTarget = Target.Invalid;

				if (world.SharedRandom.Next(100) < Info.AimChance && targetActor.MoveNext())
					shrapnelTarget = Target.FromActor(targetActor.Current);

				if (Info.ThrowWithoutTarget && shrapnelTarget.Type == TargetType.Invalid)
				{
					var rotation = WRot.FromFacing(world.SharedRandom.Next(1024));
					var range = world.SharedRandom.Next(Info.WeaponInfo.MinRange.Length, Info.WeaponInfo.Range.Length);
					var targetpos = position + new WVec(range, 0, 0).Rotate(rotation);
					var tpos = Target.FromPos(new WPos(targetpos.X, targetpos.Y, world.Map.CenterOfCell(world.Map.CellContaining(targetpos)).Z));
					if (Info.WeaponInfo.IsValidAgainst(tpos, world, self))
						shrapnelTarget = tpos;
				}

				if (shrapnelTarget.Type == TargetType.Invalid)
					continue;

				var args = new ProjectileArgs
				{
					Weapon = Info.WeaponInfo,
					Facing = (shrapnelTarget.CenterPosition - position).Yaw,

					DamageModifiers = !self.IsDead ? self.TraitsImplementing<IFirepowerModifier>()
						.Select(a => a.GetFirepowerModifier()).ToArray() : Array.Empty<int>(),

					InaccuracyModifiers = !self.IsDead ? self.TraitsImplementing<IInaccuracyModifier>()
						.Select(a => a.GetInaccuracyModifier()).ToArray() : Array.Empty<int>(),

					RangeModifiers = !self.IsDead ? self.TraitsImplementing<IRangeModifier>()
						.Select(a => a.GetRangeModifier()).ToArray() : Array.Empty<int>(),

					Source = position,
					CurrentSource = () => position,
					SourceActor = self,
					GuidedTarget = shrapnelTarget,
					PassiveTarget = shrapnelTarget.CenterPosition
				};

				if (args.Weapon.Projectile != null)
				{
					var projectile = args.Weapon.Projectile.Create(args);
					if (projectile != null)
						world.AddFrameEndTask(w => w.Add(projectile));

					if (args.Weapon.Report != null && args.Weapon.Report.Any())
						Game.Sound.Play(SoundType.World, args.Weapon.Report.Random(world.SharedRandom), position);
				}
			}
		}

		protected override void TraitEnabled(Actor self)
		{
			ticks = 0;
		}
	}
}
