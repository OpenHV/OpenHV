#region Copyright & License Information
/*
 * Copyright 2023-2025 The OpenHV Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Widgets;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Network;
using OpenRA.Primitives;
using OpenRA.Traits;
using OpenRA.Widgets;

namespace OpenRA.Mods.HV.Widgets.Logic
{
	[IncludeStaticFluentReferences(typeof(LobbyUtils))]
	public class CustomLobbyLogic : ChromeLogic, INotificationHandler<TextNotification>
	{
		[FluentReference]
		const string Add = "options-slot-admin.add-bots";

		[FluentReference]
		const string Remove = "options-slot-admin.remove-bots";

		[FluentReference]
		const string ConfigureBots = "options-slot-admin.configure-bots";

		[FluentReference("count")]
		const string NumberTeams = "options-slot-admin.teams-count";

		[FluentReference]
		const string HumanVsBots = "options-slot-admin.humans-vs-bots";

		[FluentReference]
		const string FreeForAll = "options-slot-admin.free-for-all";

		[FluentReference]
		const string ConfigureTeams = "options-slot-admin.configure-teams";

		[FluentReference]
		const string Back = "button-back";

		[FluentReference]
		const string TeamChat = "button-team-chat";

		[FluentReference]
		const string GeneralChat = "button-general-chat";

		[FluentReference("seconds")]
		const string ChatAvailability = "label-chat-availability";

		[FluentReference]
		const string ChatDisabled = "label-chat-disabled";

		static readonly Action DoNothing = () => { };

		readonly ModData modData;
		readonly Action onStart;
		readonly Action onExit;
		readonly OrderManager orderManager;
		readonly WorldRenderer worldRenderer;
		readonly bool skirmishMode;
		readonly Ruleset modRules;
		readonly WebServices services;
		readonly InternetRelayChat internetRelayChat;

		enum PanelType { Players, Options, Music, Servers, Kick, ForceStart }
		PanelType panel = PanelType.Players;

		enum ChatPanelType { Lobby, Global }
		ChatPanelType chatPanel = ChatPanelType.Lobby;
		readonly Widget lobby;
		readonly Widget editablePlayerTemplate;
		readonly Widget nonEditablePlayerTemplate;
		readonly Widget emptySlotTemplate;
		readonly Widget editableSpectatorTemplate;
		readonly Widget nonEditableSpectatorTemplate;
		readonly Widget newSpectatorTemplate;

		readonly ScrollPanelWidget lobbyChatPanel;
		readonly Dictionary<TextNotificationPool, Widget> chatTemplates = [];
		readonly TextFieldWidget chatTextField;
		readonly CachedTransform<int, string> chatAvailableIn;
		readonly string chatDisabled;

		readonly ScrollPanelWidget players;

		readonly Dictionary<string, LobbyFaction> factions = [];

		readonly IColorPickerManagerInfo colorManager;

		readonly TabCompletionLogic tabCompletion = new();

		MapPreview map;
		Session.MapStatus mapStatus;
		MapGenerationArgs lastGeneratedMap;

		bool chatEnabled;
		bool disableTeamChat;
		bool insufficientPlayerSpawns;
		bool teamChat;
		int lobbyChatUnreadMessages;
		int globalChatLastReadMessages;
		int globalChatUnreadMessages;
		bool updateDiscordStatus = true;
		bool resetOptionsButtonEnabled;
		bool mapAvailable;
		Dictionary<int, SpawnOccupant> spawnOccupants = [];

		readonly string chatLineSound;
		readonly string playerJoinedSound;
		readonly string playerLeftSound;
		readonly string lobbyOptionChangedSound;

		bool MapIsPlayable => (mapStatus & Session.MapStatus.Playable) == Session.MapStatus.Playable;

		// Listen for connection failures
		void ConnectionStateChanged(OrderManager om, string password, NetworkConnection connection)
		{
			if (connection.ConnectionState == ConnectionState.NotConnected)
			{
				// Show connection failed dialog
				Ui.CloseWindow();

				void OnConnect()
				{
					Game.OpenWindow("SERVER_LOBBY", new WidgetArgs()
					{
						{ "onExit", onExit },
						{ "onStart", onStart },
						{ "skirmishMode", false }
					});
				}

				Action<string> onRetry = pass => ConnectionLogic.Connect(connection.Target, pass, OnConnect, onExit);

				var switchPanel = CurrentServerSettings.ServerExternalMod != null ? "CONNECTION_SWITCHMOD_PANEL" : "CONNECTIONFAILED_PANEL";
				Ui.OpenWindow(switchPanel, new WidgetArgs()
				{
					{ "orderManager", om },
					{ "connection", connection },
					{ "password", password },
					{ "onAbort", onExit },
					{ "onQuit", null },
					{ "onRetry", onRetry }
				});
			}
		}

		[ObjectCreator.UseCtor]
		internal CustomLobbyLogic(Widget widget, ModData modData, WorldRenderer worldRenderer, OrderManager orderManager,
			Action onExit, Action onStart, bool skirmishMode, Dictionary<string, MiniYaml> logicArgs)
		{
			map = MapCache.UnknownMap;
			lobby = widget;
			this.modData = modData;
			this.orderManager = orderManager;
			this.worldRenderer = worldRenderer;
			this.onStart = onStart;
			this.onExit = onExit;
			this.skirmishMode = skirmishMode;

			// TODO: This needs to be reworked to support per-map tech levels, bots, etc.
			modRules = modData.DefaultRules;

			services = modData.Manifest.Get<WebServices>();
			internetRelayChat = modData.Manifest.Get<InternetRelayChat>();

			Game.LobbyInfoChanged += UpdateCurrentMap;
			Game.LobbyInfoChanged += UpdatePlayerList;
			Game.LobbyInfoChanged += UpdateDiscordStatus;
			Game.LobbyInfoChanged += UpdateSpawnOccupants;
			Game.LobbyInfoChanged += UpdateOptions;
			Game.BeforeGameStart += OnGameStart;
			Game.ConnectionStateChanged += ConnectionStateChanged;

			ChromeMetrics.TryGet("ChatLineSound", out chatLineSound);
			ChromeMetrics.TryGet("PlayerJoinedSound", out playerJoinedSound);
			ChromeMetrics.TryGet("PlayerLeftSound", out playerLeftSound);
			ChromeMetrics.TryGet("LobbyOptionChangedSound", out lobbyOptionChangedSound);

			var name = lobby.GetOrNull<LabelWidget>("SERVER_NAME");
			if (name != null)
				name.GetText = () => orderManager.LobbyInfo.GlobalSettings.ServerName;

			var mapContainer = Ui.LoadWidget("MAP_PREVIEW", lobby.Get("MAP_PREVIEW_ROOT"), new WidgetArgs
			{
				{ "orderManager", orderManager },
				{ "getMap", (Func<(MapPreview, Session.MapStatus)>)(() => (map, mapStatus)) },
				{
					"onMouseDown", (Action<MapPreviewWidget, MapPreview, MouseInput>)((preview, mapPreview, mi) =>
						LobbyUtils.SelectSpawnPoint(orderManager, preview, mapPreview, mi))
				},
				{ "getSpawnOccupants", () => spawnOccupants },
				{ "getDisabledSpawnPoints", () => orderManager.LobbyInfo.DisabledSpawnPoints },
				{ "showUnoccupiedSpawnpoints", true },
				{ "mapUpdatesEnabled", true },
				{
					"onMapUpdate", (Action<string>)(uid =>
					{
						orderManager.IssueOrder(Order.Command("map " + uid));
						Game.Settings.Server.Map = uid;
						Game.Settings.Save();
					})
				},
			});

			mapContainer.IsVisible = () => panel != PanelType.Servers;

			UpdateCurrentMap();

			var playerBin = Ui.LoadWidget("LOBBY_PLAYER_BIN", lobby.Get("TOP_PANELS_ROOT"), []);
			playerBin.IsVisible = () => panel == PanelType.Players;

			players = playerBin.Get<ScrollPanelWidget>("LOBBY_PLAYERS");
			editablePlayerTemplate = players.Get("TEMPLATE_EDITABLE_PLAYER");
			nonEditablePlayerTemplate = players.Get("TEMPLATE_NONEDITABLE_PLAYER");
			emptySlotTemplate = players.Get("TEMPLATE_EMPTY");
			editableSpectatorTemplate = players.Get("TEMPLATE_EDITABLE_SPECTATOR");
			nonEditableSpectatorTemplate = players.Get("TEMPLATE_NONEDITABLE_SPECTATOR");
			newSpectatorTemplate = players.Get("TEMPLATE_NEW_SPECTATOR");
			colorManager = modRules.Actors[SystemActors.World].TraitInfo<IColorPickerManagerInfo>();

			foreach (var f in modRules.Actors[SystemActors.World].TraitInfos<FactionInfo>())
				factions.Add(f.InternalName, new LobbyFaction { Selectable = f.Selectable, Name = f.Name, Side = f.Side, Description = f.Description });

			var gameStarting = false;
			Func<bool> configurationDisabled = () => !Game.IsHost || gameStarting ||
				panel == PanelType.Kick || panel == PanelType.ForceStart || !MapIsPlayable ||
				orderManager.LocalClient == null || orderManager.LocalClient.IsReady;

			var mapButton = lobby.GetOrNull<ButtonWidget>("CHANGEMAP_BUTTON");
			if (mapButton != null)
			{
				mapButton.IsVisible = () => panel != PanelType.Servers;
				mapButton.IsDisabled = () => gameStarting || panel == PanelType.Kick || panel == PanelType.ForceStart ||
					orderManager.LocalClient == null || orderManager.LocalClient.IsReady;
				mapButton.OnClick = () =>
				{
					var onSelect = new Action<string>(uid =>
					{
						// Don't select the same map again, and handle map becoming unavailable
						var status = modData.MapCache[uid].Status;
						if (uid == map.Uid || (status != MapStatus.Available && status != MapStatus.DownloadAvailable))
							return;

						orderManager.IssueOrder(Order.Command("map " + uid));
						Game.Settings.Server.Map = uid;
						Game.Settings.Save();
					});

					var onSelectGenerated = new Action<MapGenerationArgs>(args =>
					{
						if (args.Uid == map.Uid)
							return;

						lastGeneratedMap = args;
						orderManager.IssueOrder(Order.FromTargetString("GenerateMap", args.Serialize(), true));
						orderManager.IssueOrder(Order.Command("map " + args.Uid));
						Game.Settings.Server.Map = args.Uid;
						Game.Settings.Save();
					});

					// Check for updated maps, if the user has edited a map we'll preselect it for them
					modData.MapCache.UpdateMaps();

					Ui.OpenWindow("MAPCHOOSER_PANEL", new WidgetArgs()
					{
						{ "initialMap", modData.MapCache.PickLastModifiedMap(MapVisibility.Lobby) ?? map.Uid },
						{ "initialGeneratedMap", lastGeneratedMap },
						{ "remoteMapPool", orderManager.ServerMapPool },
						{ "initialTab", MapClassification.System },
						{ "onExit", modData.MapCache.UpdateMaps },
						{ "onSelect", Game.IsHost ? onSelect : null },
						{ "onSelectGenerated", Game.IsHost ? onSelectGenerated : null },
						{ "filter", MapVisibility.Lobby },
					});
				};
			}

			var slotsButton = lobby.GetOrNull<DropDownButtonWidget>("SLOTS_DROPDOWNBUTTON");
			if (slotsButton != null)
			{
				slotsButton.IsVisible = () => panel != PanelType.Servers && panel != PanelType.Options;
				slotsButton.IsDisabled = () => configurationDisabled() || panel != PanelType.Players ||
					(orderManager.LobbyInfo.Slots.Values.All(s => !s.AllowBots) &&
					!orderManager.LobbyInfo.Slots.Any(s => !s.Value.LockTeam && orderManager.LobbyInfo.ClientInSlot(s.Key) != null));

				slotsButton.OnMouseDown = _ =>
				{
					var botTypes = map.PlayerActorInfo.TraitInfos<IBotInfo>().Select(t => t.Type);
					var options = new Dictionary<string, IEnumerable<DropDownOption>>();

					var botController = orderManager.LobbyInfo.Clients.FirstOrDefault(c => c.IsAdmin);
					if (orderManager.LobbyInfo.Slots.Values.Any(s => s.AllowBots))
					{
						var botOptions = new List<DropDownOption>()
						{
							new()
							{
								Title = FluentProvider.GetMessage(Add),
								IsSelected = () => false,
								OnClick = () =>
								{
									foreach (var slot in orderManager.LobbyInfo.Slots)
									{
										var bot = botTypes.Random(Game.CosmeticRandom);
										var c = orderManager.LobbyInfo.ClientInSlot(slot.Key);
										if (slot.Value.AllowBots && (c == null || c.Bot != null))
											orderManager.IssueOrder(Order.Command($"slot_bot {slot.Key} {botController.Index} {bot}"));
									}
								}
							}
						};

						if (orderManager.LobbyInfo.Clients.Any(c => c.Bot != null))
						{
							botOptions.Add(new DropDownOption()
							{
								Title = FluentProvider.GetMessage(Remove),
								IsSelected = () => false,
								OnClick = () =>
								{
									foreach (var slot in orderManager.LobbyInfo.Slots)
									{
										var c = orderManager.LobbyInfo.ClientInSlot(slot.Key);
										if (c != null && c.Bot != null)
											orderManager.IssueOrder(Order.Command("slot_open " + slot.Value.PlayerReference));
									}
								}
							});
						}

						options.Add(FluentProvider.GetMessage(ConfigureBots), botOptions);
					}

					var teamCount = (orderManager.LobbyInfo.Slots.Count(s => !s.Value.LockTeam && orderManager.LobbyInfo.ClientInSlot(s.Key) != null) + 1) / 2;
					if (teamCount >= 1)
					{
						var teamOptions = Enumerable.Range(2, teamCount - 1).Reverse().Select(d => new DropDownOption
						{
							Title = FluentProvider.GetMessage(NumberTeams, "count", d),
							IsSelected = () => false,
							OnClick = () => orderManager.IssueOrder(Order.Command($"assignteams {d}"))
						}).ToList();

						if (orderManager.LobbyInfo.Slots.Any(s => s.Value.AllowBots))
						{
							teamOptions.Add(new DropDownOption
							{
								Title = FluentProvider.GetMessage(HumanVsBots),
								IsSelected = () => false,
								OnClick = () => orderManager.IssueOrder(Order.Command("assignteams 1"))
							});
						}

						teamOptions.Add(new DropDownOption
						{
							Title = FluentProvider.GetMessage(FreeForAll),
							IsSelected = () => false,
							OnClick = () => orderManager.IssueOrder(Order.Command("assignteams 0"))
						});

						options.Add(FluentProvider.GetMessage(ConfigureTeams), teamOptions);
					}

					ScrollItemWidget SetupItem(DropDownOption option, ScrollItemWidget template)
					{
						var item = ScrollItemWidget.Setup(template, option.IsSelected, option.OnClick);
						item.Get<LabelWidget>("LABEL").GetText = () => option.Title;
						return item;
					}

					slotsButton.ShowDropDown("LABEL_DROPDOWN_TEMPLATE", 175, options, SetupItem);
				};
			}

			var resetOptionsButton = lobby.GetOrNull<ButtonWidget>("RESET_OPTIONS_BUTTON");
			if (resetOptionsButton != null)
			{
				resetOptionsButton.IsVisible = () => panel == PanelType.Options;
				resetOptionsButton.IsDisabled = () => configurationDisabled() || !resetOptionsButtonEnabled;
				resetOptionsButton.OnMouseDown = _ => orderManager.IssueOrder(Order.Command("reset_options"));
			}

			var optionsBin = Ui.LoadWidget("LOBBY_OPTIONS_BIN", lobby.Get("TOP_PANELS_ROOT"), new WidgetArgs()
			{
				{ "orderManager", orderManager },
				{ "getMap", () => map },
				{ "configurationDisabled", configurationDisabled }
			});

			optionsBin.IsVisible = () => panel == PanelType.Options;

			var musicBin = Ui.LoadWidget("LOBBY_MUSIC_BIN", lobby.Get("TOP_PANELS_ROOT"), new WidgetArgs
			{
				{ "onExit", DoNothing },
				{ "world", worldRenderer.World }
			});
			musicBin.IsVisible = () => panel == PanelType.Music;

			ServerListLogic serverListLogic = null;
			if (!skirmishMode)
			{
				Action<GameServer> doNothingWithServer = _ => { };

				var serversBin = Ui.LoadWidget("LOBBY_SERVERS_BIN", lobby.Get("TOP_PANELS_ROOT"), new WidgetArgs
				{
					{ "onJoin", doNothingWithServer },
				});

				serverListLogic = serversBin.LogicObjects.Select(l => l as ServerListLogic).FirstOrDefault(l => l != null);
				serversBin.IsVisible = () => panel == PanelType.Servers;
			}

			var tabContainer = skirmishMode ? lobby.Get("SKIRMISH_TABS") : lobby.Get("MULTIPLAYER_TABS");
			tabContainer.IsVisible = () => true;

			var optionsTab = tabContainer.Get<ButtonWidget>("OPTIONS_TAB");
			optionsTab.IsHighlighted = () => panel == PanelType.Options;
			optionsTab.IsDisabled = OptionsTabDisabled;
			optionsTab.OnClick = () => panel = PanelType.Options;

			var playersTab = tabContainer.Get<ButtonWidget>("PLAYERS_TAB");
			playersTab.IsHighlighted = () => panel == PanelType.Players;
			playersTab.IsDisabled = () => panel == PanelType.Kick || panel == PanelType.ForceStart;
			playersTab.OnClick = () => panel = PanelType.Players;

			var musicTab = tabContainer.Get<ButtonWidget>("MUSIC_TAB");
			musicTab.IsHighlighted = () => panel == PanelType.Music;
			musicTab.IsDisabled = () => panel == PanelType.Kick || panel == PanelType.ForceStart;
			musicTab.OnClick = () => panel = PanelType.Music;

			var serversTab = tabContainer.GetOrNull<ButtonWidget>("SERVERS_TAB");
			if (serversTab != null)
			{
				serversTab.IsHighlighted = () => panel == PanelType.Servers;
				serversTab.IsDisabled = () => panel == PanelType.Kick || panel == PanelType.ForceStart;
				serversTab.OnClick = () =>
				{
					// Refresh the list when switching to the servers tab
					if (serverListLogic != null && panel != PanelType.Servers)
						serverListLogic.RefreshServerList();

					panel = PanelType.Servers;
				};
			}

			// Force start panel
			void StartGame()
			{
				// Refresh MapCache and check if the selected map is available before attempting to start the game
				if (modData.MapCache[map.Uid].Status == MapStatus.Available)
				{
					gameStarting = true;
					orderManager.IssueOrder(Order.Command("startgame"));
				}
				else
					modData.MapCache.UpdateMaps();
			}

			bool StartDisabled() => map.Status != MapStatus.Available ||
				orderManager.LobbyInfo.Slots.Any(sl => sl.Value.Required && orderManager.LobbyInfo.ClientInSlot(sl.Key) == null) ||
				orderManager.LobbyInfo.Slots.All(sl => orderManager.LobbyInfo.ClientInSlot(sl.Key) == null) ||
				(!orderManager.LobbyInfo.GlobalSettings.EnableSingleplayer && orderManager.LobbyInfo.NonBotPlayers.Count() < 2) ||
				insufficientPlayerSpawns;

			var startGameButton = lobby.GetOrNull<ButtonWidget>("START_GAME_BUTTON");
			if (startGameButton != null)
			{
				startGameButton.IsDisabled = () => configurationDisabled() || StartDisabled();

				startGameButton.OnClick = () =>
				{
					// Bots and admins don't count
					if (orderManager.LobbyInfo.Clients.Any(c => c.Slot != null && !c.IsAdmin && c.Bot == null && !c.IsReady))
						panel = PanelType.ForceStart;
					else
						StartGame();
				};
			}

			var forceStartBin = Ui.LoadWidget("FORCE_START_DIALOG", lobby.Get("TOP_PANELS_ROOT"), []);
			forceStartBin.IsVisible = () => panel == PanelType.ForceStart;
			forceStartBin.Get("KICK_WARNING").IsVisible = () => orderManager.LobbyInfo.Clients.Any(c => c.IsInvalid);
			var forceStartButton = forceStartBin.Get<ButtonWidget>("OK_BUTTON");
			forceStartButton.OnClick = StartGame;
			forceStartButton.IsDisabled = StartDisabled;

			forceStartBin.Get<ButtonWidget>("CANCEL_BUTTON").OnClick = () => panel = PanelType.Players;

			var disconnectButton = lobby.Get<ButtonWidget>("DISCONNECT_BUTTON");
			disconnectButton.OnClick = () =>
			{
				Ui.CloseWindow();
				onExit();
				Game.Sound.PlayNotification(modRules, null, "Sounds", playerLeftSound, null);
			};

			if (skirmishMode)
				disconnectButton.Text = FluentProvider.GetMessage(Back);

			var globalChat = Game.LoadWidget(null, "LOBBY_GLOBALCHAT_PANEL", lobby.Get("GLOBALCHAT_ROOT"), []);
			var globalChatInput = globalChat.Get<TextFieldWidget>("CHAT_TEXTFIELD");

			globalChat.IsVisible = () => chatPanel == ChatPanelType.Global;

			var globalChatTab = lobby.Get<ButtonWidget>("GLOBALCHAT_TAB");
			globalChatTab.IsHighlighted = () => chatPanel == ChatPanelType.Global;
			globalChatTab.OnClick = () =>
			{
				chatPanel = ChatPanelType.Global;
				globalChatInput.TakeKeyboardFocus();
			};

			var globalChatLabel = FluentProvider.GetMessage(globalChatTab.Text);
			globalChatTab.GetText = () =>
			{
				if (globalChatUnreadMessages == 0 || chatPanel == ChatPanelType.Global)
					return globalChatLabel;

				return globalChatLabel + $" ({globalChatUnreadMessages})";
			};

			globalChatLastReadMessages = internetRelayChat.History.Count(m => m.Type == ChatMessageType.Message);

			var lobbyChat = lobby.Get("LOBBYCHAT");
			lobbyChat.IsVisible = () => chatPanel == ChatPanelType.Lobby;
			if (logicArgs.TryGetValue("ChatTemplates", out var templateIds))
			{
				foreach (var item in templateIds.Nodes)
				{
					var key = FieldLoader.GetValue<TextNotificationPool>("key", item.Key);
					chatTemplates[key] = Ui.LoadWidget(item.Value.Value, null, []);
				}
			}

			var chatMode = lobby.Get<ButtonWidget>("CHAT_MODE");
			var team = FluentProvider.GetMessage(TeamChat);
			var all = FluentProvider.GetMessage(GeneralChat);
			chatMode.GetText = () => teamChat ? team : all;
			chatMode.OnClick = () => teamChat ^= true;
			chatMode.IsDisabled = () => disableTeamChat || !chatEnabled;

			chatTextField = lobby.Get<TextFieldWidget>("CHAT_TEXTFIELD");
			chatTextField.IsDisabled = () => !chatEnabled;
			chatTextField.MaxLength = UnitOrders.ChatMessageMaxLength;

			chatTextField.OnEnterKey = _ =>
			{
				if (chatTextField.Text.Length == 0)
					return true;

				// Always scroll to bottom when we've typed something
				lobbyChatPanel.ScrollToBottom();

				var teamNumber = 0U;
				if (teamChat && orderManager.LocalClient != null)
					teamNumber = orderManager.LocalClient.IsObserver ? uint.MaxValue : (uint)orderManager.LocalClient.Team;

				orderManager.IssueOrder(Order.Chat(chatTextField.Text, teamNumber));
				chatTextField.Text = "";
				return true;
			};

			chatTextField.OnTabKey = e =>
			{
				if (!chatMode.Key.IsActivatedBy(e) || chatMode.IsDisabled())
				{
					chatTextField.Text = tabCompletion.Complete(chatTextField.Text);
					chatTextField.CursorPosition = chatTextField.Text.Length;
				}
				else
					chatMode.OnKeyPress(e);

				return true;
			};

			chatTextField.OnEscKey = _ => chatTextField.YieldKeyboardFocus();

			var lobbyChatTab = lobby.Get<ButtonWidget>("LOBBYCHAT_TAB");
			lobbyChatTab.IsHighlighted = () => chatPanel == ChatPanelType.Lobby;
			lobbyChatTab.OnClick = () =>
			{
				chatPanel = ChatPanelType.Lobby;
				chatTextField.TakeKeyboardFocus();
			};

			var lobbyChatLabel = FluentProvider.GetMessage(lobbyChatTab.Text);
			lobbyChatTab.GetText = () =>
			{
				if (lobbyChatUnreadMessages == 0 || chatPanel == ChatPanelType.Lobby)
					return lobbyChatLabel;

				return lobbyChatLabel + $" ({globalChatUnreadMessages})";
			};

			chatAvailableIn = new CachedTransform<int, string>(x => FluentProvider.GetMessage(ChatAvailability, "seconds", x));
			chatDisabled = FluentProvider.GetMessage(ChatDisabled);

			lobbyChatPanel = lobby.Get<ScrollPanelWidget>("CHAT_DISPLAY");
			lobbyChatPanel.RemoveChildren();

			var settingsButton = lobby.GetOrNull<ButtonWidget>("SETTINGS_BUTTON");
			if (settingsButton != null)
			{
				settingsButton.OnClick = () => Ui.OpenWindow("SETTINGS_PANEL", new WidgetArgs
				{
					{ "onExit", DoNothing },
					{ "worldRenderer", worldRenderer }
				});
			}

			if (logicArgs.TryGetValue("ChatLineSound", out var yaml))
				chatLineSound = yaml.Value;
			if (logicArgs.TryGetValue("PlayerJoinedSound", out yaml))
				playerJoinedSound = yaml.Value;
			if (logicArgs.TryGetValue("PlayerLeftSound", out yaml))
				playerLeftSound = yaml.Value;
			if (logicArgs.TryGetValue("LobbyOptionChangedSound", out yaml))
				lobbyOptionChangedSound = yaml.Value;
		}

		bool disposed;
		protected override void Dispose(bool disposing)
		{
			if (disposing && !disposed)
			{
				disposed = true;
				Game.LobbyInfoChanged -= UpdateCurrentMap;
				Game.LobbyInfoChanged -= UpdatePlayerList;
				Game.LobbyInfoChanged -= UpdateDiscordStatus;
				Game.LobbyInfoChanged -= UpdateSpawnOccupants;
				Game.BeforeGameStart -= OnGameStart;
				Game.ConnectionStateChanged -= ConnectionStateChanged;
			}

			base.Dispose(disposing);
		}

		bool OptionsTabDisabled()
		{
			return !MapIsPlayable || panel == PanelType.Kick || panel == PanelType.ForceStart;
		}

		public override void Tick()
		{
			// Map may have been installed or generated in the background
			if (!mapAvailable && map.Status == MapStatus.Available)
			{
				mapAvailable = true;
				orderManager.IssueOrder(Order.Command($"state {Session.ClientState.NotReady}"));
			}

			if (panel == PanelType.Options && OptionsTabDisabled())
				panel = PanelType.Players;

			var newMessages = internetRelayChat.History.Count(m => m.Type == ChatMessageType.Message);
			globalChatUnreadMessages += newMessages - globalChatLastReadMessages;
			globalChatLastReadMessages = newMessages;

			if (chatPanel == ChatPanelType.Lobby)
				lobbyChatUnreadMessages = 0;

			if (chatPanel == ChatPanelType.Global)
				globalChatUnreadMessages = 0;

			var chatWasEnabled = chatEnabled;
			chatEnabled = worldRenderer.World.IsReplay ||
				(Game.RunTime >= TextNotificationsManager.ChatDisabledUntil && TextNotificationsManager.ChatDisabledUntil != uint.MaxValue);

			if (chatEnabled && !chatWasEnabled)
			{
				chatTextField.Text = "";
				if (Ui.KeyboardFocusWidget == null)
					chatTextField.TakeKeyboardFocus();
			}
			else if (!chatEnabled)
			{
				var remaining = 0;
				if (TextNotificationsManager.ChatDisabledUntil != uint.MaxValue)
					remaining = (int)(TextNotificationsManager.ChatDisabledUntil - Game.RunTime + 999) / 1000;

				chatTextField.Text = remaining == 0 ? chatDisabled : chatAvailableIn.Update(remaining);
			}
		}

		void INotificationHandler<TextNotification>.Handle(TextNotification notification)
		{
			lobbyChatUnreadMessages++;
			var chatLine = chatTemplates[notification.Pool].Clone();
			WidgetUtils.SetupTextNotification(chatLine, notification, lobbyChatPanel.Bounds.Width - lobbyChatPanel.ScrollbarWidth, true);

			var scrolledToBottom = lobbyChatPanel.ScrolledToBottom;
			lobbyChatPanel.AddChild(chatLine);
			if (scrolledToBottom)
				lobbyChatPanel.ScrollToBottom(smooth: true);

			switch (notification.Pool)
			{
				case TextNotificationPool.Chat:
					Game.Sound.PlayNotification(modRules, null, "Sounds", chatLineSound, null);
					break;
				case TextNotificationPool.System:
					Game.Sound.PlayNotification(modRules, null, "Sounds", lobbyOptionChangedSound, null);
					break;
				case TextNotificationPool.Join:
					Game.Sound.PlayNotification(modRules, null, "Sounds", playerJoinedSound, null);
					break;
				case TextNotificationPool.Leave:
					Game.Sound.PlayNotification(modRules, null, "Sounds", playerLeftSound, null);
					break;
			}
		}

		void UpdateCurrentMap()
		{
			mapStatus = orderManager.LobbyInfo.GlobalSettings.MapStatus;
			var uid = orderManager.LobbyInfo.GlobalSettings.Map;
			if (map.Uid == uid)
				return;

			map = modData.MapCache[uid];

			// Tell the server that we have the map
			mapAvailable = map.Status == MapStatus.Available;
			if (mapAvailable)
				orderManager.IssueOrder(Order.Command($"state {Session.ClientState.NotReady}"));

			// We don't have the map
			else if (map.Status != MapStatus.DownloadAvailable && Game.Settings.Game.AllowDownloading)
				modData.MapCache.QueryRemoteMapDetails(services.MapRepository, [uid]);
		}

		void UpdatePlayerList()
		{
			if (orderManager.LocalClient == null)
				return;

			// Check if we are not assigned to any team, and are no spectator
			// If we are a spectator, check if there are more and enable spectator chat
			// Otherwise check if our assigned team has more players
			if (orderManager.LocalClient.Team == 0 && !orderManager.LocalClient.IsObserver)
				disableTeamChat = true;
			else if (orderManager.LocalClient.IsObserver)
				disableTeamChat = !orderManager.LobbyInfo.Clients.Any(c => c != orderManager.LocalClient && c.IsObserver);
			else
				disableTeamChat = !orderManager.LobbyInfo.Clients.Any(c =>
					c != orderManager.LocalClient &&
					c.Bot == null &&
					c.Team == orderManager.LocalClient.Team);

			insufficientPlayerSpawns = LobbyUtils.InsufficientEnabledSpawnPoints(map, orderManager.LobbyInfo);

			if (disableTeamChat)
				teamChat = false;

			var isHost = Game.IsHost;
			var idx = 0;
			foreach (var kv in orderManager.LobbyInfo.Slots)
			{
				var key = kv.Key;
				var slot = kv.Value;
				var client = orderManager.LobbyInfo.ClientInSlot(key);
				Widget template = null;

				// get template for possible reuse
				if (idx < players.Children.Count)
					template = players.Children[idx];

				if (client == null)
				{
					// Empty slot
					if (template == null || template.Id != emptySlotTemplate.Id)
						template = emptySlotTemplate.Clone();

					if (isHost)
						LobbyUtils.SetupEditableSlotWidget(template, slot, client, orderManager, map, modData);
					else
						LobbyUtils.SetupSlotWidget(template, modData, slot, client);

					var join = template.Get<ButtonWidget>("JOIN");
					join.IsVisible = () => !slot.Closed;
					join.IsDisabled = () => orderManager.LocalClient.IsReady;
					join.OnClick = () => orderManager.IssueOrder(Order.Command("slot " + key));
				}
				else if ((client.Index == orderManager.LocalClient.Index) ||
						 (client.Bot != null && isHost))
				{
					// Editable player in slot
					if (template == null || template.Id != editablePlayerTemplate.Id)
						template = editablePlayerTemplate.Clone();

					LobbyUtils.SetupLatencyWidget(template, client, orderManager);

					if (client.Bot != null)
						LobbyUtils.SetupEditableSlotWidget(template, slot, client, orderManager, map, modData);
					else
						LobbyUtils.SetupEditableNameWidget(template, client, orderManager, worldRenderer);

					LobbyUtils.SetupEditableColorWidget(template, slot, client, orderManager, worldRenderer, colorManager);
					LobbyUtils.SetupEditableFactionWidget(template, slot, client, orderManager, factions);
					LobbyUtils.SetupEditableTeamWidget(template, slot, client, orderManager, map);
					LobbyUtils.SetupEditableHandicapWidget(template, slot, client, orderManager);
					LobbyUtils.SetupEditableSpawnWidget(template, slot, client, orderManager, map);
					LobbyUtils.SetupEditableReadyWidget(template, client, orderManager, map, MapIsPlayable);
				}
				else
				{
					// Non-editable player in slot
					if (template == null || template.Id != nonEditablePlayerTemplate.Id)
						template = nonEditablePlayerTemplate.Clone();

					LobbyUtils.SetupLatencyWidget(template, client, orderManager);
					LobbyUtils.SetupColorWidget(template, client);
					LobbyUtils.SetupFactionWidget(template, client, factions);

					if (isHost)
					{
						LobbyUtils.SetupEditableTeamWidget(template, slot, client, orderManager, map);
						LobbyUtils.SetupEditableHandicapWidget(template, slot, client, orderManager);
						LobbyUtils.SetupEditableSpawnWidget(template, slot, client, orderManager, map);
						LobbyUtils.SetupPlayerActionWidget(template, client, orderManager, worldRenderer,
							lobby, () => panel = PanelType.Kick, () => panel = PanelType.Players);
					}
					else
					{
						LobbyUtils.SetupNameWidget(template, client, orderManager, worldRenderer, map);
						LobbyUtils.SetupTeamWidget(template, client);
						LobbyUtils.SetupHandicapWidget(template, client);
						LobbyUtils.SetupSpawnWidget(template, client);
					}

					LobbyUtils.SetupReadyWidget(template, client);
				}

				template.IsVisible = () => true;

				if (idx >= players.Children.Count)
					players.AddChild(template);
				else if (players.Children[idx].Id != template.Id)
					players.ReplaceChild(players.Children[idx], template);

				idx++;
			}

			// Add spectators
			foreach (var client in orderManager.LobbyInfo.Clients.Where(client => client.Slot == null))
			{
				Widget template = null;
				var c = client;

				// get template for possible reuse
				if (idx < players.Children.Count)
					template = players.Children[idx];

				// Editable spectator
				if (c.Index == orderManager.LocalClient.Index)
				{
					if (template == null || template.Id != editableSpectatorTemplate.Id)
						template = editableSpectatorTemplate.Clone();

					LobbyUtils.SetupEditableNameWidget(template, c, orderManager, worldRenderer);

					if (client.IsAdmin)
						LobbyUtils.SetupEditableReadyWidget(template, client, orderManager, map, MapIsPlayable);
					else
						LobbyUtils.HideReadyWidgets(template);
				}
				else
				{
					// Non-editable spectator
					if (template == null || template.Id != nonEditableSpectatorTemplate.Id)
						template = nonEditableSpectatorTemplate.Clone();

					if (isHost)
						LobbyUtils.SetupPlayerActionWidget(template, client, orderManager, worldRenderer,
							lobby, () => panel = PanelType.Kick, () => panel = PanelType.Players);
					else
						LobbyUtils.SetupNameWidget(template, client, orderManager, worldRenderer, map);

					if (client.IsAdmin)
						LobbyUtils.SetupReadyWidget(template, client);
					else
						LobbyUtils.HideReadyWidgets(template);
				}

				LobbyUtils.SetupLatencyWidget(template, c, orderManager);
				template.IsVisible = () => true;

				if (idx >= players.Children.Count)
					players.AddChild(template);
				else if (players.Children[idx].Id != template.Id)
					players.ReplaceChild(players.Children[idx], template);

				idx++;
			}

			// Spectate button
			if (orderManager.LocalClient.Slot != null)
			{
				Widget spec = null;
				if (idx < players.Children.Count)
					spec = players.Children[idx];
				if (spec == null || spec.Id != newSpectatorTemplate.Id)
					spec = newSpectatorTemplate.Clone();

				LobbyUtils.SetupKickSpectatorsWidget(spec, orderManager, lobby,
					() => panel = PanelType.Kick, () => panel = PanelType.Players, skirmishMode);

				var btn = spec.Get<ButtonWidget>("SPECTATE");
				btn.OnClick = () => orderManager.IssueOrder(Order.Command("spectate"));
				btn.IsDisabled = () => orderManager.LocalClient.IsReady;
				btn.IsVisible = () => orderManager.LobbyInfo.GlobalSettings.AllowSpectators
					|| orderManager.LocalClient.IsAdmin;

				spec.IsVisible = () => true;

				if (idx >= players.Children.Count)
					players.AddChild(spec);
				else if (players.Children[idx].Id != spec.Id)
					players.ReplaceChild(players.Children[idx], spec);

				idx++;
			}

			while (players.Children.Count > idx)
				players.RemoveChild(players.Children[idx]);

			tabCompletion.Names = orderManager.LobbyInfo.Clients.Where(c => !c.IsBot).Select(c => c.Name).Distinct().ToList();
		}

		void UpdateDiscordStatus()
		{
			var numberOfPlayers = 0;
			var slots = 0;

			if (!skirmishMode)
			{
				foreach (var kv in orderManager.LobbyInfo.Slots)
				{
					if (kv.Value.Closed)
						continue;

					slots++;
					var client = orderManager.LobbyInfo.ClientInSlot(kv.Key);

					if (client != null)
						numberOfPlayers++;
				}
			}

			// Add extra slots to keep the join button active for spectators
			if (numberOfPlayers == slots && orderManager.LobbyInfo.GlobalSettings.AllowSpectators)
				slots = numberOfPlayers + 1;

			var details = map.Title + " - " + orderManager.LobbyInfo.GlobalSettings.ServerName;
			if (updateDiscordStatus)
			{
				string secret = null;
				if (orderManager.LobbyInfo.GlobalSettings.Dedicated)
				{
					var endpoint = CurrentServerSettings.Target.GetConnectEndPoints().First();
					secret = $"{endpoint.Address}|{endpoint.Port}";
				}

				var state = skirmishMode ? DiscordState.InSkirmishLobby : DiscordState.InMultiplayerLobby;

				DiscordService.UpdateStatus(state, details, secret, numberOfPlayers, slots);
				updateDiscordStatus = false;
			}
			else
			{
				if (!skirmishMode)
					DiscordService.UpdatePlayers(numberOfPlayers, slots);

				DiscordService.UpdateDetails(details);
			}
		}

		void UpdateSpawnOccupants()
		{
			spawnOccupants = orderManager.LobbyInfo.Clients
				.Where(c => c.SpawnPoint != 0)
				.ToDictionary(c => c.SpawnPoint, c => new SpawnOccupant(c));
		}

		void UpdateOptions()
		{
			if (map == null || map.WorldActorInfo == null)
				return;

			var serverOptions = orderManager.LobbyInfo.GlobalSettings.LobbyOptions;
			var mapOptions = map.PlayerActorInfo.TraitInfos<ILobbyOptions>()
				.Concat(map.WorldActorInfo.TraitInfos<ILobbyOptions>())
				.SelectMany(t => t.LobbyOptions(map))
				.Where(o => o.IsVisible)
				.OrderBy(o => o.DisplayOrder)
				.ToArray();

			resetOptionsButtonEnabled = mapOptions.Any(o => o.DefaultValue != serverOptions[o.Id].Value);
		}

		void OnGameStart()
		{
			Ui.CloseWindow();

			var state = skirmishMode ? DiscordState.PlayingSkirmish : DiscordState.PlayingMultiplayer;
			var details = map.Title + " - " + orderManager.LobbyInfo.GlobalSettings.ServerName;
			DiscordService.UpdateStatus(state, details);

			onStart();
		}
	}

	sealed class DropDownOption
	{
		public string Title;
		public Func<bool> IsSelected = () => false;
		public Action OnClick;
	}
}
