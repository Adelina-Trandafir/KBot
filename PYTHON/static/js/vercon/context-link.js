// VC12-09: share the accepted VERCON context with its inventory page.
// IDs are navigation preferences only; the server resolves every selection again.
const keys = ['subunit', 'gestiune', 'year'];
const pages = new Set(['/vercon', '/vercon-inventar']);
const storageKey = unit => `vercon.selection.${unit}`;
function selectionOf(source) {
  return Object.fromEntries(keys.filter(key => source?.[key] != null && source[key] !== '').map(key => [key, String(source[key])]));
}
export function rememberContext(unit, selection) {
  if (!unit) return;
  try { sessionStorage.setItem(storageKey(unit), JSON.stringify(selectionOf(selection))); } catch { /* Navigation still carries the context. */ }
  if (pages.has(location.pathname)) {
    history.replaceState(history.state, '', contextUrl(location.href, unit, selection));
  }
}
export function initialContext(unit) {
  const params = new URLSearchParams(location.search);
  if ((!params.has('unit') || params.get('unit') === unit) && keys.some(key => params.has(key))) {
    return selectionOf(Object.fromEntries(params));
  }
  try { return selectionOf(JSON.parse(sessionStorage.getItem(storageKey(unit)) || '{}')); } catch { return {}; }
}
export function contextUrl(path, unit, selection) {
  const url = new URL(path, location.origin);
  if (url.origin !== location.origin || !pages.has(url.pathname)) return path;
  for (const key of keys) url.searchParams.delete(key);
  if (unit) url.searchParams.set('unit', unit);
  for (const [key, value] of Object.entries(selectionOf(selection))) url.searchParams.set(key, value);
  return url.pathname + url.search + url.hash;
}
