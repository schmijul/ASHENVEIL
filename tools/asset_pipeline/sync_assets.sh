#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
MANIFEST_PATH="${1:-$ROOT_DIR/tools/asset_pipeline/asset_manifest.json}"

if ! command -v jq >/dev/null 2>&1; then
  echo "jq is required"
  exit 1
fi

if ! command -v curl >/dev/null 2>&1; then
  echo "curl is required"
  exit 1
fi

if [[ ! -f "$MANIFEST_PATH" ]]; then
  echo "Manifest not found: $MANIFEST_PATH"
  exit 1
fi

echo "Syncing assets from manifest: $MANIFEST_PATH"

COUNT="$(jq -r '[.sources[].assets[]] | length' "$MANIFEST_PATH")"
if [[ "$COUNT" == "0" ]]; then
  echo "No assets defined."
  exit 0
fi

jq -c '.sources[] as $src | $src.assets[] | {source: $src.name, license: $src.license, license_url: $src.license_url, id: .id, url: .url, target: .target}' "$MANIFEST_PATH" | \
while IFS= read -r row; do
  ID="$(jq -r '.id' <<<"$row")"
  URL="$(jq -r '.url' <<<"$row")"
  TARGET_REL="$(jq -r '.target' <<<"$row")"
  TARGET="$ROOT_DIR/$TARGET_REL"
  mkdir -p "$(dirname "$TARGET")"
  if [[ -f "$TARGET" && -s "$TARGET" ]]; then
    echo "skip  $ID -> $TARGET_REL"
    continue
  fi
  echo "fetch $ID -> $TARGET_REL"
  curl -L --fail --retry 2 --retry-delay 1 "$URL" -o "$TARGET"
done

LICENSE_OUT="$ROOT_DIR/godot/assets/THIRD_PARTY_ASSETS.md"
{
  echo "# Third-Party Assets"
  echo
  echo "Generated from \`tools/asset_pipeline/asset_manifest.json\`."
  echo
  jq -r '.sources[] | "## " + .name + "\n- License: " + .license + "\n- License URL: " + .license_url + "\n"' "$MANIFEST_PATH"
  echo "## Asset List"
  jq -r '.sources[] as $src | $src.assets[] | "- " + .id + " -> `" + .target + "` (" + .url + ")"' "$MANIFEST_PATH"
} > "$LICENSE_OUT"

echo "Wrote $LICENSE_OUT"
