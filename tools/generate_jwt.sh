#!/usr/bin/env bash
# generate_jwt.sh - produce an unsigned JWT (header.payload.) for use with mock JWT handler
# Usage: ./generate_jwt.sh '{"name":"user1","sub":"user1","role":"Inputter"}'

if [ -z "$1" ]; then
  echo "Usage: $0 '{\"name\":\"user1\",\"sub\":\"user1\",\"role\":\"Inputter\"}'"
  exit 1
fi

payload="$1"
header='{"alg":"none","typ":"JWT"}'

base64url() {
  openssl base64 -e | tr -d '=' | tr '/+' '_-' | tr -d '\n'
}

h=$(printf '%s' "$header" | base64url)
p=$(printf '%s' "$payload" | base64url)
# trailing dot indicates no signature
printf "%s.%s." "$h" "$p"
