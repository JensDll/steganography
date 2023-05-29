New-RootCA
New-SubordinateCA -Name image_data_hiding -PermittedDNS localhost

foreach ($service in 'steganography.web', 'steganography.api') {
  $in = Join-Path $PSScriptRoot .. services $service deploy cert.conf
  $out = Join-Path $PSScriptRoot .. certs $service

  New-Certificate -Issuer image_data_hiding -Name tls -Request $in -Destination $out

  wsl --exec chmod 444 $(ConvertTo-WSLPath $out/tls.key)
}
