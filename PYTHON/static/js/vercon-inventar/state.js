// VC12-04: quantities are exact four-decimal integers, never floating point balances.
export function scaled(raw) {
  const match = /^(\d+)(?:\.(\d{0,4}))?$/.exec(String(raw ?? '').trim());
  if (!match) throw new Error('Introduceți o cantitate pozitivă, cu cel mult 4 zecimale.');
  return BigInt(match[1]) * 10000n + BigInt((match[2] || '').padEnd(4, '0'));
}
export function text(value) {
  const sign = value < 0n ? '-' : ''; value = value < 0n ? -value : value;
  const whole = value / 10000n, tail = String(value % 10000n).padStart(4, '0').replace(/0+$/, '');
  return sign + whole + (tail ? '.' + tail : '');
}
export function cents(raw) { return (scaled(raw) + 50n) / 100n; }
export const state = { unit: null, ctx: null, selection: {}, account: null, workspace: null, rows: [],
  candidates: [], assignments: [], excel: null, candidate: null, assignment: null,
  busy: false, sent: false, uncertain: false, candidateSequence: 0, file: null, sheet: null };
export const excelLeft = row => scaled(row.quantity) - state.assignments.filter(a => a.row === row.row).reduce((q, a) => q + scaled(a.quantity), 0n);
export const verconUsed = variant => state.assignments.filter(a => a.variant === variant).reduce((q, a) => q + scaled(a.quantity), 0n);
export const verconLeft = row => scaled(row.quantity) - verconUsed(row.variant_id);
export const verconCapacity = row => scaled(row.capacity) - state.assignments.filter(a => a.variant === row.variant_id && a.changes !== false)
  .reduce((q, a) => q + scaled(a.quantity), 0n);
export function sameFields(row, candidate) {
  if (!row || !candidate || candidate.price == null) return false;
  try { return row.name === candidate.name && row.um === candidate.um && (row.code || '') === (candidate.code || '') && scaled(row.price) === scaled(candidate.price); }
  catch { return false; }
}
export function reset() {
  state.candidateSequence++; state.workspace = null; state.rows = []; state.candidates = [];
  state.assignments = []; state.excel = null; state.candidate = null; state.assignment = null;
  state.sent = false; state.uncertain = false;
}
