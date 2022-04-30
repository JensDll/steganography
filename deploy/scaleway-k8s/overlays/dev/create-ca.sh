#!/bin/bash

DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)

openssl req -x509 -nodes -days 365 -newkey rsa:4096 \
  -sha256 -extensions v3_ca \
  -out "$DIR/cert.pem" -keyout "$DIR/key.pem"  \
  -subj "/O=Dev CA"
