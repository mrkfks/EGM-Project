try {
    $body = @{ sicil = 425394; password = 'S5s5mr.kfks' } | ConvertTo-Json
    $r = Invoke-RestMethod -Uri 'http://localhost:5117/api/auth/login' -Method Post -Headers @{ Origin='http://10.0.48.214:4200' } -ContentType 'application/json' -Body $body -TimeoutSec 15
    Write-Output 'Response:'
    $r | ConvertTo-Json -Depth 4
} catch {
    Write-Output 'ERROR:'
    Write-Output $_.Exception.Message
    if ($_.Exception.Response) {
        try { $s = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream()); Write-Output $s.ReadToEnd() } catch {}
    }
}
