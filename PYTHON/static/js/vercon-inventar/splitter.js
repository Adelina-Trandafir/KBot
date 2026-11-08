// VC12-08: page-local splitter. Only the panel-size CSS variable is changed.
import { applicationScale } from '../utils/layout-geometry.js';

export function mountSplitter({ host, handle, bottom, signal }) {
  let share = null, drag = null;
  function bounds() {
    const style = getComputedStyle(host);
    const min = parseFloat(style.getPropertyValue('--vi-assigned-min')) || 120;
    const topMin = parseFloat(style.getPropertyValue('--vi-work-min')) || 200;
    const available = host.clientHeight - handle.offsetHeight - 2 * (parseFloat(style.rowGap) || 0);
    return { min, max: Math.max(min, available - topMin), available };
  }
  function aria() {
    const { min, max } = bounds();
    handle.setAttribute('aria-valuemin', String(Math.round(min)));
    handle.setAttribute('aria-valuemax', String(Math.round(max)));
    handle.setAttribute('aria-valuenow', String(Math.round(bottom.offsetHeight)));
    handle.setAttribute('aria-valuetext', `${Math.round(bottom.offsetHeight)} pixeli pentru lista de asocieri`);
    handle.setAttribute('aria-disabled', String(max <= min));
  }
  function resize(height) {
    const { min, max, available } = bounds();
    const value = Math.max(min, Math.min(max, height));
    share = available > 0 ? value / available : null;
    host.style.setProperty('--vi-assigned-height', `${value}px`);
    aria();
  }
  function stop(event) {
    if (drag && event?.pointerId != null && event.pointerId !== drag.id) return;
    const pointer = drag?.id;
    drag = null; handle.classList.remove('is-dragging');
    if (pointer != null && handle.hasPointerCapture(pointer)) handle.releasePointerCapture(pointer);
  }
  handle.addEventListener('pointerdown', event => {
    if (drag || event.button !== 0 || bounds().max <= bounds().min) return;
    event.preventDefault(); handle.focus();
    drag = { id: event.pointerId, y: event.clientY, height: bottom.offsetHeight, scale: applicationScale() };
    handle.setPointerCapture(event.pointerId); handle.classList.add('is-dragging');
  }, { signal });
  handle.addEventListener('pointermove', event => {
    if (!drag || drag.id !== event.pointerId) return;
    resize(drag.height - (event.clientY - drag.y) / drag.scale);
  }, { signal });
  for (const name of ['pointerup', 'pointercancel', 'lostpointercapture']) handle.addEventListener(name, stop, { signal });
  function reset() { share = null; host.style.removeProperty('--vi-assigned-height'); aria(); }
  handle.addEventListener('dblclick', reset, { signal });
  handle.addEventListener('keydown', event => {
    if (!['ArrowUp', 'ArrowDown', 'Home', 'End', 'Enter'].includes(event.key)) return;
    event.preventDefault();
    if (event.key === 'Enter') { reset(); return; }
    const { min, max } = bounds(), step = event.shiftKey ? 50 : 20;
    resize(event.key === 'Home' ? min : event.key === 'End' ? max : bottom.offsetHeight + (event.key === 'ArrowUp' ? step : -step));
  }, { signal });
  const observer = new ResizeObserver(() => {
    if (share !== null) resize(share * bounds().available); else aria();
  });
  observer.observe(host); aria();
  signal.addEventListener('abort', () => { stop(); observer.disconnect(); }, { once: true });
}
