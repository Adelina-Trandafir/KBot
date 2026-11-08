import { confirmBox } from '../utils/confirm-box.js';
import { decimalText } from '../vercon/decimal-text.js';
import { state, scaled, text, cents, excelLeft, verconLeft, verconCapacity, sameFields } from './state.js';
import { $, say } from './controls.js';
import { post } from './api.js';

export function actions({ api, lock, inputs, grids, run, enable }) {
  let sendPayload = null;
  const selection = () => ({ ...state.selection, workspace: state.workspace });
  function detail() {
    inputs.excelPrice.setValue(state.excel?.price);
    inputs.verconPrice.setValue(state.candidate?.price);
    const source = state.excel, target = state.candidate;
    if (source && target) {
      const identical = sameFields(source, target);
      const q = [excelLeft(source), verconLeft(target), identical ? verconLeft(target) : verconCapacity(target)].reduce((a, b) => a < b ? a : b);
      inputs.quantity.setValue(text(q > 0n ? q : 0n));
      $('vi-match-info').textContent = `Scor: ${target.score}/100 (orientativ)\n${target.reason}\n` +
        `Magazie: ${decimalText(target.warehouse, 4)}; folosință: ${decimalText(text(scaled(target.quantity) - scaled(target.warehouse)), 4)}.\n` +
        (identical ? 'Poziție identică: asocierea păstrează documentele existente.' :
          `Disponibil pentru corecție: ${decimalText(text(verconCapacity(target)), 4)} (ținând cont de documentele ulterioare).`);
    } else {
      inputs.quantity.setValue('');
      $('vi-match-info').textContent = source ? 'Alegeți un candidat VERCON.' : 'Selectați un rând Excel, apoi un candidat.';
    }
    enable();
  }
  async function chooseExcel(clicked) {
    const source = state.rows.find(r => r.row === clicked.row);
    if (!source || state.busy || state.sent || state.uncertain) return;
    state.excel = source; state.candidate = null; state.candidates = [];
    const sequence = ++state.candidateSequence;
    grids.paint(); detail();
    try {
      const result = await post(api, '/api/vercon/inventory/candidates', { ...selection(), row: source.row });
      if (sequence !== state.candidateSequence || state.excel?.row !== source.row) return;
      state.candidates = result.candidates;
      grids.paint(); detail();
    } catch (error) { if (error.name !== 'AbortError') say(error.message, 'error'); }
  }
  function chooseCandidate(clicked) {
    state.candidate = state.candidates.find(r => r.variant_id === clicked.variant_id) || null;
    detail();
  }
  async function associate() {
    if (!state.excel || !state.candidate) throw new Error('Selectați rândul Excel și candidatul VERCON.');
    const row = state.excel, source = state.candidate;
    const quantity = scaled(inputs.quantity.getValue());
    const changes = !sameFields(row, source);
    if (quantity <= 0n || quantity > excelLeft(row) || quantity > verconLeft(source) || (changes && quantity > verconCapacity(source)))
      throw new Error('Cantitatea trebuie să fie pozitivă și să nu depășească resturile Excel/VERCON sau disponibilul pentru corecție.');
    if (source.price == null) throw new Error('Candidatul VERCON nu are preț. Corectați sursa înainte de asociere.');
    const different = cents(row.price) !== cents(source.price);
    if (different && !await confirmBox(`Prețurile diferă la două zecimale: Excel ${decimalText(row.price, 2)} lei, VERCON ${decimalText(source.price, 2)} lei. Se va păstra prețul Excel. Continuați?`,
      { title: 'Prețuri diferite', yes: 'Asociază cu prețul Excel', no: 'Renunță', warning: true })) return;
    state.assignments.push({ id: crypto.randomUUID(), row: row.row, serial: row.serial, name: row.name,
      vercon_name: source.name, variant: source.variant_id, quantity: text(quantity),
      excel_price: row.price, vercon_price: source.price, price_confirmed: different, changes });
    if (excelLeft(row) === 0n) { state.excel = null; state.candidates = []; state.candidate = null; ++state.candidateSequence; }
    else if (verconLeft(source) === 0n) state.candidate = null;
    grids.paint(); detail();
    say('Asociere păstrată local. Cantitățile rămase sunt afișate în grile.');
  }
  function undo() {
    if (!state.assignment) return;
    state.assignments = state.assignments.filter(a => a.id !== state.assignment.id);
    state.assignment = null; grids.paint(); detail();
    say('Asocierea a fost anulată; cantitățile sunt din nou disponibile.');
  }
  async function send() {
    if (!state.workspace || state.sent) return;
    if (!state.uncertain) {
      const rest = state.rows.reduce((q, r) => q + excelLeft(r), 0n);
      const message = `${state.assignments.length} asocieri vor fi trimise. Diferențele se aplică prin corecții la 31.12.2025; pozițiile identice se păstrează. ` +
        `Rest neasociat: ${decimalText(text(rest), 4)}, adăugat în stoc inițial.\n\n` +
        'Pentru folosință se generează bonuri de corecție BS și BP, cu explicația «Reclasificare inventar», la același deținător. ' +
        'Denumirea, UM și prețul Excel se aplică numai cantităților trimise. Documentele istorice se păstrează. Trimiteți lotul?';
      if (!await confirmBox(message, { title: 'Trimite în VERCON', yes: 'Trimite în VERCON', no: 'Revizuiește', warning: true })) return;
      sendPayload = { ...selection(), assignments: state.assignments.map(a => ({ row: a.row, variant: a.variant, quantity: a.quantity, price_confirmed: a.price_confirmed })) };
    }
    await lock.acquire(state.selection);
    try {
      state.uncertain = true;
      say('Se salvează lotul în VERCON, într-o singură tranzacție…');
      const result = await post(api, '/api/vercon/inventory/send', sendPayload);
      state.uncertain = false; state.sent = true; state.excel = null; state.candidate = null; state.candidates = [];
      grids.paint(); detail();
      say(`Lot trimis: ${result.assignments} asocieri, ${decimalText(result.new_quantity, 4)} cantitate nouă; ${result.documents.length} documente la 31.12.2025.`, 'success');
    } catch (error) {
      if (error.status && error.status < 500) state.uncertain = false;
      if (state.uncertain) throw new Error('Nu am primit confirmarea completă. Reapăsați «Trimite în VERCON» pentru același lot; acesta nu se dublează. ' + error.message);
      throw error;
    } finally { await lock.release(); enable(); }
  }
  $('vi-associate').addEventListener('click', () => run(associate));
  $('vi-undo').addEventListener('click', () => run(undo));
  $('vi-send').addEventListener('click', () => run(send));
  return { chooseExcel, chooseCandidate, detail, clearSend: () => { sendPayload = null; } };
}
