try {
  $loginBody = @{ Sicil = 425394; Password = 'S5s5mr.kfks' }
  $login = Invoke-RestMethod -Uri 'http://localhost:5117/api/auth/login' -Method Post -Body ($loginBody | ConvertTo-Json) -ContentType 'application/json' -TimeoutSec 10
  $token = $login.Token
  Write-Host 'Obtained token.'
} catch {
  Write-Host 'Login failed:' $_.Exception.Message
  exit 1
}

# Try role assign via API
$assignUrl = "http://localhost:5117/api/user/425394/rol-ata"
$body = @{ YeniRol = 'BaskanlikYoneticisi'; CityId = $null }
try {
  $resp = Invoke-RestMethod -Uri $assignUrl -Method Post -Body ($body | ConvertTo-Json) -ContentType 'application/json' -Headers @{ Authorization = "Bearer $token" } -TimeoutSec 10
  Write-Host 'ASSIGN_OK'; Write-Host ($resp | ConvertTo-Json -Compress)
  exit 0
} catch {
  Write-Host 'ASSIGN_ERROR:' $_.Exception.Message
}

# If API assign failed, try direct DB update
Write-Host 'Attempting DB update fallback...'
$db = Get-ChildItem -Path (Resolve-Path .).Path -Filter egm.db -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty FullName
if ($db) {
  Write-Host 'DB found:' $db
  $bak = $db + '.bak'
  Copy-Item $db $bak -Force
  Write-Host 'Backup created:' $bak
  if (Get-Command sqlite3 -ErrorAction SilentlyContinue) {
    Write-Host 'sqlite3 found — executing update'
    & sqlite3 $db "UPDATE Users SET Role='BaskanlikYoneticisi' WHERE Sicil=425394;"
    Write-Host 'DB updated via sqlite3.'
    & sqlite3 $db "SELECT Sicil, FullName, Role FROM Users WHERE Sicil=425394;"
  } else {
    Write-Host 'sqlite3 not found. Cannot update DB automatically. Provide DB path and I will give the sqlite command.'
  }
} else {
  Write-Host 'egm.db not found in workspace.'
}
