/** Shared dialog buttons (SLICE-VC21-01). Load dialog-button.css alongside this module.
 * new DialogButton(containerOrNull, { kind, label, title, disabled, className, onClick }).
 *   kind 'ok1'    main action (save / confirm)            default label: Save (Romanian, see KINDS)
 *   kind 'cancel' give up / close the page                default label: Cancel (Romanian, see KINDS)
 *   kind 'ok2'    extra action, looks different from OK1  label is required
 * The label is configurable at any time with setLabel(). The button is available as .element;
 * a null container leaves it detached. An unknown kind or a missing OK2 label throws.
 * dialogButtonRow(container) makes the .k-dialog-buttons row (cancel left, ok2, ok1 right).
 */
const KINDS = {
  ok1: { className: 'k-dialog-button--ok1', defaultLabel: 'Salvează' },
  cancel: { className: 'k-dialog-button--cancel', defaultLabel: 'Renunță' },
  ok2: { className: 'k-dialog-button--ok2', defaultLabel: '' },
};

export class DialogButton {
  constructor(container, options = {}) {
    const kind = KINDS[options.kind];
    if (!kind) throw new Error(`DialogButton: unknown kind «${options.kind}» (ok1, cancel, ok2)`);
    this.kind = options.kind;
    this.options = { label: kind.defaultLabel, title: '', disabled: false, ...options };
    if (!String(this.options.label ?? '').trim()) throw new Error(`DialogButton: kind «${this.kind}» needs a label`);
    this._abort = new AbortController();
    this.element = document.createElement('button');
    this.element.type = 'button';
    this.element.className = `k-dialog-button ${kind.className}`;
    for (const name of (options.className || '').split(/\s+/).filter(Boolean)) this.element.classList.add(name);
    this.setLabel(this.options.label);
    this.setEnabled(!this.options.disabled);
    this.element.addEventListener('click', (event) => this.options.onClick?.(event, this), { signal: this._abort.signal });
    container?.append(this.element);
  }

  setLabel(label) {
    const text = String(label ?? '').trim();
    if (!text) throw new Error(`DialogButton: kind «${this.kind}» needs a label`);
    this.options.label = text;
    this.element.textContent = text;
    this.element.title = this.options.title || '';
  }

  setEnabled(enabled) {
    this.options.disabled = !enabled;
    this.element.disabled = !enabled;
  }

  destroy() {
    this._abort.abort();
    this.element.remove();
  }
}

export function dialogButtonRow(container, className = '') {
  const row = document.createElement('div');
  row.className = `k-dialog-buttons ${className}`.trim();
  container?.append(row);
  return row;
}
