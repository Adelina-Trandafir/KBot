#!/bin/bash
# Slice 0110-04: builds /etc/nginx/ca/calificate.pem (the roots + qualified intermediates nginx
# trusts for client certificates) from the .crt files downloaded into /root/ca.
# Run on the VPS as root:  bash /root/build-calificate.sh
set -u

SRC=/root/ca
OUT=/etc/nginx/ca/calificate.pem
FILES="certsign-rootg2 certsign-qualifiedca certsign-rootcasign2023rsa certsign-qualifiedca2023rsa DigiSignQualifiedRootCAv3 DigiSignQualifiedCAClass32017 ts_root_g2 ts_qca_g2 ts_root_g3 ts_qca_g3"

mkdir -p /etc/nginx/ca
TMP=$(mktemp)
for f in $FILES; do
  path="$SRC/$f.crt"
  if [ ! -f "$path" ]; then echo "LIPSESTE $path"; continue; fi
  # PEM first, DER as the fallback; one PEM block per certificate
  if openssl x509 -in "$path" 2>/dev/null >> "$TMP"; then
    echo "ok (PEM) $f"
  elif openssl x509 -inform DER -in "$path" >> "$TMP" 2>/dev/null; then
    echo "ok (DER) $f"
  else
    echo "ESEC $f (nu se citeste ca certificat)"
  fi
done

COUNT=$(grep -c "BEGIN CERTIFICATE" "$TMP")
if [ "$COUNT" -eq 0 ]; then
  echo "Niciun certificat citit, fisierul NU a fost scris."
  rm -f "$TMP"
  exit 1
fi
mv "$TMP" "$OUT"
chmod 644 "$OUT"
echo "Gata: $COUNT certificate in $OUT (asteptat: 8 sau 10 cu cele Trans Sped G3)"
