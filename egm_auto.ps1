Write-Host '--- START: register/login/db update ---'
$regBody = @{ Sicil=425394; Password='S5s5mr.kfks'; FullName='Omer KAFKAS'; Email='mr.kfks@outlook.com'; GSM='+905300803955'; CityId=0 }
try {
  $reg = Invoke-RestMethod -Uri 'http://localhost:5117/api/auth/register' -Method Post -Body ($regBody | ConvertTo-Json -Depth 10) -ContentType 'application/json' -TimeoutSec 10
  Write-Host 'REGISTER_OK'
} catch {
  Write-Host 'REGISTER_ERROR:' $_.Exception.Message
}
$login = $null
try {
  $loginBody = @{ Sicil=425394; Password='S5s5mr.kfks' }
  $login = Invoke-RestMethod -Uri 'http://localhost:5117/api/auth/login' -Method Post -Body ($loginBody | ConvertTo-Json) -ContentType 'application/json' -TimeoutSec 10
  Write-Host 'LOGIN_OK'
  Write-Host 'TOKEN:' $login.Token
} catch {
  Write-Host 'LOGIN_ERROR:' $_.Exception.Message
}
if (-not $login) {
  Write-Host 'Login failed — searching for egm.db...'
  $db = Get-ChildItem -Path (Resolve-Path .).Path -Filter egm.db -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty FullName
  if ($db) {
    Write-Host 'DB_FOUND:' $db
    $bak = $db + '.bak'
    Copy-Item $db $bak -Force
    Write-Host 'DB backup created:' $bak
    if (Get-Command sqlite3 -ErrorAction SilentlyContinue) {
      Write-Host 'sqlite3 found — updating Role for Sicil 425394'
      & sqlite3 $db "UPDATE Users SET Role='BaskanlikYoneticisi' WHERE Sicil=425394;"
      Write-Host 'UPDATE executed.'
      & sqlite3 $db "SELECT Sicil, FullName, Role FROM Users WHERE Sicil=425394;"
    } else {
      Write-Host 'sqlite3 not found on PATH. Cannot update DB automatically.'
    }
  } else {
    Write-Host 'DB_NOT_FOUND in workspace.'
  }
}
Write-Host '--- END ---'
