"""Child lookup for bank matching across visible catalog groups, without a month filter."""
from .domain import require


def search(repo, query):
    query = query.strip()
    require(len(query) <= 100, 'SEARCH', 'Căutarea poate avea maximum 100 de caractere.', 400)
    if len(query) < 2:
        return {'rows': []}
    pattern = '%' + query.replace('=', '==').replace('%', '=%').replace('_', '=_') + '%'
    rows = repo.query(
        'SELECT c.IDP, c.IDG, c.Nume, SUBSTR(c.CNP,-4) AS CNP_ULTIME, g.Grupa '
        'FROM AD_Platitori c JOIN AD_Grupe g ON g.SubunitId=c.SubunitId AND g.IDG=c.IDG '
        'WHERE c.SubunitId=%s AND g.Ascunsa=0 '
        "AND (c.Nume LIKE %s ESCAPE '=' OR c.CNP LIKE %s ESCAPE '=')",
        (repo.scope(), pattern, pattern))
    # The shared client control orders all matches in Romanian before limiting the visible choices.
    return {'rows': rows}
