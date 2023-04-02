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
using System.Linq;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Widgets;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Network;
using OpenRA.Widgets;

namespace OpenRA.Mods.HV.Widgets.Logic
{
	public class CustomMultiplayerLogic : ChromeLogic
	{
		static readonly Action DoNothing = () => { };

		readonly Action onStart;
		readonly Action onExit;
		readonly CustomServerListLogic serverListLogic;

		[ObjectCreator.UseCtor]
		public CustomMultiplayerLogic(Widget widget, ModData modData, Action onStart, Action onExit, ConnectionTarget directConnectEndPoint)
		{
			// MultiplayerLogic is a superset of the ServerListLogic
			// but cannot be a direct subclass because it needs to pass object-level state to the constructor
			serverListLogic = new CustomServerListLogic(widget, modData, Join);

			this.onStart = onStart;
			this.onExit = onExit;

			var directConnectButton = widget.Get<ButtonWidget>("DIRECTCONNECT_BUTTON");
			directConnectButton.OnClick = () =>
			{
				Ui.OpenWindow("DIRECTCONNECT_PANEL", new WidgetArgs
				{
					{ "openLobby", OpenLobby },
					{ "onExit", DoNothing },
					{ "directConnectEndPoint", null },
				});
			};

			var createServerButton = widget.Get<ButtonWidget>("CREATE_BUTTON");
			createServerButton.OnClick = () =>
			{
				Ui.OpenWindow("MULTIPLAYER_CREATESERVER_PANEL", new WidgetArgs
				{
					{ "openLobby", OpenLobby },
					{ "onExit", DoNothing }
				});
			};

			var hasMaps = modData.MapCache.Any(p => !p.Visibility.HasFlag(MapVisibility.Shellmap));
			createServerButton.Disabled = !hasMaps;

			widget.Get<ButtonWidget>("BACK_BUTTON").OnClick = () => { Ui.CloseWindow(); onExit(); };
			Game.LoadWidget(null, "GLOBALCHAT_PANEL", widget.Get("GLOBALCHAT_ROOT"), new WidgetArgs());

			if (directConnectEndPoint != null)
			{
				// The connection window must be opened at the end of the tick for the widget hierarchy to
				// work out, but we also want to prevent the server browser from flashing visible for one tick.
				widget.Visible = false;
				Game.RunAfterTick(() =>
				{
					Ui.OpenWindow("DIRECTCONNECT_PANEL", new WidgetArgs
					{
						{ "openLobby", OpenLobby },
						{ "onExit", DoNothing },
						{ "directConnectEndPoint", directConnectEndPoint },
					});

					widget.Visible = true;
				});
			}
		}

		void OpenLobby()
		{
			// Close the multiplayer browser
			Ui.CloseWindow();

			Action onLobbyExit = () =>
			{
				// Open a fresh copy of the multiplayer browser
				Ui.OpenWindow("MULTIPLAYER_PANEL", new WidgetArgs
				{
					{ "onStart", onStart },
					{ "onExit", onExit },
					{ "directConnectEndPoint", null },
				});

				Game.Disconnect();

				DiscordService.UpdateStatus(DiscordState.InMenu);
			};

			Game.OpenWindow("SERVER_LOBBY", new WidgetArgs
			{
				{ "onStart", onStart },
				{ "onExit", onLobbyExit },
				{ "skirmishMode", false }
			});
		}

		void Join(GameServer server)
		{
			if (server == null || !server.IsJoinable)
				return;

			var host = server.Address.Split(':')[0];
			var port = Exts.ParseIntegerInvariant(server.Address.Split(':')[1]);

			ConnectionLogic.Connect(new ConnectionTarget(host, port), "", OpenLobby, DoNothing);
		}

		bool disposed;
		protected override void Dispose(bool disposing)
		{
			if (disposing && !disposed)
			{
				disposed = true;
				serverListLogic.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
