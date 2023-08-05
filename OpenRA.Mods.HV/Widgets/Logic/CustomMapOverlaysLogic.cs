#region Copyright & License Information
/*
 * Copyright 2023 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using OpenRA.Mods.Common.Lint;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Widgets;
using OpenRA.Mods.HV.Traits;
using OpenRA.Widgets;

namespace OpenRA.Mods.HV.Widgets.Logic
{
	[ChromeLogicArgsHotkeys("ToggleGridOverlayKey", "ToggleBuildableOverlayKey", "ToggleTerrainTypeOverlayKey")]
	public class CustomMapOverlaysLogic : ChromeLogic
	{
		[Flags]
		enum MapOverlays
		{
			None = 0,
			Grid = 1,
			Buildable = 2,
			Type = 4,
		}

		readonly TerrainGeometryOverlay terrainGeometryTrait;
		readonly BuildableTerrainOverlay buildableTerrainTrait;
		readonly TerrainTypeOverlay terrainTypeTrait;

		[ObjectCreator.UseCtor]
		public CustomMapOverlaysLogic(Widget widget, World world, ModData modData, Dictionary<string, MiniYaml> logicArgs)
		{
			terrainGeometryTrait = world.WorldActor.Trait<TerrainGeometryOverlay>();
			buildableTerrainTrait = world.WorldActor.Trait<BuildableTerrainOverlay>();
			terrainTypeTrait = world.WorldActor.Trait<TerrainTypeOverlay>();

			var toggleGridKey = new HotkeyReference();
			if (logicArgs.TryGetValue("ToggleGridOverlayKey", out var yaml))
				toggleGridKey = modData.Hotkeys[yaml.Value];

			var toggleBuildableKey = new HotkeyReference();
			if (logicArgs.TryGetValue("ToggleBuildableOverlayKey", out yaml))
				toggleBuildableKey = modData.Hotkeys[yaml.Value];

			var toggleTerrainTypeKey = new HotkeyReference();
			if (logicArgs.TryGetValue("ToggleTerrainTypeOverlayKey", out yaml))
				toggleTerrainTypeKey = modData.Hotkeys[yaml.Value];

			var keyhandler = widget.Get<LogicKeyListenerWidget>("OVERLAY_KEYHANDLER");
			keyhandler.AddHandler(e =>
			{
				if (e.Event != KeyInputEvent.Down)
					return false;

				if (toggleGridKey.IsActivatedBy(e))
				{
					terrainGeometryTrait.Enabled ^= true;
					return true;
				}

				if (toggleBuildableKey.IsActivatedBy(e))
				{
					buildableTerrainTrait.Enabled ^= true;
					return true;
				}

				if (toggleTerrainTypeKey.IsActivatedBy(e))
				{
					terrainTypeTrait.Enabled ^= true;
					return true;
				}

				return false;
			});

			var overlayPanel = CreateOverlaysPanel();

			var overlayDropdown = widget.GetOrNull<DropDownButtonWidget>("OVERLAY_BUTTON");
			if (overlayDropdown != null)
			{
				overlayDropdown.OnMouseDown = _ =>
				{
					overlayDropdown.RemovePanel();
					overlayDropdown.AttachPanel(overlayPanel);
				};
			}
		}

		Widget CreateOverlaysPanel()
		{
			var categoriesPanel = Ui.LoadWidget("OVERLAY_PANEL", null, new WidgetArgs());
			var categoryTemplate = categoriesPanel.Get<CheckboxWidget>("CATEGORY_TEMPLATE");

			MapOverlays[] allCategories = { MapOverlays.Grid, MapOverlays.Buildable, MapOverlays.Type };
			foreach (var cat in allCategories)
			{
				var category = (CheckboxWidget)categoryTemplate.Clone();
				category.GetText = () => cat.ToString();
				category.IsVisible = () => true;

				if (cat.HasFlag(MapOverlays.Grid))
				{
					category.IsChecked = () => terrainGeometryTrait.Enabled;
					category.OnClick = () => terrainGeometryTrait.Enabled ^= true;
				}
				else if (cat.HasFlag(MapOverlays.Buildable))
				{
					category.IsChecked = () => buildableTerrainTrait.Enabled;
					category.OnClick = () => buildableTerrainTrait.Enabled ^= true;
				}
                else if (cat.HasFlag(MapOverlays.Type))
				{
					category.IsChecked = () => terrainTypeTrait.Enabled;
					category.OnClick = () => terrainTypeTrait.Enabled ^= true;
				}

				categoriesPanel.AddChild(category);
			}

			return categoriesPanel;
		}
	}
}
