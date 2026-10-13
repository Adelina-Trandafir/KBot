"""Read-only number proposals. Saving still allocates/checks inside its transaction."""
from . import documents


def check(cursor, ctx, raw=None, exclude=None):
    cursor.execute('SELECT LastNumber FROM VC_Numbering WHERE SubunitId=%s AND GestiuneId=%s '
                   'AND ExercitiuId=%s AND Kind=%s', (*documents._scope(ctx), 'NIR'))
    counter = cursor.fetchone()
    proposed = int(counter['LastNumber'] if counter else 0) + 1
    while documents._nir_number_taken(cursor, ctx, proposed, False, exclude):
        proposed += 1
    manual = documents._int(raw)
    empty = raw in (None, '')
    valid = empty or manual is not None and manual > 0
    duplicate = bool(valid and not empty and documents._nir_number_taken(cursor, ctx, manual, False, exclude))
    return {'suggested': proposed, 'valid': valid, 'duplicate': duplicate, 'reserved': False}
