foreach ($service in $('web', 'api')) {
  $in = Join-Path $PSScriptRoot .. services $service cert.conf
  $out = Join-Path $PSScriptRoot .. certs $service

  New-DevOpToolsCertificate -Name tls -RequestConfig $in -Destination $out
  wsl --exec chmod 444 $(ConvertTo-WSLPath $out/tls.key)
}
