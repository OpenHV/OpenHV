#!/bin/sh
# Shared between Makefile and AppImage build scripts.
set -e

TEMPLATE_ROOT="${PACKAGING_DIR}/../../"

install_executables() {
	BINDIR="${1}"
	TEMPLATE_ROOT="${2}"
	ENGINE_DIRECTORY="${3}"
	MOD_ID="${4}"
	TAG="${5}"
	PACKAGING_DISPLAY_NAME="${6}"
	PACKAGING_INSTALLER_NAME="${7}"
	PACKAGING_FAQ_URL="${8}"

	install -d "${BINDIR}"

	sed "s#lib/openra#lib/openhv#" "${TEMPLATE_ROOT}/${ENGINE_DIRECTORY}/packaging/linux/openra.appimage.in" | sed "s/openra-{MODID}/openhv/g" | sed "s/{MODID}/${MOD_ID}/g" | sed "s/{TAG}/${TAG}/g" | sed "s/{MODNAME}/${PACKAGING_DISPLAY_NAME}/g" | sed "s/{MODINSTALLERNAME}/${PACKAGING_INSTALLER_NAME}/g" | sed "s|{MODFAQURL}|${PACKAGING_FAQ_URL}|g" > "${BINDIR}/openhv"
	chmod 0755 "${BINDIR}/openhv"

	sed "s#lib/openra#lib/openhv#" "${TEMPLATE_ROOT}/${ENGINE_DIRECTORY}/packaging/linux/openra-server.appimage.in" | sed "s/{MODID}/${MOD_ID}/g" > "${BINDIR}/openhv-server"
	chmod 0755 "${BINDIR}/openhv-server"

	sed "s#lib/openra#lib/openhv#" "${TEMPLATE_ROOT}/${ENGINE_DIRECTORY}/packaging/linux/openra-utility.appimage.in" | sed "s/{MODID}/${MOD_ID}/g" > "${BINDIR}/openhv-utility"
	chmod 0755 "${BINDIR}/openhv-utility"
}

install_metadata() {
	DATADIR="${1}"
	TEMPLATE_ROOT="${2}"
	ENGINE_DIRECTORY="${3}"
	MOD_ID="${4}"
	TAG="${5}"
	PACKAGING_DISPLAY_NAME="${6}"
	PACKAGING_DISCORD_APPID="${7}"
	PACKAGING_DIR="${8}"
	ARTWORK_DIR="${9}"

	if [ -n "${PACKAGING_DISCORD_APPID}" ]; then
		sed "s/{DISCORDAPPID}/${PACKAGING_DISCORD_APPID}/g" "${TEMPLATE_ROOT}/${ENGINE_DIRECTORY}/packaging/linux/openra.desktop.discord.in" > temp.desktop.in
		sed "s/{DISCORDAPPID}/${PACKAGING_DISCORD_APPID}/g" "${TEMPLATE_ROOT}/${ENGINE_DIRECTORY}/packaging/linux/openra-mimeinfo.xml.discord.in" > temp.xml.in
	else
		cp "${TEMPLATE_ROOT}/${ENGINE_DIRECTORY}/packaging/linux/openra.desktop.in" temp.desktop.in
		cp "${TEMPLATE_ROOT}/${ENGINE_DIRECTORY}/packaging/linux/openra-mimeinfo.xml.in" temp.xml.in
	fi

	mkdir -p "${DATADIR}/applications"
	chmod 0644 temp.desktop.in
	sed "s/openra-{MODID}-{TAG}/openra-${MOD_ID}-${TAG}/g" temp.desktop.in | sed "s/openra-{MODID}/openhv/g" | sed "s/OpenRA - {MODNAME}/OpenHV/g" | sed "s/{MODNAME}/${PACKAGING_DISPLAY_NAME}/g" | sed "s/{TAG}/${TAG}/g" > "${DATADIR}/applications/openhv.desktop"
	rm temp.desktop.in

	mkdir -p "${DATADIR}/mime/packages"
	chmod 0644 temp.xml.in
	sed "s/openra-{MODID}-{TAG}/openra-${MOD_ID}-${TAG}/g" temp.xml.in | sed "s/openra-{MODID}/openhv/g" | sed "s/{TAG}/${TAG}/g" > "${DATADIR}/mime/packages/openhv.xml"
	rm temp.xml.in

	if [ -f "${ARTWORK_DIR}/icon_scalable.svg" ]; then
		install -Dm644 "${ARTWORK_DIR}/icon_scalable.svg" "${DATADIR}/icons/hicolor/scalable/apps/openhv.svg"
	fi

	for i in 16x16 32x32 48x48 64x64 128x128 256x256 512x512 1024x1024; do
		if [ -f "${ARTWORK_DIR}/icon_${i}.png" ]; then
			install -Dm644 "${ARTWORK_DIR}/icon_${i}.png" "${DATADIR}/icons/hicolor/${i}/apps/openhv.png"
		fi
	done

	install -Dm0644 "${PACKAGING_DIR}/openhv.metainfo.xml" "${DATADIR}/metainfo/openhv.metainfo.xml"
}
