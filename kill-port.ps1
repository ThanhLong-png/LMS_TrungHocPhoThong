# Script: kill-port.ps1
# Dung: .\kill-port.ps1          -> Kill port mac dinh 5016
# Dung: .\kill-port.ps1 -Port 7200 -> Kill port tuy chinh

param([int]$Port = 5016)

$connections = Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue
if ($connections) {
    foreach ($conn in $connections) {
        $proc = Get-Process -Id $conn.OwningProcess -ErrorAction SilentlyContinue
        Write-Host "Killing PID $($conn.OwningProcess) ($($proc.ProcessName)) on port $Port..." -ForegroundColor Yellow
        Stop-Process -Id $conn.OwningProcess -Force -ErrorAction SilentlyContinue
    }
    Write-Host "Done! Port $Port is now free." -ForegroundColor Green
} else {
    Write-Host "Port $Port is already free." -ForegroundColor Cyan
}
