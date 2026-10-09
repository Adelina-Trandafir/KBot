"""SLICE-ADE1/4: ADE blueprint using portal authentication and common infrastructure."""
import logging
from functools import wraps
from flask import Blueprint, current_app, g, jsonify, request, send_from_directory
from .domain import SCHEMA, EDITABLE, DomainError, require, group_educators
from .repository import transaction
from .calculations import calculate
from . import service
from . import importer
from . import catalog_forms
from routes.portal import drepturi

SECTION = "AD"

logger = logging.getLogger(__name__)


def create_blueprint(authenticate=None, repository_factory=None, rights=None):
    bp = Blueprint('adechit', __name__)
    # rights(context) -> the set of operations the signed-in user may do in this unit. Default: the
    # AD roles of AVACONT_COMUN (slice AD10-01); the local preview passes its own.
    if rights is None:
        def rights(context):
            return drepturi.operations_of(context["email"], context["db_name"], SECTION)

    def guard(operation):
        def decorate(function):
            @wraps(function)
            def guarded(*args, **kwargs):
                def run():
                    try:
                        context = g.portal
                        require(request.headers.get('X-Ade-Unit') == context['db_name'] or request.path.endswith('/context'),
                                'CONTEXT_CHANGED', 'Unitatea s-a schimbat. Reîncărcați pagina.', 409)
                        with transaction(context['db_name'], repository_factory) as repo:
                            g.ade_operations = rights(context)
                            require(operation in g.ade_operations, 'FORBIDDEN', 'Nu aveți dreptul necesar pentru această operație ADE.', 403)
                            result = function(repo, *args, **kwargs)
                        logger.info('ADE operation=%s outcome=success', operation)
                        return jsonify(result)
                    except DomainError as error:
                        logger.warning('ADE operation=%s refusal=%s', operation, error.reason)
                        return jsonify(error=str(error), reason=error.reason), error.status
                    except (KeyError, TypeError) as error:
                        logger.warning('ADE malformed request: %s', type(error).__name__)
                        return jsonify(error='Cerere incompletă sau invalidă.', reason='REQUEST'), 400
                    except Exception:
                        logger.exception('ADE operation=%s failed', operation)
                        return jsonify(error='Operația nu a fost finalizată. Reîncercați cu aceeași cheie.', reason='SERVER'), 500
                if authenticate:
                    return authenticate(run)()
                from routes.portal.portal import require_portal_session
                from utils.timing import timed
                return require_portal_session(timed('ade.' + operation)(run))()
            return guarded
        return decorate

    @bp.get('/adechit')
    def page():
        response = send_from_directory(current_app.static_folder, 'adechit.html')
        if repository_factory is not None:
            response.direct_passthrough = False
            response.set_data(response.get_data(as_text=True).replace('<html lang="ro">', '<html lang="ro" data-ade-preview="true">', 1))
        response.headers.update({'Cache-Control': 'no-store', 'X-Content-Type-Options': 'nosniff',
                                 'Content-Security-Policy': "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; frame-ancestors 'none'; base-uri 'none'"})
        return response

    @bp.get('/api/adechit/context')
    @guard('read')
    def context(repo):
        groups = repo.rows('Grupe')
        educators = repo.rows('Grupe_Educator')
        from datetime import date
        for group in groups:
            group['Educator'] = group_educators(group, educators, date.today().isoformat())
        return {'unit': g.portal['db_name'], 'email': g.portal['email'], 'schema': SCHEMA,
                'editable': EDITABLE, 'months': repo.rows('LunaD'), 'groups': groups,
                'permissions': sorted(g.ade_operations),
                'preview': repository_factory is not None,
                'limitations': ['Paritatea completă Access nu este validată.', 'Rapoartele și tipăririle sunt amânate.']}

    @bp.get('/api/adechit/receipt-defaults')
    @guard('read')
    def receipt_defaults(repo):
        month_id = request.args.get('IDL', '')
        require(month_id.isdigit(), 'FILTER', 'Filtru invalid.', 400)
        configs = repo.query('SELECT * FROM AVACONT_COMUN.Unitati_Chitante WHERE DC=%s', (g.portal['db_name'],))
        if len(configs) != 1:
            return {'explanation': ''}
        return {'explanation': service.receipt_explanation(configs[0], repo.get('LunaD', int(month_id)))}

    @bp.get('/api/adechit/catalog-data')
    @guard('read')
    def catalog_data(repo):
        return catalog_forms.catalog_data(repo)

    @bp.get('/api/adechit/rows/<table>')
    @guard('read')
    def rows(repo, table):
        data = repo.rows(table)
        for key in ('IDL', 'IDG', 'IDP', 'IDZ'):
            if key in request.args:
                require(request.args[key].isdigit(), 'FILTER', 'Filtru invalid.', 400)
                data = [r for r in data if r.get(key) == int(request.args[key])]
        return {'rows': data}

    def command(repo, operation, function):
        body = request.get_json(silent=True)
        require(isinstance(body, dict), 'JSON', 'Este necesară o cerere JSON.', 400)
        return service.idempotent(repo, request.headers.get('Idempotency-Key'), operation, body, lambda: function(body))

    @bp.post('/api/adechit/catalog/<table>')
    @guard('catalog')
    def catalog(repo, table):
        return command(repo, 'catalog/' + table, lambda body: service.save_catalog(repo, table, body))

    @bp.post('/api/adechit/catalog-save')
    @guard('catalog')
    def catalog_save(repo):
        return command(repo, 'catalog-save', lambda body: service.save_catalog_batch(repo, body))

    @bp.post('/api/adechit/group-save')
    @guard('catalog')
    def group_save(repo):
        return command(repo, 'group-save', lambda body: catalog_forms.save_group(repo, body))

    @bp.post('/api/adechit/child-save')
    @guard('catalog')
    def child_save(repo):
        return command(repo, 'child-save', lambda body: catalog_forms.save_child(repo, body, g.portal['email'], 'transfer' in g.ade_operations))

    @bp.post('/api/adechit/attendance')
    @guard('attendance')
    def attendance(repo):
        return command(repo, 'attendance', lambda body: service.save_attendance(repo, body))

    @bp.post('/api/adechit/attendance/prepare')
    @guard('attendance')
    def prepare_attendance(repo):
        return command(repo, 'attendance/prepare', lambda body: service.prepare_attendance(repo, body))

    @bp.get('/api/adechit/situation/<int:month_id>')
    @guard('read')
    def situation(repo, month_id):
        month = repo.get('LunaD', month_id)
        if month['Inchisa']:
            return {'rows': [r for r in repo.rows('SS_Buget') if r['IDL'] == month_id], 'saved': True}
        calculated, _ = calculate(repo.data(), month_id)
        attendance = {row['IDZ']: row for row in repo.rows('Prezenta') if row['IDL'] == month_id}
        for row in calculated:
            source = attendance[row['IDZ']]
            row.update(Version=source['Version'], ZileLuna=service.month_days(month), IDV=source['IDV'])
        return {'rows': calculated, 'saved': False, 'parity_verified': False}

    @bp.post('/api/adechit/documents')
    @guard('collect')
    def documents(repo):
        return command(repo, 'documents', lambda body: service.emit_document(repo, g.portal['db_name'], body))

    @bp.post('/api/adechit/cancel')
    @guard('cancel')
    def cancel(repo):
        return command(repo, 'cancel', lambda body: service.cancel_document(repo, body))

    @bp.post('/api/adechit/close')
    @guard('close')
    def close(repo):
        return command(repo, 'close', lambda body: service.close_month(repo, body))

    @bp.post('/api/adechit/reopen')
    @guard('reopen')
    def reopen(repo):
        return command(repo, 'reopen', lambda body: service.reopen_month(repo, body))

    @bp.post('/api/adechit/transfer')
    @guard('transfer')
    def transfer(repo):
        raise DomainError('M06', 'Transferul așteaptă clarificarea efectului asupra istoricului.')

    @bp.post('/api/adechit/import')
    @guard('import')
    def import_data(repo):
        body = request.get_json(silent=True)
        require(isinstance(body, dict) and isinstance(body.get('dataset'), dict),
                'IMPORT_FORMAT', 'Este necesar un extract ADE valid.', 400)
        return importer.import_dataset(repo, body['dataset'], g.portal['db_name'],
                                       str(body.get('source_file') or 'uploaded.json'))

    @bp.post('/api/adechit/reconcile')
    @guard('import')
    def reconcile(repo):
        body = request.get_json(silent=True)
        require(isinstance(body, dict) and isinstance(body.get('dataset'), dict),
                'IMPORT_FORMAT', 'Este necesar un extract ADE valid.', 400)
        return importer.reconcile_dataset(repo, body['dataset'], g.portal['db_name'])

    return bp


adechit_bp = create_blueprint()
