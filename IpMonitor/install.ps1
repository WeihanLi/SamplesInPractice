$serviceName = "IpMonitor"
Write-Output "serviceName: $serviceName"

dotnet publish -c Release -o out
$destDir = Resolve-Path ".\out"
$ipMonitorPath = "$destDir\IpMonitor.exe"
Remove-Item $destDir -Recurse

Write-Output "Installing service... $ipMonitorPath $destDir"
New-Service $serviceName -BinaryPathName $ipMonitorPath
Start-Service $serviceName
Write-Output "Service $serviceName started"
