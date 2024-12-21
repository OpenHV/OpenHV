#!/bin/bash
# OpenRA packaging script for Linux (AppImage)
set -e

command -v make >/dev/null 2>&1 || { echo >&2 "The OpenRA mod SDK Linux packaging requires make."; exit 1; }
command -v curl >/dev/null 2>&1 || command -v wget > /dev/null 2>&1 || { echo >&2 "The OpenRA mod SDK Linux packaging requires curl or wget."; exit 1; }

require_variables() {
	missing=""
	for i in "$@"; do
		eval check="\$$i"
		[ -z "${check}" ] && missing="${missing}   ${i}\n"
	done
	if [ -n "${missing}" ]; then
		printf "Required mod.config variables are missing:\n%sRepair your mod.config (or user.config) and try again.\n" "${missing}"
		exit 1
	fi
}

if [ $# -eq "0" ]; then
	echo "Usage: $(basename "$0") version [outputdir]"
	exit 1
fi

PACKAGING_DIR=$(dirname $(realpath "$0"))
TEMPLATE_ROOT="${PACKAGING_DIR}/../../"
ARTWORK_DIR="${PACKAGING_DIR}/../artwork/"

# shellcheck source=mod.config
. "${TEMPLATE_ROOT}/mod.config"

if [ -f "${TEMPLATE_ROOT}/user.config" ]; then
	# shellcheck source=user.config
	. "${TEMPLATE_ROOT}/user.config"
fi

require_variables "MOD_ID" "ENGINE_DIRECTORY" "PACKAGING_DISPLAY_NAME" "PACKAGING_INSTALLER_NAME" "PACKAGING_COPY_CNC_DLL" "PACKAGING_COPY_D2K_DLL" \
	"PACKAGING_FAQ_URL" "PACKAGING_OVERWRITE_MOD_VERSION"

TAG="$1"
if [ $# -eq "1" ]; then
	OUTPUTDIR=$(realpath .)
else
	OUTPUTDIR=$(realpath "$2")
fi

APPDIR="${PACKAGING_DIR}/${PACKAGING_INSTALLER_NAME}.appdir"
PREFIX="/usr"

# Set the working dir to the location of this script
cd "${PACKAGING_DIR}"

if [ ! -f "${TEMPLATE_ROOT}/${ENGINE_DIRECTORY}/Makefile" ]; then
	echo "Required engine files not found."
	echo "Run \`make\` in the mod directory to fetch and build the required files, then try again.";
	exit 1
fi

. "${TEMPLATE_ROOT}/${ENGINE_DIRECTORY}/packaging/functions.sh"
. "${TEMPLATE_ROOT}/packaging/linux/functions.sh"
. "${TEMPLATE_ROOT}/packaging/functions.sh"

if [ ! -d "${OUTPUTDIR}" ]; then
	echo "Output directory '${OUTPUTDIR}' does not exist.";
	exit 1
fi

echo "Building core files"
install_assemblies "${TEMPLATE_ROOT}/${ENGINE_DIRECTORY}" "${APPDIR}${PREFIX}/lib/openhv" "linux-x64" "net6" "True" "${PACKAGING_COPY_CNC_DLL}" "${PACKAGING_COPY_D2K_DLL}"
install_data "${TEMPLATE_ROOT}/${ENGINE_DIRECTORY}" "${APPDIR}${PREFIX}/lib/openhv"
rm -rf "${APPDIR}${PREFIX}/lib/openhv/global mix database.dat"

for f in ${PACKAGING_COPY_ENGINE_FILES}; do
	mkdir -p "${APPDIR}${PREFIX}/lib/openhv/$(dirname "${f}")"
	cp -r "${TEMPLATE_ROOT}/${ENGINE_DIRECTORY}/${f}" "${APPDIR}${PREFIX}/lib/openhv/${f}"
done

echo "Building mod files"
install_mod_assemblies "${TEMPLATE_ROOT}" "${APPDIR}${PREFIX}/lib/openhv" "linux-x64" "net6" "${TEMPLATE_ROOT}/${ENGINE_DIRECTORY}"

cp -Lr "${TEMPLATE_ROOT}/mods/"* "${APPDIR}${PREFIX}/lib/openhv/mods"

set_engine_version "${ENGINE_VERSION}" "${APPDIR}${PREFIX}/lib/openhv"
if [ "${PACKAGING_OVERWRITE_MOD_VERSION}" == "True" ]; then
	set_mod_version "${TAG}" "${APPDIR}${PREFIX}/lib/openhv/mods/${MOD_ID}/mod.yaml"
else
	MOD_VERSION=$(grep 'Version:' "${APPDIR}${PREFIX}/lib/openhv/mods/${MOD_ID}/mod.yaml" | awk '{print $2}')
	echo "Mod version ${MOD_VERSION} will remain unchanged.";
fi

# Add native libraries
echo "Downloading appimagetool"
if command -v curl >/dev/null 2>&1; then
	curl -s -L -O https://github.com/AppImage/appimagetool/releases/download/continuous/appimagetool-x86_64.AppImage || exit 3
else
	wget -cq https://github.com/AppImage/appimagetool/releases/download/continuous/appimagetool-x86_64.AppImage || exit 3
fi

echo "Building AppImage"

# Add launcher and icons
sed "s/openra-{MODID}/openhv/g" "${TEMPLATE_ROOT}/${ENGINE_DIRECTORY}/packaging/linux/AppRun.in" | sed "s/{MODNAME}/${PACKAGING_DISPLAY_NAME}/g" > "${APPDIR}/AppRun"
chmod 0755 "${APPDIR}/AppRun"

BINDIR="${APPDIR}${PREFIX}/bin"
install_executables "${BINDIR}" "${TEMPLATE_ROOT}" "${ENGINE_DIRECTORY}" "${MOD_ID}" "${TAG}" "${PACKAGING_DISPLAY_NAME}" "${PACKAGING_INSTALLER_NAME}" "${PACKAGING_FAQ_URL}"

DATADIR="${APPDIR}${PREFIX}/share"
install_metadata "${DATADIR}" "${TEMPLATE_ROOT}" "${ENGINE_DIRECTORY}"  "${MOD_ID}" "${TAG}" "${PACKAGING_DISPLAY_NAME}" "${PACKAGING_DISCORD_APPID}" "${PACKAGING_DIR}" "${ARTWORK_DIR}"
cp "${DATADIR}/applications/openhv.desktop" "${APPDIR}/openhv.desktop"
cp "${DATADIR}/icons/hicolor/256x256/apps/openhv.png" "${APPDIR}/openhv.png"

install -m 0755 "${TEMPLATE_ROOT}/${ENGINE_DIRECTORY}/packaging/linux/gtk-dialog.py" "${BINDIR}/gtk-dialog.py"

chmod a+x appimagetool-x86_64.AppImage
APPIMAGE="${PACKAGING_INSTALLER_NAME}-${TAG}-x86_64.AppImage"
ARCH=x86_64 ./appimagetool-x86_64.AppImage -u "gh-releases-zsync|OpenHV|OpenHV|latest|OpenHV-*.AppImage.zsync" "${APPDIR}" "${OUTPUTDIR}/${APPIMAGE}"
zsyncmake -u "https://github.com/OpenHV/OpenHV/releases/download/${TAG}/${APPIMAGE}" -o "${OUTPUTDIR}/${APPIMAGE}.zsync" "${OUTPUTDIR}/${APPIMAGE}"

# Clean up
rm -rf appimagetool-x86_64.AppImage "${PACKAGING_APPIMAGE_DEPENDENCIES_TEMP_ARCHIVE_NAME}" "${APPDIR}"
