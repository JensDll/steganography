#!/bin/bash

DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)

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
  -extensions ca_ext
