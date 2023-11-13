New-RootCA
New-SubordinateCA -Name steganography -PermittedDNS localhost

foreach ($service in 'spa', 'api') {
  $in = Join-Path $PSScriptRoot .. src $service deploy cert.conf
  $out = Join-Path $PSScriptRoot .. certs $service
  New-Certificate -Issuer steganography -Name tls -Request $in -Destination $out
}
