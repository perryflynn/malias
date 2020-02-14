#!/bin/bash

VERSION=1.0.0
TARGETS=( win-x64 linux-x64 linux-arm osx-x64 )

cd "$(dirname "$0")"
rm -rf publish
mkdir publish
cd publish


# -> Build standard version

targetname="malias-${VERSION}"
mkdir -p "$targetname"

BuildVersion=$VERSION \
dotnet publish \
    -o $targetname \
    -c Release \
    ../maliasmgr.csproj

rm ${targetname}/malias
find "$targetname" -name *.pdb -exec rm -f {} \;
zip "${targetname}.zip" -r "${targetname}"


# -> Build Portable versions

for target in "${TARGETS[@]}"
do

    targetname="malias-${VERSION}-portable-${target}"
    mkdir -p "$targetname"

    BuildVersion=$VERSION \
    dotnet publish \
        --self-contained \
        -o $targetname \
        -r $target \
        -c Release \
        ../maliasmgr.csproj

    find "$targetname" -name *.pdb -exec rm -f {} \;
    zip "${targetname}.zip" -r "${targetname}"

done
