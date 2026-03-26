$conn = Get-NetTCPConnection -LocalPort 5117 -ErrorAction SilentlyContinue
if ($null -ne $conn) {
    Stop-Process -Id $conn.OwningProcess -Force
    Write-Output "killed $($conn.OwningProcess)"
} else {
    Write-Output "no pid"
}
