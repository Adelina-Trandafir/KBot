import { openMenu } from '../components/menu/menu.js';
import { tooltipSettingsMenuEntry } from '../components/tooltip/tooltip-settings.js';
import { searchSettingsMenuEntry } from './search-settings-dialog.js';
import { applicationAccountEntries } from '../portal/application-access.js';

let applicationMenu;

async function openApplications(anchor) {
  if (!applicationMenu || applicationMenu.signal?.aborted) return;
  const { context, navigate } = applicationMenu;
  const entries = await applicationAccountEntries(navigate);
  if (applicationMenu.signal?.aborted) return;
  openMenu(anchor, [
    { icon: '🏢', label: context() || 'Contul curent', disabled: true },
    ...entries.applications,
    ...(window.location.pathname === '/vercon-inventar' ? [{ label: 'VERCON', run: () => navigate('/vercon') }] : []),
    { label: 'Inventar Excel', run: () => navigate('/vercon-inventar') },
    { icon: '📋', label: 'ADECHIT', run: () => navigate('/adechit') },
    { icon: '📒', label: 'Avacont · în curând', disabled: true },
    ...entries.certificates,
    { ...searchSettingsMenuEntry(), rule: true },
    { ...tooltipSettingsMenuEntry(), rule: false },
  ], { label: 'Aplicații', below: anchor.closest('header') });
}

export function bindApplicationMenu(anchor, { context, navigate, signal }) {
  applicationMenu = { context, navigate, signal };
  anchor.addEventListener('click', () => openApplications(anchor), { signal });
}

export function reportApplicationButton() {
  const source = document.getElementById('vc-apps');
  if (!source) return null;
  const button = source.cloneNode(true);
  button.removeAttribute('id');
  button.setAttribute('aria-expanded', 'false');
  button.addEventListener('click', () => openApplications(button), { signal: applicationMenu?.signal });
  return button;
}
