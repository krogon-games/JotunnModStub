#!/bin/bash
# Cloud sessions: install the .NET 8 SDK and a stand-in Valheim folder so the three
# plugins build (see docs/ARCHITECTURE.md -> Build and test). This is the setup script
# of the "Valheim Mod" cloud environment; keep the two in step.
# The stand-in game files go to /opt/valheim-standin, OUTSIDE the repository, and are
# never committed. A failure only prints a warning, so sessions still start.

apt-get update -qq || echo "cloud-setup: apt-get update failed"
DEBIAN_FRONTEND=noninteractive apt-get install -y -qq dotnet-sdk-8.0 unzip \
  || echo "cloud-setup: dotnet-sdk-8.0 install failed (build check will be NOT SET UP)"

V=/opt/valheim-standin
if [ ! -f "$V/valheim_Data/Managed/publicized_assemblies/assembly_valheim_publicized.dll" ]; then
  T=$(mktemp -d)
  N=https://api.nuget.org/v3-flatcontainer
  if curl -sSfL -o "$T/a.nupkg" "$N/pfhoenix.valheim.modprojectreferences/1.0.26/pfhoenix.valheim.modprojectreferences.1.0.26.nupkg" \
    && curl -sSfL -o "$T/b.nupkg" "$N/digitalroot.valheim.common.references/1.0.21/digitalroot.valheim.common.references.1.0.21.nupkg" \
    && unzip -q "$T/a.nupkg" -d "$T/a" && unzip -q "$T/b.nupkg" -d "$T/b"; then
    mkdir -p "$V/valheim_Data/Managed/publicized_assemblies" "$V/BepInEx/core"
    cp "$T"/a/lib/net46/*.dll "$V/valheim_Data/Managed/"
    for f in "$T"/b/ref/net48/*-publicized.dll; do
      cp "$f" "$V/valheim_Data/Managed/publicized_assemblies/$(basename "$f" -publicized.dll)_publicized.dll"
    done
    cp "$V/valheim_Data/Managed/0Harmony.dll" "$V/BepInEx/core/"
    cp "$V/valheim_Data/Managed/bepinex.dll" "$V/BepInEx/core/bepinex.dll"
    cp "$V/valheim_Data/Managed/bepinex.dll" "$V/BepInEx/core/BepInEx.dll"
  else
    echo "cloud-setup: stand-in Valheim download failed (build check will be NOT SET UP)"
  fi
  rm -rf "$T"
fi
exit 0
