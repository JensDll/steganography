[CmdletBinding()]
param()

try {
  Push-Location $PSScriptRoot

  wsl --exec "cd $PSScriptRoot"

  if (-not (Test-Path 'private')) {
    New-Item 'certs', 'db', 'private' -ItemType Directory 1> $null
    New-Item 'db/index' -ItemType File 1> $null
  }

  $domain = Get-Content 'root-ca.conf' | Select-String -Pattern '^domain' | Select-Object -ExpandProperty Line
  $domain = $domain.Split('=')[1].Trim()

  $cert = Get-ChildItem -Path Cert:\CurrentUser\Root -SSLServerAuthentication -DnsName $domain

  if ($cert) {
    Write-Verbose 'Using existing certificate'

    $password = New-Object System.Security.SecureString

    Export-PfxCertificate -Cert $cert -FilePath 'certs/tls.p12' -Password $password 1> $null

    wsl --exec openssl pkcs12 `
      -in 'certs/tls.p12' `
      -out 'certs/tls.crt' `
      -nokeys `
      -noenc `
      -password 'pass:'
    
    wsl --exec openssl pkcs12 `
      -in 'certs/tls.p12' `
      -out 'private/tls.key' `
      -nocerts `
      -noenc `
      -password 'pass:'
      
    return
  }

  wsl --exec './create.sh'

  Import-PfxCertificate -FilePath 'certs/tls.p12' -CertStoreLocation Cert:\CurrentUser\Root -Exportable 1> $null
} finally {
  Pop-Location
}
