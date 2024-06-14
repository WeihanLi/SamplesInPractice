$serviceName = "IpMonitor"
Write-Output "serviceName: $serviceName"

# Remove-Item $destDir -Recurse
dotnet publish IpMonitor.csproj -c Release -o out
$destDir = Resolve-Path ".\out"
$ipMonitorPath = "$destDir\IpMonitor.exe"

Write-Output "Installing service... $ipMonitorPath $destDir"
New-Service $serviceName -BinaryPathName $ipMonitorPath
Start-Service $serviceName
Write-Output "Service $serviceName started"
Get-Service $serviceName
