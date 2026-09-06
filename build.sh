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
# URL-Normalisierung würde "/index.html" sofort wieder auf "/" zurückführen. Deshalb wird
# nur die ausgelieferte Kopie umbenannt (lokales `dotnet run` bleibt unberührt);
# _redirects zeigt entsprechend auf den neuen Namen.
mv output/index.html output/_index.html
