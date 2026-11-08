// VERCON: one rule for every box that starts a search while typing (the grid DGV has its own and is not touched).
// - the search starts after SEARCH_MIN_CHARS typed characters (spaces do not count);
// - it waits for a pause in typing (debounce, 0.3 s by default, editable in "Setari cautare" of the application menu).
import { searchTooShort } from '../utils/search-text.js';

export const SEARCH_MIN_CHARS = 2;
export const SEARCH_DELAY_DEFAULT = 300;
export const SEARCH_DELAY_MIN = 100;
export const SEARCH_DELAY_MAX = 3000;
const KEY = 'vercon.search.settings';
let current = null;

function normalize(k_value) {
  const k_ms = Math.round(Number(k_value));
  return Number.isFinite(k_ms) ? Math.min(SEARCH_DELAY_MAX, Math.max(SEARCH_DELAY_MIN, k_ms)) : SEARCH_DELAY_DEFAULT;
}

export function searchDelay() {
  if (current === null) {
    try { current = normalize(JSON.parse(localStorage.getItem(KEY) || '{}').debounceMs ?? SEARCH_DELAY_DEFAULT); }
    catch (k_error) { console.warn('VERCON search settings could not be read', k_error); current = SEARCH_DELAY_DEFAULT; }
  }
  return current;
}

export function saveSearchDelay(k_value) {
  current = normalize(k_value);
  let persisted = true;
  try { localStorage.setItem(KEY, JSON.stringify({ debounceMs: current })); }
  catch (k_error) { console.warn('VERCON search settings could not be saved', k_error); persisted = false; }
  return { delay: current, persisted };
}

// Options for the shared Combobox / TreeView / ComboTree: delay, minimum length, spaces not counted.
export function searchOptions() {
  return { searchDelay: searchDelay(), minSearchLength: SEARCH_MIN_CHARS, minSearchIgnoreSpaces: true };
}

// The text a search should really use: a typed-but-too-short text means "no filter" (the full list).
export function effectiveSearchText(k_text) {
  return searchTooShort(k_text, SEARCH_MIN_CHARS) ? '' : String(k_text ?? '').trim();
}

// Debounce for the boxes that run their own search. `run` gets the arguments of the LAST call; `.cancel()` drops a pending one.
export function debounced(k_run) {
  let k_timer = 0;
  const k_call = (...k_args) => { clearTimeout(k_timer); k_timer = setTimeout(() => { k_timer = 0; k_run(...k_args); }, searchDelay()); };
  k_call.cancel = () => { clearTimeout(k_timer); k_timer = 0; };
  return k_call;
}
