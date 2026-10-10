"""SLICE-AD11: unit-wide parent identity, serialized by AD_Lock(0)."""
import re
import secrets
from .domain import cnp_code, require


def normalize_email(value):
    email = str(value or '').strip().lower()
    require(not email or len(email) <= 255 and re.fullmatch(r'[^\s@]+@[^\s@]+\.[^\s@]+', email),
            'EMAIL', 'Introduceți o adresă de email validă.', 400)
    return email


def prepare_parent(repo, values, existing=None):
    changing_email = existing and 'EMail' in values
    merged = {**(existing or {}), **values}
    email = normalize_email(merged.get('EMail'))
    cnp = str(merged.get('CNP_Platitor') or '').strip()
    own_id = (existing or {}).get('IDS', -1)
    old_cnp = str((existing or {}).get('CNP_Platitor') or '').strip()
    if old_cnp and old_cnp != cnp:
        remaining = repo.query('SELECT IDS FROM AD_Platitori_sub WHERE TRIM(CNP_Platitor)=%s AND IDS<>%s', (old_cnp,own_id))
        if not remaining:
            # Release a corrected identity's email and invalidate its previous login context.
            repo.execute('DELETE FROM AD_PortalParents WHERE CNP=%s', (old_cnp,))
    if email:
        duplicates = repo.query('SELECT p.Nume,p.CNP_Platitor,c.Nume AS ChildName FROM AD_Platitori_sub p '
            'LEFT JOIN AD_Platitori c ON c.IDP=p.IDP AND c.SubunitId=p.SubunitId '
            'WHERE LOWER(TRIM(p.EMail))=%s AND p.IDS<>%s AND COALESCE(TRIM(p.CNP_Platitor),\'\')<>%s', (email, own_id, cnp))
        require(not duplicates, 'EMAIL_CONFLICT', 'Adresa de email este deja utilizată de ' + '; '.join(
            f"părintele {r['Nume']}, CNP {r['CNP_Platitor'] or '(necompletat)'}, copil {r['ChildName'] or '(necompletat)'}"
            for r in duplicates) + '.', 409)
        canonical = repo.query('SELECT CNP FROM AD_PortalParents WHERE Email=%s AND CNP<>%s', (email, cnp))
        require(not canonical, 'EMAIL_CONFLICT', 'Adresa de email este asociată altui CNP în registrul părinților.', 409)
    values['EMail'] = email or None
    values['CNP_Platitor'] = cnp or None
    if cnp_code(cnp) != -1:
        values['CodAccesPortal'] = None
        return
    identities = repo.query('SELECT * FROM AD_PortalParents WHERE CNP=%s', (cnp,))
    identity = identities[0] if identities else None
    # An edit of the same identity changes its email across all child associations.
    changing_email = changing_email and existing.get('CNP_Platitor') == cnp
    if identity and not changing_email:
        require(not email or not identity['Email'] or email == identity['Email'], 'EMAIL_CONFLICT',
                'Acest CNP are deja alt email. Modificați emailul în înregistrarea existentă a părintelui.', 409)
        email = email or identity['Email'] or ''
        values['EMail'] = email or None
    child = repo.get('Platitori', merged['IDP'])
    code = identity['CodAccesPortal'] if identity else None
    if not code and not child.get('Plecat'):
        code = secrets.token_hex(16)
    if identity:
        repo.execute('UPDATE AD_PortalParents SET Email=%s,CodAccesPortal=%s WHERE CNP=%s', (email or None, code, cnp))
    else:
        repo.execute('INSERT INTO AD_PortalParents (CNP,Email,CodAccesPortal) VALUES (%s,%s,%s)', (cnp, email or None, code))
    repo.execute('UPDATE AD_Platitori_sub SET EMail=%s,CodAccesPortal=%s,Version=Version+1 '
                 'WHERE TRIM(CNP_Platitor)=%s AND IDS<>%s', (email or None, code, cnp, own_id))
    values['CodAccesPortal'] = code


def provision_existing(repo):
    """Provision imported parents, never accepting credentials from a source extract."""
    rows = repo.rows('Platitori_sub')
    emails = {}
    for row in rows:
        cnp = str(row.get('CNP_Platitor') or '').strip()
        email = normalize_email(row.get('EMail'))
        require(not cnp or not email or cnp not in emails or emails[cnp] == email, 'EMAIL_CONFLICT',
                f"Părintele {row.get('Nume')}, CNP {cnp}, are emailuri diferite în import.", 409)
        if cnp and email:
            emails[cnp] = email
    for row in rows:
        values = {'EMail': row.get('EMail'), 'CNP_Platitor': row.get('CNP_Platitor')}
        # An import cannot replace a contact already configured for this CNP in another subunit.
        current_identity = {**row, 'CNP_Platitor': None}
        prepare_parent(repo, values, current_identity)
        current = repo.get('Platitori_sub', row['IDS'])
        repo.update('Platitori_sub', current, values)
