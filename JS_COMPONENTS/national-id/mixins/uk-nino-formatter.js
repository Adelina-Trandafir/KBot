/**
 * 🇬🇧 UK NINO FORMATTER MIXIN
 * Mixin pentru formatare UK National Insurance Number
 * Format: AB 12 34 56 C
 * Metode cu prefix: uk_nino
 *
 * @version 3.0.0
 */

export const ukNinoFormatterMixin = {
  // State pentru cursor management
  _cursor_pos_uk_nino: 0,
  _old_value_uk_nino: '',

  /**
   * Extrage doar caractere alfanumerice și convertește la uppercase
   * @param {string} value
   * @returns {string}
   */
  extract_uk_nino(value) {
    return value.replace(/[^A-Za-z0-9]/g, '').toUpperCase();
  },

  /**
   * Formatează NINO vizual: AB 12 34 56 C
   * @param {string} clean - Alfanumeric curat
   * @returns {string}
   */
  format_uk_nino(clean) {
    if (!clean || clean.length === 0) {
      return '';
    }

    // Salvează poziția curentă
    this._cursor_pos_uk_nino = this.inputElement.selectionStart || 0;
    this._old_value_uk_nino = this.inputElement.value;

    // Format: AB 12 34 56 C
    // Pozițiile: 0-1 | 2-3 | 4-5 | 6-7 | 8
    const parts = [
      clean.substring(0, 2), // AB (litere)
      clean.substring(2, 4), // 12 (cifre)
      clean.substring(4, 6), // 34 (cifre)
      clean.substring(6, 8), // 56 (cifre)
      clean.substring(8, 9), // C (literă)
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
  calculate_cursor_position_uk_nino(oldValue, newValue, oldCursorPos) {
    const oldSpaces = (oldValue.substring(0, oldCursorPos).match(/ /g) || []).length;
    const newSpaces = (newValue.substring(0, oldCursorPos).match(/ /g) || []).length;
    const newCursorPos = oldCursorPos + (newSpaces - oldSpaces);
    return Math.max(0, Math.min(newCursorPos, newValue.length));
  },

  /**
   * Aplică formatarea în input și setează cursorul corect
   * @param {string} clean
   */
  apply_format_uk_nino(clean) {
    const formatted = this.format_uk_nino(clean);
    const oldCursorPos = this.inputElement.selectionStart || 0;

    this.inputElement.value = formatted;

    const newCursorPos = this.calculate_cursor_position_uk_nino(
      this._old_value_uk_nino,
      formatted,
      oldCursorPos
    );

    this.inputElement.setSelectionRange(newCursorPos, newCursorPos);
    this.log('📝 Formatat:', { clean, formatted, cursorPos: newCursorPos });
  },

  /**
   * Validează caracterul introdus
   * @param {KeyboardEvent} e
   * @param {string} currentValue - Valoarea curentă din input
   * @returns {boolean} - true dacă caracterul e permis
   */
  validate_key_press_uk_nino(e, currentValue) {
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

    const clean = this.extract_uk_nino(currentValue);
    const currentLength = clean.length;

    // Limitează la 9 caractere
    if (currentLength >= 9) {
      this.log('⛔ Limită atinsă: 9 caractere');
      return false;
    }

    // Primele 2 caractere: doar litere
    if (currentLength < 2) {
      if (!/^[A-Za-z]$/.test(e.key)) {
        this.log('⛔ Primele 2 caractere trebuie să fie litere');
        return false;
      }
    }
    // Caracterele 3-8: doar cifre
    else if (currentLength >= 2 && currentLength < 8) {
      if (!/^\d$/.test(e.key)) {
        this.log('⛔ Caracterele 3-8 trebuie să fie cifre');
        return false;
      }
    }
    // Ultimul caracter (9): doar literă
    else if (currentLength === 8) {
      if (!/^[A-Za-z]$/.test(e.key)) {
        this.log('⛔ Ultimul caracter trebuie să fie literă');
        return false;
      }
    }

    return true;
  },

  /**
   * Handler pentru keydown (validare caracter)
   * @param {KeyboardEvent} e
   */
  handle_keydown_uk_nino(e) {
    if (!this.validate_key_press_uk_nino(e, this.inputElement.value)) {
      e.preventDefault();
    }
  },
};

export default ukNinoFormatterMixin;
