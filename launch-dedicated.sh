#!/bin/sh
# example launch script, see https://github.com/OpenHV/OpenHV/wiki/Server for details

# Usage:
#  $ ./launch-dedicated.sh # Launch a dedicated server with default settings
#  $ NAME="My Server" ./launch-dedicated.sh # Launch a dedicated server with default settings but override the server name.
#  Read the file to see which settings you can override

set -e
if ! command -v dotnet >/dev/null 2>&1; then
	{ echo >&2 "OpenHV requires dotnet."; exit 1; }
fi

require_variables() {
	missing=""
	for i in "$@"; do
		eval check="\$$i"
		[ -z "${check}" ] && missing="${missing}   ${i}\n"
	done
	if [ ! -z "${missing}" ]; then
		echo "Required mod.config variables are missing:\n${missing}Repair your mod.config (or user.config) and try again."
		exit 1
	fi
}

TEMPLATE_LAUNCHER=$(realpath "$0")
TEMPLATE_ROOT=$(dirname "${TEMPLATE_LAUNCHER}")
MOD_SEARCH_PATHS="${TEMPLATE_ROOT}/mods,./mods"

# shellcheck source=mod.config
. "${TEMPLATE_ROOT}/mod.config"

if [ -f "${TEMPLATE_ROOT}/user.config" ]; then
	# shellcheck source=user.config
	. "${TEMPLATE_ROOT}/user.config"
fi

require_variables "MOD_ID" "ENGINE_VERSION" "ENGINE_DIRECTORY"

NAME="${Name:-"Dedicated Server"}"
LAUNCH_MOD="${Mod:-"${MOD_ID}"}"
LISTEN_PORT="${ListenPort:-"1234"}"
ADVERTISE_ONLINE="${AdvertiseOnline:-"True"}"
PASSWORD="${Password:-""}"
RECORD_REPLAYS="${RecordReplays:-"False"}"

REQUIRE_AUTHENTICATION="${RequireAuthentication:-"False"}"
PROFILE_ID_BLACKLIST="${ProfileIDBlacklist:-""}"
PROFILE_ID_WHITELIST="${ProfileIDWhitelist:-""}"

ENABLE_SINGLE_PLAYER="${EnableSingleplayer:-"False"}"
ENABLE_SYNC_REPORTS="${EnableSyncReports:-"False"}"
ENABLE_GEOIP="${EnableGeoIP:-"True"}"
ENABLE_LINT_CHECKS="${EnableLintChecks:-"True"}"
SHARE_ANONYMISED_IPS="${ShareAnonymizedIPs:-"True"}"

FLOOD_LIMIT_JOIN_COOLDOWN="${FloodLimitJoinCooldown:-"5000"}"

SUPPORT_DIR="${SupportDir:-""}"

cd "${TEMPLATE_ROOT}"
if [ ! -f "${ENGINE_DIRECTORY}/bin/OpenRA.Server.dll" ] || [ "$(cat "${ENGINE_DIRECTORY}/VERSION")" != "${ENGINE_VERSION}" ]; then
	echo "Required engine files not found."
	echo "Run \`make\` in the mod directory to fetch and build the required files, then try again.";
	exit 1
fi

cd "${ENGINE_DIRECTORY}"

while true; do
	MOD_SEARCH_PATHS="${MOD_SEARCH_PATHS}" \
	dotnet bin/OpenRA.Server.dll Engine.EngineDir=".." Game.Mod="${LAUNCH_MOD}" \
	Server.Name="${NAME}" \
	Server.Map="${MAP}" \
	Server.ListenPort="${LISTEN_PORT}" \
	Server.AdvertiseOnline="${ADVERTISE_ONLINE}" \
	Server.Password="${PASSWORD}" \
	Server.RecordReplays="${RECORD_REPLAYS}" \
	Server.RequireAuthentication="${REQUIRE_AUTHENTICATION}" \
	Server.ProfileIDBlacklist="${PROFILE_ID_BLACKLIST}" \
	Server.ProfileIDWhitelist="${PROFILE_ID_WHITELIST}" \
	Server.EnableSingleplayer="${ENABLE_SINGLE_PLAYER}" \
	Server.EnableSyncReports="${ENABLE_SYNC_REPORTS}" \
	Server.EnableGeoIP="${ENABLE_GEOIP}" \
	Server.EnableLintChecks="${ENABLE_LINT_CHECKS}" \
	Server.ShareAnonymizedIPs="${SHARE_ANONYMISED_IPS}" \
	Server.FloodLimitJoinCooldown="${FLOOD_LIMIT_JOIN_COOLDOWN}" \
	Engine.SupportDir="${SUPPORT_DIR}"
done
