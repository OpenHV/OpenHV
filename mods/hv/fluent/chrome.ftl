## editor.yaml
label-new-map-bg-title = New Map
label-new-map-bg-tile = Background:
label-new-map-bg-width = Width:
label-new-map-bg-height = Height:
button-new-map-bg-create = Create

label-save-map-panel-author = Author:
label-save-map-panel-visibility = Visibility:
dropdownbutton-save-map-panel-visibility-dropdown = Map Visibility
label-save-map-panel-directory = Directory:
label-save-map-panel-filename = Filename:
button-save-map-panel = Save
label-actor-edit-panel-id = ID
button-container-ok = OK
label-tiles-bg-search = Search:
label-actors-bg-search = Search:
label-actor-bg-owner = Owner:

button-editor-world-root-undo =
   .label = Undo
   .tooltip = Undo last step

button-editor-world-root-redo =
   .label = Redo
   .tooltip = Redo last step

dropdownbutton-editor-world-root-overlay-button = Overlays

button-editor-world-root-autotile =
   .label = Auto Tile
   .tooltip = Even out tile transitions

button-select-categories-buttons-all = All
button-select-categories-buttons-none = None

## encyclopedia.yaml
label-encyclopedia-content-title = Encyclopedia

## gamesave-loading.yaml
label-gamesave-loading-screen-title = Loading Saved Game
label-gamesave-loading-screen-desc = Press Escape to cancel loading and return to the main menu

## ingame-menu.yaml
label-menu-buttons-title = Options

## ingame-observer.yaml
button-observer-widgets-options = Options (Esc)
button-replay-player-pause-tooltip = Pause
button-replay-player-play-tooltip = Play

button-replay-player-slow =
   .tooltip = Slow speed
   .label = 50%

button-replay-player-regular =
   .tooltip = Regular speed
   .label = 100%

button-replay-player-fast =
   .tooltip = Fast speed
   .label = 200%

button-replay-player-maximum =
   .tooltip = Maximum speed
   .label = MAX

label-basic-stats-player-header = Player
label-basic-stats-cash-header = Cash
label-basic-stats-power-header = Power
label-basic-stats-kills-header = Kills
label-basic-stats-deaths-header = Deaths
label-basic-stats-assets-destroyed-header = Destroyed
label-basic-stats-assets-lost-header = Lost
label-basic-stats-experience-header = Score
label-basic-stats-actions-min-header = APM
label-economy-stats-player-header = Player
label-economy-stats-cash-header = Cash
label-economy-stats-income-header = Income
label-economy-stats-assets-header = Assets
label-economy-stats-earned-header = Earned
label-economy-stats-spent-header = Spent
label-economy-stats-miners-header = Miners
label-production-stats-player-header = Player
label-production-stats-header = Production
label-support-powers-player-header = Player
label-support-powers-header = Support Powers
label-army-player-header = Player
label-army-header = Army
label-combat-stats-player-header = Player
label-combat-stats-assets-destroyed-header = Destroyed
label-combat-stats-assets-lost-header = Lost
label-combat-stats-units-killed-header = U. Killed
label-combat-stats-units-dead-header = U. Lost
label-combat-stats-buildings-killed-header = B. Killed
label-combat-stats-buildings-dead-header = B. Lost
label-combat-stats-army-value-header = Army Value
label-combat-stats-vision-header = Vision

## ingame-player.yaml
supportpowers-support-powers-palette =
   .ready = READY
   .hold = ON HOLD

button-command-bar-attack-move =
   .tooltip = Attack Move
   .tooltipdesc = Selected units will move to the desired location
    and attack any enemies they encounter en route.

    Hold <(Ctrl)> while targeting to order an Assault Move
    that attacks any units or structures encountered en route.

    Left-click icon then right-click on target location.

button-command-bar-force-move =
   .tooltip = Force Move
   .tooltipdesc = Selected units will move to the desired location
     - Default activity for the target is suppressed
     - Vehicles will attempt to crush enemies at the target location
     - Helicopters will land at the target location

    Left-click icon then right-click on target.
    Hold <(Alt)> to activate temporarily while commanding units.

button-command-bar-force-attack =
   .tooltip = Force Attack
   .tooltipdesc = Selected units will attack the targeted unit or location
     - Default activity for the target is suppressed
     - Allows targeting of own or ally forces
     - Long-range artillery units will always target the
       location, ignoring units and buildings

    Left-click icon then right-click on target.
    Hold <(Ctrl)> to activate temporarily while commanding units.

button-command-bar-guard =
   .tooltip = Guard
   .tooltipdesc = Selected units will follow the targeted unit.

    Left-click icon then right-click on target unit.

