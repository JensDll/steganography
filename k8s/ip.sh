#!/bin/bash

declare -r region="$SCW_DEFAULT_REGION"
declare -r zone="$region-1"
declare -r project_id="30d46788-5868-4e3e-a1cd-f82176804cb8"

get_ips() 
{
  curl -X GET "https://api.scaleway.com/lb/v1/regions/$region/ips" \
    -H "X-Auth-Token: $SCW_SECRET_KEY"
}

create_ip()
{
  curl -X POST "https://api.scaleway.com/lb/v1/regions/$region/ips" \
    -H "X-Auth-Token: $SCW_SECRET_KEY" \
    -H "Content-Type: application/json" \
    -d "{\"project_id\":\"$project_id\"}" | jq .
}

case $1 in
  get)
    get_ips
    ;;
  create)
    create_ip
    ;;
esac