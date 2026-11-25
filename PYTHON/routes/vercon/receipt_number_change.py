"""SLICE-VC16-04: save a NIR number validated by receipt_number."""
from . import documents as d, protocol, receipt_number
from .nomenclatoare import NomError


def change(cursor, repo, ctx, doc_id, body, request_id):
    table, pk, before = d._load_for_write(repo, ctx, 'nir', doc_id, body.get('version'))
    d._guard_dependents(cursor, ctx, 'nir', doc_id, 'edit')
    old_pair = (abs(before['Numar'] or 0), int(before.get('NumarSub') or 0))
    day = protocol.to_date(str(before['Data'])[:10], 'Data NIR')
    number, fraction = receipt_number.assign_text(cursor, ctx, body.get('number'), day, doc_id, current=old_pair)
    values = {'Numar': number}
    if d._numarsub_column(cursor, ctx):
        values['NumarSub'] = fraction
    if repo.update(table, values, {pk: doc_id, 'Version': before['Version']}, bump_version=True) != 1:
        raise NomError('CONFLICT', 'Documentul a fost modificat de altcineva. Reîncărcați documentul.', 409)
    old_text = '%s%s' % (old_pair[0], d._sub_text(old_pair[1]))
    text = '%s%s' % (number, d._sub_text(fraction))
    protocol.audit(cursor, ctx, 'nir.number', table, doc_id, {'number': old_text}, {'number': text}, None, request_id)
    return {'id': doc_id, 'number': text}
