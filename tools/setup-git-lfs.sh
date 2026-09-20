#!/usr/bin/env bash
set -euo pipefail

tools_dir="$(cd "$(dirname "$0")" && pwd)"
root="$(cd "$tools_dir/.." && pwd)"
example="$tools_dir/.gitattributes.example"
dest="$root/.gitattributes"

if [[ ! -f "$example" ]]; then
  echo "Missing $example" >&2
  exit 1
fi

if ! git lfs version >/dev/null 2>&1; then
  echo "Git LFS is not installed. Install it from https://git-lfs.com then re-run this script." >&2
  exit 1
fi

cd "$root"
git lfs install
cp "$example" "$dest"

echo "Git LFS is installed for this repo."
echo "Copied tools/.gitattributes.example to .gitattributes"
echo "Commit .gitattributes. New matching files (fbx, png, wav, …) go to LFS."
echo "Files already in Git stay as normal objects unless you run git lfs migrate."
