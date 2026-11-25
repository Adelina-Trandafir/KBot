import { subText } from './receipt-number.js';

// Suggestions are read-only. Saving the document rechecks its family's number under the context lock.
export function documentNumbers({ family, state, allocation, selection, api, aborted, controls, error, track, clearStatus }) {
  let sequence = 0, refused = '';
  return {
    invalidate() { ++sequence; refused = ''; },
    async check(offer = false) {
      const returnOffer = family === 'br' && state.mode === 'add' && offer;
      if (!(returnOffer || ['nir', 'storno', 'bc', 'bp', 'bt', 'bs'].includes(family)) || state.mode === 'view' || allocation?.multi()) return;
      const draft = state.draft, receipt = ['nir', 'storno'].includes(family);
      const day = draft.header.Data || '';
      if (returnOffer && !day) return;
      const nir = family === 'nir' && draft.tip !== 'STOC_INITIAL';
      // A blur of the empty number field (it fires while the new form is being drawn) must not cancel the automatic offer
      // that is already in flight: on a fresh draft an empty number is always answered with the next free one.
      if (state.mode === 'add' && draft.autoNumber && !draft.header.Numar) offer = true;
      const automatic = !nir && receipt && state.mode === 'add' && offer;
      if (automatic) draft.autoNumber = true;
      const currentNumber = draft.header.Numar || '', currentSub = Number(draft.header.NumarSub) || 0;
      const raw = automatic || returnOffer ? '' : currentNumber, sub = automatic ? 0 : currentSub, token = ++sequence;
      const isCurrent = () => !aborted() && draft === state.draft && token === sequence && draft.header.Data === day
        && String(draft.header.Numar || '') === String(currentNumber) && (Number(draft.header.NumarSub) || 0) === currentSub
        && (!automatic || draft.autoNumber);
      const context = state.mode === 'add' && allocation ? allocation.numberSelection() : selection();
      let answer;
      try {
        answer = await track(api(`/api/vercon/doc/${family}/number?${new URLSearchParams(context)}&number=${encodeURIComponent(raw)}&sub=${sub}&date=${encodeURIComponent(day)}${family === 'nir' ? `&initial=${draft.tip === 'STOC_INITIAL' ? '1' : '0'}` : ''}${family === 'bc' && draft.header.IdMasina ? `&account=${encodeURIComponent(draft.header.IdMasina)}` : ''}${state.mode === 'edit' ? `&exclude=${state.doc.id}` : ''}`, { headers: { 'X-Vercon-Version': '' } }));
      } catch (failure) {
        if (returnOffer && !isCurrent()) return;
        if (returnOffer && failure?.reason === 'NUMBER_CHRONOLOGY') {
          draft.header.Numar = '';
          controls().input?._clearValue();
          const sink = error(); if (sink) sink.textContent = failure.message;
        }
        // The page shows this refusal in its status line; remember it so a later successful check (new date or number) can take it down.
        if (failure?.name !== 'AbortError' && token === sequence) refused = failure?.message || '';
        throw failure;
      }
      if (!isCurrent()) return;
      if (refused) { clearStatus?.(refused); refused = ''; }
      const { input, subGroup, subBox, setLocked } = controls();
      if (receipt) {
        draft.nirNumberLocked = false;
        setLocked?.(false);
        if (!raw) {
          draft.header.Numar = String(answer.suggested);
          draft.header.NumarSub = Number(answer.suggested_sub) || 0;
          if (input?._box) input._box.setValue(draft.header.Numar); else if (input) input.value = draft.header.Numar;
          subBox?.setValue(subText(draft.header.NumarSub));
        }
        if (subGroup) subGroup.hidden = !draft.header.NumarSub;
      } else if (offer && !raw) {
        draft.header.Numar = String(answer.suggested);
        if (input?._box) input._box.setValue(draft.header.Numar); else if (input) input.value = draft.header.Numar;
      }
      if (receipt) {
        const sink = error(); if (sink) sink.textContent = '';
        if (input) input.title = '';
        return;
      }
      const custody = ['bc', 'bp', 'bt', 'bs'].includes(family);
      const message = !answer.valid ? custody ? 'Introduceți un număr întreg pozitiv.' : 'Introduceți un număr întreg pozitiv (și un sub-număr de cel mult 2 cifre).' : answer.duplicate ? 'Numărul este deja folosit. Alegeți alt număr.' : '';
      const sink = error(); if (sink) sink.textContent = message;
      if (input && !message) input.title = offer && !raw ? 'Propus automat. Verificat din nou la salvare.' : !raw ? `Următorul număr disponibil: ${answer.suggested}.` : answer.valid && !answer.duplicate ? 'Număr disponibil. Verificat din nou la salvare.' : `Număr disponibil propus: ${answer.suggested}.`;
    },
  };
}
