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

using System;
using System.Collections.Generic;
using OpenRA.GameRules;
using OpenRA.Graphics;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Effects;
using OpenRA.Mods.Common.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;
using Util = OpenRA.Mods.Common.Util;

namespace OpenRA.Mods.HV.Projectiles
{
	[Desc("Instant and usually direct-on-target projectile, with tracer ammunition.")]
	public class InstantHitWithTracersInfo : IProjectileInfo
	{
		[Desc("The maximum/constant/incremental inaccuracy used in conjunction with the InaccuracyType property.")]
		public readonly WDist Inaccuracy = WDist.Zero;

		[Desc("Controls the way inaccuracy is calculated. Possible values are 'Maximum' - scale from 0 to max with range, 'PerCellIncrement' - scale from 0 with range and 'Absolute' - use set value regardless of range.")]
		public readonly InaccuracyType InaccuracyType = InaccuracyType.Maximum;

		[Desc("Projectile can be blocked.")]
		public readonly bool Blockable = false;

		[Desc("The width of the projectile.")]
		public readonly WDist Width = new(1);

		[Desc("Scan radius for actors with projectile-blocking trait. If set to a negative value (default), it will automatically scale",
			"to the blocker with the largest health shape. Only set custom values if you know what you're doing.")]
		public readonly WDist BlockerScanRadius = new(-1);

		[Desc("Amount of Tracers.")]
		public readonly int TracerAmount = 0;

		[Desc("Tracer spawn interval. Only useful when Amount > 0")]
		public readonly int TracerSpawnInterval = 3;

		[Desc("Tracer speed")]
		public readonly int TracerSpeed = 1024;

		[Desc("Tracer inaccuracy, use set value regardless of range")]
		public readonly WDist TracerInaccuracy = WDist.Zero;

		[Desc("Image to display.")]
		public readonly string Image = null;

		[SequenceReference(nameof(Image), allowNullImage: true)]
		[Desc("Loop sequence of Image from this list while this projectile is moving.")]
		public readonly string Sequence = "idle";

		[Desc("The palette used to draw this projectile.")]
		[PaletteReference(nameof(IsPlayerPalette))]
		public readonly string Palette = "effect";

		public readonly bool IsPlayerPalette = false;

		[Desc("Trail animation.")]
		public readonly string TrailImage = null;

		[Desc("Loop sequence of TrailImage from this list while this projectile is moving.")]
		[SequenceReference(nameof(TrailImage), allowNullImage: true)]
		public readonly string TrailSequence = "idle";

		[Desc("Interval in ticks between each spawned Trail animation.")]
		public readonly int TrailInterval = 2;

		[Desc("Palette used to render the trail sequence.")]
		[PaletteReference(nameof(TrailUsePlayerPalette))]
		public readonly string TrailPalette = "effect";

		[Desc("Use the Player Palette to render the trail sequence.")]
		public readonly bool TrailUsePlayerPalette = false;

		[Desc("When set, display a line behind the tracer bullet. Length is measured in ticks after appearing.")]
		public readonly int ContrailLength = 0;

		[Desc("Time (in ticks) after which the line should appear. Controls the distance to the actor.")]
		public readonly int ContrailDelay = 1;

		[Desc("Equivalent to sequence ZOffset. Controls Z sorting.")]
		public readonly int ContrailZOffset = 2047;

		[Desc("Thickness of the emitted line at the start of the contrail.")]
		public readonly WDist ContrailStartWidth = new(64);

		[Desc("Thickness of the emitted line at the end of the contrail. Will default to " + nameof(ContrailStartWidth) + " if left undefined")]
		public readonly WDist? ContrailEndWidth = null;

		[Desc("RGB color at the contrail start.")]
		public readonly Color ContrailStartColor = Color.White;

		[Desc("Use player remap color instead of a custom color at the contrail the start.")]
		public readonly bool ContrailStartColorUsePlayerColor = false;

		[Desc("The alpha value [from 0 to 255] of color at the contrail the start.")]
		public readonly int ContrailStartColorAlpha = 255;

