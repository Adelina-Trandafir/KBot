#!/usr/bin/env python3
"""
Dry run of the received-invoice sync (slice 00EF-17): shows what ANAF would give for a unit, WRITES NOTHING to any database.

    cd /root/AVACONT/PYTHON
    set -a; . /etc/avacont/efactura.env; set +a
    /root/AVACONT/.venv/bin/python3 ../tools/efactura/proba_primite.py <DC> [--zile 60] [--cui 7273547] [--descarca] [--salveaza DIR]

  <DC>        the unit database name (8 characters, as in the session)
  --zile      1..60 (default 60)
  --cui       only messages whose issuer CUI matches (RO, spaces and dots ignored)
  --descarca  also downloads each message and reads the invoice (number, date, supplier, VAT per rate, total, lines, files)
  --cif-unitate  the unit's own CUI. With the environment variable EF_TOKEN_PROBA (an access token) the script needs no database and
              no server configuration: it calls ANAF directly with that token, on any PC. The token is never printed or stored.
  --salveaza  with --descarca: writes the invoice XML of each message into DIR

Without --descarca it makes ONE call to ANAF (the list). With it, one more per message (ANAF limits downloads, keep the list short with --cui).
The token is renewed by the server code when needed (that is the only thing written, in AVACONT_COMUN.EF_Token). Output has invoice data:
do not paste it anywhere public.
"""
import argparse
import os
import sys

sys.path.insert(0, os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "PYTHON"))

from routes.efactura import anaf_api, primite_ubl, tokens, trimitere  # noqa: E402
from routes.efactura.tokens import EfEroare  # noqa: E402


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("dc")
    parser.add_argument("--zile", type=int, default=60)
    parser.add_argument("--cui")
    parser.add_argument("--descarca", action="store_true")
    parser.add_argument("--salveaza")
    parser.add_argument("--cif-unitate")
    args = parser.parse_args()

    pasted = os.environ.get("EF_TOKEN_PROBA", "").strip()
    try:
        if pasted:
            # trial on any PC: ANAF is called directly with the pasted token; no database, no server configuration
            if not args.cif_unitate or not args.zile or not 1 <= args.zile <= 60:
                print("Cu EF_TOKEN_PROBA trebuie --cif-unitate <CUI> si --zile 1..60.")
                return 2
            items_all = [m for m in anaf_api.lista_mesaje(pasted, args.cif_unitate, args.zile) if m.get("tip") == trimitere.RECEIVED]
            listed = {"cui": args.cif_unitate, "mesaje": items_all}
        else:
            listed = trimitere.mesaje(args.dc, args.zile, True)
    except EfEroare as err:
        print(f"EROARE ({err.reason}): {err}")
        return 2
    wanted = primite_ubl.normalize_cui(args.cui) if args.cui else None
    items = [m for m in listed["mesaje"]
             if not wanted or primite_ubl.normalize_cui(m.get("cif_emitent")) == wanted]
    print(f"Unitate CUI {listed['cui']}: {len(listed['mesaje'])} mesaje primite in {args.zile} zile, "
          f"{len(items)} dupa filtru.")
    if args.salveaza:
        os.makedirs(args.salveaza, exist_ok=True)
    token = (pasted or tokens.access_token(args.dc)[1]) if args.descarca else None

    for m in items:
        print(f"\n[{m.get('data_creare', '')}] id_solicitare={m.get('id_solicitare')} emitent={m.get("cif_emitent")} "
              f"deja_in_baza={m.get('deja_in_baza')}\n   {m.get('detalii', '')}")
        if not args.descarca or not anaf_api.is_id(m.get("id", "")):
            continue
        try:
            xml = anaf_api.factura_din_zip(anaf_api.descarca(token, m["id"]))
            p = primite_ubl.parse(xml)
        except (EfEroare, primite_ubl.XmlNeinteles) as err:
            print(f"   NU SE POATE CITI: {err}")
            continue
        print(f"   {p['Tip']} {p['NrFact']} din {p['DataFact']} scadenta {p['DataScad']} | {p['DenumireP']} ({p['CUI']})")
        print(f"   baza {p['Valoare']}  TVA {p['TVA']}  total {p['Total']}  semn {p['Semn']}  ref {p['Ref']}")
        for r in p["cote"]:
            print(f"     cota {r['CotaTVA']}% ({r['Categorie']}): baza {r['Baza']} TVA {r['TVA']}")
        print(f"   {len(p['linii'])} linii, {len(p['note'])} note, atasamente: {[a['nume'] for a in p['atasamente']] or '-'}")
        if args.salveaza:
            with open(os.path.join(args.salveaza, f"{m['id_solicitare']}.xml"), "wb") as handle:
                handle.write(xml)
    return 0


if __name__ == "__main__":
    sys.exit(main())
