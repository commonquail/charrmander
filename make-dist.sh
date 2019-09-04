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
    [[ -f "$MS_BUILD" ]] || fatal "$MS_BUILD does not exist"

    type 7z > /dev/null 2>&1 || fatal "7z (7-Zip) not found"
}

clean()
{
	"$MS_BUILD" \
		-nologo \
		Charrmander.sln \
		-target:Clean \
		-maxcpucount \
		-logger:FileLogger,Microsoft.Build.Engine
}

build()
{
    "$MS_BUILD" \
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
