# OpenHV [![Continuous Integration](https://img.shields.io/github/actions/workflow/status/OpenHV/OpenHV/ci.yml)](https://github.com/OpenHV/OpenHV/actions/workflows/ci.yml) [![Documentation Status](https://readthedocs.org/projects/openhv/badge/?version=latest)](https://openhv.readthedocs.io/en/latest/?badge=latest) [![Discord](https://discordapp.com/api/guilds/840983316395720715/widget.png)](https://discord.gg/X3VUtPtBTu) [![Matrix](https://matrix.to/img/matrix-badge.svg)](https://matrix.to/#/+openhv:matrix.org) [![IRC/Jabber](https://img.shields.io/badge/IRC/Jabber-on%20FreeGameDev-blue.svg)](https://freegamedev.net/irc/#openhv)

A mod for [OpenRA](https://www.openra.net) based on the [Hard Vacuum](https://lostgarden.home.blog/2005/03/27/game-post-mortem-hard-vacuum/) design by Daniel Cook. It aims to be an open content real-time strategy game with no exceptions. Set in the distant future where mega corporation battle themselves this standalone title comes with multiplayer (LAN and internet) support, competent skirmish bots as well as an integrated map editor. It allows for spectators to join and replays to be shared.

![Turncoat Trail](https://www.openhv.net/images/readme/turncoat-trail.png)

# Getting Started [![Packaging status](https://repology.org/badge/tiny-repos/openhv.svg)](https://repology.org/project/openra/versions)

To launch the project from the development environment you must first compile the project by running `make.cmd` (Windows), or opening a terminal in the SDK directory and running `make` (Linux / macOS). You can then run `launch-game.cmd` (Windows) or `launch-game.sh` (Linux / macOS) to run the game. More details on [building](https://github.com/OpenHV/OpenHV/wiki/Build) the game are available at the wiki.

![MiniYAML](https://www.openhv.net/images/readme/miniyaml.png)

Game rules are defined in text files using a dialect called `MiniYAML` which has [IDE support in Visual Studio Code](https://marketplace.visualstudio.com/items?itemName=openra.oraide-vscode).

![MiniYAML](https://www.openhv.net/images/readme/lua.png)

Script missions or mini-games in Lua. See the [API](https://openhv.readthedocs.io/en/latest/release/lua/) for details and use the [VS Code extension](https://marketplace.visualstudio.com/items?itemName=openra.vscode-openra-lua) for code completion.

# Licensing ![GPL](https://img.shields.io/github/license/OpenHV/OpenHV)

OpenHV just like the OpenRA engine and SDK scripts is made available under the [GPLv3](https://github.com/OpenHV/OpenHV/blob/main/COPYING) license.

The mod data files (artwork, sound files, yaml, etc) are not part of the source code and are distributed under different terms. Various [Creative Commons](https://creativecommons.org/) licenses apply. Check the ReadMe files in the sub folders for details.
