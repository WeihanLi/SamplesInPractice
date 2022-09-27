$serviceName = "IpMonitor"
Write-Output "serviceName: $serviceName"

$destDir = Resolve-Path ".\out"
$ipMonitorPath = "$destDir\IpMonitor.exe"
Remove-Item $destDir -Recurse
dotnet publish -c Release -o out

Write-Output "Installing service... $ipMonitorPath $destDir"
New-Service $serviceName -BinaryPathName $ipMonitorPath
Start-Service $serviceName
Write-Output "Service $serviceName started"
