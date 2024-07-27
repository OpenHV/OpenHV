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

using System.Collections.Generic;
using OpenRA.Graphics;
using OpenRA.Mods.HV.Graphics;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Draws an arc between a " + nameof(Hacker) + " and all its victims",
		"or an actively exploited actor and it's controller.")]
	public class WithHackedConnectionArcInfo : TraitInfo
	{
		[Desc("Color of the arc.")]
		public readonly Color Color = Color.Red;

		public readonly bool UsePlayerColor = false;

		public readonly int Transparency = 255;

		[Desc("Relative offset from the actor's center position where the arc should start.")]
		public readonly WVec Offset = new(0, 0, 0);

		[Desc("The angle of the arc.")]
		public readonly WAngle Angle = new(90);

		[Desc("The width of the arc.")]
		public readonly WDist Width = new(96);

		[Desc("Controls how fine-grained the resulting arc should be.")]
		public readonly int QuantizedSegments = 16;

		[Desc("Equivalent to sequence ZOffset. Controls Z sorting.")]
		public readonly int ZOffset = 0;

		public override object Create(ActorInitializer init) { return new WithHackedConnectionArc(this); }
	}

	public class WithHackedConnectionArc : IRenderAboveShroudWhenSelected, INotifySelected, INotifyCreated
	{
		readonly WithHackedConnectionArcInfo info;
		Hacker hacker;
		Hackable hackable;

		public WithHackedConnectionArc(WithHackedConnectionArcInfo info)
		{
			this.info = info;
		}

		void INotifyCreated.Created(Actor self)
		{
			hacker = self.TraitOrDefault<Hacker>();
			hackable = self.TraitOrDefault<Hackable>();
		}

		void INotifySelected.Selected(Actor a) { }

		IEnumerable<IRenderable> IRenderAboveShroudWhenSelected.RenderAboveShroud(Actor self, WorldRenderer wr)
		{
			var color = Color.FromArgb(info.Transparency, info.UsePlayerColor ? self.Owner.Color : info.Color);

			if (hacker != null)
			{
				foreach (var victim in hacker.Victims)
					yield return new ArcRenderable(
						self.CenterPosition + info.Offset,
						victim.CenterPosition + info.Offset,
						info.ZOffset, info.Angle, color, info.Width, info.QuantizedSegments);

				yield break;
			}

			if (hackable == null || hackable.Attacker == null || !hackable.Attacker.IsInWorld)
				yield break;

			yield return new ArcRenderable(
				hackable.Attacker.CenterPosition + info.Offset,
				self.CenterPosition + info.Offset,
				info.ZOffset, info.Angle, color, info.Width, info.QuantizedSegments);
		}

		bool IRenderAboveShroudWhenSelected.SpatiallyPartitionable => false;
	}
}
