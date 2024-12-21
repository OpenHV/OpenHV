#region Copyright & License Information
/*
 * Copyright 2019-2024 The OpenHV Developers (see CREDITS)
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
	public class CustomMainMenuLogic : ChromeLogic
	{
		[FluentReference]
		const string LoadingNews = "label-loading-news";

		[FluentReference("author", "datetime")]
		const string AuthorDateTime = "label-author-datetime";

		protected enum MenuType { Main, Singleplayer, Extras, MapEditor, StartupPrompts, None }

		protected enum MenuPanel { None, Missions, Skirmish, Multiplayer, MapEditor, Replays, GameSaves }

		protected MenuType menuType = MenuType.Main;
		readonly Widget rootMenu;
		readonly ScrollPanelWidget newsPanel;
		readonly Widget newsTemplate;
		readonly LabelWidget newsStatus;

		protected static MenuPanel lastGameState = MenuPanel.None;

		bool newsOpen;

		void SwitchMenu(MenuType type)
		{
			menuType = type;

			DiscordService.UpdateStatus(DiscordState.InMenu);

			// Update button mouseover
			Game.RunAfterTick(Ui.ResetTooltips);
		}

		[ObjectCreator.UseCtor]
		public CustomMainMenuLogic(Widget widget, World world, ModData modData)
		{
			rootMenu = widget;

			// Menu buttons
			var mainMenu = widget.Get("MAIN_MENU");
			mainMenu.IsVisible = () => menuType == MenuType.Main;

			mainMenu.Get<ButtonWidget>("SINGLEPLAYER_BUTTON").OnClick = () => SwitchMenu(MenuType.Singleplayer);

			mainMenu.Get<ButtonWidget>("MULTIPLAYER_BUTTON").OnClick = OpenMultiplayerPanel;

			mainMenu.Get<ButtonWidget>("SETTINGS_BUTTON").OnClick = () =>
			{
				SwitchMenu(MenuType.None);
				Game.OpenWindow("SETTINGS_PANEL", new WidgetArgs
				{
					{ "onExit", () => SwitchMenu(MenuType.Main) }
				});
			};

			mainMenu.Get<ButtonWidget>("EXTRAS_BUTTON").OnClick = () => SwitchMenu(MenuType.Extras);

			mainMenu.Get<ButtonWidget>("ENCYCLOPEDIA_BUTTON").OnClick = OpenEncyclopediaPanel;

			mainMenu.Get<ButtonWidget>("QUIT_BUTTON").OnClick = Game.Exit;

			// Singleplayer menu
			var singleplayerMenu = widget.Get("SINGLEPLAYER_MENU");
			singleplayerMenu.IsVisible = () => menuType == MenuType.Singleplayer;

			var missionsButton = singleplayerMenu.Get<ButtonWidget>("MISSIONS_BUTTON");
			missionsButton.OnClick = () => OpenMissionBrowserPanel(modData.MapCache.PickLastModifiedMap(MapVisibility.MissionSelector));

			var hasCampaign = modData.Manifest.Missions.Length > 0;
			var hasMissions = modData.MapCache
				.Any(p => p.Status == MapStatus.Available && p.Visibility.HasFlag(MapVisibility.MissionSelector));

			missionsButton.Disabled = !hasCampaign && !hasMissions;

			var hasMaps = modData.MapCache.Any(p => p.Visibility.HasFlag(MapVisibility.Lobby));
			var skirmishButton = singleplayerMenu.Get<ButtonWidget>("SKIRMISH_BUTTON");
			skirmishButton.OnClick = StartSkirmishGame;
			skirmishButton.Disabled = !hasMaps;

			var loadButton = singleplayerMenu.Get<ButtonWidget>("LOAD_BUTTON");
			loadButton.IsDisabled = () => !GameSaveBrowserLogic.IsLoadPanelEnabled(modData.Manifest);
			loadButton.OnClick = OpenGameSaveBrowserPanel;

			singleplayerMenu.Get<ButtonWidget>("BACK_BUTTON").OnClick = () => SwitchMenu(MenuType.Main);

			// Extras menu
			var extrasMenu = widget.Get("EXTRAS_MENU");
			extrasMenu.IsVisible = () => menuType == MenuType.Extras;

			extrasMenu.Get<ButtonWidget>("REPLAYS_BUTTON").OnClick = OpenReplayBrowserPanel;

			extrasMenu.Get<ButtonWidget>("MUSIC_BUTTON").OnClick = () =>
			{
				SwitchMenu(MenuType.None);
				Ui.OpenWindow("MUSIC_PANEL", new WidgetArgs
				{
					{ "onExit", () => SwitchMenu(MenuType.Extras) },
					{ "world", world }
				});
			};

			extrasMenu.Get<ButtonWidget>("MAP_EDITOR_BUTTON").OnClick = () => SwitchMenu(MenuType.MapEditor);

			var assetBrowserButton = extrasMenu.GetOrNull<ButtonWidget>("ASSETBROWSER_BUTTON");
			if (assetBrowserButton != null)
				assetBrowserButton.OnClick = () =>
				{
					SwitchMenu(MenuType.None);
					Game.OpenWindow("ASSETBROWSER_PANEL", new WidgetArgs
					{
						{ "onExit", () => SwitchMenu(MenuType.Extras) },
					});
				};

			extrasMenu.Get<ButtonWidget>("CREDITS_BUTTON").OnClick = () =>
			{
				SwitchMenu(MenuType.None);
				Ui.OpenWindow("CREDITS_PANEL", new WidgetArgs
				{
					{ "onExit", () => SwitchMenu(MenuType.Extras) },
				});
			};

			extrasMenu.Get<ButtonWidget>("BACK_BUTTON").OnClick = () => SwitchMenu(MenuType.Main);

			// Map editor menu
			var mapEditorMenu = widget.Get("MAP_EDITOR_MENU");
			mapEditorMenu.IsVisible = () => menuType == MenuType.MapEditor;

			// Loading into the map editor
			Game.BeforeGameStart += RemoveShellmapUI;

			var onSelect = new Action<string>(uid => LoadMapIntoEditor(modData.MapCache[uid].Uid));

			var newMapButton = widget.Get<ButtonWidget>("NEW_MAP_BUTTON");
			newMapButton.OnClick = () =>
			{
				SwitchMenu(MenuType.None);
				Game.OpenWindow("NEW_MAP_BG", new WidgetArgs()
				{
					{ "onSelect", onSelect },
					{ "onExit", () => SwitchMenu(MenuType.MapEditor) }
				});
			};

			var loadMapButton = widget.Get<ButtonWidget>("LOAD_MAP_BUTTON");
			loadMapButton.OnClick = () =>
			{
				SwitchMenu(MenuType.None);
				Game.OpenWindow("MAPCHOOSER_PANEL", new WidgetArgs()
				{
					{ "initialMap", null },
					{ "remoteMapPool", null },
					{ "initialTab", MapClassification.User },
					{ "onExit", () => SwitchMenu(MenuType.MapEditor) },
					{ "onSelect", onSelect },
					{ "filter", MapVisibility.Lobby | MapVisibility.Shellmap | MapVisibility.MissionSelector },
				});
			};

			loadMapButton.Disabled = !hasMaps;

			mapEditorMenu.Get<ButtonWidget>("BACK_BUTTON").OnClick = () => SwitchMenu(MenuType.Extras);

			var newsBG = widget.GetOrNull("NEWS_BG");
			if (newsBG != null)
			{
				newsBG.IsVisible = () => Game.Settings.Game.FetchNews && menuType != MenuType.None && menuType != MenuType.StartupPrompts;

				newsPanel = Ui.LoadWidget<ScrollPanelWidget>("NEWS_PANEL", null, new WidgetArgs());
				newsTemplate = newsPanel.Get("NEWS_ITEM_TEMPLATE");
				newsPanel.RemoveChild(newsTemplate);

				newsStatus = newsPanel.Get<LabelWidget>("NEWS_STATUS");
				SetNewsStatus(FluentProvider.GetString(LoadingNews));
			}

			Game.OnRemoteDirectConnect += OnRemoteDirectConnect;

			// Check for updates in the background
			var webServices = modData.Manifest.Get<GitHubWebServices>();
			if (Game.Settings.Debug.CheckVersion)
				webServices.FetchRelease(() => LoadAndDisplayNews(webServices, newsBG));

			var updateLabel = rootMenu.GetOrNull("UPDATE_NOTICE");
			if (updateLabel != null)
				updateLabel.IsVisible = () => !newsOpen && menuType != MenuType.None &&
					menuType != MenuType.StartupPrompts &&
					webServices.ModVersionStatus == ModVersionStatus.Outdated;

			var playerProfile = widget.GetOrNull("PLAYER_PROFILE_CONTAINER");
			if (playerProfile != null)
			{
				Func<bool> minimalProfile = () => Ui.CurrentWindow() != null;
				Game.LoadWidget(world, "LOCAL_PROFILE_PANEL", playerProfile, new WidgetArgs()
				{
					{ "minimalProfile", minimalProfile }
				});
			}

			menuType = MenuType.StartupPrompts;

			if (IntroductionPromptLogic.ShouldShowPrompt())
			{
				Game.OpenWindow("MAINMENU_INTRODUCTION_PROMPT", new WidgetArgs
				{
					{ "onComplete", () => SwitchMenu(MenuType.Main) }
				});
			}
			else
				SwitchMenu(MenuType.Main);

			Game.OnShellmapLoaded += OpenMenuBasedOnLastGame;

			DiscordService.UpdateStatus(DiscordState.InMenu);
		}

		void LoadAndDisplayNews(GitHubWebServices webServices, Widget newsBG)
		{
			if (!Game.Settings.Game.FetchNews)
				return;

			if (newsBG != null)
			{
				var newsButton = newsBG.GetOrNull<DropDownButtonWidget>("NEWS_BUTTON");
				if (newsButton != null)
				{
					DisplayNews(webServices);
					newsButton.OnClick = () => OpenNewsPanel(newsButton);

					if (menuType == MenuType.None || menuType == MenuType.StartupPrompts)
						return;

					if (webServices.NewsAlert)
						OpenNewsPanel(newsButton);
				}
			}
		}

		void DisplayNews(GitHubWebServices webServices)
		{
			newsPanel.RemoveChildren();
			SetNewsStatus("");

			var newsWidget = newsTemplate.Clone();

			var titleLabel = newsWidget.Get<LabelWidget>("TITLE");
			var newsItem = webServices.NewsItem;
			if (newsItem == null)
			{
				Log.Write("debug", "News retrieval failed.");
				return;
			}

			titleLabel.GetText = () => newsItem.Title;

			var authorDateTimeLabel = newsWidget.Get<LabelWidget>("AUTHOR_DATETIME");
			var authorDateTime = FluentProvider.GetString(AuthorDateTime,
					"author", newsItem.Author,
					"datetime", newsItem.DateTime.ToLocalTime());

			authorDateTimeLabel.GetText = () => authorDateTime;

			var contentLabel = newsWidget.Get<LabelWidget>("CONTENT");
			var content = newsItem.Content;

			content = WidgetUtils.WrapText(content, contentLabel.Bounds.Width, Game.Renderer.Fonts[contentLabel.Font]);
			contentLabel.GetText = () => content;
			contentLabel.Bounds.Height = Game.Renderer.Fonts[contentLabel.Font].Measure(content).Y;
			newsWidget.Bounds.Height += contentLabel.Bounds.Height;

			newsPanel.AddChild(newsWidget);
			newsPanel.Layout.AdjustChildren();
		}

		void OpenNewsPanel(DropDownButtonWidget button)
		{
			newsOpen = true;
			button.AttachPanel(newsPanel, () => newsOpen = false);
		}

		void OnRemoteDirectConnect(ConnectionTarget endpoint)
		{
			SwitchMenu(MenuType.None);
			Ui.OpenWindow("MULTIPLAYER_PANEL", new WidgetArgs
			{
				{ "onStart", RemoveShellmapUI },
				{ "onExit", () => SwitchMenu(MenuType.Main) },
				{ "directConnectEndPoint", endpoint },
			});
		}

		static void LoadMapIntoEditor(string uid)
		{
			// HACK: Work around a synced-code change check.
			// It's not clear why this is needed here, but not in the other places that load maps.
			Game.RunAfterTick(() => Game.LoadEditor(uid));

			DiscordService.UpdateStatus(DiscordState.InMapEditor);

			lastGameState = MenuPanel.MapEditor;
		}

		void SetNewsStatus(string message)
		{
			message = WidgetUtils.WrapText(message, newsStatus.Bounds.Width, Game.Renderer.Fonts[newsStatus.Font]);
			newsStatus.GetText = () => message;
		}

		void RemoveShellmapUI()
		{
			rootMenu.Parent.RemoveChild(rootMenu);
		}

		void StartSkirmishGame()
		{
			var map = Game.ModData.MapCache.ChooseInitialMap(Game.Settings.Server.Map, Game.CosmeticRandom);
			Game.Settings.Server.Map = map;
			Game.Settings.Save();

			ConnectionLogic.Connect(Game.CreateLocalServer(map),
				"",
				OpenSkirmishLobbyPanel,
				() => { Game.CloseServer(); SwitchMenu(MenuType.Main); });
		}

		void OpenMissionBrowserPanel(string map)
		{
			SwitchMenu(MenuType.None);
			Game.OpenWindow("MISSIONBROWSER_PANEL", new WidgetArgs
			{
				{ "onExit", () => SwitchMenu(MenuType.Singleplayer) },
				{ "onStart", () => { RemoveShellmapUI(); lastGameState = MenuPanel.Missions; } },
				{ "initialMap", map }
			});
		}

		void OpenEncyclopediaPanel()
		{
			SwitchMenu(MenuType.None);
			Game.OpenWindow("ENCYCLOPEDIA_PANEL", new WidgetArgs
			{
				{ "onExit", () => SwitchMenu(MenuType.Main) }
			});
		}

		void OpenSkirmishLobbyPanel()
		{
			SwitchMenu(MenuType.None);
			Game.OpenWindow("SERVER_LOBBY", new WidgetArgs
			{
				{ "onExit", () => { Game.Disconnect(); SwitchMenu(MenuType.Singleplayer); } },
				{ "onStart", () => { RemoveShellmapUI(); lastGameState = MenuPanel.Skirmish; } },
				{ "skirmishMode", true }
			});
		}

		void OpenMultiplayerPanel()
		{
			SwitchMenu(MenuType.None);
			Ui.OpenWindow("MULTIPLAYER_PANEL", new WidgetArgs
			{
				{ "onStart", () => { RemoveShellmapUI(); lastGameState = MenuPanel.Multiplayer; } },
				{ "onExit", () => SwitchMenu(MenuType.Main) },
				{ "directConnectEndPoint", null },
			});
		}

		void OpenReplayBrowserPanel()
		{
			SwitchMenu(MenuType.None);
			Ui.OpenWindow("REPLAYBROWSER_PANEL", new WidgetArgs
			{
				{ "onExit", () => SwitchMenu(MenuType.Extras) },
				{ "onStart", () => { RemoveShellmapUI(); lastGameState = MenuPanel.Replays; } }
			});
		}

		void OpenGameSaveBrowserPanel()
		{
			SwitchMenu(MenuType.None);
			Ui.OpenWindow("GAMESAVE_BROWSER_PANEL", new WidgetArgs
			{
				{ "onExit", () => SwitchMenu(MenuType.Singleplayer) },
				{ "onStart", () => { RemoveShellmapUI(); lastGameState = MenuPanel.GameSaves; } },
				{ "isSavePanel", false },
				{ "world", null }
			});
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Game.OnRemoteDirectConnect -= OnRemoteDirectConnect;
				Game.BeforeGameStart -= RemoveShellmapUI;
			}

			Game.OnShellmapLoaded -= OpenMenuBasedOnLastGame;
			base.Dispose(disposing);
		}

		void OpenMenuBasedOnLastGame()
		{
			switch (lastGameState)
			{
				case MenuPanel.Missions:
					OpenMissionBrowserPanel(null);
					break;

				case MenuPanel.Replays:
					OpenReplayBrowserPanel();
					break;

				case MenuPanel.Skirmish:
					StartSkirmishGame();
					break;

				case MenuPanel.Multiplayer:
					OpenMultiplayerPanel();
					break;

				case MenuPanel.MapEditor:
					SwitchMenu(MenuType.MapEditor);
					break;

				case MenuPanel.GameSaves:
					SwitchMenu(MenuType.Singleplayer);
					break;
			}

			lastGameState = MenuPanel.None;
		}
	}
}
