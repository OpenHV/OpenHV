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

using OpenRA.Widgets;

namespace OpenRA.Mods.HV.Widgets.Logic
{
	public class AssetBrowserLauncherLogic : ChromeLogic
	{
		[ObjectCreator.UseCtor]
		public AssetBrowserLauncherLogic()
		{
			Game.OpenWindow("ASSETBROWSER_PANEL", new WidgetArgs
			{
				{ "onExit", Game.Exit },
			});
		}
	}
}
