// VERCON: the "Setari cautare" window of the application menu (the pause before a search starts while typing).
// It reuses the look of the tooltip settings window (same classes and stylesheet).
import { ensureTooltipStyle } from '../components/tooltip/tooltip.js';
import { SEARCH_DELAY_DEFAULT, SEARCH_DELAY_MIN, SEARCH_DELAY_MAX, SEARCH_MIN_CHARS, searchDelay, saveSearchDelay } from './search-settings.js';

let opened;
export function openSearchSettings() {
  if (opened?.isConnected) { opened.focus(); return; }
  ensureTooltipStyle();
  const origin = document.activeElement;
  const dialog = document.createElement('dialog');
  opened = dialog; dialog.className = 'kbot-tooltip-settings';
  dialog.setAttribute('aria-label', 'Setări căutare');
  const form = document.createElement('form');
  const heading = document.createElement('h2'); heading.className = 'kbot-tooltip-settings__header';
  heading.textContent = '🔍 Setări căutare';
  const body = document.createElement('div'); body.className = 'kbot-tooltip-settings__body';
  const label = document.createElement('label'); label.className = 'kbot-tooltip-settings__field';
  const caption = document.createElement('span'); caption.textContent = 'Pauza înainte de căutare (ms)';
  const input = document.createElement('input');
  input.type = 'number'; input.min = String(SEARCH_DELAY_MIN); input.max = String(SEARCH_DELAY_MAX); input.step = '50'; input.required = true;
  label.append(caption, input); body.append(label);
  const note = document.createElement('p'); note.className = 'kbot-tooltip-settings__note';
  note.textContent = `Valabilă pentru casetele de căutare din VERCON (cu excepția tabelelor). Căutarea pornește după ce ați tastat cel puțin ${SEARCH_MIN_CHARS} caractere (spațiile nu se numără) și o pauză în tastare. 1000 ms = 1 secundă. Se aplică în acest browser, la deschiderea următoarei pagini.`;
  body.append(note);
  const feedback = document.createElement('p'); feedback.className = 'kbot-tooltip-settings__note';
  feedback.setAttribute('role', 'status'); body.append(feedback);
  const footer = document.createElement('footer'); footer.className = 'kbot-tooltip-settings__footer';
  const button = (text, run, type = 'button') => {
    const item = document.createElement('button'); item.type = type; item.textContent = text;
    if (run) item.addEventListener('click', run);
    footer.append(item);
  };
  button('Implicit', () => { input.value = String(SEARCH_DELAY_DEFAULT); feedback.textContent = ''; });
  button('Renunță', () => dialog.close()); button('Salvează', null, 'submit');
  form.addEventListener('submit', (event) => {
    event.preventDefault();
    if (!form.reportValidity()) return;
    const { persisted } = saveSearchDelay(input.value);
    if (!persisted) { feedback.textContent = 'Setarea este aplicată pentru această pagină. Browserul nu permite memorarea ei; nu va fi păstrată după reîncărcare.'; return; }
    dialog.close();
  });
  dialog.addEventListener('close', () => {
    dialog.remove(); opened = null;
    if (origin?.isConnected) origin.focus({ preventScroll: true });
  }, { once: true });
  input.value = String(searchDelay());
  form.append(heading, body, footer); dialog.append(form);
  document.body.append(dialog); dialog.showModal(); input.focus();
}

export function searchSettingsMenuEntry() {
  return { icon: '🔍', label: 'Setări căutare', run: openSearchSettings };
}
