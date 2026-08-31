#!/bin/sh
set -e

# 以 root 启动时（默认），确保挂载卷与上传目录属主正确后再降权为 app 用户运行
if [ "$(id -u)" = "0" ]; then
    mkdir -p /app/App_Data /app/wwwroot/uploads
    chown -R app:app /app/App_Data /app/wwwroot/uploads
    exec setpriv --reuid=1654 --regid=1654 --init-groups dotnet TreasureChamber.WebApp.dll
fi

exec dotnet TreasureChamber.WebApp.dll
