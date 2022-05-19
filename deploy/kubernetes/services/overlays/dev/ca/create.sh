#!/bin/bash

DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)

if [[ ! -d "${DIR}/private" ]]
then
  mkdir certs db private
  chmod 700 private
  touch db/index
fi

openssl req -new \
  -config "$DIR/root-ca.conf" \
  -out "$DIR/certs/root-ca.csr" \
  -keyout "$DIR/private/tls.key" \
  -noenc

openssl ca -selfsign \
  -create_serial \
  -config "$DIR/root-ca.conf" \
  -in "$DIR/certs/root-ca.csr" \
  -out "$DIR/certs/tls.crt" \
  -extensions ca_ext \
  -batch

openssl pkcs12 -export \
  -in "$DIR/certs/tls.crt" \
  -inkey "$DIR/private/tls.key" \
  -name "Image data hiding development certificate" \
  -out "$DIR/certs/tls.p12" \
  -password "pass:"
