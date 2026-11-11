#!/usr/bin/env bash
#
# Build a self-contained Linux package for the Avalonia UI and the CLI.
#
# Output (under dist/):
#   SoMRandomizer-<version>-<rid>/app/SoMRandomizer      Avalonia GUI
#   SoMRandomizer-<version>-<rid>/cli/SoMRandomizer.Cli  command-line generator
#   SoMRandomizer-<version>-<rid>.tar.gz                 the same tree, archived
#
# Environment variables:
#   PACKAGE_VERSION  package version label (default: 0.0.0)
#   RID            .NET runtime identifier (default: linux-x64)
#   CONFIGURATION  build configuration (default: Release)
#   PACKAGE_NAME   override the package/directory name
#   DOTNET         dotnet executable (default: dotnet from PATH)
#
# Requires the .NET 8 SDK. The result does not require a .NET runtime on the
# target machine (self-contained publish).
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DOTNET="${DOTNET:-dotnet}"
CONFIGURATION="${CONFIGURATION:-Release}"
RID="${RID:-linux-x64}"
PACKAGE_VERSION="${PACKAGE_VERSION:-0.0.0}"
PACKAGE_NAME="${PACKAGE_NAME:-SoMRandomizer-$PACKAGE_VERSION-$RID}"

DIST="$ROOT/dist"
STAGE="$DIST/$PACKAGE_NAME"
ARCHIVE="$DIST/$PACKAGE_NAME.tar.gz"

if ! command -v "$DOTNET" >/dev/null 2>&1; then
    echo "error: '$DOTNET' not found. Install the .NET 8 SDK or set DOTNET=/path/to/dotnet." >&2
    exit 1
fi

echo "== Publishing SoMRandomizer-$PACKAGE_VERSION for $RID ($CONFIGURATION) =="
rm -rf "$STAGE" "$ARCHIVE"
mkdir -p "$STAGE/app" "$STAGE/cli" "$STAGE/share/doc"

publish_args=(
    -c "$CONFIGURATION"
    -r "$RID"
    --self-contained true
    -p:DebugType=None
    -p:DebugSymbols=false
)

"$DOTNET" publish "$ROOT/src/SoMRandomizer.App/SoMRandomizer.App.csproj" \
    "${publish_args[@]}" -o "$STAGE/app"

"$DOTNET" publish "$ROOT/src/SoMRandomizer.Cli/SoMRandomizer.Cli.csproj" \
    "${publish_args[@]}" -o "$STAGE/cli"

cp "$ROOT/LICENSE" "$STAGE/share/doc/LICENSE"
cp "$ROOT/README.md" "$STAGE/share/doc/README.md"

cat > "$STAGE/README.txt" <<'EOF'
Secret of Mana Randomizer - Linux build
=======================================

Layout:
  app/SoMRandomizer       Avalonia desktop randomizer
  cli/SoMRandomizer.Cli   command-line ROM generator
  share/doc/              license and readme

The binaries are self-contained: no .NET runtime is required.

GUI:
  ./app/SoMRandomizer

CLI:
  ./cli/SoMRandomizer.Cli srcRom=/path/to/SoM.smc \
      dstRom=/path/to/out.smc seed=0123456789ABCDEF \
      "options=version=1.49 mode=open"
EOF

echo "== Creating $ARCHIVE =="
tar -C "$DIST" -czf "$ARCHIVE" "$PACKAGE_NAME"

if command -v sha256sum >/dev/null 2>&1; then
    (cd "$DIST" && sha256sum "$PACKAGE_NAME.tar.gz" > "$PACKAGE_NAME.tar.gz.sha256")
fi

echo "== Done =="
echo "  package: $STAGE"
echo "  archive: $ARCHIVE"
