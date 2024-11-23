############################# INSTRUCTIONS #############################
#
# to compile, run:
#   make
#
# to compile using Mono (version 6.12 or greater) instead of .NET 6, run:
#   make RUNTIME=mono
#
# to compile using system libraries for native dependencies, run:
#   make [RUNTIME=net6] TARGETPLATFORM=unix-generic
#
# to remove the files created by compiling, run:
#   make clean
#
# to set the mods version, run:
#   make version [VERSION="custom-version"]
#
# to check lua scripts for syntax errors, run:
#   make check-scripts
#
# to check the engine and your mod dlls for StyleCop violations, run:
#   make [RUNTIME=net6] check
#
# to check your mod yaml for errors, run:
#   make [RUNTIME=net6] test

.PHONY: engine all clean version check-scripts check test install
.DEFAULT_GOAL := all

VERSION = $(shell git name-rev --name-only --tags --no-undefined HEAD 2>/dev/null || echo git-`git rev-parse --short HEAD`)
MOD_ID = $(shell cat user.config mod.config 2> /dev/null | awk -F= '/MOD_ID/ { print $$2; exit }')
ENGINE_DIRECTORY = $(shell cat user.config mod.config 2> /dev/null | awk -F= '/ENGINE_DIRECTORY/ { print $$2; exit }')
MOD_SEARCH_PATHS = "$(shell realpath "$0")/mods,./mods"

