#region Copyright & License Information
/*
 * Copyright 2025-2026 The OpenHV Developers (see CREDITS)
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
		[FluentReference]
		const string AppliesToArea = "label-applies-to-area";

		[FluentReference]
		const string AppliesToWholeMap = "label-applies-to-whole-map";

		public LabelWidget AreaSelectionLabel;

		readonly EditorViewportControllerWidget editor;

		bool cliff;

		[ObjectCreator.UseCtor]
		public AutoTilerLogic(Widget widget, World world, ModData modData, WorldRenderer worldRenderer, Dictionary<string, MiniYaml> logicArgs)
		{
			var editorActionManager = world.WorldActor.Trait<EditorActionManager>();
			editor = widget.Parent.Parent.Parent.Parent.Get<EditorViewportControllerWidget>("MAP_EDITOR");

			var autoTileButton = widget.Get<ButtonWidget>("AUTOTILE_BUTTON");
			autoTileButton.OnClick = () =>
			{
				var selection = editor.DefaultBrush.Selection.Area ?? world.Map.AllCells.CellCoords;
				editorActionManager.Add(new AutoConnectEditorAction(world.Map, selection, cliff));
			};

			var cliffCheckboxWidget = widget.Get<CheckboxWidget>("CLIFF_CHECKBOX");
			cliffCheckboxWidget.IsChecked = () => cliff;
			cliffCheckboxWidget.OnClick = () => cliff ^= true;

			editor.DefaultBrush.SelectionChanged += HandleSelectionChanged;
			AreaSelectionLabel = widget.Get<LabelWidget>("AREA_SELECTION");
			HandleSelectionChanged();
		}

		protected override void Dispose(bool disposing)
		{
			editor.DefaultBrush.SelectionChanged -= HandleSelectionChanged;
			base.Dispose(disposing);
		}

		void HandleSelectionChanged()
		{
			var selectedRegion = editor.DefaultBrush.Selection.Area;
			if (selectedRegion == null)
			{
				AreaSelectionLabel.GetText = () => FluentProvider.GetMessage(AppliesToWholeMap);
				return;
			}

			var selectionSize = selectedRegion.Value.BottomRight - selectedRegion.Value.TopLeft + new CPos(1, 1);

			var areaSelectionLabel =
				$"{FluentProvider.GetMessage(AppliesToArea)} ({DimensionsAsString(selectionSize)}) " +
				$"{PositionAsString(selectedRegion.Value.TopLeft)} : {PositionAsString(selectedRegion.Value.BottomRight)}";

			AreaSelectionLabel.GetText = () => areaSelectionLabel;
		}

		static string PositionAsString(CPos cell) => $"{cell.X},{cell.Y}";
		static string DimensionsAsString(CPos cell) => $"{cell.X}x{cell.Y}";
	}
}
