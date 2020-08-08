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

		[SequenceReference("Image", true)]
		public readonly string TopLeftSequence = "top-left";

		[SequenceReference("Image", true)]
		public readonly string TopRightSequence = "top-right";

		[SequenceReference("Image", true)]
		public readonly string BottomLeftSequence = "bottom-left";

		[SequenceReference("Image", true)]
		public readonly string BottomRightSequence = "bottom-right";

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
		}

		protected override int2 GetDecorationPosition(Actor self, WorldRenderer wr, DecorationPosition pos)
		{
			var bounds = interactable.DecorationBounds(self, wr);
			switch (pos)
			{
				case DecorationPosition.TopLeft: return bounds.TopLeft;
				case DecorationPosition.TopRight: return bounds.TopRight;
				case DecorationPosition.BottomLeft: return bounds.BottomLeft;
				case DecorationPosition.BottomRight: return bounds.BottomRight;
				case DecorationPosition.Top: return new int2(bounds.Left + bounds.Size.Width / 2, bounds.Top);
				default: return bounds.TopLeft + new int2(bounds.Size.Width / 2, bounds.Size.Height / 2);
			}
		}

		void INotifyDamageStateChanged.DamageStateChanged(Actor self, AttackInfo e)
		{
			if (e.DamageState == DamageState.Heavy)
			{
				topLeft.ChangeImage(outlinedDecorationsInfo.DamagedImage, outlinedDecorationsInfo.Image);
				topRight.ChangeImage(outlinedDecorationsInfo.DamagedImage, outlinedDecorationsInfo.Image);
				bottomLeft.ChangeImage(outlinedDecorationsInfo.DamagedImage, outlinedDecorationsInfo.Image);
				bottomRight.ChangeImage(outlinedDecorationsInfo.DamagedImage, outlinedDecorationsInfo.Image);
			}
			else if (e.DamageState == DamageState.Critical)
			{
				topLeft.ChangeImage(outlinedDecorationsInfo.CriticallyDamagedImage, outlinedDecorationsInfo.Image);
				topRight.ChangeImage(outlinedDecorationsInfo.CriticallyDamagedImage, outlinedDecorationsInfo.Image);
				bottomLeft.ChangeImage(outlinedDecorationsInfo.CriticallyDamagedImage, outlinedDecorationsInfo.Image);
				bottomRight.ChangeImage(outlinedDecorationsInfo.CriticallyDamagedImage, outlinedDecorationsInfo.Image);
			}
			else
			{
				topLeft.ChangeImage(outlinedDecorationsInfo.Image, outlinedDecorationsInfo.Image);
				topRight.ChangeImage(outlinedDecorationsInfo.Image, outlinedDecorationsInfo.Image);
				bottomLeft.ChangeImage(outlinedDecorationsInfo.Image, outlinedDecorationsInfo.Image);
				bottomRight.ChangeImage(outlinedDecorationsInfo.Image, outlinedDecorationsInfo.Image);
			}
		}

		protected override IEnumerable<IRenderable> RenderSelectionBox(Actor self, WorldRenderer wr, Color color)
		{
			var palette = wr.Palette(outlinedDecorationsInfo.Palette);
			var offset = new int2(topRight.Image.Bounds.Width, 0);

			var topLeftScreenPosition = wr.Viewport.WorldToViewPx(GetDecorationPosition(self, wr, DecorationPosition.TopLeft));
			yield return new UISpriteRenderable(topLeft.Image, self.CenterPosition, topLeftScreenPosition, 0, palette, 1f);

			var topRightScreenPosition = wr.Viewport.WorldToViewPx(GetDecorationPosition(self, wr, DecorationPosition.TopRight)) - offset;
			yield return new UISpriteRenderable(topRight.Image, self.CenterPosition, topRightScreenPosition, 0, palette, 1f);

			var bottomLeftScreenPosition = wr.Viewport.WorldToViewPx(GetDecorationPosition(self, wr, DecorationPosition.BottomLeft));
			yield return new UISpriteRenderable(bottomLeft.Image, self.CenterPosition, bottomLeftScreenPosition, 0, palette, 1f);

			var bottomRightScreenPosition = wr.Viewport.WorldToViewPx(GetDecorationPosition(self, wr, DecorationPosition.BottomRight)) - offset;
			yield return new UISpriteRenderable(bottomRight.Image, self.CenterPosition, bottomRightScreenPosition, 0, palette, 1f);
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
