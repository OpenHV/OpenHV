#region Copyright & License Information
/*
 * Copyright 2019-2021 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Lint;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Widgets;
using OpenRA.Mods.HV.Traits;
using OpenRA.Widgets;

namespace OpenRA.Mods.HV.Widgets.Logic
{
	[ChromeLogicArgsHotkeys("ChangeZoomKey")]
	public class CustomMapEditorLogic : ChromeLogic
	{
		MapCopyFilters copyFilters = MapCopyFilters.All;

		[ObjectCreator.UseCtor]
		public CustomMapEditorLogic(Widget widget, World world, WorldRenderer worldRenderer)
		{
			var editorViewport = widget.Get<EditorViewportControllerWidget>("MAP_EDITOR");

			var gridButton = widget.GetOrNull<ButtonWidget>("GRID_BUTTON");
			if (gridButton != null)
			{
				var terrainGeometryTrait = world.WorldActor.Trait<TerrainGeometryOverlay>();
				gridButton.OnClick = () => terrainGeometryTrait.Enabled ^= true;
				gridButton.IsHighlighted = () => terrainGeometryTrait.Enabled;
			}

			var lockButton = widget.GetOrNull<ButtonWidget>("BUILDABLE_BUTTON");
			if (lockButton != null)
			{
				var buildableTerrainTrait = world.WorldActor.TraitOrDefault<BuildableTerrainOverlay>();
				if (buildableTerrainTrait != null)
				{
					lockButton.OnClick = () => buildableTerrainTrait.Enabled ^= true;
					lockButton.IsHighlighted = () => buildableTerrainTrait.Enabled;
				}
				else
					lockButton.Disabled = true;
			}

			var autoTileButton = widget.GetOrNull<ButtonWidget>("AUTOTILE_BUTTON");
			if (autoTileButton != null)
			{
				autoTileButton.OnClick = () =>
				{
					var autoTiler = world.WorldActor.Trait<EditorAutoTiler>();
					autoTiler.CleanEdges();
				};
			}

			var debugButton = widget.GetOrNull<ButtonWidget>("DEBUG_BUTTON");
			if (debugButton != null)
			{
				var terrainDebugTrait = world.WorldActor.Trait<TerrainDebugOverlay>();
				debugButton.OnClick = () => terrainDebugTrait.Enabled ^= true;
				debugButton.IsHighlighted = () => terrainDebugTrait.Enabled;
			}

			var copypasteButton = widget.GetOrNull<ButtonWidget>("COPYPASTE_BUTTON");
			if (copypasteButton != null)
			{
				// HACK: Replace Ctrl with Cmd on macOS
				// TODO: Add platform-specific override support to HotkeyManager
				// and then port the editor hotkeys to this system.
				var copyPasteKey = copypasteButton.Key.GetValue();
				if (Platform.CurrentPlatform == PlatformType.OSX && copyPasteKey.Modifiers.HasModifier(Modifiers.Ctrl))
				{
					var modified = new Hotkey(copyPasteKey.Key, copyPasteKey.Modifiers & ~Modifiers.Ctrl | Modifiers.Meta);
					copypasteButton.Key = FieldLoader.GetValue<HotkeyReference>("Key", modified.ToString());
				}

				copypasteButton.OnClick = () => editorViewport.SetBrush(new EditorCopyPasteBrush(editorViewport, worldRenderer, () => copyFilters));
				copypasteButton.IsHighlighted = () => editorViewport.CurrentBrush is EditorCopyPasteBrush;
			}

			var copyFilterDropdown = widget.Get<DropDownButtonWidget>("COPYFILTER_BUTTON");
			copyFilterDropdown.OnMouseDown = _ =>
			{
				copyFilterDropdown.RemovePanel();
				copyFilterDropdown.AttachPanel(CreateCategoriesPanel());
			};

			var coordinateLabel = widget.GetOrNull<LabelWidget>("COORDINATE_LABEL");
			if (coordinateLabel != null)
			{
				coordinateLabel.GetText = () =>
				{
					var cell = worldRenderer.Viewport.ViewToWorld(Viewport.LastMousePos);
					var map = worldRenderer.World.Map;
					return map.Height.Contains(cell) ?
						"{0},{1} ({2})".F(cell, map.Height[cell], map.Tiles[cell].Type) : "";
				};
			}

			var undoButton = widget.GetOrNull<ButtonWidget>("UNDO_BUTTON");
			var redoButton = widget.GetOrNull<ButtonWidget>("REDO_BUTTON");
			if (undoButton != null && redoButton != null)
			{
				var actionManager = world.WorldActor.Trait<EditorActionManager>();
				undoButton.IsDisabled = () => !actionManager.HasUndos();
				undoButton.OnClick = () => actionManager.Undo();
				redoButton.IsDisabled = () => !actionManager.HasRedos();
				redoButton.OnClick = () => actionManager.Redo();
			}
		}

		Widget CreateCategoriesPanel()
		{
			var categoriesPanel = Ui.LoadWidget("COPY_FILTER_PANEL", null, new WidgetArgs());
			var categoryTemplate = categoriesPanel.Get<CheckboxWidget>("CATEGORY_TEMPLATE");

			MapCopyFilters[] allCategories = { MapCopyFilters.Terrain, MapCopyFilters.Resources, MapCopyFilters.Actors };
			foreach (var cat in allCategories)
			{
				var category = (CheckboxWidget)categoryTemplate.Clone();
				category.GetText = () => cat.ToString();
				category.IsChecked = () => copyFilters.HasFlag(cat);
				category.IsVisible = () => true;
				category.OnClick = () => copyFilters ^= cat;

				categoriesPanel.AddChild(category);
			}

			return categoriesPanel;
		}
	}
}
