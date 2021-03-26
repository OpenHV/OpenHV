#!/bin/sh
# Helper functions for packaging and installing projects using the OpenRA Mod SDK

####
# This file must stay /bin/sh and POSIX compliant for macOS and BSD portability.
# Copy-paste the entire script into http://shellcheck.net to check.
####

# Compile and publish (using Mono) any mod assemblies to the target directory
# Arguments:
#   SRC_PATH: Path to the root OpenRA directory
#   DEST_PATH: Path to the root of the install destination (will be created if necessary)
#   TARGETPLATFORM: Platform type (win-x86, win-x64, osx-x64, linux-x64, unix-generic)
install_mod_assemblies_mono() {
	SRC_PATH="${1}"
	DEST_PATH="${2}"
	TARGETPLATFORM="${3}"
	ENGINE_PATH="${4}"

	echo "Building assemblies"
	ORIG_PWD=$(pwd)
	cd "${SRC_PATH}" || exit 1

	rm -rf "${ENGINE_PATH:?}/bin"

	find . -maxdepth 1 -name '*.sln' -exec msbuild -verbosity:m -nologo -t:Build -restore -p:Configuration=Release -p:TargetPlatform="${TARGETPLATFORM}" -p:Mono=true \;

	cd "${ORIG_PWD}" || exit 1
	for LIB in "${ENGINE_PATH}/bin/"*.dll "${ENGINE_PATH}/bin/"*.dll.config; do
		install -m644 "${LIB}" "${DEST_PATH}"
	done

	if [ "${TARGETPLATFORM}" = "linux-x64" ]; then
		for LIB in "${ENGINE_PATH}/bin/"*.so; do
			install -m755 "${LIB}" "${DEST_PATH}"
		done
	fi

	if [ "${TARGETPLATFORM}" = "osx-x64" ]; then
		for LIB in "${ENGINE_PATH}/bin/"*.dylib; do
			install -m755 "${LIB}" "${DEST_PATH}"
		done
	fi
}

# Compile and publish any mod assemblies to the target directory
# Arguments:
#   SRC_PATH: Path to the root SDK directory
#   DEST_PATH: Path to the root of the install destination (will be created if necessary)
#   TARGETPLATFORM: Platform type (win-x86, win-x64, osx-x64, linux-x64, unix-generic)
install_mod_assemblies() {
	SRC_PATH="${1}"
	DEST_PATH="${2}"
	TARGETPLATFORM="${3}"

	ORIG_PWD=$(pwd)
	cd "${SRC_PATH}" || exit 1
	find . -maxdepth 1 -name '*.sln' -exec dotnet publish -c Release -p:TargetPlatform="${TARGETPLATFORM}" -p:PublishTrimmed=true -r "${TARGETPLATFORM}" -o "${DEST_PATH}" --self-contained true \;
	cd "${ORIG_PWD}" || exit 1
}
