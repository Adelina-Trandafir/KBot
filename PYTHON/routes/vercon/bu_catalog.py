"""Destination accounts, supplier and nomenclature for a BU."""
from decimal import Decimal

from utils import vercon_suppliers as suppliers
from utils.clock import utc_now

from . import nomenclatoare, protocol, receipt_products, report_identity, supplier_identity
from .documents import _all, _one
from .nomenclatoare import NomError


def destinations(cursor, ctx):
    rows = _all(cursor, 'SELECT s.SubunitId AS subunit,s.Name AS subunit_name,g.GestiuneId AS gestiune,'
                'g.Name AS name,g.Zec AS zec,g.Scara AS scale FROM VC_Subunitati s JOIN VC_Gestiuni g '
                'ON g.SubunitId=s.SubunitId WHERE s.Active=1 AND g.Active=1 AND g.Type=%s AND s.SubunitId<>%s '
                'AND EXISTS (SELECT 1 FROM VC_Exercitii e WHERE e.SubunitId=g.SubunitId AND e.GestiuneId=g.GestiuneId)',
                (ctx['type'], ctx['subunit']))
    result = []
    for row in rows:
        scope = (row['subunit'], row['gestiune'])
        holders = _all(cursor, 'SELECT IDC AS id,Denumire AS label FROM VC_Clienti '
                       'WHERE SubunitId=%s AND GestiuneId=%s AND Activ=1', scope)
        accounts = _all(cursor, 'SELECT IdMasina AS id,Masina AS label,TIP FROM VC_Masini '
                        'WHERE SubunitId=%s AND GestiuneId=%s AND Activ=1', scope)
        warehouse = [account for account in accounts if receipt_products.warehouse(ctx['type'], account['TIP'])]
        use = [account for account in accounts if str(account['TIP'] or '').strip().upper() in (
            'MFF' if ctx['type'] == 'MF' else 'OIF', 'FOLOSINTA', 'FOLOSINȚĂ', '')]
        if warehouse:
            result.append({**row, 'holders': holders if use else [], 'warehouse_accounts': warehouse, 'use_accounts': use})
    return result


def supplier(cursor, ctx, request_id):
    supplier_identity.require(cursor)
    identity = report_identity.read(cursor, ctx['unit'], ctx)
    fiscal = suppliers.code(identity['fiscal_code'])
    if not fiscal or len(fiscal) > 50:
        raise NomError('BU_SUPPLIER', 'Completați codul fiscal al unității înainte de transfer.', 422)
    partner = suppliers.find(cursor, fiscal)
    if partner:
        return fiscal, partner['DenumirePartener'] or identity['name']
    # Parteneri requires a local Unitati row. Prefer the principal sector
    # and serialize automatic inserts of this fiscal identity.
    unit = _one(cursor, "SELECT IdUnitate FROM Unitati WHERE Ascuns=0 "
                "ORDER BY (SursaSector='01A') DESC,An DESC,IdUnitate LIMIT 1 FOR UPDATE", ())
    if not unit:
        raise NomError('BU_SUPPLIER', 'Nomenclatorul de furnizori nu are o unitate locală activă.', 409)
    partner = suppliers.find(cursor, fiscal)
    if partner:
        return fiscal, partner['DenumirePartener'] or identity['name']
    existing = _one(cursor, 'SELECT 1 FROM Parteneri WHERE IdUnitate=%s AND CodPartener=%s',
                    (unit['IdUnitate'], fiscal))
    if existing:
        raise NomError('BU_SUPPLIER', 'Codul de partener al unității este ocupat de alt furnizor.', 409)
    cursor.execute('INSERT INTO Parteneri (IdUnitate,CodPartener,DenumirePartener,CodFiscal,Adresa,Tip,Ascuns,Provenienta) '
                   "VALUES (%s,%s,%s,%s,%s,'1',0,'VERCON_BU')",
                   (unit['IdUnitate'], fiscal, identity['name'], fiscal, identity['address'] or None))
    protocol.audit(cursor, ctx, 'bu.supplier.create', 'Parteneri', cursor.lastrowid, None,
                   {'CodFiscal': fiscal, 'DenumirePartener': identity['name']}, None, request_id)
    return fiscal, identity['name']


def zero_vat(repo):
    rows = repo.select('VC_TVA', {'Activ': 1, 'TVA': Decimal(0)}, columns=('IdTVA',))
    if len(rows) != 1:
        raise NomError('BU_VAT', 'Configurați o singură cotă TVA activă de 0% în gestiunea de destinație.', 422)
    return rows[0]['IdTVA']


def source_product(cursor, ctx, variant):
    row = _one(cursor, 'SELECT v.COD,v.PUTVA,p.DenumireP,p.UM,p.CPV,p.CC,p.DNF,p.IdMasina '
               'FROM VC_ProduseP v JOIN VC_Produse p ON p.SubunitId=v.SubunitId AND p.GestiuneId=v.GestiuneId AND p.ID=v.ID '
               'WHERE v.SubunitId=%s AND v.GestiuneId=%s AND v.IdProdus=%s',
               (ctx['subunit'], ctx['gestiune'], variant))
    if not row or not (row['DenumireP'] or '').strip() or not (row['UM'] or '').strip():
        raise NomError('BU_PRODUCT', 'Completați denumirea și UM ale produsului din sursă.', 422)
    return row


def product(cursor, repo, ctx, source, account, request_id):
    fields = ('DenumireP', 'UM', 'CPV', 'CC', 'DNF')
    rows = _all(cursor, "SELECT ID,Activ FROM VC_Produse WHERE SubunitId=%s AND GestiuneId=%s AND IdMasina=%s "
                "AND TRIM(DenumireP)=TRIM(%s) AND TRIM(UM)=TRIM(%s) "
                "AND NULLIF(TRIM(CPV),'') <=> NULLIF(TRIM(%s),'') AND (%s<>'MF' OR CC <=> %s)",
                (repo.subunit, repo.gestiune, account, source['DenumireP'], source['UM'], source['CPV'],
                 ctx['type'], source['CC']))
    if len(rows) == 1 and rows[0]['Activ']:
        return rows[0]['ID']
    if rows:
        raise NomError('BU_PRODUCT', f"Fișa produsului «{source['DenumireP']}» este inactivă sau ambiguă la destinație.", 409)
    values = nomenclatoare.clean(nomenclatoare.SPECS['produse'], ctx['type'],
                                  {**{key: source[key] for key in fields}, 'IdMasina': account},
                                  ctx['scale'], repo)
    identity = repo.insert('VC_Produse', {**values, 'Activ': 1, 'DataSalvare': utc_now()})
    protocol.audit(cursor, ctx, 'bu.product.create', 'VC_Produse', identity, None, values, None, request_id)
    return identity


def variant(cursor, repo, ctx, product_id, code, price, request_id):
    rows = _all(cursor, "SELECT IdProdus,Activ FROM VC_ProduseP WHERE SubunitId=%s AND GestiuneId=%s "
                "AND ID=%s AND PUTVA=%s AND NULLIF(TRIM(COD),'') <=> NULLIF(TRIM(%s),'')",
                (repo.subunit, repo.gestiune, product_id, price, code))
    if len(rows) == 1 and rows[0]['Activ']:
        return rows[0]['IdProdus']
    if rows:
        raise NomError('BU_PRODUCT', 'Varianta de produs este inactivă sau ambiguă la destinație.', 409)
    values = {'ID': product_id, 'COD': code, 'PUTVA': price, 'Activ': 1}
    identity = repo.insert('VC_ProduseP', {**values, 'DataSalvare': utc_now()})
    protocol.audit(cursor, ctx, 'bu.variant.create', 'VC_ProduseP', identity, None, values, None, request_id)
    return identity
