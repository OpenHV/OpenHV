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

using OpenRA.Graphics;
using OpenRA.Mods.Common.Widgets;
using OpenRA.Primitives;
using OpenRA.Widgets;

namespace OpenRA.Mods.HV.Widgets
{
	public class MarqueeDropDownButtonWidget : DropDownButtonWidget
	{
		public float ScrollSpeed = 2.0f;
		public int ScrollPauseMs = 300;

		float scrollOffset;
		long lastTickTime;
		long pauseStartTime;
		bool isPaused = true;
		bool scrollingBack;
		string lastText = "";
		int lastTextWidth;
		bool wasHovered;

		CachedTransform<(bool Disabled, bool Pressed, bool Hover, bool Focused, bool Highlighted), Sprite> getMarkerImage;
		CachedTransform<(bool Disabled, bool Pressed, bool Hover, bool Focused, bool Highlighted), Sprite> getSeparatorImage;

		[ObjectCreator.UseCtor]
		public MarqueeDropDownButtonWidget(ModData modData)
			: base(modData)
		{
			lastTickTime = Game.RunTime;
			pauseStartTime = Game.RunTime;
		}

		protected MarqueeDropDownButtonWidget(MarqueeDropDownButtonWidget other)
			: base(other)
		{
			ScrollSpeed = other.ScrollSpeed;
			ScrollPauseMs = other.ScrollPauseMs;
			scrollOffset = 0;
			lastTickTime = Game.RunTime;
			pauseStartTime = Game.RunTime;
			isPaused = true;
			scrollingBack = false;
			wasHovered = false;
		}

		bool IsHovered()
		{
			if (Ui.MouseOverWidget == this)
				return true;

			foreach (var child in Children)
				if (child == Ui.MouseOverWidget)
					return true;

			return false;
		}

		public override void Tick()
		{
			base.Tick();

			var currentTime = Game.RunTime;
			var deltaTime = currentTime - lastTickTime;
			lastTickTime = currentTime;

			var font = Game.Renderer.Fonts[Font];
			var text = GetText();
			var isHovered = IsHovered();

			if (wasHovered && !isHovered)
			{
				scrollOffset = 0;
				isPaused = true;
				scrollingBack = false;
				pauseStartTime = currentTime;
			}

			wasHovered = isHovered;

			if (!isHovered)
				return;

			if (text != lastText)
			{
				lastText = text;
				lastTextWidth = font.Measure(text).X;
				scrollOffset = 0;
				isPaused = true;
				scrollingBack = false;
				pauseStartTime = currentTime;
			}

			var textWidth = lastTextWidth;
			var availableWidth = UsableWidth - LeftMargin - RightMargin;

			if (textWidth <= availableWidth)
			{
				scrollOffset = 0;
				return;
			}

			var maxScroll = textWidth - availableWidth;

			if (isPaused)
			{
				if (currentTime - pauseStartTime >= ScrollPauseMs)
					isPaused = false;

				return;
			}

			if (!scrollingBack)
			{
				scrollOffset += ScrollSpeed * deltaTime / Ui.Timestep;

				if (scrollOffset >= maxScroll)
				{
					scrollOffset = maxScroll;
					scrollingBack = true;
					isPaused = true;
					pauseStartTime = currentTime;
				}
			}
			else
			{
				scrollOffset -= ScrollSpeed * deltaTime / Ui.Timestep;

				if (scrollOffset <= 0)
				{
					scrollOffset = 0;
					scrollingBack = false;
					isPaused = true;
					pauseStartTime = currentTime;
				}
			}
		}

		public override void Draw()
		{
			var rb = RenderBounds;
			var disabled = IsDisabled();
			var highlighted = IsHighlighted();
			var font = Game.Renderer.Fonts[Font];
			var text = GetText();
			var color = GetColor();
			var colorDisabled = GetColorDisabled();
			var bgDark = GetContrastColorDark();
			var bgLight = GetContrastColorLight();

			var stateOffset = Depressed ? new int2(VisualHeight, VisualHeight) : int2.Zero;

			var hover = IsHovered();
			DrawBackground(rb, disabled, Depressed, hover, highlighted);

			var textSize = font.Measure(text);
			var availableWidth = UsableWidth - LeftMargin - RightMargin;

			if (textSize.X <= availableWidth)
			{
				var position = GetTextPositionInternal(text, font, rb);
				DrawText(font, text, position + stateOffset, disabled ? colorDisabled : color, bgDark, bgLight);
			}
			else
			{
				var textAreaX = rb.X + LeftMargin;
				var textAreaWidth = availableWidth;
				var scissorRect = new Rectangle(textAreaX + stateOffset.X, rb.Y, textAreaWidth, rb.Height);

				var textY = rb.Y + (Bounds.Height - textSize.Y - font.TopOffset) / 2;
				var textPosition = new int2(textAreaX - (int)scrollOffset, textY) + stateOffset;

				Game.Renderer.EnableScissor(scissorRect);
				DrawText(font, text, textPosition, disabled ? colorDisabled : color, bgDark, bgLight);
				Game.Renderer.DisableScissor();
			}

			DrawDropDownDecorations(rb, stateOffset, disabled, hover, highlighted);
		}

		void DrawDropDownDecorations(Rectangle rb, int2 stateOffset, bool isDisabled, bool isHover, bool highlighted)
		{
			getMarkerImage ??= WidgetUtils.GetCachedStatefulImage(Decorations, DecorationMarker);

			var arrowImage = getMarkerImage.Update((isDisabled, Depressed, isHover, false, highlighted));
			WidgetUtils.DrawSprite(
				arrowImage,
				stateOffset + new float2(
					rb.Right - (int)((rb.Height + arrowImage.Size.X) / 2),
					rb.Top + (int)((rb.Height - arrowImage.Size.Y) / 2)));

			getSeparatorImage ??= WidgetUtils.GetCachedStatefulImage(Separators, SeparatorImage);

			var separatorImage = getSeparatorImage.Update((isDisabled, Depressed, isHover, false, highlighted));
			if (separatorImage != null)
				WidgetUtils.DrawSprite(
					separatorImage,
					stateOffset + new float2(-3, 0) + new float2(
						rb.Right - rb.Height + 4,
						rb.Top + (int)((rb.Height - separatorImage.Size.Y) / 2)));
		}

		void DrawText(SpriteFont font, string text, int2 position, Color color, Color bgDark, Color bgLight)
		{
			if (Contrast)
				font.DrawTextWithContrast(text, position, color, bgDark, bgLight, ContrastRadius);
			else if (Shadow)
				font.DrawTextWithShadow(text, position, color, bgDark, bgLight, 1);
			else
				font.DrawText(text, position, color);
		}

		int2 GetTextPositionInternal(string text, SpriteFont font, Rectangle rb)
		{
			var textSize = font.Measure(text);
			var y = rb.Y + (Bounds.Height - textSize.Y - font.TopOffset) / 2;

			switch (Align)
			{
				case TextAlign.Left:
					return new int2(rb.X + LeftMargin, y);
				case TextAlign.Center:
				default:
					return new int2(rb.X + (UsableWidth - textSize.X) / 2, y);
				case TextAlign.Right:
					return new int2(rb.X + UsableWidth - textSize.X - RightMargin, y);
			}
		}

		public override MarqueeDropDownButtonWidget Clone() { return new MarqueeDropDownButtonWidget(this); }
	}
}
