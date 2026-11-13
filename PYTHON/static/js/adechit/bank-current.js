// SLICE-AD21-01: "Asocieri curente" mode of the bank window - the associations of the chosen month, and the
// complete deletion of one allocation (link + Alte plati document + payment) from an open month.
import { DataGrid, ValueType } from '../dgv/datagrid.js';
import { confirmBox } from '../utils/confirm-box.js';

const $ = (id) => document.getElementById(id);
const text = (key, title, width = 150) => ({ key, title, width, minWidth: 50, valueType: ValueType.Text });
const money = (value) => Number(value || 0).toLocaleString('ro-RO', { maximumFractionDigits: 2 });

export function bindBankCurrent({ api, context, month, run, message, refresh }) {
  const host = $('ade-bank-current-list');
  const remove = $('ade-bank-current-delete');
  const note = $('ade-bank-current-note');
  let grid = null; let rows = []; let selected = null; let closed = false; let sequence = 0;
  const canDelete = () => Boolean(context()?.permissions.includes('cancel'));

  function sync() {
    remove.disabled = !selected || !selected.deletable || !canDelete();
  }
  function describe() {
    if (!month()) note.textContent = 'Alegeți luna.';
    else if (closed) note.textContent = 'Luna este închisă: asocierile pot fi doar consultate. Ștergerea este posibilă numai în luna deschisă.';
    else if (!canDelete()) note.textContent = 'Nu aveți dreptul de a șterge documente.';
    else note.textContent = 'Ștergerea elimină complet documentul din Alte plăți; încasarea (sau diferența) poate fi asociată din nou.';
  }
  function build() {
    destroy();
    grid = new DataGrid(host, { rows: [], rowKey: 'IDA', autoSize: false, headerHeight: 32, mobileRowScale: 1.2,
      footer: true, footerCaption: '{0} asocieri', layout: { fill: 'platitor_nume' },
      theme: document.documentElement.dataset.theme === 'dark' ? 'dark' : 'modern',
      onSelect: (row) => { selected = row; sync(); },
      columns: [
        { ...text('DataExtras', 'Data extras', 95), valueType: ValueType.DateTime },
        text('NrExtras', 'Nr. extras', 90),
        text('platitor_nume', 'Plătitor bancă', 220),
        { ...text('suma_credit', 'Valoare extras', 100), valueType: ValueType.Number, align: 'right', formatter: money },
        text('copil', 'Copil', 200),
        text('NrDoc', 'Nr. document', 90),
        { ...text('suma_asociata', 'Valoare asociată', 110), valueType: ValueType.Number, align: 'right', formatter: money },
        text('Stare', 'Stare', 70)] });
  }
  async function load() {
    const own = ++sequence;
    const period = month();
    selected = null; rows = []; sync();
    if (!period) { closed = false; describe(); grid?.setRows([]); return; }
    const result = await api(`/api/adechit/bank-extracts/current?IDL=${period.IDL}`);
    if (own !== sequence) return;
    closed = Boolean(result.month_closed);
    rows = result.rows.map((row) => ({ ...row, Stare: row.Cancelled ? 'Anulat' : '' }));
    if (!grid) build();
    grid.setRows(rows); describe(); sync();
  }
  function destroy() { sequence += 1; grid?.destroy(); grid = null; selected = null; rows = []; }

  remove.addEventListener('click', () => run(async () => {
    const item = selected;
    if (!item || !item.deletable || !canDelete()) return;
    const accepted = await confirmBox(
      `${item.copil} · nr. ${item.NrDoc}: ${money(item.suma_asociata)} lei\n`
      + `Extras din ${String(item.DataExtras || '').slice(0, 10)} · ${item.platitor_nume} · ${money(item.suma_credit)} lei\n\n`
      + 'Documentul din Alte plăți și plata lui se șterg complet, nu se anulează. Suma revine în lista încasărilor neasociate și poate fi asociată din nou. Operația nu poate fi retrasă.',
      { title: 'Ștergere asociere', yes: 'Șterge asocierea', no: 'Renunță' });
    if (!accepted) return;
    await api('/api/adechit/bank-extracts/delete-association', { method: 'POST', key: crypto.randomUUID(),
      body: JSON.stringify({ extract_id: item.IDFXE, child_id: item.IDP, document_version: item.document_version }) });
    try { await refresh(); await load(); }
    catch (error) { throw new Error(`Asocierea a fost ștearsă. Reîncărcați listele: ${error.message}`); }
    message('Asocierea a fost ștearsă. Documentul din Alte plăți a fost eliminat complet.');
  }));

  return { load, sync, destroy };
}
