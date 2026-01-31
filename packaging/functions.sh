#!/bin/sh
# Helper functions for packaging and installing projects using the OpenRA Mod SDK

####
# This file must stay /bin/sh and POSIX compliant for macOS and BSD portability.
# Copy-paste the entire script into http://shellcheck.net to check.
####

# Compile and publish any mod assemblies to the target directory
# Arguments:
#   SRC_PATH: Path to the root SDK directory
#   DEST_PATH: Path to the root of the install destination (will be created if necessary)
#   TARGETPLATFORM: Platform type (win-x64, osx-x64, linux-x64, unix-generic)
#   ENGINE_PATH: Path to the engine root directory
install_mod_assemblies() {
	SRC_PATH="${1}"
	DEST_PATH="${2}"
	TARGETPLATFORM="${3}"
	ENGINE_PATH="${4}"

	ORIG_PWD=$(pwd)
	cd "${SRC_PATH}" || exit 1

	mkdir -p "${DEST_PATH}"

	find . -maxdepth 1 -name '*.sln' -exec dotnet publish -c Release -p:TargetPlatform="${TARGETPLATFORM}" -r "${TARGETPLATFORM}" --property:PublishDir="${DEST_PATH}" --self-contained true \;
	cd "${ORIG_PWD}" || exit 1
}

# Copy the core engine and specified mod data to the target directory
# Arguments:
#   SRC_PATH: Path to the root engine directory
#   DEST_PATH: Path to the root of the install destination (will be created if necessary)
# Used by:
#   Makefile (install target for local installs and downstream packaging)
#   Linux AppImage packaging
#   macOS packaging
#   Windows packaging
#   Mod SDK Linux AppImage packaging
#   Mod SDK macOS packaging
#   Mod SDK Windows packaging
install_data() (
	set -o errexit || exit $?

	SRC_PATH="${1}"
	DEST_PATH="${2}"
	shift 2

	echo "Installing engine files to ${DEST_PATH}"
	for FILE in VERSION AUTHORS COPYING IP2LOCATION-LITE-DB1.IPV6.BIN.ZIP; do
		install -m644 "${SRC_PATH}/${FILE}" "${DEST_PATH}"
	done

	cp -r "${SRC_PATH}/glsl" "${DEST_PATH}"

	install -d "${DEST_PATH}/mods"

	echo "Installing common mod files to ${DEST_PATH}"
	cp -r "${SRC_PATH}/mods/common" "${DEST_PATH}/mods/"
	rm "${DEST_PATH}/mods/common/FreeSans.ttf"
	rm "${DEST_PATH}/mods/common/FreeSansBold.ttf"

	echo "Installing Hard Vacuum mod files to ${DEST_PATH}"
	cp -r "${SRC_PATH}/../mods/hv" "${DEST_PATH}/mods/"
)
