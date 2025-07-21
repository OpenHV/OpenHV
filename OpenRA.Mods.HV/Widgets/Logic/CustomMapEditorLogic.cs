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
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Widgets;
using OpenRA.Mods.HV.Traits;
using OpenRA.Widgets;

namespace OpenRA.Mods.HV.Widgets.Logic
{
	public class CustomMapEditorLogic : ChromeLogic
	{
		[ObjectCreator.UseCtor]
		public CustomMapEditorLogic(Widget widget, World world, WorldRenderer worldRenderer)
		{
			var editorViewport = widget.Get<EditorViewportControllerWidget>("MAP_EDITOR");

			var autoTileButton = widget.GetOrNull<ButtonWidget>("AUTOTILE_BUTTON");
			if (autoTileButton != null)
			{
				autoTileButton.OnClick = () =>
				{
					var autoTiler = world.WorldActor.Trait<EditorAutoTiler>();
					autoTiler.CleanEdges();
				};
			}

			var coordinateLabel = widget.GetOrNull<LabelWidget>("COORDINATE_LABEL");
			if (coordinateLabel != null)
			{
				coordinateLabel.GetText = () =>
				{
					var cell = worldRenderer.Viewport.ViewToWorld(Viewport.LastMousePos);
					var map = worldRenderer.World.Map;
					return map.Height.Contains(cell) ? $"{cell},{map.Height[cell]} ({map.Tiles[cell].Type})" : "";
				};
			}

			var undoButton = widget.GetOrNull<ButtonWidget>("UNDO_BUTTON");
			var redoButton = widget.GetOrNull<ButtonWidget>("REDO_BUTTON");
			if (undoButton != null && redoButton != null)
			{
				var actionManager = world.WorldActor.Trait<EditorActionManager>();
				undoButton.IsDisabled = () => !actionManager.HasUndos();
				undoButton.OnClick = actionManager.Undo;
				redoButton.IsDisabled = () => !actionManager.HasRedos();
				redoButton.OnClick = actionManager.Redo;
			}
		}
	}
}
