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

using System.Collections.Generic;
using OpenRA.Effects;
using OpenRA.Graphics;
using OpenRA.Mods.HV.Traits;

namespace OpenRA.Mods.HV.Effects
{
	class Cloud : IEffect, ISpatiallyPartitionable
	{
		readonly World world;
		readonly Animation animation;
		readonly string palette;
		readonly WPos edge;
		readonly int facing;
		readonly WDist[] speed;
		readonly WDist closeEnough;

		WPos position;

		public Cloud(World world, Animation animation, WPos position, WPos edge, int facing, CloudSpawnerInfo info)
		{
			this.world = world;
			this.animation = animation;
			this.position = position;
			this.edge = edge;
			this.facing = facing;

			palette = info.Palette;
			speed = info.Speed;
			closeEnough = info.CloseEnough;

			world.ScreenMap.Add(this, position, animation.Image);
		}

		IEnumerable<IRenderable> IEffect.Render(WorldRenderer r)
		{
			if (world.FogObscures(position))
				return SpriteRenderable.None;

			return animation.Render(position, r.Palette(palette));
		}

		void IEffect.Tick(World world)
		{
			if ((edge - position).Length < closeEnough.Length)
			{
				world.AddFrameEndTask(w => { w.Remove(this); w.ScreenMap.Remove(this); });
				return;
			}

			var forward = speed.Length == 2
					? world.SharedRandom.Next(speed[0].Length, speed[1].Length)
					: speed[0].Length;

			// Needs to be defined the same way delta is defined in CloudSpawner.SpawnCloud to ensure facing consistency.
			var offset = new WVec(0, -forward, 0);
			offset = offset.Rotate(WRot.FromFacing(facing));

			animation.Tick();
			position += offset;
			world.ScreenMap.Update(this, position, animation.Image);
		}
	}
}
