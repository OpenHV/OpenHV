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
using OpenRA.Graphics;
using OpenRA.Mods.HV.Graphics;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits.Render
{
	[Desc("Displays thick and outlined healthbars as well as sprite based selection boxes.")]
	public class OutlinedSelectionDecorationsInfo : SelectionDecorationsBaseInfo, Requires<InteractableInfo>
	{
		[Desc("Image used for the undamaged decoration.")]
		public readonly string Image = "outline";

		public readonly string DamagedImage = "outline-yellow";

		public readonly string CriticallyDamagedImage = "outline-red";

		[SequenceReference("Image")]
		public readonly string TopLeftSequence = "top-left";

		[SequenceReference("Image")]
		public readonly string TopRightSequence = "top-right";

		[SequenceReference("Image")]
		public readonly string BottomLeftSequence = "bottom-left";

		[SequenceReference("Image")]
		public readonly string BottomRightSequence = "bottom-right";

		[Desc("Render left, bottom, right and top as well.")]
		public readonly bool Spacers = false;

		[SequenceReference("Image")]
		public readonly string LeftSequence = "left";

		[SequenceReference("Image")]
		public readonly string BottomSequence = "bottom";

		[SequenceReference("Image")]
		public readonly string RightSequence = "right";

		[SequenceReference("Image")]
		public readonly string TopSequence = "top";

		[Desc("Palette to render the sprite in. Reference the world actor's PaletteFrom* traits.")]
		public readonly string Palette = "cursor";

		public override object Create(ActorInitializer init) { return new OutlinedSelectionDecorations(init.Self, this); }
	}

	public class OutlinedSelectionDecorations : SelectionDecorationsBase, IRender, INotifyDamageStateChanged
	{
		readonly Interactable interactable;
		readonly OutlinedSelectionDecorationsInfo outlinedDecorationsInfo;

		protected Animation topLeft;
		protected Animation topRight;
		protected Animation bottomLeft;
		protected Animation bottomRight;

		protected Animation left;
		protected Animation right;
		protected Animation bottom;
		protected Animation top;

		public OutlinedSelectionDecorations(Actor self, OutlinedSelectionDecorationsInfo info)
			: base(info)
		{
			interactable = self.Trait<Interactable>();
			outlinedDecorationsInfo = info;

			topLeft = new Animation(self.World, info.Image);
			topLeft.Play(info.TopLeftSequence);

			topRight = new Animation(self.World, info.Image);
			topRight.Play(info.TopRightSequence);

			bottomLeft = new Animation(self.World, info.Image);
			bottomLeft.Play(info.BottomLeftSequence);

			bottomRight = new Animation(self.World, info.Image);
			bottomRight.Play(info.BottomRightSequence);

			if (info.Spacers)
			{
				left = new Animation(self.World, info.Image);
				left.Play(info.LeftSequence);

				bottom = new Animation(self.World, info.Image);
				bottom.Play(info.BottomSequence);

				right = new Animation(self.World, info.Image);
				right.Play(info.RightSequence);

				top = new Animation(self.World, info.Image);
				top.Play(info.TopSequence);
			}
		}

		int2 GetDecorationPosition(Actor self, WorldRenderer wr, string pos)
		{
			var bounds = interactable.DecorationBounds(self, wr);
			switch (pos)
			{
				case "TopLeft": return bounds.TopLeft;
				case "TopRight": return bounds.TopRight;
				case "BottomLeft": return bounds.BottomLeft;
				case "BottomRight": return bounds.BottomRight;
				case "Top": return new int2(bounds.Left + bounds.Size.Width / 2, bounds.Top);
				case "Bottom": return new int2(bounds.Left + bounds.Size.Width / 2, bounds.Bottom);
				case "Left": return new int2(bounds.Left, bounds.Top + bounds.Size.Height / 2);
				case "Right": return new int2(bounds.Right, bounds.Top + bounds.Size.Height / 2);
				default: return bounds.TopLeft + new int2(bounds.Size.Width / 2, bounds.Size.Height / 2);
			}
		}

		static int2 GetDecorationMargin(string pos, int2 margin)
		{
			switch (pos)
			{
				case "TopLeft": return margin;
				case "TopRight": return new int2(-margin.X, margin.Y);
				case "BottomLeft": return new int2(margin.X, -margin.Y);
				case "BottomRight": return -margin;
				case "Top": return new int2(0, margin.Y);
				case "Bottom": return new int2(0, -margin.Y);
				case "Left": return new int2(margin.X, 0);
				case "Right": return new int2(-margin.X, 0);
				default: return int2.Zero;
			}
		}

		protected override int2 GetDecorationOrigin(Actor self, WorldRenderer wr, string pos, int2 margin)
		{
			return wr.Viewport.WorldToViewPx(GetDecorationPosition(self, wr, pos)) + GetDecorationMargin(pos, margin);
		}

		void INotifyDamageStateChanged.DamageStateChanged(Actor self, AttackInfo e)
		{
			if (e.DamageState == DamageState.Heavy)
			{
				topLeft.ChangeImage(outlinedDecorationsInfo.DamagedImage, outlinedDecorationsInfo.Image);
				topRight.ChangeImage(outlinedDecorationsInfo.DamagedImage, outlinedDecorationsInfo.Image);
				bottomLeft.ChangeImage(outlinedDecorationsInfo.DamagedImage, outlinedDecorationsInfo.Image);
				bottomRight.ChangeImage(outlinedDecorationsInfo.DamagedImage, outlinedDecorationsInfo.Image);

				if (outlinedDecorationsInfo.Spacers)
				{
					top.ChangeImage(outlinedDecorationsInfo.DamagedImage, outlinedDecorationsInfo.Image);
					bottom.ChangeImage(outlinedDecorationsInfo.DamagedImage, outlinedDecorationsInfo.Image);
					left.ChangeImage(outlinedDecorationsInfo.DamagedImage, outlinedDecorationsInfo.Image);
					right.ChangeImage(outlinedDecorationsInfo.DamagedImage, outlinedDecorationsInfo.Image);
				}
			}
			else if (e.DamageState == DamageState.Critical)
			{
				topLeft.ChangeImage(outlinedDecorationsInfo.CriticallyDamagedImage, outlinedDecorationsInfo.Image);
				topRight.ChangeImage(outlinedDecorationsInfo.CriticallyDamagedImage, outlinedDecorationsInfo.Image);
				bottomLeft.ChangeImage(outlinedDecorationsInfo.CriticallyDamagedImage, outlinedDecorationsInfo.Image);
				bottomRight.ChangeImage(outlinedDecorationsInfo.CriticallyDamagedImage, outlinedDecorationsInfo.Image);

				if (outlinedDecorationsInfo.Spacers)
				{
					top.ChangeImage(outlinedDecorationsInfo.CriticallyDamagedImage, outlinedDecorationsInfo.Image);
					bottom.ChangeImage(outlinedDecorationsInfo.CriticallyDamagedImage, outlinedDecorationsInfo.Image);
					left.ChangeImage(outlinedDecorationsInfo.CriticallyDamagedImage, outlinedDecorationsInfo.Image);
					right.ChangeImage(outlinedDecorationsInfo.CriticallyDamagedImage, outlinedDecorationsInfo.Image);
				}
			}
			else
			{
				topLeft.ChangeImage(outlinedDecorationsInfo.Image, outlinedDecorationsInfo.Image);
				topRight.ChangeImage(outlinedDecorationsInfo.Image, outlinedDecorationsInfo.Image);
				bottomLeft.ChangeImage(outlinedDecorationsInfo.Image, outlinedDecorationsInfo.Image);
				bottomRight.ChangeImage(outlinedDecorationsInfo.Image, outlinedDecorationsInfo.Image);

				if (outlinedDecorationsInfo.Spacers)
				{
					top.ChangeImage(outlinedDecorationsInfo.Image, outlinedDecorationsInfo.Image);
					bottom.ChangeImage(outlinedDecorationsInfo.Image, outlinedDecorationsInfo.Image);
					left.ChangeImage(outlinedDecorationsInfo.Image, outlinedDecorationsInfo.Image);
					right.ChangeImage(outlinedDecorationsInfo.Image, outlinedDecorationsInfo.Image);
				}
			}
		}

		protected override IEnumerable<IRenderable> RenderSelectionBox(Actor self, WorldRenderer wr, Color color)
		{
			var palette = wr.Palette(outlinedDecorationsInfo.Palette);
			var widthOffset = new int2(topRight.Image.Bounds.Width, 0);
			var heightOffset = new int2(0, topRight.Image.Bounds.Height);

			var topLeftScreenPosition = wr.Viewport.WorldToViewPx(GetDecorationPosition(self, wr, "TopLeft"));
			yield return new UISpriteRenderable(topLeft.Image, self.CenterPosition, topLeftScreenPosition, 0, palette, 1f);

			var topRightScreenPosition = wr.Viewport.WorldToViewPx(GetDecorationPosition(self, wr, "TopRight")) - widthOffset;
			yield return new UISpriteRenderable(topRight.Image, self.CenterPosition, topRightScreenPosition, 0, palette, 1f);

			var bottomLeftScreenPosition = wr.Viewport.WorldToViewPx(GetDecorationPosition(self, wr, "BottomLeft")) - heightOffset;
			yield return new UISpriteRenderable(bottomLeft.Image, self.CenterPosition, bottomLeftScreenPosition, 0, palette, 1f);

			var bottomRightScreenPosition = wr.Viewport.WorldToViewPx(GetDecorationPosition(self, wr, "BottomRight")) - widthOffset - heightOffset;
			yield return new UISpriteRenderable(bottomRight.Image, self.CenterPosition, bottomRightScreenPosition, 0, palette, 1f);

			if (outlinedDecorationsInfo.Spacers)
			{
				var topScreenPosition = wr.Viewport.WorldToViewPx(GetDecorationPosition(self, wr, "Top")) - widthOffset / 2;
				yield return new UISpriteRenderable(top.Image, self.CenterPosition, topScreenPosition, 0, palette, 1f);

				var bottomScreenPosition = wr.Viewport.WorldToViewPx(GetDecorationPosition(self, wr, "Bottom")) - widthOffset / 2 - heightOffset;
				yield return new UISpriteRenderable(bottom.Image, self.CenterPosition, bottomScreenPosition, 0, palette, 1f);

				var leftScreenPosition = wr.Viewport.WorldToViewPx(GetDecorationPosition(self, wr, "Left"));
				yield return new UISpriteRenderable(left.Image, self.CenterPosition, leftScreenPosition, 0, palette, 1f);

				var rightScreenPosition = wr.Viewport.WorldToViewPx(GetDecorationPosition(self, wr, "Right")) - widthOffset;
				yield return new UISpriteRenderable(right.Image, self.CenterPosition, rightScreenPosition, 0, palette, 1f);
			}
		}

		protected override IEnumerable<IRenderable> RenderSelectionBars(Actor self, WorldRenderer wr, bool displayHealth, bool displayExtra)
		{
			// Don't render the selection bars for non-selectable actors
			if (!(interactable is Selectable) || (!displayHealth && !displayExtra))
				yield break;

			var bounds = interactable.DecorationBounds(self, wr);
			yield return new OutlinedSelectionBoxAnnotationRenderable(self, bounds, displayHealth, displayExtra);
		}

		IEnumerable<IRenderable> IRender.Render(Actor self, WorldRenderer wr)
		{
			yield break;
		}

		IEnumerable<Rectangle> IRender.ScreenBounds(Actor self, WorldRenderer wr)
		{
			yield return interactable.DecorationBounds(self, wr);
		}
	}
}
