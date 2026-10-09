// Slice 0110-03 -- the web area of registered users: sign-in (password, then the mailed code),
// the unit picker, and the session monitor. The data views come with slice 0110-06.
//
// The portal token lives in sessionStorage and travels in X-Portal-Token (no cookie). Everything
// the server refuses comes back with its Romanian sentence in `error` and an ASCII `reason`.

import sessionMonitoring from '../session/session-monitoring.js';
import { createApp } from './app.js';
import { createMenu } from './menu.js';
import { installLayouts, setAdmin } from './layouts.js';
import { createCertificate } from './certificat.js';
import eventBus, { EVENTS } from '../event-bus/event-bus.js';

const TOKEN_KEY = 'kbot-portal-token';
const PORTAL_URL = '/portal';

// Slice ADE9-01: /portal?next=/adechit -- a page that needs the sign-in sends the user here and takes
// them back once a unit is open. Only these pages are allowed, never a free address.
const NEXT_PAGES = ['/adechit'];
const nextPage = (() => {
  const wanted = new URLSearchParams(window.location.search).get('next') || '';
  return NEXT_PAGES.includes(wanted) ? wanted : '';
})();
function goNext() {
  if (nextPage) window.location.replace(nextPage);
}

const $ = (id) => document.getElementById(id);

function readToken() {
  try {
    return window.sessionStorage.getItem(TOKEN_KEY) || '';
  } catch (err) {
    console.error('[portal] sessionStorage is blocked', err);
    return '';
  }
}

function writeToken(value) {
  try {
    if (value) window.sessionStorage.setItem(TOKEN_KEY, value);
    else window.sessionStorage.removeItem(TOKEN_KEY);
  } catch (err) {
    console.error('[portal] sessionStorage is blocked', err);
  }
}

// ---- one call to the server -> {ok, status, data}
async function call(method, path, body) {
  const headers = { Accept: 'application/json' };
  const token = readToken();
  if (token) headers['X-Portal-Token'] = token;
  if (body !== undefined) headers['Content-Type'] = 'application/json';
  try {
    const response = await fetch(path, {
      method,
      headers,
      body: body === undefined ? undefined : JSON.stringify(body),
      cache: 'no-store',
    });
    const data = await response.json().catch(() => ({}));
    return { ok: response.ok, status: response.status, data };
  } catch (err) {
    console.error(`[portal] ${method} ${path} did not reach the server`, err);
    return {
      ok: false,
      status: 0,
      data: { error: 'Serverul nu răspunde. Verificați conexiunea și reîncercați.', reason: 'NETWORK' },
    };
  }
}

// One GET that returns the raw bytes (a signed PDF) -> {ok, status, data}: an ArrayBuffer when ok,
// else the server's {error, reason}.
async function callBytes(path) {
  const headers = {};
  const token = readToken();
  if (token) headers['X-Portal-Token'] = token;
  try {
    const response = await fetch(path, { headers, cache: 'no-store' });
    if (response.ok) return { ok: true, status: response.status, data: await response.arrayBuffer() };
    const data = await response.json().catch(() => ({}));
    return { ok: false, status: response.status, data };
  } catch (err) {
    console.error(`[portal] GET ${path} did not reach the server`, err);
    return { ok: false, status: 0, data: { error: 'Serverul nu răspunde. Verificați conexiunea și reîncercați.', reason: 'NETWORK' } };
  }
}

function say(id, text) {
  const el = $(id);
  el.textContent = text || '';
  el.hidden = !text;
}

function show(which) {
  ['card-login', 'card-code', 'card-app'].forEach((id) => {
    $(id).hidden = id !== which;
  });
  $('user-info').hidden = which !== 'card-app';
  $('pmenu-nav').hidden = which !== 'card-app';
}

// ---- header menu: the pages behind its entries come later, until then a click says so
let noticeTimer = 0;
document.querySelectorAll('.js-back-ang').forEach((b) => b.addEventListener('click', () => app.closePage()));

const menu = createMenu($('pmenu'), (key, label) => {
  if (['extrase', 'clasificatii', 'parteneri', 'admin'].includes(key)) {
    app.openPage(key);
    return;
  }
  app.closePage();
  say('msg-app', 'Secțiunea «' + label + '» va fi disponibilă în curând.');
  window.clearTimeout(noticeTimer);
  noticeTimer = window.setTimeout(() => say('msg-app', ''), 3500);
});

let pending = '';
let me = null;
let monitorStarted = false;

