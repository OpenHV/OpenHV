#region Copyright & License Information
/*
 * Copyright 2023-2025 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BeaconLib;
using OpenRA.Graphics;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Widgets;
using OpenRA.Network;
using OpenRA.Primitives;
using OpenRA.Server;
using OpenRA.Support;
using OpenRA.Traits;
using OpenRA.Widgets;

namespace OpenRA.Mods.HV.Widgets.Logic
{
	public class CustomServerListLogic : ChromeLogic
	{
		[FluentReference]
		const string SearchStatusFailed = "label-search-status-failed";

		[FluentReference]
		const string SearchStatusNoGames = "label-search-status-no-games";

		[FluentReference("players")]
		const string PlayersOnline = "label-players-online-count";

		[FluentReference]
		const string NoServerSelected = "label-no-server-selected";

		[FluentReference]
		const string MapStatusSearching = "label-map-status-searching";

		[FluentReference]
		const string MapClassificationUnknown = "label-map-classification-unknown";

		[FluentReference("players")]
		const string PlayersLabel = "label-players-count";

		[FluentReference("bots")]
		const string BotsLabel = "label-bots-count";

		[FluentReference]
		const string BotPlayer = "label-bot-player";

		[FluentReference("spectators")]
		const string SpectatorsLabel = "label-spectators-count";

		[FluentReference]
		const string Players = "label-players";

		[FluentReference("team")]
		const string TeamNumber = "label-team-name";

		[FluentReference]
		const string NoTeam = "label-no-team";

		[FluentReference]
		const string Spectators = "label-spectators";

		[FluentReference("players")]
		const string OtherPlayers = "label-other-players-count";

		[FluentReference]
		const string Playing = "label-playing";

		[FluentReference]
		const string Waiting = "label-waiting";

		[FluentReference("minutes")]
		const string InProgress = "label-in-progress-for";

		[FluentReference]
		const string PasswordProtected = "label-password-protected";

		[FluentReference]
		const string WaitingForPlayers = "label-waiting-for-players";

		[FluentReference]
		const string ServerShuttingDown = "label-server-shutting-down";

		[FluentReference]
		const string UnknownServerState = "label-unknown-server-state";

		readonly string noServerSelected;
		readonly string mapStatusSearching;
		readonly string mapClassificationUnknown;
		readonly string playing;
		readonly string waiting;

		readonly Color incompatibleVersionColor;
		readonly Color incompatibleProtectedGameColor;
		readonly Color protectedGameColor;
		readonly Color incompatibleWaitingGameColor;
		readonly Color waitingGameColor;
		readonly Color incompatibleGameStartedColor;
		readonly Color gameStartedColor;
		readonly Color incompatibleGameColor;
		readonly ModData modData;
		readonly WebServices services;
		readonly GitHubWebServices webServices;
		readonly Probe lanGameProbe;

		readonly Widget serverList;
		readonly ScrollItemWidget serverTemplate;
		readonly ScrollItemWidget headerTemplate;
		readonly Widget noticeContainer;
		readonly Widget clientContainer;
		readonly ScrollPanelWidget clientList;
		readonly ScrollItemWidget clientTemplate, clientHeader;
		readonly MapPreviewWidget mapPreview;
		readonly ButtonWidget joinButton;
		readonly int joinButtonY;

		readonly Action<GameServer> onJoin;

		GameServer currentServer;
		MapPreview currentMap;
		bool showNotices;
		int playerCount;

		enum SearchStatus { Fetching, Failed, NoGames, Hidden }

		SearchStatus searchStatus = SearchStatus.Fetching;

		bool activeQuery;
		IEnumerable<BeaconLocation> lanGameLocations;

		readonly CachedTransform<int, string> players;
		readonly CachedTransform<int, string> bots;
		readonly CachedTransform<int, string> spectators;

		readonly CachedTransform<double, string> minutes;
		readonly string passwordProtected;
		readonly string waitingForPlayers;
		readonly string serverShuttingDown;
		readonly string unknownServerState;

		public string ProgressLabelText()
		{
			switch (searchStatus)
			{
				case SearchStatus.Failed: return FluentProvider.GetMessage(SearchStatusFailed);
				case SearchStatus.NoGames: return FluentProvider.GetMessage(SearchStatusNoGames);
				default: return "";
			}
		}

		[ObjectCreator.UseCtor]
		public CustomServerListLogic(Widget widget, ModData modData, Action<GameServer> onJoin)
		{
			this.modData = modData;
			this.onJoin = onJoin;

			playing = FluentProvider.GetMessage(Playing);
			waiting = FluentProvider.GetMessage(Waiting);

			noServerSelected = FluentProvider.GetMessage(NoServerSelected);
			mapStatusSearching = FluentProvider.GetMessage(MapStatusSearching);
			mapClassificationUnknown = FluentProvider.GetMessage(MapClassificationUnknown);

			players = new CachedTransform<int, string>(i => FluentProvider.GetMessage(PlayersLabel, "players", i));
			bots = new CachedTransform<int, string>(i => FluentProvider.GetMessage(BotsLabel, "bots", i));
			spectators = new CachedTransform<int, string>(i => FluentProvider.GetMessage(SpectatorsLabel, "spectators", i));

			minutes = new CachedTransform<double, string>(i => FluentProvider.GetMessage(InProgress, "minutes", i));
			passwordProtected = FluentProvider.GetMessage(PasswordProtected);
			waitingForPlayers = FluentProvider.GetMessage(WaitingForPlayers);
			serverShuttingDown = FluentProvider.GetMessage(ServerShuttingDown);
			unknownServerState = FluentProvider.GetMessage(UnknownServerState);

			services = modData.Manifest.Get<WebServices>();
			webServices = modData.Manifest.Get<GitHubWebServices>();

			incompatibleVersionColor = ChromeMetrics.Get<Color>("IncompatibleVersionColor");
			incompatibleGameColor = ChromeMetrics.Get<Color>("IncompatibleGameColor");
			incompatibleProtectedGameColor = ChromeMetrics.Get<Color>("IncompatibleProtectedGameColor");
			protectedGameColor = ChromeMetrics.Get<Color>("ProtectedGameColor");
			waitingGameColor = ChromeMetrics.Get<Color>("WaitingGameColor");
			incompatibleWaitingGameColor = ChromeMetrics.Get<Color>("IncompatibleWaitingGameColor");
			gameStartedColor = ChromeMetrics.Get<Color>("GameStartedColor");
			incompatibleGameStartedColor = ChromeMetrics.Get<Color>("IncompatibleGameStartedColor");

			serverList = widget.Get<ScrollPanelWidget>("SERVER_LIST");
			headerTemplate = serverList.Get<ScrollItemWidget>("HEADER_TEMPLATE");
			serverTemplate = serverList.Get<ScrollItemWidget>("SERVER_TEMPLATE");

			noticeContainer = widget.GetOrNull("NOTICE_CONTAINER");
			if (noticeContainer != null)
			{
				noticeContainer.IsVisible = () => showNotices;
				noticeContainer.Get("OUTDATED_VERSION_LABEL").IsVisible = () => webServices.ModVersionStatus == ModVersionStatus.Outdated;
				noticeContainer.Get("UNKNOWN_VERSION_LABEL").IsVisible = () => webServices.ModVersionStatus == ModVersionStatus.Unknown;
				noticeContainer.Get("PRERELEASE_AVAILABLE_LABEL").IsVisible = () => webServices.ModVersionStatus == ModVersionStatus.PrereleaseAvailable;
			}

			var noticeWatcher = widget.Get<LogicTickerWidget>("NOTICE_WATCHER");
			if (noticeWatcher != null && noticeContainer != null)
			{
				var containerHeight = noticeContainer.Bounds.Height;
				noticeWatcher.OnTick = () =>
				{
					var show = webServices.ModVersionStatus != ModVersionStatus.NotChecked && webServices.ModVersionStatus != ModVersionStatus.Latest;
					if (show != showNotices)
					{
						var dir = show ? 1 : -1;
						serverList.Bounds.Y += dir * containerHeight;
						serverList.Bounds.Height -= dir * containerHeight;
						showNotices = show;
					}
				};
			}

			joinButton = widget.GetOrNull<ButtonWidget>("JOIN_BUTTON");
			if (joinButton != null)
			{
				joinButton.IsVisible = () => currentServer != null;
				joinButton.IsDisabled = () => !currentServer.IsJoinable;
				joinButton.OnClick = () => onJoin(currentServer);
				joinButtonY = joinButton.Bounds.Y;
			}

			// Display the progress label over the server list
			// The text is only visible when the list is empty
			var progressText = widget.Get<LabelWidget>("PROGRESS_LABEL");
			progressText.IsVisible = () => searchStatus != SearchStatus.Hidden;
			progressText.GetText = ProgressLabelText;

			var gameSettings = Game.Settings.Game;
			void ToggleFilterFlag(MPGameFilters filter)
			{
				gameSettings.MPGameFilters ^= filter;
				Game.Settings.Save();
				RefreshServerList();
			}

			var filtersButton = widget.GetOrNull<DropDownButtonWidget>("FILTERS_DROPDOWNBUTTON");
			if (filtersButton != null)
			{
				// HACK: MULTIPLAYER_FILTER_PANEL doesn't follow our normal procedure for dropdown creation
				// but we still need to be able to set the dropdown width based on the parent
				// The yaml should use PARENT_WIDTH instead of DROPDOWN_WIDTH
				var filtersPanel = Ui.LoadWidget("MULTIPLAYER_FILTER_PANEL", filtersButton, []);
				filtersButton.Children.Remove(filtersPanel);

				var showWaitingCheckbox = filtersPanel.GetOrNull<CheckboxWidget>("WAITING_FOR_PLAYERS");
				if (showWaitingCheckbox != null)
				{
					showWaitingCheckbox.IsChecked = () => gameSettings.MPGameFilters.HasFlag(MPGameFilters.Waiting);
					showWaitingCheckbox.OnClick = () => ToggleFilterFlag(MPGameFilters.Waiting);
				}

				var showEmptyCheckbox = filtersPanel.GetOrNull<CheckboxWidget>("EMPTY");
				if (showEmptyCheckbox != null)
				{
					showEmptyCheckbox.IsChecked = () => gameSettings.MPGameFilters.HasFlag(MPGameFilters.Empty);
					showEmptyCheckbox.OnClick = () => ToggleFilterFlag(MPGameFilters.Empty);
				}

				var showAlreadyStartedCheckbox = filtersPanel.GetOrNull<CheckboxWidget>("ALREADY_STARTED");
				if (showAlreadyStartedCheckbox != null)
				{
					showAlreadyStartedCheckbox.IsChecked = () => gameSettings.MPGameFilters.HasFlag(MPGameFilters.Started);
					showAlreadyStartedCheckbox.OnClick = () => ToggleFilterFlag(MPGameFilters.Started);
				}

				var showProtectedCheckbox = filtersPanel.GetOrNull<CheckboxWidget>("PASSWORD_PROTECTED");
				if (showProtectedCheckbox != null)
				{
					showProtectedCheckbox.IsChecked = () => gameSettings.MPGameFilters.HasFlag(MPGameFilters.Protected);
					showProtectedCheckbox.OnClick = () => ToggleFilterFlag(MPGameFilters.Protected);
				}

				var showIncompatibleCheckbox = filtersPanel.GetOrNull<CheckboxWidget>("INCOMPATIBLE_VERSION");
				if (showIncompatibleCheckbox != null)
				{
					showIncompatibleCheckbox.IsChecked = () => gameSettings.MPGameFilters.HasFlag(MPGameFilters.Incompatible);
					showIncompatibleCheckbox.OnClick = () => ToggleFilterFlag(MPGameFilters.Incompatible);
				}

				filtersButton.IsDisabled = () => searchStatus == SearchStatus.Fetching;
				filtersButton.OnMouseDown = _ =>
				{
					filtersButton.RemovePanel();
					filtersButton.AttachPanel(filtersPanel);
				};
			}

			var reloadButton = widget.GetOrNull<ButtonWidget>("RELOAD_BUTTON");
			if (reloadButton != null)
			{
				reloadButton.IsDisabled = () => searchStatus == SearchStatus.Fetching;
				reloadButton.OnClick = RefreshServerList;

				var reloadIcon = reloadButton.GetOrNull<ImageWidget>("IMAGE_RELOAD");
				if (reloadIcon != null)
				{
					var disabledFrame = 0;
					var disabledImage = "disabled-" + disabledFrame.ToStringInvariant();
					reloadIcon.GetImageName = () => searchStatus == SearchStatus.Fetching ? disabledImage : reloadIcon.ImageName;

					var reloadTicker = reloadIcon.Get<LogicTickerWidget>("ANIMATION");
					if (reloadTicker != null)
					{
						reloadTicker.OnTick = () =>
						{
							disabledFrame = searchStatus == SearchStatus.Fetching ? (disabledFrame + 1) % 12 : 0;
							disabledImage = "disabled-" + disabledFrame.ToStringInvariant();
						};
					}
				}
			}

			var playersLabel = widget.GetOrNull<LabelWidget>("PLAYER_COUNT");
			if (playersLabel != null)
			{
				var playersText = new CachedTransform<int, string>(p => FluentProvider.GetMessage(PlayersOnline, "players", p));
				playersLabel.IsVisible = () => playerCount != 0;
				playersLabel.GetText = () => playersText.Update(playerCount);
			}

			mapPreview = widget.GetOrNull<MapPreviewWidget>("SELECTED_MAP_PREVIEW");
			if (mapPreview != null)
				mapPreview.Preview = () => currentMap;

			var mapTitle = widget.GetOrNull<LabelWithTooltipWidget>("SELECTED_MAP");
			if (mapTitle != null)
			{
				var font = Game.Renderer.Fonts[mapTitle.Font];
				var title = new CachedTransform<MapPreview, string>(m =>
				{
					var truncated = WidgetUtils.TruncateText(m.Title, mapTitle.Bounds.Width, font);

					if (m.Title != truncated)
						mapTitle.GetTooltipText = () => m.Title;
					else
						mapTitle.GetTooltipText = null;

					return truncated;
				});

				mapTitle.GetText = () =>
				{
					if (currentMap == null)
						return noServerSelected;

					if (currentMap.Status == MapStatus.Searching)
						return mapStatusSearching;

					if (currentMap.Class == MapClassification.Unknown)
						return mapClassificationUnknown;

					return title.Update(currentMap);
				};
			}

			var ip = widget.GetOrNull<LabelWidget>("SELECTED_IP");
			if (ip != null)
			{
				ip.IsVisible = () => currentServer != null;
				ip.GetText = () => currentServer.Address;
			}

			var status = widget.GetOrNull<LabelWidget>("SELECTED_STATUS");
			if (status != null)
			{
				status.IsVisible = () => currentServer != null;
				status.GetText = () => GetStateLabel(currentServer);
				status.GetColor = () => GetStateColor(currentServer, status);
			}

			var modVersion = widget.GetOrNull<LabelWidget>("SELECTED_MOD_VERSION");
			if (modVersion != null)
			{
				modVersion.IsVisible = () => currentServer != null;
				modVersion.GetColor = () => currentServer.IsCompatible ? modVersion.TextColor : incompatibleVersionColor;

				var font = Game.Renderer.Fonts[modVersion.Font];
				var version = new CachedTransform<GameServer, string>(s => WidgetUtils.TruncateText(s.ModLabel, modVersion.Bounds.Width, font));
				modVersion.GetText = () => version.Update(currentServer);
			}

			var selectedPlayers = widget.GetOrNull<LabelWidget>("SELECTED_PLAYERS");
			if (selectedPlayers != null)
			{
				selectedPlayers.IsVisible = () => currentServer != null && (clientContainer == null || currentServer.Clients.Length == 0);
				selectedPlayers.GetText = () => PlayerLabel(currentServer);
			}

			clientContainer = widget.GetOrNull("CLIENT_LIST_CONTAINER");
			if (clientContainer != null)
			{
				clientList = Ui.LoadWidget("MULTIPLAYER_CLIENT_LIST", clientContainer, []) as ScrollPanelWidget;
				clientList.IsVisible = () => currentServer != null && currentServer.Clients.Length > 0;
				clientHeader = clientList.Get<ScrollItemWidget>("HEADER");
				clientTemplate = clientList.Get<ScrollItemWidget>("TEMPLATE");
				clientList.RemoveChildren();
			}

			lanGameLocations = [];
			try
			{
				lanGameProbe = new Probe("OpenRALANGame");
				lanGameProbe.BeaconsUpdated += locations => lanGameLocations = locations;
				lanGameProbe.Start();
			}
			catch (Exception ex)
			{
				Log.Write("debug", "BeaconLib.Probe: " + ex.Message);
			}

			RefreshServerList();
		}

		string PlayerLabel(GameServer game)
		{
			var label = players.Update(game.Players);

			if (game.Bots > 0)
				label += " " + bots.Update(game.Bots);

			if (game.Spectators > 0)
				label += " " + spectators.Update(game.Spectators);

			return label;
		}

		public void RefreshServerList()
		{
			// Query in progress
			if (activeQuery)
				return;

			searchStatus = SearchStatus.Fetching;

			var queryURL = new HttpQueryBuilder(services.ServerList)
			{
				{ "protocol", GameServer.ProtocolVersion },
				{ "engine", Game.EngineVersion },
				{ "mod", Game.ModData.Manifest.Id },
				{ "version", Game.ModData.Manifest.Metadata.Version }
			}.ToString();

			Task.Run(async () =>
			{
				List<GameServer> games = null;
				activeQuery = true;

				try
				{
					var client = HttpClientFactory.Create();
					var httpResponseMessage = await client.GetAsync(queryURL);
					var result = await httpResponseMessage.Content.ReadAsStreamAsync();

					var yaml = MiniYaml.FromStream(result, queryURL);
					games = [];
					foreach (var node in yaml)
					{
						try
						{
							var gs = new GameServer(node.Value);
							if (gs.Address != null)
								games.Add(gs);
						}
						catch
						{
							// Ignore any invalid games advertised.
						}
					}
				}
				catch (Exception e)
				{
					searchStatus = SearchStatus.Failed;
					Log.Write("debug", $"Failed to query server list with exception: {e}");
				}

				var lanGames = new List<GameServer>();
				var stringPool = new HashSet<string>(); // Reuse common strings in YAML
				foreach (var bl in lanGameLocations)
				{
					try
					{
						if (string.IsNullOrEmpty(bl.Data))
							continue;

						var game = new MiniYamlBuilder(MiniYaml.FromString(
							bl.Data, $"BeaconLocation_{bl.Address}_{bl.LastAdvertised:s}", stringPool: stringPool).First().Value);
						var idNode = game.NodeWithKeyOrDefault("Id");

						// Skip beacons created by this instance and replace Id by expected int value
						if (idNode != null && idNode.Value.Value != Platform.SessionGUID.ToString())
						{
							idNode.Value.Value = "-1";

							// Rewrite the server address with the correct IP
							var addressNode = game.NodeWithKeyOrDefault("Address");
							if (addressNode != null)
								addressNode.Value.Value = bl.Address.ToString().Split(':')[0] + ":" + addressNode.Value.Value.Split(':')[1];

							game.Nodes.Add(new MiniYamlNodeBuilder("Location", "Local Network"));

							lanGames.Add(new GameServer(game.Build()));
						}
					}
					catch
					{
						// Ignore any invalid LAN games advertised.
					}
				}

				var groupedLanGames = lanGames.GroupBy(gs => gs.Address).Select(g => g.Last());
				if (games != null)
					games.AddRange(groupedLanGames);
				else if (groupedLanGames.Any())
					games = groupedLanGames.ToList();

				Game.RunAfterTick(() => RefreshServerListInner(games));

				activeQuery = false;
			});
		}

		int GroupSortOrder(GameServer testEntry)
		{
			// Games that we can't join are sorted last
			if (!testEntry.IsCompatible)
				return testEntry.Mod == modData.Manifest.Id ? 1 : 0;

			// Games for the current mod+version are sorted first
			if (testEntry.Mod == modData.Manifest.Id)
				return testEntry.Version == modData.Manifest.Metadata.Version ? 4 : 3;

			// Followed by games for different mods that are joinable
			return 2;
		}

		void SelectServer(GameServer server)
		{
			currentServer = server;
			currentMap = server != null ? modData.MapCache[server.Map] : null;

			// Can only show factions if the server is running the same mod
			if (server != null && mapPreview != null)
			{
				var spawns = currentMap.SpawnPoints;
				var occupants = server.Clients
					.Where(c => (c.SpawnPoint - 1 >= 0) && (c.SpawnPoint - 1 < spawns.Length))
					.ToDictionary(c => c.SpawnPoint, c => new SpawnOccupant(c, server.Mod != modData.Manifest.Id));

				mapPreview.SpawnOccupants = () => occupants;
				mapPreview.DisabledSpawnPoints = () => server.DisabledSpawnPoints;
			}

			if (server == null || server.Clients.Length == 0)
			{
				if (joinButton != null)
					joinButton.Bounds.Y = joinButtonY;

				return;
			}

			if (joinButton != null)
				joinButton.Bounds.Y = clientContainer.Bounds.Bottom;

			if (clientList == null)
				return;

			clientList.RemoveChildren();

			var players = server.Clients
				.Where(c => !c.IsSpectator)
				.GroupBy(p => p.Team)
				.OrderBy(g => g.Key)
				.ToList();

			var teams = new Dictionary<string, IEnumerable<GameClient>>();
			var noTeams = players.Count == 1;
			foreach (var p in players)
			{
				var label = noTeams ? FluentProvider.GetMessage(Players) : p.Key > 0
					? FluentProvider.GetMessage(TeamNumber, "team", p.Key)
					: FluentProvider.GetMessage(NoTeam);
				teams.Add(label, p);
			}

			if (server.Clients.Any(c => c.IsSpectator))
				teams.Add(FluentProvider.GetMessage(Spectators), server.Clients.Where(c => c.IsSpectator));

			var factionInfo = modData.DefaultRules.Actors[SystemActors.World].TraitInfos<FactionInfo>();
			foreach (var kv in teams)
			{
				var group = kv.Key;
				if (group.Length > 0)
				{
					var header = ScrollItemWidget.Setup(clientHeader, () => false, () => { });
					header.Get<LabelWidget>("LABEL").GetText = () => group;
					clientList.AddChild(header);
				}

				foreach (var option in kv.Value)
				{
					var o = option;
					var playerName = new CachedTransform<(MapStatus, int, SpriteFont), string>(s =>
					{
						var name = o.IsBot
							? currentMap.TryGetMessage(o.Name, out var msg) ? msg : FluentProvider.GetMessage(BotPlayer)
							: o.Name;

						return WidgetUtils.TruncateText(name, s.Item2, s.Item3);
					});

					var item = ScrollItemWidget.Setup(clientTemplate, () => false, () => { });
					if (!o.IsSpectator && server.Mod == modData.Manifest.Id)
					{
						var label = item.Get<LabelWidget>("LABEL");
						var font = Game.Renderer.Fonts[label.Font];
						label.GetText = () => playerName.Update((currentMap.Status, label.Bounds.Width, font));
						label.GetColor = () => o.Color;

						var flag = item.Get<ImageWidget>("FLAG");
						flag.IsVisible = () => true;
						flag.GetImageCollection = () => "flags";
						flag.GetImageName = () => (factionInfo != null && factionInfo.Any(f => f.InternalName == o.Faction)) ? o.Faction : "Random";
					}
					else
					{
						var label = item.Get<LabelWidget>("NOFLAG_LABEL");
						var font = Game.Renderer.Fonts[label.Font];

						// Force spectator color to prevent spoofing by the server
						var color = o.IsSpectator ? Color.White : o.Color;
						label.GetText = () => playerName.Update((currentMap.Status, label.Bounds.Width, font));
						label.GetColor = () => color;
					}

					clientList.AddChild(item);
				}
			}
		}

		void RefreshServerListInner(List<GameServer> games)
		{
			ScrollItemWidget nextServerRow = null;
			List<Widget> rows = null;

			if (games != null)
				rows = LoadGameRows(games, out nextServerRow);

			Game.RunAfterTick(() =>
			{
				serverList.RemoveChildren();
				SelectServer(null);

				if (games == null)
				{
					searchStatus = SearchStatus.Failed;
					return;
				}

				if (rows.Count == 0)
				{
					searchStatus = SearchStatus.NoGames;
					return;
				}

				searchStatus = SearchStatus.Hidden;

				// Search for any unknown maps
				if (Game.Settings.Game.AllowDownloading)
					modData.MapCache.QueryRemoteMapDetails(services.MapRepository, games.Where(g => !Filtered(g)).Select(g => g.Map));

				foreach (var row in rows)
					serverList.AddChild(row);

				nextServerRow?.OnClick();

				playerCount = games.Sum(g => g.Players);
			});
		}

		List<Widget> LoadGameRows(List<GameServer> games, out ScrollItemWidget nextServerRow)
		{
			nextServerRow = null;
			var rows = new List<Widget>();
			var mods = games.GroupBy(g => g.ModLabel)
				.OrderByDescending(g => GroupSortOrder(g.First()))
				.ThenByDescending(g => g.Count());

			foreach (var modGames in mods)
			{
				if (modGames.All(Filtered))
					continue;

				var header = ScrollItemWidget.Setup(headerTemplate, () => false, () => { });

				var headerTitle = modGames.First().ModLabel;
				header.Get<LabelWidget>("LABEL").GetText = () => headerTitle;
				rows.Add(header);

				static int ListOrder(GameServer gameServer)
				{
					// Servers waiting for players are always first
					if (gameServer.State == (int)ServerState.WaitingPlayers && gameServer.Players > 0)
						return 0;

					// Then servers with spectators
					if (gameServer.State == (int)ServerState.WaitingPlayers && gameServer.Spectators > 0)
						return 1;

					// Then active games
					if (gameServer.State >= (int)ServerState.GameStarted)
						return 2;

					// Empty servers are shown at the end because a flood of empty servers
					// at the top of the game list make the community look dead
					return 3;
				}

				foreach (var modGamesByState in modGames.GroupBy(ListOrder).OrderBy(g => g.Key))
				{
					// Sort 'Playing' games by Started, others by number of players
					foreach (var game in modGamesByState.Key == 2 ? modGamesByState.OrderByDescending(g => g.Started) : modGamesByState.OrderByDescending(g => g.Players))
					{
						if (Filtered(game))
							continue;

						var canJoin = game.IsJoinable;
						var item = ScrollItemWidget.Setup(serverTemplate, () => currentServer == game, () => SelectServer(game), () => onJoin(game));
						var title = item.GetOrNull<LabelWithTooltipWidget>("TITLE");
						if (title != null)
						{
							WidgetUtils.TruncateLabelToTooltip(title, game.Name);
							title.GetColor = () => canJoin ? title.TextColor : incompatibleGameColor;
						}

						var password = item.GetOrNull<ImageWidget>("PASSWORD_PROTECTED");
						if (password != null)
						{
							password.IsVisible = () => game.Protected;
							password.GetImageName = () => canJoin ? "protected" : "protected-disabled";
						}

						var auth = item.GetOrNull<ImageWidget>("REQUIRES_AUTHENTICATION");
						if (auth != null)
						{
							auth.IsVisible = () => game.Authentication;
							auth.GetImageName = () => canJoin ? "authentication" : "authentication-disabled";

							if (game.Protected && password != null)
								auth.Bounds.X -= password.Bounds.Width + 5;
						}

						var players = item.GetOrNull<LabelWithTooltipWidget>("PLAYERS");
						if (players != null)
						{
							var label =
								$"{game.Players + game.Bots} / {game.MaxPlayers + game.Bots}"
								+ (game.Spectators > 0 ? $" + {game.Spectators}" : "");

							var color = canJoin ? players.TextColor : incompatibleGameColor;
							players.GetText = () => label;
							players.GetColor = () => color;

							if (game.Clients.Length > 0)
							{
								var preview = modData.MapCache[game.Map];
								var tooltip = new CachedTransform<MapStatus, string>(s =>
								{
									var displayClients = game.Clients.Select(c => c.IsBot
										? preview.TryGetMessage(c.Name, out var msg) ? msg : FluentProvider.GetMessage(BotPlayer)
										: c.Name);

									if (game.Clients.Length > 10)
										displayClients = displayClients
											.Take(9)
											.Append(FluentProvider.GetMessage(OtherPlayers, "players", game.Clients.Length - 9));

									return displayClients.JoinWith("\n");
								});

								players.GetTooltipText = () => tooltip.Update(preview.Status);
							}
							else
								players.GetTooltipText = null;
						}

						var state = item.GetOrNull<LabelWidget>("STATUS");
						if (state != null)
						{
							var label = game.State >= (int)ServerState.GameStarted ? playing : waiting;
							state.GetText = () => label;

							var color = GetStateColor(game, state, !canJoin);
							state.GetColor = () => color;
						}

						var location = item.GetOrNull<LabelWidget>("LOCATION");
						if (location != null)
						{
							var font = Game.Renderer.Fonts[location.Font];
							var label = WidgetUtils.TruncateText(game.Location, location.Bounds.Width, font);
							location.GetText = () => label;
							location.GetColor = () => canJoin ? location.TextColor : incompatibleGameColor;
						}

						if (currentServer != null && game.Address == currentServer.Address)
							nextServerRow = item;

						rows.Add(item);
					}
				}
			}

			return rows;
		}

		string GetStateLabel(GameServer game)
		{
			if (game == null)
				return string.Empty;

			if (game.State == (int)ServerState.GameStarted)
			{
				var totalMinutes = Math.Ceiling(game.PlayTime / 60.0);
				return minutes.Update(totalMinutes);
			}

			if (game.State == (int)ServerState.WaitingPlayers)
				return game.Protected ? passwordProtected : waitingForPlayers;

			if (game.State == (int)ServerState.ShuttingDown)
				return serverShuttingDown;

			return unknownServerState;
		}

		Color GetStateColor(GameServer game, LabelWidget label, bool darkened = false)
		{
			if (!game.Protected && game.State == (int)ServerState.WaitingPlayers)
				return darkened ? incompatibleWaitingGameColor : waitingGameColor;

			if (game.Protected && game.State == (int)ServerState.WaitingPlayers)
				return darkened ? incompatibleProtectedGameColor : protectedGameColor;

			if (game.State == (int)ServerState.GameStarted)
				return darkened ? incompatibleGameStartedColor : gameStartedColor;

			return label.TextColor;
		}

		bool Filtered(GameServer game)
		{
			var filters = Game.Settings.Game.MPGameFilters;
			if (game.State == (int)ServerState.GameStarted && !filters.HasFlag(MPGameFilters.Started))
				return true;

			if (game.State == (int)ServerState.WaitingPlayers && !filters.HasFlag(MPGameFilters.Waiting) && game.Players + game.Spectators != 0)
				return true;

			if (game.Players + game.Spectators == 0 && !filters.HasFlag(MPGameFilters.Empty))
				return true;

			if (!game.IsCompatible && !filters.HasFlag(MPGameFilters.Incompatible))
				return true;

			if (game.Protected && !filters.HasFlag(MPGameFilters.Protected))
				return true;

			return false;
		}

		bool disposed;
		protected override void Dispose(bool disposing)
		{
			if (disposing && !disposed)
			{
				disposed = true;
				lanGameProbe?.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
