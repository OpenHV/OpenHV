#!/bin/bash

echo "Updating openhv.readthedocs.io"

rm -rf "${HOME}/openhv-docs"
git clone "https://${GITHUB_TOKEN_DOCS}@github.com/OpenHV/docs.git" "${HOME}/openhv-docs"
git config --local user.email "deploy@travis-ci.org"
git config --local user.name "Travis CI"

../utility.sh --docs "$1" > "${HOME}/openhv-docs/api/release/traits.md.in"
sed "s/of OpenRA/of OpenHV/g" "${HOME}/openhv-docs/api/release/traits.md.in" > "${HOME}/openhv-docs/api/release/traits.md"
rm "${HOME}/openhv-docs/api/release/traits.md.in"
../utility.sh --weapon-docs "$1" > "${HOME}/openhv-docs/api/release/weapons.md.in"
sed "s/of OpenRA/of OpenHV/g" "${HOME}/openhv-docs/api/release/weapons.md.in" > "${HOME}/openhv-docs/api/release/weapons.md"
rm "${HOME}/openhv-docs/api/release/weapons.md.in"
../utility.sh --lua-docs "$1" > "${HOME}/openhv-docs/api/release/lua.md"
sed "s/of OpenRA/of OpenHV/g" "${HOME}/openhv-docs/api/release/lua.md.in" | sed "s/OpenRA allows/OpenHV allows/g" > "${HOME}/openhv-docs/api/release/lua.md"
rm "${HOME}/openhv-docs/api/release/lua.md.in"

pushd "${HOME}/openhv-docs/api/release" || exit 1
git add "traits.md"
git add "weapons.md"
git add "lua.md"
git commit -m "Update documentation" &&
git push origin master
popd || exit
