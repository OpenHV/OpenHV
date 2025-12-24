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

using System.Collections.Generic;
using System.Collections.Immutable;
using OpenRA.GameRules;
using OpenRA.Graphics;
using OpenRA.Mods.HV.Graphics;
using OpenRA.Primitives;
using OpenRA.Support;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Projectiles
{
	[Desc("Renders an energy discharge.")]
	public class EnergyBoltInfo : IProjectileInfo
	{
		[Desc("The maximum duration (in ticks) of the beam's existence.")]
		public readonly int Duration = 10;

		[Desc("Color of the beam. Default falls back to player color.")]
		public readonly Color Color = Color.Transparent;

		[Desc("Inner lightness of the beam.")]
		public readonly byte InnerLightness = 0xff;

		[Desc("Outer lightness of the beam.")]
		public readonly byte OuterLightness = 0x80;

		[FieldLoader.Require]
		[Desc("The radius of the beam.")]
		public readonly int Radius = 3;

		[Desc("Distortion offset.")]
		public readonly int Distortion = 0;

		[Desc("Distortion animation offset.")]
		public readonly int DistortionAnimation = 0;

		[Desc("Maximum length per segment.")]
		public readonly WDist SegmentLength = WDist.Zero;

		[Desc("Fade the beams out during lifetime.")]
		public readonly bool FadeOut = false;

		[Desc("Equivalent to sequence ZOffset. Controls Z sorting.")]
		public readonly int ZOffset = 0;

		public IProjectile Create(ProjectileArgs args) { return new EnergyBolt(args, this); }
	}

	public class EnergyBolt : IProjectile, ISync
	{
		readonly EnergyBoltInfo info;
		readonly Color[] colors;
		readonly WPos[] offsets;

		readonly WVec leftVector;
		readonly WVec upVector;
		readonly MersenneTwister random;
		readonly int fadeout;

		[VerifySync]
		readonly WPos target;

		[VerifySync]
		readonly WPos source;

		int ticks;

		public EnergyBolt(ProjectileArgs args, EnergyBoltInfo info)
		{
			this.info = info;

			colors = new Color[info.Radius];
			for (var i = 0; i < info.Radius; i++)
			{
				var color = info.Color == Color.Transparent ? args.SourceActor.Owner.Color : info.Color;
				var bw = (float)((info.InnerLightness - info.OuterLightness) * i / info.Radius + info.OuterLightness) / 0xff;
				var dstR = bw > .5 ? 1 - (1 - 2 * (bw - .5)) * (1 - (float)color.R / 0xff) : 2 * bw * ((float)color.R / 0xff);
				var dstG = bw > .5 ? 1 - (1 - 2 * (bw - .5)) * (1 - (float)color.G / 0xff) : 2 * bw * ((float)color.G / 0xff);
				var dstB = bw > .5 ? 1 - (1 - 2 * (bw - .5)) * (1 - (float)color.B / 0xff) : 2 * bw * ((float)color.B / 0xff);
				colors[i] = Color.FromArgb((int)(dstR * 0xff), (int)(dstG * 0xff), (int)(dstB * 0xff));
			}

			fadeout = 255 / info.Duration;
			if (fadeout == 0)
				fadeout++;

			target = args.PassiveTarget;
			source = args.Source;

			var direction = target - source;

			if (info.Distortion != 0 || info.DistortionAnimation != 0)
			{
				leftVector = new WVec(direction.Y, -direction.X, 0);
				if (leftVector.Length != 0)
					leftVector = 1024 * leftVector / leftVector.Length;

				upVector = leftVector.Length != 0
					? new WVec(
					-direction.X * direction.Z,
					-direction.Z * direction.Y,
					direction.X * direction.X + direction.Y * direction.Y)
					: new WVec(direction.Z, direction.Z, 0);
				if (upVector.Length != 0)
					upVector = 1024 * upVector / upVector.Length;

				random = args.SourceActor.World.SharedRandom;
			}

			if (this.info.SegmentLength == WDist.Zero)
				offsets = [source, target];
			else
			{
				var numSegments = (direction.Length - 1) / info.SegmentLength.Length + 1;
				offsets = new WPos[numSegments + 1];
				offsets[0] = source;
				offsets[^1] = target;

				for (var i = 1; i < numSegments; i++)
				{
					var segmentStart = direction / numSegments * i;
					offsets[i] = source + segmentStart;

					if (info.Distortion != 0)
					{
						var angle = WAngle.FromDegrees(random.Next(360));
						var distortion = random.Next(info.Distortion);

						var offset = distortion * angle.Cos() * leftVector / (1024 * 1024)
							+ distortion * angle.Sin() * upVector / (1024 * 1024);

						offsets[i] += offset;
					}
				}
			}

			args.Weapon.Impact(Target.FromPos(args.PassiveTarget), new WarheadArgs(args));
		}

		public void Tick(World world)
		{
			if (++ticks >= info.Duration)
				world.AddFrameEndTask(w => w.Remove(this));
			else
			{
				if (info.DistortionAnimation != 0)
				{
					for (var i = 1; i < offsets.Length - 1; i++)
					{
						var angle = WAngle.FromDegrees(random.Next(360));
						var distortion = random.Next(info.DistortionAnimation);

						var offset = distortion * angle.Cos() * leftVector / (1024 * 1024)
							+ distortion * angle.Sin() * upVector / (1024 * 1024);

						offsets[i] += offset;
					}
				}

				if (info.FadeOut)
				{
					for (var i = 0; i < colors.Length; i++)
					{
						colors[i] = Color.FromArgb(colors[i].A - fadeout, colors[i].R, colors[i].G, colors[i].B);
					}
				}
			}
		}

		public IEnumerable<IRenderable> Render(WorldRenderer worldRenderer)
		{
			if (worldRenderer.World.FogObscures(target) &&
				worldRenderer.World.FogObscures(source))
				yield break;

			for (var i = 0; i < offsets.Length - 1; i++)
				for (var j = 0; j < info.Radius; j++)
					yield return new EnergyBoltRenderable(offsets.ToImmutableArray(), info.ZOffset, new WDist(32 + (info.Radius - j - 1) * 64), colors[j]);
		}
	}
}