MANIFEST_PATH = "mods/$(MOD_ID)/mod.yaml"
HAS_LUAC = $(shell command -v luac 2> /dev/null)
LUA_FILES = $(shell find mods/*/maps/* -iname '*.lua' 2> /dev/null)
MOD_SOLUTION_FILES = $(shell find . -maxdepth 1 -iname '*.sln' 2> /dev/null)
SPRITE_FILES ?= $(shell find mods/*/bits/* -maxdepth 1 -iname '*.png' 2> /dev/null)
PREVIEW_FILES = $(shell find mods/*/maps/* -maxdepth 1 -iname 'map.png' 2> /dev/null)
MAP_FOLDERS = $(shell find mods/hv/maps/* -maxdepth 0 -type d 2> /dev/null)
OGG_FILES := $(shell find mods/*/bits/audio/* -maxdepth 2 -iname '*.ogg' 2> /dev/null | sed 's/ /\\ /g')

MSBUILD = msbuild -verbosity:m -nologo
DOTNET = dotnet

RUNTIME ?= net6
DOTNET_RID = $(shell ${DOTNET} --info | grep RID: | cut -w -f3)

ifndef TARGETPLATFORM
UNAME_S := $(shell uname -s)
UNAME_M := $(shell uname -m)
ifeq ($(UNAME_S),Darwin)
ifeq ($(RUNTIME)-$(DOTNET_RID),net6-osx-arm64)
TARGETPLATFORM = osx-arm64
else
TARGETPLATFORM = osx-x64
endif
else
ifeq ($(UNAME_M),x86_64)
TARGETPLATFORM = linux-x64
else
ifeq ($(UNAME_M),aarch64)
TARGETPLATFORM = linux-arm64
else
TARGETPLATFORM = unix-generic
endif
endif
endif
endif

prefix ?= /usr/local
datadir ?= $(prefix)/share
mandir ?= $(datadir)/man
bindir = $(prefix)/bin
libdir = $(prefix)/lib
gamedir = $(libdir)/openhv

fetch-engine:
	@./fetch-engine.sh || (printf "Unable to continue without engine files\n"; exit 1)

engine: fetch-engine
	@echo "Compiling engine..."
	@cd $(ENGINE_DIRECTORY) && make RUNTIME=$(RUNTIME) TARGETPLATFORM=$(TARGETPLATFORM) all

all: engine
ifeq ($(RUNTIME), mono)
	@command -v $(MSBUILD) >/dev/null || (echo "OpenHV requires the '$(MSBUILD)' tool provided by Mono >= 6.12."; exit 1)
ifneq ("$(MOD_SOLUTION_FILES)","")
	@find . -maxdepth 1 -name '*.sln' -exec $(MSBUILD) -t:Build -restore -p:Configuration=Release -p:TargetPlatform=$(TARGETPLATFORM) -p:Mono=true \;
endif
else
	@find . -maxdepth 1 -name '*.sln' -exec $(DOTNET) build -c Release -p:TargetPlatform=$(TARGETPLATFORM) \;
endif

install: install-assemblies install-executables install-metadata install-data install-man

install-assemblies:
	@mkdir -p $(DESTDIR)$(gamedir)
	@cp -r $(ENGINE_DIRECTORY)/bin/* $(DESTDIR)$(gamedir)
	@rm $(DESTDIR)$(gamedir)/OpenRA.Mods.Cnc.dll
	@rm $(DESTDIR)$(gamedir)/OpenRA.Mods.D2k.dll

install-executables:
	@sh -c '. ./packaging/linux/functions.sh; install_executables $(DESTDIR)$(bindir) . ./engine hv $(VERSION) OpenHV OpenHV https://github.com/OpenHV/OpenHV/wiki/FAQ'

install-metadata:
	@sh -c '. ./packaging/linux/functions.sh; install_metadata $(DESTDIR)$(datadir) . ./engine hv $(VERSION) OpenHV 730762985772941312 ./packaging/linux ./packaging/artwork'

install-data:
	@sh -c '. ./packaging/functions.sh; install_data ./engine $(DESTDIR)$(gamedir)'

install-man:
	@mkdir -p $(DESTDIR)$(mandir)/man6/
	@./utility.sh --man-page > $(DESTDIR)$(mandir)/man6/openhv.6
	@sed -i 's/An Open Source modernization of the early 2D Command \& Conquer games./An Open Source Pixelart Science-Fiction Real-Time-Strategy game/' $(DESTDIR)$(mandir)/man6/openhv.6
	@sed -i 's#http://bugs.openra.net#https://github.com/OpenHV/OpenHV/issues#g' $(DESTDIR)$(mandir)/man6/openhv.6
	@sed -i 's/OPENRA/OPENHV/g' $(DESTDIR)$(mandir)/man6/openhv.6
	@sed -i 's/OpenRA/OpenHV/g' $(DESTDIR)$(mandir)/man6/openhv.6
	@sed -i 's/openra/openhv/g' $(DESTDIR)$(mandir)/man6/openhv.6
	@sed -i '/Game\.Mod/d' $(DESTDIR)$(mandir)/man6/openhv.6

clean: engine
ifneq ("$(MOD_SOLUTION_FILES)","")
ifeq ($(RUNTIME), mono)
	@find . -maxdepth 1 -name '*.sln' -exec $(MSBUILD) -t:clean \;
else
	@find . -maxdepth 1 -name '*.sln' -exec $(DOTNET) clean \;
endif
endif
	@cd $(ENGINE_DIRECTORY) && make clean

version: fetch-engine
	@sh -c '. $(ENGINE_DIRECTORY)/packaging/functions.sh; set_mod_version $(VERSION) $(MANIFEST_PATH)'
	@printf "Version changed to $(VERSION).\n"

check-scripts:
ifeq ("$(HAS_LUAC)","")
	@printf "'luac' not found.\n" && exit 1
endif
	@echo
	@echo "Checking for Lua syntax errors..."
ifneq ("$(LUA_FILES)","")
	@luac -p $(LUA_FILES)
endif

check: engine
ifneq ("$(MOD_SOLUTION_FILES)","")
	@echo "Compiling in debug mode..."
ifeq ($(RUNTIME), mono)
	@$(MSBUILD) -t:clean\;build -restore -p:Configuration=Debug -p:TargetPlatform=$(TARGETPLATFORM) -p:Mono=true -p:EnforceCodeStyleInBuild=true -p:GenerateDocumentationFile=true
else
	@$(DOTNET) clean -c Debug --nologo --verbosity minimal
	@$(DOTNET) build -c Debug -p:TargetPlatform=$(TARGETPLATFORM) -p:EnforceCodeStyleInBuild=true -p:GenerateDocumentationFile=true
endif
endif
	@echo "Checking for explicit interface violations..."
	@./utility.sh --check-explicit-interfaces
	@echo "Checking for incorrect conditional trait interface overrides..."
	@./utility.sh --check-conditional-trait-interface-overrides

test: all
	@echo
	@echo "Testing $(MOD_ID) mod MiniYAML..."
	@./utility.sh --check-yaml
	@echo
	@echo "Checking $(MOD_ID) sprite sequences..."
	@./utility.sh --check-missing-sprites

docs: engine
	@echo
	@echo "Generating trait documentation..."
	@./utility.sh --docs $(VERSION) | python3 ./engine/packaging/format-docs.py > traits.md
	@echo "Generating weapon documentation..."
	@./utility.sh --weapon-docs $(VERSION) | python3 ./engine/packaging/format-docs.py > weapons.md
	@echo "Generating settings documentation..."
	@./utility.sh --sprite-sequence-docs $(VERSION) | python3 ./engine/packaging/format-docs.py > sprites.md
	@echo "Generating Lua documentation..."
	@./utility.sh --lua-docs $(VERSION) > lua.md

bits: engine
ifneq ("$(SPRITE_FILES)","")
	@echo "Adding metadata to PNG sheets..."
	@for SPRITE in $(SPRITE_FILES); do \
		echo $${SPRITE}; \
		./utility.sh --png-sheet-import ../$${SPRITE}; \
	done

	@echo "Recompressing sprite PNGs"
	@for SPRITE in $(SPRITE_FILES); do \
		zopflipng --keepcolortype --keepchunks=tEXt -y -m $${SPRITE} $${SPRITE}; \
	done
endif

maps:
ifneq ("$(PREVIEW_FILES)","")
	@echo "Recompressing map preview PNGs"
	@for SPRITE in $(PREVIEW_FILES); do \
		zopflipng -y -m $${SPRITE} $${SPRITE}; \
	done
endif

check-sprites: engine
ifneq ("$(SPRITE_FILES)","")
	@echo "Checking PNG sheet metadata..."
	@for SPRITE in $(SPRITE_FILES); do \
		CURRENT_DIR=$(shell pwd)/; \
		case "$${SPRITE}" in \
			"$${CURRENT_DIR}"*) \
				SPRITE="$${SPRITE}" ;; \
			*) \
				SPRITE="$${CURRENT_DIR}$${SPRITE}" ;; \
		esac; \
		./utility.sh --check-sprite-metadata $${SPRITE} || exit 1; \
	done
endif

check-pngs:
ifneq ("$(SPRITE_FILES)","")
	@echo "Checking PNG consistency and palette...";
	@status=0; \
	for file in $(SPRITE_FILES); do \
		result=$$(pngcheck -c $$file); \
		echo "$$result" | grep -q "32-bit RGB"; \
		if [ $$? -eq 0 ]; then \
			echo "$$file is not an 8-bit indexed PNG."; \
			status=1; \
		fi; \
	done; \
	exit $$status
endif

check-maps: all
ifneq ("$(MAP_FOLDERS)","")
	@echo "Checking Resource Center...";
	@status=0; \
	for MAP in $(MAP_FOLDERS); do \
		if ! grep -q "Categories: Conquest" $${MAP}/map.yaml; then \
			continue; \
		fi; \
		HASH=$$(./utility.sh --map-hash ../$${MAP}); \
		HTTP_CODE=$$(curl -s -o /dev/null -I -w "%{http_code}" "https://resource.openra.net/map/$$HASH"); \
		if [ "$$HTTP_CODE" = "404" ]; then \
			echo "$$MAP is missing!"; \
			status=1; \
			(cd "$${MAP}" && zip -rq "upload.oramap" .); \
		fi; \
	done; \
	exit $$status
endif

check-ogg:
ifneq ("$(OGG_FILES)","")
	@echo "Checking Sounds...";
	@VALIDATION_FAILED=0; \
	for OGG in $(OGG_FILES); do \
		oggz validate "$${OGG}" || VALIDATION_FAILED=1; \
	done; \
	if [ $$VALIDATION_FAILED -eq 1 ]; then \
		exit 1; \
	fi
endif
