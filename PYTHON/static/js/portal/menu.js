// The horizontal menu of the portal header (next to the K-BOT brand). Built by hand, not from a
// library, so anything about it can be changed: the entries are plain data in MENU, the markup
// and the look (.pmenu* in portal.css) are ours.
//
// An entry is {key, label} for a button, or {key, label, items: [{key, label}]} for a button that
// opens a list. A click on a leaf fires `onPick(key)`; the page decides what each key does.

const MENU = [
  {
    key: 'nomenclatoare',
    label: 'Nomenclatoare',
    items: [
      { key: 'clasificatii', label: 'Clasificații' },
      { key: 'parteneri', label: 'Parteneri' },
    ],
  },
  { key: 'extrase', label: 'Extrase' },
];

export function createMenu(host, onPick) {
  let openKey = '';

  function closeAll() {
    openKey = '';
    host.querySelectorAll('.pmenu__top[aria-expanded="true"]').forEach((b) => b.setAttribute('aria-expanded', 'false'));
    host.querySelectorAll('.pmenu__list').forEach((l) => { l.hidden = true; });
  }

  function open(top, list, key) {
    closeAll();
    openKey = key;
    top.setAttribute('aria-expanded', 'true');
    list.hidden = false;
  }

  MENU.forEach((entry) => {
    const li = document.createElement('li');
    li.className = 'pmenu__item';
    const top = document.createElement('button');
    top.type = 'button';
    top.className = 'pmenu__top';
    top.textContent = entry.label;
    li.appendChild(top);

    if (entry.items) {
      top.setAttribute('aria-haspopup', 'menu');
      top.setAttribute('aria-expanded', 'false');
      top.insertAdjacentHTML('beforeend', ' <span class="pmenu__caret" aria-hidden="true">▾</span>');
      const list = document.createElement('ul');
      list.className = 'pmenu__list card';
      list.setAttribute('role', 'menu');
      list.hidden = true;
      entry.items.forEach((sub) => {
        const sli = document.createElement('li');
        sli.setAttribute('role', 'none');
        const b = document.createElement('button');
        b.type = 'button';
        b.className = 'pmenu__sub';
        b.setAttribute('role', 'menuitem');
        b.textContent = sub.label;
        b.addEventListener('click', () => { closeAll(); onPick(sub.key, sub.label); });
        sli.appendChild(b);
        list.appendChild(sli);
      });
      top.addEventListener('click', () => (openKey === entry.key ? closeAll() : open(top, list, entry.key)));
      li.appendChild(list);
    } else {
      top.addEventListener('click', () => { closeAll(); onPick(entry.key, entry.label); });
    }
    host.appendChild(li);
  });

  // a click anywhere else, or Escape, closes the open list
  document.addEventListener('click', (e) => { if (!host.contains(e.target)) closeAll(); });
  document.addEventListener('keydown', (e) => { if (e.key === 'Escape') closeAll(); });
  return { closeAll };
}
