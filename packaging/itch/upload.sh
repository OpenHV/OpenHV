#!/bin/bash

export GIT_TAG="$1"
export BUILD_OUTPUT_DIR="$2"

if [[ "$OSTYPE" == "darwin"* ]]; then
	echo "itch upload requires a Linux host."
	exit 0
fi

if command -v curl >/dev/null 2>&1; then
	curl -L -o butler-linux-amd64.zip https://broth.itch.ovh/butler/linux-amd64/LATEST/archive/default
else
	wget -cq -O butler-linux-amd64.zip https://broth.itch.ovh/butler/linux-amd64/LATEST/archive/default
fi

unzip butler-linux-amd64.zip
chmod +x butler
./butler -V
./butler login

./butler push "${BUILD_OUTPUT_DIR}/OpenHV-${GIT_TAG}-x64-winportable.zip" "openhv/openhv:win" --userversion ${GIT_TAG}
./butler push "${BUILD_OUTPUT_DIR}/OpenHV-${GIT_TAG}-macOS.zip" "openhv/openhv:osx" --userversion ${GIT_TAG}
./butler push --fix-permissions "${BUILD_OUTPUT_DIR}/OpenHV-${GIT_TAG}-x86_64.AppImage" "openhv/openhv:linux" --userversion ${GIT_TAG}
