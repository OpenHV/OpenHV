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

using System;
using OpenRA.Mods.Common.Widgets;
using OpenRA.Primitives;
using OpenRA.Widgets;

namespace OpenRA.Mods.HV.Widgets
{
	public class MarqueeLabelWidget : LabelWidget
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

		[ObjectCreator.UseCtor]
		public MarqueeLabelWidget(ModData modData)
			: base(modData)
		{
			lastTickTime = Game.RunTime;
			pauseStartTime = Game.RunTime;
		}

		protected MarqueeLabelWidget(MarqueeLabelWidget other)
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
			return Ui.MouseOverWidget == this;
		}

		public override void Tick()
		{
			base.Tick();

			var currentTime = Game.RunTime;
			var deltaTime = currentTime - lastTickTime;
			lastTickTime = currentTime;

			if (!Game.Renderer.Fonts.TryGetValue(Font, out var font))
				return;

			var text = GetText();
			if (text == null)
				return;

			if (WordWrap)
				return;

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
			var availableWidth = Bounds.Width;

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
			if (!Game.Renderer.Fonts.TryGetValue(Font, out var font))
				throw new ArgumentException($"Requested font '{Font}' was not found.");

			var text = GetText();
			if (text == null)
				return;

			if (WordWrap)
			{
				base.Draw();
				return;
			}

			var textSize = font.Measure(text);
			var availableWidth = Bounds.Width;

			if (textSize.X <= availableWidth)
			{
				base.Draw();
				return;
			}

			var rb = RenderBounds;
			var scissorRect = new Rectangle(rb.X, rb.Y, availableWidth, rb.Height);

			var position = RenderOrigin;
			var offset = font.TopOffset;

			if (VAlign == TextVAlign.Top)
				position += new int2(0, -offset);
			else if (VAlign == TextVAlign.Middle)
				position += new int2(0, (Bounds.Height - textSize.Y - offset) / 2);
			else if (VAlign == TextVAlign.Bottom)
				position += new int2(0, Bounds.Height - textSize.Y);

			position = new int2(position.X - (int)scrollOffset, position.Y);

			Game.Renderer.EnableScissor(scissorRect);
			DrawInner(text, font, GetColor(), position);
			Game.Renderer.DisableScissor();
		}

		public override MarqueeLabelWidget Clone() { return new MarqueeLabelWidget(this); }
	}
}
