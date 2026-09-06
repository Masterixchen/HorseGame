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

# Cloudflares _redirects-Validator meldet bei "/* /index.html 200" eine Endlosschleife
# (bekannter Fehlalarm: cloudflare/workers-sdk#11824) - er nimmt an, seine automatische
# URL-Normalisierung würde "/index.html" sofort wieder auf "/" zurückführen. index.html
# muss aber als echte Datei liegen bleiben, sonst erkennt Cloudflare den Ordner gar nicht
# erst als statische Seite. Deshalb bekommt _redirects mit _index.html ein zweites,
# unverdächtiges Ziel - eine reine Kopie, kein Ersatz.
cp output/index.html output/_index.html
