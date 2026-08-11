#!/usr/bin/env bash

set -e
trap 'kill 0' EXIT

cd "$(dirname "$0")"

echo "==> Tests"
dotnet test backend

echo "==> API sur http://localhost:5066"
dotnet run --project backend/src/backend.Api &
sleep 5

echo "==> Frontend sur http://localhost:5173"
cd frontend
npm install
npm run dev