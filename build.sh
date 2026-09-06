#!/usr/bin/env bash
set -e
curl -sSL https://dot.net/v1/dotnet-install.sh > dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh -c 8.0 -InstallDir ./dotnet
./dotnet/dotnet publish Web/Web.csproj -c Release -o publish

# Cloudflare Pages erwartet index.html & Co. direkt im Output-Verzeichnis, aber
# `dotnet publish` legt die eigentlichen statischen Dateien in einen wwwroot-
# Unterordner - also die Ausgabe flach ziehen statt den Zwischenordner zu behalten.
rm -rf output
mv publish/wwwroot output
