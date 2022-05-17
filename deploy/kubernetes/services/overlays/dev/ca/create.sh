#!/bin/bash

DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)

if [[ ! -d "${DIR}/private" ]]
then
  mkdir certs db private
  chmod 700 private
  touch db/index
fi

openssl rand -hex 16 > "$DIR/db/serial"

openssl req -new \
  -config "$DIR/root-ca.conf" \
  -out "$DIR/root-ca.csr" \
  -keyout "$DIR/private/tls.key" \
  -noenc

openssl ca -selfsign \
  -config "$DIR/root-ca.conf" \
  -in "$DIR/root-ca.csr" \
  -out "$DIR/tls.crt" \
  -extensions ca_ext \
  -batch

openssl pkcs12 -export \
  -in "$DIR/tls.crt" \
  -inkey "$DIR/private/tls.key" \
  -name "Image data hiding development certificate" \
  -out "$DIR/tls.p12" \
  -passout pass:
