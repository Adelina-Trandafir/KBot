import { portalToken } from '../vercon/api.js';
import { state } from './state.js';
import { beginSessionExit } from '../utils/session-exit.js';

export function login() { window.location.replace('/portal?next=' + encodeURIComponent('/vercon-inventar')); }
export function makeApi(signal, getLock) {
  return async function api(path, options = {}) {
    const headers = { Accept: 'application/json', 'X-Portal-Token': portalToken(), 'X-Request-Id': crypto.randomUUID(),
      ...(state.unit ? { 'X-Vercon-Unit': state.unit } : {}),
      ...(state.ctx ? { 'X-Vercon-Version': state.ctx.version } : {}),
      ...(getLock()?.token ? { 'X-Vercon-Lock': getLock().token } : {}), ...options.headers };
    const response = await fetch(path, { ...options, headers, signal: options.signal || signal });
    const binary = options.responseType === 'arrayBuffer';
    const result = response.ok && binary ? null : await response.json().catch(() => ({}));
    if (response.status === 401 || result?.reason === 'UNITATE_NEDESCHISA') { beginSessionExit(); login(); throw new DOMException('Autentificare necesară.', 'AbortError'); }
    if (!response.ok) {
      const detail = Object.values(result.fields || {}).join(' ');
      throw Object.assign(new Error((result.error || `Eroare HTTP ${response.status}`) + (detail ? ' ' + detail : '')),
        { status: response.status, reason: result.reason });
    }
    return binary ? response.arrayBuffer() : result;
  };
}
export function post(api, path, data) { return api(path, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(data) }); }