button-command-bar-deploy =
   .tooltip = Deploy
   .tooltipdesc = Selected units will perform their default deploy activity
     - Scouts will start scanning
     - Transports will unload their passengers
     - Minelayers will deploy a mine
     - Aircraft will return to base

    Acts immediately on selected units.

button-command-bar-scatter =
   .tooltip = Scatter
   .tooltipdesc = Selected units will stop their current activity
    and move to a nearby location.

    Acts immediately on selected units.

button-command-bar-stop =
   .tooltip = Stop
   .tooltipdesc = Selected units will stop their current activity.

    Acts immediately on selected units.

button-command-bar-queue-orders =
   .tooltip = Waypoint Mode
   .tooltipdesc = Use Waypoint Mode to give multiple linking commands
    to the selected units. Units will execute the commands
    immediately upon receiving them.

    Left-click icon then give commands in the game world.
    Hold <(Shift)> to activate temporarily while commanding units.

button-stance-bar-attackanything =
   .tooltip = Attack Anything Stance
   .tooltipdesc = Set the selected units to Attack Anything stance:
     - Units will attack enemy units and structures on sight
     - Units will pursue attackers across the battlefield

button-stance-bar-defend =
   .tooltip = Defend Stance
   .tooltipdesc = Set the selected units to Defend stance:
     - Units will attack enemy units on sight
     - Units will not move or pursue enemies

button-stance-bar-returnfire =
   .tooltip = Return Fire Stance
   .tooltipdesc = Set the selected units to Return Fire stance:
     - Units will retaliate against enemies that attack them
     - Units will not move or pursue enemies

button-stance-bar-holdfire =
   .tooltip = Hold Fire Stance
   .tooltipdesc = Set the selected units to Hold Fire stance:
     - Units will not fire upon enemies
     - Units will not move or pursue enemies

label-mute-indicator = Audio Muted
button-top-buttons-beacon-tooltip = Place Beacon
button-top-buttons-sell-tooltip = Sell
button-top-buttons-power-tooltip = Power Down
button-top-buttons-repair-tooltip = Repair
button-top-buttons-options-tooltip = Options
button-top-buttons-debug-tooltip = Debug Menu
labelwithtooltip-player-widgets-cash = <0>
labelwithtooltip-player-widgets-power = <0>

productionpalette-sidebar-production-palette =
   .ready = READY
   .hold = ON HOLD

button-production-types-building-tooltip = Buildings
button-production-types-defense-tooltip = Defense
button-production-types-pod-tooltip = Pods
button-production-types-light-vehicle-tooltip = Utility
button-production-types-heavy-vehicle-tooltip = Tank
button-production-types-aircraft-tooltip = Aircraft
button-production-types-naval-tooltip = Naval
button-production-types-trade-tooltip = Trade
button-production-types-scroll-up-tooltip = Scroll up
button-production-types-scroll-down-tooltip = Scroll down

## lobby-globalchat.yaml, multiplayer-globalchat.yaml
button-globalchat-main-panel-disconnect = Leave Chat
label-globalchat-connect-panel-global-chat = Global Chat
label-globalchat-connect-panel-nickname = Nickname:
button-globalchat-connect-panel = Connect

## lobby-options.yaml
label-lobby-options-bin-title = Map Options

## lobby-servers.yaml
image-lobby-servers-bin-password-protected-tooltip = Requires Password
image-lobby-servers-bin-requires-authentication-tooltip = Requires OpenRA forum account
dropdownbutton-lobby-servers-bin-filters = Filter Games

## lobby-servers.yaml, multiplayer-browser.yaml
label-container-name = Server
label-container-players = Players
label-container-location = Location
label-container-status = Status
label-notice-container-outdated-version = You are running an outdated version of OpenHV. Download the latest version from www.openhv.net
label-notice-container-unknown-version = You are running an unrecognized version of OpenHV. Download the latest version from www.openhv.net
label-notice-container-prerelease-available = A preview of the next OpenHV release is available for testing. Download the pre-release from www.openhv.net

## lobby.yaml
dropdownbutton-server-lobby-slots = Slot Admin
button-skirmish-tabs-players-tab = Players
button-skirmish-tabs-options-tab = Options
button-skirmish-tabs-music-tab = Music
button-multiplayer-tabs-players-tab = Players
button-multiplayer-tabs-options-tab = Options
button-multiplayer-tabs-music-tab = Music
button-multiplayer-tabs-servers-tab = Servers
button-server-lobby-changemap = Change Map
button-server-lobby-lobbychat-tab = Game
button-server-lobby-globalchat-tab = Global

button-lobbychat-chat-mode =
   .label = Team
   .tooltip = Toggle chat mode