		[Desc("RGB color at the contrail end. Will default to " + nameof(ContrailStartColor) + " if left undefined")]
		public readonly Color? ContrailEndColor;

		[Desc("Use player remap color instead of a custom color at the contrail end.")]
		public readonly bool ContrailEndColorUsePlayerColor = false;

		[Desc("The alpha value [from 0 to 255] of color at the contrail end.")]
		public readonly int ContrailEndColorAlpha = 0;

		public IProjectile Create(ProjectileArgs args) { return new InstantHitWithTracers(this, args); }
	}

	public class InstantHitWithTracers : IProjectile
	{
		readonly ProjectileArgs args;
		readonly InstantHitWithTracersInfo info;
		readonly TracerWrapper[] tracers;
		readonly World world;
		readonly string animationPalette;
		readonly string trailPalette;

		Target target;
		bool notImpacted = true;
		int ticks;
		WPos tracerEndBasePosition;

		struct TracerWrapper
		{
			public int Time;
			public int OverallTime;
			public ContrailRenderable Contrail;
			public Animation Animation;
			public WPos EndPosition;
			public WPos Position;
			public WPos SourcePosition;
			public WAngle Facing;
		}

		public InstantHitWithTracers(InstantHitWithTracersInfo info, ProjectileArgs args)
		{
			this.args = args;
			this.info = info;
			world = args.SourceActor.World;

			var owner = args.SourceActor.Owner;

			animationPalette = info.Palette;
			if (info.IsPlayerPalette)
				animationPalette += owner.InternalName;

			trailPalette = info.TrailPalette;
			if (info.TrailUsePlayerPalette)
				trailPalette += owner.InternalName;

			if (info.TracerAmount > 0)
			{
				tracers = new TracerWrapper[info.TracerAmount];
				var startColor = Color.FromArgb(info.ContrailStartColorAlpha, info.ContrailStartColor);
				var endColor = Color.FromArgb(info.ContrailEndColorAlpha, info.ContrailEndColor ?? startColor);
				for (var i = 0; i < tracers.Length; i++)
				{
					var contrail = info.ContrailLength <= 0 ? null : new ContrailRenderable(world, owner.PlayerActor, startColor, info.ContrailStartColorUsePlayerColor, endColor, info.ContrailEndColorUsePlayerColor, info.ContrailStartWidth, info.ContrailEndWidth ?? info.ContrailStartWidth, info.ContrailLength, info.ContrailDelay, info.ContrailZOffset);

					tracers[i] = new TracerWrapper
					{
						Time = -1,
						OverallTime = 0,
						Contrail = contrail,
						EndPosition = WPos.Zero
					};
				}
			}

			if (args.Weapon.TargetActorCenter)
				target = args.GuidedTarget;
			else if (info.Inaccuracy.Length > 0)
			{
				var maxInaccuracyOffset = Util.GetProjectileInaccuracy(info.Inaccuracy.Length, info.InaccuracyType, args);
				var inaccuracyOffset = WVec.FromPDF(args.SourceActor.World.SharedRandom, 2) * maxInaccuracyOffset / 1024;
				target = Target.FromPos(args.PassiveTarget + inaccuracyOffset);
			}
			else
				target = Target.FromPos(args.PassiveTarget);
		}

