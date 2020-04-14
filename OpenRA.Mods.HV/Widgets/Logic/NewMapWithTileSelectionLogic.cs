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

using System;
using System.Linq;
using OpenRA.Primitives;
using OpenRA.Widgets;

namespace OpenRA.Mods.Common.Widgets.Logic
{
	public class NewMapWithTileSelectionLogic : ChromeLogic
	{
		readonly Widget panel;

		[ObjectCreator.UseCtor]
		public NewMapWithTileSelectionLogic(Action onExit, Action<string> onSelect, Widget widget, World world, ModData modData)
		{
			panel = widget;

			panel.Get<ButtonWidget>("CANCEL_BUTTON").OnClick = () => { Ui.CloseWindow(); onExit(); };

			var tileDropDown = panel.Get<DropDownButtonWidget>("TILE");

			var tileset = modData.DefaultTileSets.First().Value;

			var clearTiles = tileset.Templates.Where(t => t.Value.PickAny)
				.Select(t => new Pair<ushort, string>(t.Value.Id, t.Value.Categories.First()));

			Func<string, ScrollItemWidget, ScrollItemWidget> setupItem = (option, template) =>
			{
				var item = ScrollItemWidget.Setup(template,
					() => tileDropDown.Text == option,
					() => { tileDropDown.Text = option; });
				item.Get<LabelWidget>("LABEL").GetText = () => option;
				return item;
			};
			tileDropDown.Text = clearTiles.First().Second;
			var options = clearTiles.Select(t => t.Second);
			tileDropDown.OnClick = () =>
				tileDropDown.ShowDropDown("LABEL_DROPDOWN_TEMPLATE", 210, options, setupItem);

			var widthTextField = panel.Get<TextFieldWidget>("WIDTH");
			var heightTextField = panel.Get<TextFieldWidget>("HEIGHT");

			panel.Get<ButtonWidget>("CREATE_BUTTON").OnClick = () =>
			{
				int width, height;
				int.TryParse(widthTextField.Text, out width);
				int.TryParse(heightTextField.Text, out height);

				// Require at least a 2x2 playable area so that the
				// ground is visible through the edge shroud
				width = Math.Max(2, width);
				height = Math.Max(2, height);

				var maxTerrainHeight = world.Map.Grid.MaximumTerrainHeight;
				var map = new Map(Game.ModData, tileset, width + 2, height + maxTerrainHeight + 2);

				var tl = new PPos(1, 1 + maxTerrainHeight);
				var br = new PPos(width, height + maxTerrainHeight);
				map.SetBounds(tl, br);

				var type = clearTiles.First(c => c.Second == tileDropDown.Text).First;
				for (var j = map.Bounds.Top; j < map.Bounds.Bottom; j++)
				{
					for (var i = map.Bounds.Left; i < map.Bounds.Right; i++)
					{
						var template = tileset.Templates[type];
						map.Tiles[new MPos(i, j)] = new TerrainTile(type, 0);
					}
				}

				map.PlayerDefinitions = new MapPlayers(map.Rules, 0).ToMiniYaml();
				map.FixOpenAreas();

				Action<string> afterSave = uid =>
				{
					// HACK: Work around a synced-code change check.
					// It's not clear why this is needed here, but not in the other places that load maps.
					Game.RunAfterTick(() =>
					{
						ConnectionLogic.Connect(System.Net.IPAddress.Loopback.ToString(),
							Game.CreateLocalServer(uid), "",
							() => Game.LoadEditor(uid),
							() => { Game.CloseServer(); onExit(); });
					});

					Ui.CloseWindow();
					onSelect(uid);
				};

				Ui.OpenWindow("SAVE_MAP_PANEL", new WidgetArgs()
				{
					{ "onSave", afterSave },
					{ "onExit", () => { Ui.CloseWindow(); onExit(); } },
					{ "map", map },
					{ "playerDefinitions", map.PlayerDefinitions },
					{ "actorDefinitions", map.ActorDefinitions }
				});
			};
		}
	}
}
