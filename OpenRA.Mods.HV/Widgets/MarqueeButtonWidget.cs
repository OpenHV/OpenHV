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
	public class MarqueeButtonWidget : ButtonWidget
	{
		public float ScrollSpeed = 1.0f;
		public int ScrollPauseMs = 2000;

		float scrollOffset;
		long lastTickTime;
		long pauseStartTime;
		bool isPaused = true;
		bool scrollingBack;
		string lastText = "";
		int lastTextWidth;

		[ObjectCreator.UseCtor]
		public MarqueeButtonWidget(ModData modData)
			: base(modData)
		{
			lastTickTime = Game.RunTime;
			pauseStartTime = Game.RunTime;
		}

		protected MarqueeButtonWidget(MarqueeButtonWidget other)
			: base(other)
		{
			ScrollSpeed = other.ScrollSpeed;
			ScrollPauseMs = other.ScrollPauseMs;
			scrollOffset = 0;
			lastTickTime = Game.RunTime;
			pauseStartTime = Game.RunTime;
			isPaused = true;
			scrollingBack = false;
		}

		public override void Tick()
		{
			base.Tick();

			var currentTime = Game.RunTime;
			var deltaTime = currentTime - lastTickTime;
			lastTickTime = currentTime;

			var font = Game.Renderer.Fonts[Font];
			var text = GetText();

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

			var hover = Ui.MouseOverWidget == this;
			foreach (var child in Children)
			{
				if (child == Ui.MouseOverWidget)
				{
					hover = true;
					break;
				}
			}

			DrawBackground(rb, disabled, Depressed, hover, highlighted);

			var textSize = font.Measure(text);
			var availableWidth = UsableWidth - LeftMargin - RightMargin;

			if (textSize.X <= availableWidth)
			{
				var position = GetTextPositionInternal(text, font, rb);
				DrawText(font, text, position + stateOffset, disabled ? colorDisabled : color, bgDark, bgLight);
				return;
			}

			var textAreaX = rb.X + LeftMargin;
			var textAreaWidth = availableWidth;
			var scissorRect = new Rectangle(textAreaX + stateOffset.X, rb.Y, textAreaWidth, rb.Height);

			var textY = rb.Y + (Bounds.Height - textSize.Y - font.TopOffset) / 2;
			var textPosition = new int2(textAreaX - (int)scrollOffset, textY) + stateOffset;

			Game.Renderer.EnableScissor(scissorRect);
			DrawText(font, text, textPosition, disabled ? colorDisabled : color, bgDark, bgLight);
			Game.Renderer.DisableScissor();
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

		public override MarqueeButtonWidget Clone() { return new MarqueeButtonWidget(this); }
	}
}
