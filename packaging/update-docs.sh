#!/bin/bash

echo "Updating openhv.readthedocs.io"

rm -rf "${HOME}/openhv-docs"
git clone "https://${GITHUB_TOKEN_DOCS}@github.com/OpenHV/docs.git" "${HOME}/openhv-docs"

../utility.sh --docs "$1" > "${HOME}/openhv-docs/api/release/traits.md"
sed "s/of OpenRA/of OpenHV/g" "${HOME}/openhv-docs/api/release/traits.md"
../utility.sh --weapon-docs "$1" > "${HOME}/openhv-docs/api/release/weapons.md"
sed "s/of OpenRA/of OpenHV/g" "${HOME}/openhv-docs/api/release/weapons.md"
../utility.sh --lua-docs "$1" > "${HOME}/openhv-docs/api/release/lua.md"
sed "s/of OpenRA/of OpenHV/g" | sed sed "s/OpenRA allows/OpenHV allows/g" "${HOME}/openhv-docs/api/release/weapons.md"

pushd "${HOME}/openhv-docs/api/release" || exit 1
git add "traits.md"
git add "weapons.md"
git add "lua.md"
git commit -m "Update documentation" &&
git push origin master
popd || exit
