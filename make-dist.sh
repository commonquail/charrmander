#!/bin/bash

main()
{
    validate_deps && clean && build && archive
}

fatal()
{
    echo "fatal: $1"
    exit 1;
}

validate_deps()
{
    type dotnet.exe > /dev/null 2>&1 || fatal "dotnet.exe not found"
    type 7z > /dev/null 2>&1 || fatal "7z (7-Zip) not found"
}

clean()
{
    dotnet.exe clean Charrmander.sln
}

build()
{
    dotnet.exe test Charrmander.sln
    dotnet.exe publish -p:PublishProfile=Properties/PublishProfiles/Release.pubxml Charrmander.sln
}

archive()
{
    7z a \
        "charrmander-$(git describe --abbrev=0).zip" \
        "./dist/Charrmander.exe"
}

main
