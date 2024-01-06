# dotnet-update-all-tools

## Intro

[![dotnet-update-all-tools](https://img.shields.io/nuget/v/dotnet-update-all-tools)](https://www.nuget.org/packages/dotnet-update-all-tools/)

A dotnet tool for upgrading all dotnet tools, update all dotnet-tool in a command

Install:

``` sh
dotnet tool install --global dotnet-update-all-tools
```

Usage:

``` sh
dotnet-update-all-tools
```

Build && Publish:

``` pwsh
# pack
dotnet pack

# push
dotnet nuget push ./artifacts/package/release/dotnet-update-all-tools.1.2.0.nupkg -k $env:Nuget__ApiKey -s https://api.nuget.org/v3/index.json
```

- 中文介绍： <https://mp.weixin.qq.com/s?__biz=MzAxMjE2NTMxMw==&mid=2456608884&idx=1&sn=4f95911acd20861d6bcb04392009cc2d&chksm=8c2e49dabb59c0cc7e2bdb677daece3b8cba20cc30b7082c520bb72dc03ae1248322028acc4e&token=1973448370&lang=zh_CN#rd>
