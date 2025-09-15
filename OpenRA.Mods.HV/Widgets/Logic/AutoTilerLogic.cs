#region Copyright & License Information
/*
 * Copyright 2025 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Collections.Generic;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Widgets;
using OpenRA.Mods.HV.Traits;
using OpenRA.Widgets;

namespace OpenRA.Mods.HV.Widgets.Logic
{
	[IncludeStaticFluentReferences(typeof(EditorAutoTilerInfo))]
	public sealed class AutoTilerLogic : ChromeLogic
	{
		bool cliff;

		[ObjectCreator.UseCtor]
		public AutoTilerLogic(Widget widget, World world, ModData modData, WorldRenderer worldRenderer, Dictionary<string, MiniYaml> logicArgs)
		{
			var editorActionManager = world.WorldActor.Trait<EditorActionManager>();
			var editor = widget.Parent.Parent.Parent.Parent.Get<EditorViewportControllerWidget>("MAP_EDITOR");

			var autoTileButton = widget.Get<ButtonWidget>("AUTOTILE_BUTTON");
			autoTileButton.OnClick = () =>
			{
				var area = editor.DefaultBrush.Selection.Area ?? world.Map.AllCells;
				editorActionManager.Add(new AutoConnectEditorAction(world.Map, area, cliff));
			};

			var settingsPanel = widget.Get<ScrollPanelWidget>("SETTINGS_PANEL");
			var cliffCheckboxWidget = settingsPanel.Get<CheckboxWidget>("CLIFF_CHECKBOX");
			cliffCheckboxWidget.IsChecked = () => cliff;
			cliffCheckboxWidget.OnClick = () => cliff ^= true;
		}
	}
}
