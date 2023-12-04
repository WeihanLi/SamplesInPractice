# https://github.com/dotnet/roslyn/issues/70771#issuecomment-1806956462
# $env:DOTNET_CLI_USE_MSBUILD_SERVER = 0
dotnet build-server shutdown
dotnet build ./CSharp12Sample.csproj
