#region Copyright & License Information
/*
 * Copyright 2026 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using OpenRA.Mods.Common.Widgets;
using OpenRA.Widgets;

namespace OpenRA.Mods.HV.Widgets.Logic
{
	public class DevelopmentMainMenuLogic : ChromeLogic
	{
		bool menuVisible = true;

		[ObjectCreator.UseCtor]
		public DevelopmentMainMenuLogic(Widget widget)
		{
			var mainMenu = widget.Get("MAIN_MENU");
			mainMenu.IsVisible = () => menuVisible;

			mainMenu.Get<ButtonWidget>("ASSETBROWSER_BUTTON").OnClick = () =>
			{
				menuVisible = false;
				Game.OpenWindow("ASSETBROWSER_PANEL", new WidgetArgs
				{
					{ "onExit", () => menuVisible = true },
				});
			};

			mainMenu.Get<ButtonWidget>("TRANSLATIONS_BUTTON").OnClick = () =>
			{
				menuVisible = false;
				Game.OpenWindow("TRANSLATIONS_PANEL", new WidgetArgs
				{
					{ "onExit", () => menuVisible = true },
				});
			};

			mainMenu.Get<ButtonWidget>("QUIT_BUTTON").OnClick = Game.Exit;
		}
	}
}
