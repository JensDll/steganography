#!/bin/bash

hostsFile="/etc/resolv.conf"

add() 
{
  echo "Writing dev host entries to location: $hostsFile"

  cat <<HERE >> $hostsFile
192.46.238.243 dev.imagedatahiding.com
192.46.238.243 www.dev.imagedatahiding.com
192.46.238.243 api.dev.imagedatahiding.com
HERE
}

remove()
{
  sed -i '/dev.imagedatahiding.com/d' $hostsFile
}

case $1 in
add)
  add
  ;;
remove)
  remove
  ;;
*)
  echo "Usage: $0 (add|remove)"
  ;;
esac