New-RootCA
New-SubordinateCA -Name steganography -PermittedDNS localhost

foreach ($service in 'steganography.web', 'steganography.api') {
  $in = Join-Path $PSScriptRoot services $service deploy cert.conf
  $out = Join-Path $PSScriptRoot certs $service
  New-Certificate -Issuer steganography -Name tls -Request $in -Destination $out
}