		public void Tick(World world)
		{
			ticks++;

			if (notImpacted)
			{
				notImpacted = false;

				// If GuidedTarget has become invalid due to getting killed the same tick,
				// we need to set target to args.PassiveTarget to prevent target.CenterPosition below from crashing.
				if (target.Type == TargetType.Invalid)
					target = Target.FromPos(args.PassiveTarget);

				// Check for blocking actors
				if (info.Blockable && BlocksProjectiles.AnyBlockingActorsBetween(world, args.SourceActor.Owner, args.Source, target.CenterPosition, info.Width, out var blockedPos))
					target = Target.FromPos(blockedPos);

				tracerEndBasePosition = target.CenterPosition;

				var warheadArgs = new WarheadArgs(args)
				{
					ImpactOrientation = new WRot(WAngle.Zero, Util.GetVerticalAngle(args.Source, target.CenterPosition), args.Facing),
					ImpactPosition = target.CenterPosition,
				};

				args.Weapon.Impact(target, warheadArgs);
			}

			var allFakeBulletDone = true;

			// Tracer generate/render position process
			if (tracers != null)
			{
				for (var i = 0; i < tracers.Length; i++)
				{
					// Generate the bullet when time has come
					if (ticks >= i * info.TracerSpawnInterval && tracers[i].OverallTime == 0 && !args.SourceActor.IsDead)
					{
						allFakeBulletDone = false;

						// Tracer contains only renderable objects, so don't use shared random.
						tracers[i].SourcePosition = args.CurrentSource();
						var cosmeticInaccuracyOffset = WVec.FromPDF(Game.CosmeticRandom, 2) * info.TracerInaccuracy.Length / 1024;
						var vec = tracerEndBasePosition + cosmeticInaccuracyOffset - tracers[i].SourcePosition;
						var time = Math.Max(vec.Length / info.TracerSpeed, 1);
						tracers[i].Time = time;
						tracers[i].OverallTime = time;
						tracers[i].Position = tracers[i].SourcePosition;
						tracers[i].EndPosition = tracerEndBasePosition + cosmeticInaccuracyOffset;

						// Get pitch.
						// Note: OpenRA defines north as -y.
						tracers[i].Facing = vec.LengthSquared == 0 ? WAngle.Zero : WAngle.ArcTan(vec.Z, vec.HorizontalLength);

						if (!string.IsNullOrEmpty(info.Image))
						{
							var facing = tracers[i].Facing;
							tracers[i].Animation = new Animation(world, info.Image, () => facing);
							tracers[i].Animation.PlayRepeating(info.Sequence);
						}
					}

					// Process the existing bullet when it does not expire.
					if (tracers[i].Time > 0 && tracers[i].OverallTime != 0)
					{
						allFakeBulletDone = false;
						var currentPos = WPos.Lerp(tracers[i].SourcePosition, tracers[i].EndPosition, tracers[i].OverallTime - tracers[i].Time, tracers[i].OverallTime);
						tracers[i].Position = currentPos;
						tracers[i].Contrail?.Update(currentPos);
						tracers[i].Animation?.Tick();

						if (!string.IsNullOrEmpty(info.TrailImage) && (info.TrailInterval == 0 || ((tracers[i].OverallTime - tracers[i].Time) % info.TrailInterval == 0)))
						{
							var pos = tracers[i].Position;
							var facing = tracers[i].Facing;
							world.AddFrameEndTask(w => w.Add(new SpriteEffect(pos, facing, world,
								info.TrailImage, info.TrailSequence, trailPalette)));
						}

						if (--tracers[i].Time <= 0 && info.ContrailLength > 0)
						{
							var contrail = tracers[i].Contrail;
							var pos = tracers[i].EndPosition;
							world.AddFrameEndTask(w => w.Add(new ContrailFader(pos, contrail)));
						}
					}
				}

				// Process the exsting bullet when it does not expire.
				if (ticks <= (tracers.Length - 1) * info.TracerSpawnInterval)
					allFakeBulletDone = false;
			}

			if (allFakeBulletDone)
				world.AddFrameEndTask(w => w.Remove(this));
		}

		public IEnumerable<IRenderable> Render(WorldRenderer wr)
		{
			if (tracers == null)
				yield break;

			foreach (var c in tracers)
			{
				if (c.Time <= 0)
					yield break;

				if (info.ContrailLength > 0)
					yield return c.Contrail;

				if (c.Animation != null && !world.FogObscures(c.Position))
				{
					var palette = wr.Palette(animationPalette);
					foreach (var r in c.Animation.Render(c.Position, palette))
						yield return r;
				}
			}
		}
	}
}
