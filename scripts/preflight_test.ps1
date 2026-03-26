try {
    $r = Invoke-WebRequest -Uri 'http://localhost:5117/api/auth/login' -Method OPTIONS -Headers @{ Origin='http://10.0.48.214:4200'; 'Access-Control-Request-Method'='POST' } -UseBasicParsing -TimeoutSec 10
    Write-Output 'StatusCode: ' + $r.StatusCode
    Write-Output 'Headers:'
    $r.Headers.GetEnumerator() | ForEach-Object { Write-Output ("$($_.Name): $($_.Value)") }
} catch {
    Write-Output 'Exception: '
    Write-Output $_.Exception.Message
    if ($_.Exception.Response) {
        try { $h = $_.Exception.Response.Headers; $h.GetEnumerator() | ForEach-Object { Write-Output ("$($_.Name): $($_.Value)") } } catch {}
    }
}
