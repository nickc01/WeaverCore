#!/usr/bin/env bash
set -euo pipefail

# Simple helper to build WeaverCore.Game using Unity's bundled MSBuild

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
SETTINGS_FILE="$SCRIPT_DIR/../Settings.txt"
CS_PROJ="$SCRIPT_DIR/WeaverCore.Game.csproj"

if [[ ! -f "$SETTINGS_FILE" ]]; then
  echo "Settings file not found: $SETTINGS_FILE" >&2
  exit 1
fi

# Grab the first non-empty line after the [Unity Editor Location] header
UNITY_EDITOR_PATH="$(
  awk '
    /^\[Unity Editor Location\]/ {getline; while($0 ~ /^[[:space:]]*$/){getline}; gsub(/^[[:space:]]+|[[:space:]]+$/, "", $0); print; exit}
  ' "$SETTINGS_FILE"
)"

if [[ -z "$UNITY_EDITOR_PATH" ]]; then
  echo "Could not read Unity Editor Location from $SETTINGS_FILE" >&2
  exit 1
fi

# Normalize the path: Settings.txt usually points to .../Editor/Data/Managed/UnityEngine
if [[ -f "$UNITY_EDITOR_PATH" ]]; then
  UNITY_EDITOR_PATH="$(dirname "$UNITY_EDITOR_PATH")"
fi

if [[ "$UNITY_EDITOR_PATH" == *"/Managed"* ]]; then
  DATA_DIR="${UNITY_EDITOR_PATH%%/Managed*}"
else
  DATA_DIR="$(cd -- "$UNITY_EDITOR_PATH/.." && pwd)"
fi

MSBUILD_BIN="$DATA_DIR/MonoBleedingEdge/bin/msbuild"
USE_DOTNET=false
if [[ ! -x "$MSBUILD_BIN" ]]; then
  USE_DOTNET=true
  MSBUILD_BIN="dotnet msbuild"
fi
FRAMEWORK_OVERRIDE="$DATA_DIR/MonoBleedingEdge/lib/mono/4.7.2-api"

if [[ ! -d "$FRAMEWORK_OVERRIDE" ]]; then
  echo "Framework override path missing: $FRAMEWORK_OVERRIDE" >&2
  exit 1
fi

echo "Using msbuild: $MSBUILD_BIN"
echo "Framework override: $FRAMEWORK_OVERRIDE"
echo "Project: $CS_PROJ"
echo

if [[ "$USE_DOTNET" == true ]]; then
  exec dotnet msbuild "$CS_PROJ" \
    /p:FrameworkPathOverride="$FRAMEWORK_OVERRIDE" \
    /p:Configuration=Release "$@"
else
  exec "$MSBUILD_BIN" "$CS_PROJ" \
    /p:FrameworkPathOverride="$FRAMEWORK_OVERRIDE" \
    /p:Configuration=Release "$@"
fi
