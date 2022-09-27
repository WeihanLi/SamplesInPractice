$serviceName = "IpMonitor"
Stop-Service $serviceName
Write-Output "Service $serviceName stopped"
Remove-Service $serviceName
Write-Output "Service $serviceName removed"
