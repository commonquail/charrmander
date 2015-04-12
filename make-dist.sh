#!/bin/bash
ms_build="$WINDIR/Microsoft.NET/Framework/v4.0.30319/msbuild.exe"

main()
{
    validate_deps && build && archive
}

fatal()
{
    echo "fatal: $1"
    exit 1;
}

validate_deps()
{
    [[ -f "$ms_build" ]] || fatal "$ms_build does not exist"

    type 7z > /dev/null 2>&1 || fatal "7z (7-Zip) not found"
}

build()
{
    "$ms_build" \
        -nologo \
        Charrmander.sln \
        -target:Build \
        -property:Configuration=Release \
        -maxcpucount \
        -logger:FileLogger,Microsoft.Build.Engine
}

archive()
{
    7z a \
        "charrmander-$(git describe --abbrev=0).zip" \
        "./bin/Release/Charrmander.exe"
}

main
