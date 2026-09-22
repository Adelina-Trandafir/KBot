/**
 * 🇷🇴 RO CNP FORMATTER MIXIN
 * Mixin pentru formatare CNP românesc
 * Format: S AA LL ZZ JJ NNN C (1 23 45 67 89 012 3)
 * Metode cu prefix: ro_cnp
 *
 * @version 3.0.0
 */

export const roCnpFormatterMixin = {
  // State pentru cursor management
  _cursor_pos_ro_cnp: 0,
  _old_value_ro_cnp: '',

  /**
   * Extrage doar cifrele din input
   * @param {string} value
   * @returns {string}
   */
  extract_ro_cnp(value) {
    return value.replace(/\D/g, '');
  },

  /**
   * Formatează CNP-ul vizual: 1 23 45 67 89 012 3
   * @param {string} digits - Doar cifre
   * @returns {string}
   */
  format_ro_cnp(digits) {
    if (!digits || digits.length === 0) {
      return '';
    }

    // Salvează poziția curentă
    this._cursor_pos_ro_cnp = this.inputElement.selectionStart || 0;
    this._old_value_ro_cnp = this.inputElement.value;

    // Format: S AA LL ZZ JJ NNN C
    // Pozițiile: 1 | 2-3 | 4-5 | 6-7 | 8-9 | 10-12 | 13
    const parts = [
      digits.substring(0, 1), // S
      digits.substring(1, 3), // AA
      digits.substring(3, 5), // LL
      digits.substring(5, 7), // ZZ
      digits.substring(7, 9), // JJ
      digits.substring(9, 12), // NNN
      digits.substring(12, 13), // C
    ];

    const formatted = parts.filter((p) => p).join(' ');

    return formatted;
  },

  /**
   * Calculează noua poziție a cursorului după formatare
   * @param {string} oldValue - Valoarea veche
   * @param {string} newValue - Valoarea nouă formatată
   * @param {number} oldCursorPos - Poziția veche a cursorului
   * @returns {number}
   */
  calculate_cursor_position_ro_cnp(oldValue, newValue, oldCursorPos) {
    const oldSpaces = (oldValue.substring(0, oldCursorPos).match(/ /g) || []).length;
    const newSpaces = (newValue.substring(0, oldCursorPos).match(/ /g) || []).length;
    const newCursorPos = oldCursorPos + (newSpaces - oldSpaces);
    return Math.max(0, Math.min(newCursorPos, newValue.length));
  },

  /**
   * Aplică formatarea în input și setează cursorul corect
   * @param {string} digits
   */
  apply_format_ro_cnp(digits) {
    const formatted = this.format_ro_cnp(digits);
    const oldCursorPos = this.inputElement.selectionStart || 0;

    this.inputElement.value = formatted;

    const newCursorPos = this.calculate_cursor_position_ro_cnp(
      this._old_value_ro_cnp,
      formatted,
      oldCursorPos
    );

    this.inputElement.setSelectionRange(newCursorPos, newCursorPos);
    this.log('📝 Formatat:', { digits, formatted, cursorPos: newCursorPos });
  },

  /**
   * Validează caracterul introdus (permite doar cifre)
   * @param {KeyboardEvent} e
   * @returns {boolean} - true dacă caracterul e permis
   */
  validate_key_press_ro_cnp(e) {
    // Permite: Backspace, Delete, Tab, Escape, Enter, Home, End, Arrow keys
    const allowedKeys = [
      'Backspace',
      'Delete',
      'Tab',
      'Escape',
      'Enter',
      'Home',
      'End',
      'ArrowLeft',
      'ArrowRight',
      'ArrowUp',
      'ArrowDown',
    ];

    if (allowedKeys.includes(e.key)) {
      return true;
    }

    // Permite: Ctrl+A, Ctrl+C, Ctrl+V, Ctrl+X
    if (e.ctrlKey || e.metaKey) {
      return true;
    }

    // Blochează tot ce nu e cifră
    if (!/^\d$/.test(e.key)) {
      this.log('⛔ Caracter blocat:', e.key);
      return false;
    }

    // Limitează la 13 cifre
    const currentDigits = this.extract_ro_cnp(this.inputElement.value);
    if (currentDigits.length >= 13) {
      this.log('⛔ Limită atinsă: 13 cifre');
      return false;
    }

    return true;
  },

  /**
   * Handler pentru keydown (validare caracter)
   * @param {KeyboardEvent} e
   */
  handle_keydown_ro_cnp(e) {
    if (!this.validate_key_press_ro_cnp(e)) {
      e.preventDefault();
    }
  },
};

export default roCnpFormatterMixin;
