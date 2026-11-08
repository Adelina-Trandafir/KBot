// VC12-09: the standard VERCON header, using the same controls, styles and menus.
import { bindApplicationMenu } from '../vercon/application-menu.js';
import { bindHelp } from '../vercon/help.js';
import { messageHistory } from '../vercon/messages.js';
import { openMenu } from '../components/menu/menu.js';
import { bindMenuZoom } from '../components/menu/menu-zoom.js';
import { preview, scope } from '../vercon/ui.js';
import { $, combo, say } from './controls.js';

export function mountHeader({ api, onSubunit, onGestiune, onAccount, onTheme, navigate, signal }) {
  const owner = scope();
  let subgestiuni = [], accounts = [];
  const picks = {
    subunit: combo('vc-subunit-combo', onSubunit, { readonly: true, dropDownStyle: 'search', placeholder: 'Subunitate' }),
    gestiune: combo('vc-gestiune-combo', onGestiune, { readonly: true, dropDownStyle: 'search', placeholder: 'Gestiune' }),
    account: combo('vi-account', onAccount, { readonly: true, dropDownStyle: 'search', placeholder: 'Cont OIM' }),
  };
  for (const [name, pick] of Object.entries(picks)) pick.box.input.setAttribute('aria-label', { subunit: 'Gestiune', gestiune: 'Subgestiune', account: 'Cont OIM' }[name]);
  bindApplicationMenu($('vc-apps'), { context: () => $('vc-context').textContent, navigate, signal });
  bindMenuZoom($('vc-help'));
  const help = bindHelp({ api, report: error => say(error.message, 'error'), signal });
  $('vc-help').addEventListener('click', () => openMenu($('vc-help'), [
    { icon: 'ⓘ', label: 'Cum lucrez cu VERCON', run: help },
    { icon: '◷', label: 'Mesaje din sesiune', run: () => preview('Mesaje VERCON', [{ title: 'Mesaje',
      columns: [{ key: 'at', title: 'Data', width: 210 }, { key: 'level', title: 'Tip', width: 90 },
        { key: 'message', title: 'Mesaj', width: 600 }], rows: messageHistory() }], owner) },
  ]), { signal });
  $('vc-theme').addEventListener('click', () => {
    const theme = document.documentElement.dataset.theme === 'dark' ? 'light' : 'dark';
    document.documentElement.dataset.theme = theme;
    try { localStorage.setItem('kbot-portal-theme', theme); } catch { /* Keep this visit usable. */ }
    onTheme(theme === 'dark' ? 'dark' : 'modern');
  }, { signal });
  signal.addEventListener('abort', () => owner.clear(), { once: true });
  return {
    picks,
    identity(me) {
      const unit = me.units?.find(item => item.DC === me.db_name);
      $('vc-context').textContent = me.email || '';
      $('vc-subunit-combo').title = `Unitate: ${unit?.NumeUnitate || me.db_name} (${me.db_name})`;
    },
    paint(ctx, selection, list, account) {
      subgestiuni = ctx?.options.gestiuni || [];
      accounts = list;
      picks.subunit.set(ctx?.options.subunits || [], selection.subunit);
      picks.gestiune.set(subgestiuni, selection.gestiune);
      picks.account.set(accounts, account);
      $('vc-gestiune-combo').hidden = selection.subunit == null || subgestiuni.length <= 1;
      $('vi-account').hidden = accounts.length <= 1;
    },
    enable(on) {
      picks.subunit.enable(on);
      picks.gestiune.enable(on && !$('vc-gestiune-combo').hidden);
      picks.account.enable(on && accounts.length > 1);
    },
  };
}
