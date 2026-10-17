import { startSessionTimer } from '/static/js/adechit/session.js';
import { messageBox } from '/static/js/utils/confirm-box.js';
import { trackSessionWindow } from '/static/js/utils/session-exit.js';

const start = document.getElementById('start'), status = document.getElementById('status');
start.addEventListener('click', () => {
  start.disabled = true;
  status.textContent = 'Test în curs: mesaj la 02:00, avertizare serioasă la 01:00, închidere la 00:00.';
  const deadline = Date.now() + 180000;
  startSessionTimer({ token: () => '', host: document.getElementById('vc-session-timer'),
    warningSeconds: 120, criticalSeconds: 60, showFromSeconds: 180,
    checkSession: async () => ({ expires_in: Math.max(0, (deadline - Date.now()) / 1000) }),
    onExpired: () => window.location.replace('/session-ended'),
  });
});
document.getElementById('open-form').addEventListener('click', () => {
  const box = messageBox('Lăsați acest formular deschis până la expirarea sesiunii.', { title: 'Formular nesalvat', button: 'Închide formularul' });
  const dialog = [...document.querySelectorAll('dialog')].at(-1);
  const label = document.createElement('label'); label.className = 'session-demo__field'; label.textContent = 'Modificări nesalvate';
  const input = document.createElement('textarea'); input.rows = 3; input.value = 'Text de probă nesalvat'; label.append(input);
  dialog.querySelector('footer').before(label); input.focus();
  void box.closed;
});
document.getElementById('open-window').addEventListener('click', () => {
  const target = trackSessionWindow(window.open('', '_blank'));
  if (!target) { messageBox('Permiteți deschiderea ferestrei de probă în browser.', { title: 'Fereastră de probă' }); return; }
  target.document.title = 'Fereastră legată de sesiunea de probă';
  const heading = target.document.createElement('h1'); heading.textContent = 'Această fereastră se închide automat la zero.';
  const input = target.document.createElement('textarea'); input.value = 'Modificări nesalvate';
  target.document.body.append(heading, input);
});
document.getElementById('restart').addEventListener('click', () => window.location.reload());
