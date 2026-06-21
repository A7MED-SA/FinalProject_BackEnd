#!/usr/bin/env bash
set -e
fuser -k 5000/tcp 2>/dev/null || true
dotnet run --project src/Athary.API