button-server-lobby-start-game = Start Game
button-server-lobby-disconnect = Leave Game

## main-menu.yaml
label-main-menu-mainmenu-title = OpenHV
button-main-menu-encyclopedia = Encyclopedia
button-singleplayer-menu-skirmish = Skirmish
button-singleplayer-menu-load = Load
button-extras-menu-replays = Replays
button-extras-menu-assetbrowser = Asset Browser
dropdownbutton-news-bg-button = Subspace Transmissions
label-update-notice-a = You are running an outdated version of OpenHV.
label-update-notice-b = Download the latest version from www.github.com

## mainmenu-prompts.yaml
label-mainmenu-introduction-prompt-title = Incoming subspace transmission
label-mainmenu-introduction-prompt-desc-a = Greetings Commander! Initialize combat parameters using the options below.
label-mainmenu-introduction-prompt-desc-b = Additional options can be configured later from the Settings menu.

label-section-header =
   .label = Profile
   .label = Input
   .label = Display

label-player-container = Player Name:
label-playercolor-container-color = Preferred Color:
label-mouse-control-container = Control Scheme:
label-mouse-control-desc-classic-selection = - Select units using the <Left> mouse button
label-mouse-control-desc-classic-commands = - Command units using the <Left> mouse button
label-mouse-control-desc-classic-buildigs = - Place structures using the <Left> mouse button
label-mouse-control-desc-classic-support = - Target support powers using the <Left> mouse button
label-mouse-control-desc-classic-zoom = - Zoom the battlefield using the <Scroll Wheel>
label-mouse-control-desc-classic-zoom-modifier = - Zoom the battlefield using <MODIFIER + Scroll Wheel>
label-mouse-control-desc-classic-scroll-right = - Pan the battlefield using the <Right> mouse button
label-mouse-control-desc-classic-scroll-middle = - Pan the battlefield using the <Middle> mouse button
label-mouse-control-desc-classic-edgescroll = or by moving the cursor to the edge of the screen
label-mouse-control-desc-modern-selection = - Select units using the <Left> mouse button
label-mouse-control-desc-modern-commands = - Command units using the <Right> mouse button
label-mouse-control-desc-modern-buildigs = - Place structures using the <Left> mouse button
label-mouse-control-desc-modern-support = - Target support powers using the <Left> mouse button
label-mouse-control-desc-modern-zoom = - Zoom the battlefield using the <Scroll Wheel>
label-mouse-control-desc-modern-zoom-modifier = - Zoom the battlefield using <MODIFIER + Scroll Wheel>
label-mouse-control-desc-modern-scroll-right = - Pan the battlefield using the <Right> mouse button
label-mouse-control-desc-modern-scroll-middle = - Pan the battlefield using the <Middle> mouse button
label-mouse-control-desc-modern-edgescroll = or by moving the cursor to the edge of the screen
checkbox-edgescroll-container = Screen Edge Panning
label-battlefield-camera-dropdown-container = Battlefield Camera:
label-ui-scale-dropdown-container = UI Scale:
checkbox-cursordouble-container = Increase Cursor Size
button-mainmenu-introduction-prompt-continue = Continue

## multiplayer-browser.yaml
image-multiplayer-panel-password-protected-tooltip = Requires Password
image-multiplayer-panel-requires-authentication-tooltip = Requires OpenRA forum account
button-selected-server-join = Join
dropdownbutton-multiplayer-panel-filters = Filter Games
button-multiplayer-panel-directconnect = Direct IP
button-multiplayer-panel-create = Create

## settings-advanced.yaml
label-network-section-header = Advanced
checkbox-nat-discovery-container = Enable UPnP/NAT-PMP Discovery
checkbox-fetch-news-container = Fetch Community News
checkbox-perfgraph-container = Show Performance Graph
checkbox-check-version-container = Check for Updates
checkbox-perftext-container = Show Performance Text
checkbox-sendsysinfo-container = Send System Information
label-sendsysinfo-checkbox-container-desc = Your Operating System, OpenGL and .NET runtime versions, and language settings will be sent along with an anonymous ID to help prioritize future development.
label-debug-section-header = Developer
label-debug-hidden-container-a = Additional developer-specific options can be enabled via the
label-debug-hidden-container-b = Debug.DisplayDeveloperSettings setting or launch flag
checkbox-botdebug-container = Show Bot Debug Messages
checkbox-checkbotsync-container = Check Sync around BotModule Code
checkbox-luadebug-container = Show Map Debug Messages
checkbox-checkunsynced-container = Check Sync around Unsynced Code
checkbox-replay-commands-container = Enable Debug Commands in Replays
checkbox-perflogging-container = Enable Tick Performance Logging
