/**
 * 🇬🇧 UK NINO VALIDATOR MIXIN
 * Mixin pentru validare UK National Insurance Number
 * Format: AB 12 34 56 C (2 litere, 6 cifre, 1 literă)
 * Metode cu prefix: uk_nino
 *
 * @version 3.0.0
 */

export const ukNinoValidatorMixin = {
  /**
   * Litere interzise pentru prima poziție
   */
  _forbidden_first_letters_uk_nino: ['D', 'F', 'I', 'Q', 'U', 'V'],

  /**
   * Litere interzise pentru a doua poziție
   */
  _forbidden_second_letters_uk_nino: ['D', 'F', 'I', 'O', 'Q', 'U', 'V'],

  /**
   * Prefixe interzise (combinații de 2 litere)
   */
  _forbidden_prefixes_uk_nino: ['BG', 'GB', 'NK', 'KN', 'TN', 'NT', 'ZZ'],

  /**
   * Litere valide pentru suffix (ultima poziție)
   */
  _valid_suffix_letters_uk_nino: ['A', 'B', 'C', 'D'],

  /**
   * Extrage litere și cifre (alfanumeric)
   * @param {string} str
   * @returns {string}
   */
  _extract_alphanumeric_uk_nino(str) {
    return str.replace(/[^A-Za-z0-9]/g, '').toUpperCase();
  },

  /**
   * Verifică dacă un string conține doar cifre
   * @param {string} str
   * @returns {boolean}
   */
  _is_numeric_uk_nino(str) {
    return /^\d+$/.test(str);
  },

  /**
   * Verifică dacă un string conține doar litere
   * @param {string} str
   * @returns {boolean}
   */
  _is_alpha_uk_nino(str) {
    return /^[A-Za-z]+$/.test(str);
  },

  /**
   * Parsează și validează un NINO
   * @param {string} nino - NINO cu sau fără spații
   * @returns {Object|null}
   */
  parse_uk_nino(nino) {
    // Extrage doar alfanumeric și convertește la uppercase
    const clean = this._extract_alphanumeric_uk_nino(nino);

    // Validare lungime
    if (clean.length !== 9) {
      this.log('❌ Lungime invalidă:', clean.length, '(trebuie 9)');
      return null;
    }

    // Extrage componentele
    const prefix = clean.substring(0, 2);
    const firstLetter = prefix[0];
    const secondLetter = prefix[1];
    const numbers = clean.substring(2, 8);
    const suffix = clean.substring(8, 9);

    // Validare: primele 2 trebuie să fie litere
    if (!this._is_alpha_uk_nino(prefix)) {
      this.log('❌ Primele 2 caractere nu sunt litere:', prefix);
      return null;
    }

    // Validare: caracterele 3-8 trebuie să fie cifre
    if (!this._is_numeric_uk_nino(numbers)) {
      this.log('❌ Caracterele 3-8 nu sunt cifre:', numbers);
      return null;
    }

    // Validare: ultimul caracter trebuie să fie literă
    if (!this._is_alpha_uk_nino(suffix)) {
      this.log('❌ Ultimul caracter nu este literă:', suffix);
      return null;
    }

    // Validare: prima literă nu poate fi din lista interzisă
    if (this._forbidden_first_letters_uk_nino.includes(firstLetter)) {
      this.log('❌ Prima literă este interzisă:', firstLetter);
      return null;
    }

    // Validare: a doua literă nu poate fi din lista interzisă
    if (this._forbidden_second_letters_uk_nino.includes(secondLetter)) {
      this.log('❌ A doua literă este interzisă:', secondLetter);
      return null;
    }

    // Validare: prefixul nu poate fi din lista interzisă
    if (this._forbidden_prefixes_uk_nino.includes(prefix)) {
      this.log('❌ Prefix interzis:', prefix);
      return null;
    }

    // Validare: suffix trebuie să fie A, B, C sau D
    if (!this._valid_suffix_letters_uk_nino.includes(suffix)) {
      this.log('❌ Suffix invalid:', suffix, '(trebuie A, B, C sau D)');
      return null;
    }

    const result = {
      valid: true,
      country: 'UK',
      value: clean,
      prefix,
      numbers,
      suffix,
      flagCode: 'gb',
      formatted: `${prefix} ${numbers.substring(0, 2)} ${numbers.substring(2, 4)} ${numbers.substring(4, 6)} ${suffix}`,
    };

    this.log('✅ NINO valid:', result);
    return result;
  },

  /**
   * Validare parțială pentru feedback în timpul tastării
   * Returnează true dacă input-ul poate deveni valid
   * @param {string} partial
   * @returns {boolean}
   */
  can_be_valid_uk_nino(partial) {
    const clean = this._extract_alphanumeric_uk_nino(partial);

    if (clean.length === 0) return true;
    if (clean.length > 9) return false;

    // Verifică primele 2 caractere (trebuie litere)
    if (clean.length >= 1) {
      if (!this._is_alpha_uk_nino(clean[0])) return false;
      if (this._forbidden_first_letters_uk_nino.includes(clean[0])) return false;
    }

    if (clean.length >= 2) {
      if (!this._is_alpha_uk_nino(clean[1])) return false;
      if (this._forbidden_second_letters_uk_nino.includes(clean[1])) return false;
      const prefix = clean.substring(0, 2);
      if (this._forbidden_prefixes_uk_nino.includes(prefix)) return false;
    }

    // Verifică caracterele 3-8 (trebuie cifre)
    if (clean.length >= 3 && clean.length <= 8) {
      const numberPart = clean.substring(2, Math.min(clean.length, 8));
      if (!this._is_numeric_uk_nino(numberPart)) return false;
    }

    // Verifică ultimul caracter (trebuie literă validă)
    if (clean.length === 9) {
      if (!this._valid_suffix_letters_uk_nino.includes(clean[8])) return false;
    }

    return true;
  },
};

export default ukNinoValidatorMixin;