// ---- step 1: e-mail + password
$('form-login').addEventListener('submit', async (ev) => {
  ev.preventDefault();
  say('msg-login', '');
  const email = $('p-email').value.trim();
  const parola = $('p-parola').value;
  if (!email || !parola) {
    say('msg-login', 'Introduceți adresa de e-mail și parola.');
    return;
  }
  $('btn-login').disabled = true;
  const r = await call('POST', '/api/portal/login', { email, parola });
  $('btn-login').disabled = false;
  if (!r.ok) {
    say('msg-login', r.data.error || 'Autentificarea nu a reușit. Reîncercați.');
    return;
  }
  $('p-parola').value = '';
  pending = r.data.pending;
  $('code-to').textContent = r.data.email_masked;
  $('p-cod').value = '';
  say('msg-code', '');
  show('card-code');
  $('p-cod').focus();
});

// ---- step 2: the code
$('btn-code-back').addEventListener('click', () => {
  pending = '';
  show('card-login');
  $('p-email').focus();
});

$('form-code').addEventListener('submit', async (ev) => {
  ev.preventDefault();
  say('msg-code', '');
  const cod = $('p-cod').value.trim();
  if (!cod) {
    say('msg-code', 'Introduceți codul primit pe e-mail.');
    return;
  }
  $('btn-code').disabled = true;
  const r = await call('POST', '/api/portal/verifica', { pending, cod });
  $('btn-code').disabled = false;
  if (!r.ok) {
    say('msg-code', r.data.error || 'Codul nu a putut fi verificat. Reîncercați.');
    if (['COD_EXPIRAT', 'COD_EPUIZAT', 'RATE_LIMITED'].includes(r.data.reason)) {
      pending = '';
      say('msg-login', r.data.error);
      show('card-login');
    }
    return;
  }
  pending = '';
  writeToken(r.data.token);
  await openApp();
});

// ---- signed in
installLayouts(); // the grids ask layouts.js for their column order, visibility and widths
const app = createApp({
  call,
  callBytes,
  onUnitOpened: goNext,
  onUnauthorized: (message) => {
    writeToken('');
    say('msg-login', message || 'Sesiunea a expirat. Autentificați-vă din nou.');
    show('card-login');
  },
});

async function openApp() {
  const r = await call('GET', '/api/portal/me');
  if (!r.ok) {
    writeToken('');
    say('msg-login', r.data.error || '');
    show('card-login');
    return;
  }
  me = r.data;
  menu.setAdmin(me.is_admin === true);
  setAdmin(me.is_admin === true); // the administrator's own column layouts apply only to administrator accounts
  $('user-email').textContent = me.email;
  show('card-app');
  startMonitor();
  await app.open(me);
  if (me.db_name) goNext(); // a unit is already open (the only one, or from earlier): straight back
}

// ---- light / dark look (the saved choice is applied early by a script in portal.html)
function paintThemeButton() {
  const dark = document.documentElement.dataset.theme === 'dark';
  $('btn-theme').textContent = dark ? '☀️' : '🌙';
  $('btn-theme').setAttribute('aria-pressed', dark ? 'true' : 'false');
}
$('btn-theme').addEventListener('click', () => {
  const dark = document.documentElement.dataset.theme !== 'dark';
  if (dark) document.documentElement.dataset.theme = 'dark';
  else delete document.documentElement.dataset.theme;
  try { localStorage.setItem('kbot-portal-theme', dark ? 'dark' : 'light'); } catch (e) { /* storage blocked: the look just is not remembered */ }
  paintThemeButton();
});
paintThemeButton();

$('btn-logout').addEventListener('click', async () => {
  await call('POST', '/api/portal/logout');
  writeToken('');
  window.location.href = PORTAL_URL;
});

// ---- the session timer (adapted from JS_COMPONENTS/session)
// The monitor announces what it does on the shared event bus, which complains (in the console)
// about every event nobody listens to. The page needs none of them yet, so each one gets a quiet ear.
[EVENTS.USER_ACTIVITY, EVENTS.SESSION_WARNING, EVENTS.SESSION_FINAL_WARNING, EVENTS.SESSION_EXTENDED,
  EVENTS.SESSION_EXPIRED, EVENTS.SESSION_CLEANUP, EVENTS.SESSION_ERROR].forEach((name) => eventBus.on(name, () => {}));

function startMonitor() {
  if (monitorStarted) return;
  monitorStarted = true;
  sessionMonitoring
    .init({
      getToken: readToken,
      loginUrl: PORTAL_URL,
      onExpired: () => writeToken(''),
    })
    .catch((err) => console.error('[portal] session monitor failed to start', err));
}

// ---- digital certificate (slice 0110-04): computers only, taken out of the page on a phone/tablet
createCertificate({
  call,
  say,
  onSignedIn: async (token) => {
    writeToken(token);
    await openApp();
  },
});

// ---- start: a token from earlier in this tab -> straight in, else the sign-in card
(async function start() {
  if (readToken()) {
    await openApp();
    return;
  }
  show('card-login');
  $('p-email').focus();
})();
