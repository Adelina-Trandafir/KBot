/**
 * 📱 PHONE TOOLS - MAIN CLASS
 * Generic ES6 module for phone input formatting and validation
 *
 * @version 1.0.0
 */

import { phonePatterns, euCountries } from './phoneTools-data.js';

export class PhoneTools {
  /**
   * @param {HTMLInputElement} inputElement - Input element pentru telefon
   * @param {Object} options - Opțiuni
   * @param {string} options.countryCode - Cod țară default (ex: 'RO')
   * @param {Object} options.eventBus - EventBus pentru notificări
   * @param {Function} options.onValidation - Callback la validare (result) => {}
   * @param {Function} options.onCountryChange - Callback la schimbare țară (country) => {}
   * @param {Function} options.onComplete - Callback când număr complet (phone) => {}
   * @param {boolean} options.debugMode - Activează logging (default: false)
   */
  constructor(inputElement, options = {}) {
    // Validare input element
    if (!inputElement || !(inputElement instanceof HTMLInputElement)) {
      throw new Error('PhoneTools: inputElement trebuie să fie un HTMLInputElement valid');
    }

    this.inputElement = inputElement;
    this.currentCountryCode = options.countryCode || 'RO';
    this.eventBus = options.eventBus || null;
    this.debugMode = options.debugMode || false;

    // Callbacks
    this.onValidation = options.onValidation || null;
    this.onCountryChange = options.onCountryChange || null;
    this.onComplete = options.onComplete || null;

    // Bind methods pentru a putea fi folosite ca event handlers
    this.handleKeydown = this.handleKeydown.bind(this);
    this.handleInput = this.handleInput.bind(this);
    this.handlePaste = this.handlePaste.bind(this);
    this.handleClick = this.handleClick.bind(this);
    this.handleFocus = this.handleFocus.bind(this);
    this.handleUpdate = this.handleUpdate.bind(this);

    // Inițializare
    this.applyMask(this.currentCountryCode);

    this.log('✅ PhoneTools initialized', {
      country: this.currentCountryCode,
      hasEventBus: !!this.eventBus,
    });
  }

  // ==================== EVENT HANDLERS ====================

  /**
   * Handler pentru update - ruleaza la schimbare valorii containerului
   * @param {KeyboardEvent} e
   */
  handleUpdate(e) {
    this.log('Change event detected');
  }

  /**
   * Handler pentru keydown - blochează modificarea prefixului
   * @param {KeyboardEvent} e
   */
  handleKeydown(e) {
    const prefix = this.inputElement.dataset.prefix;
    const prefixLength = prefix.length + 1; // +1 pentru spațiu
    const cursorPos = this.inputElement.selectionStart;

    // Blochează modificarea prefixului
    if (cursorPos < prefixLength) {
      if (!['ArrowLeft', 'ArrowRight', 'Home', 'End', 'Tab', 'Enter'].includes(e.key)) {
        e.preventDefault();

        if (e.key === 'Backspace' || e.key === 'Delete') {
          this.inputElement.setSelectionRange(prefixLength, prefixLength);
        }

        if (/^\d$/.test(e.key)) {
          const currentDigits = this.extractDigits(this.inputElement.value.substring(prefixLength));
          const maxDigits = parseInt(this.inputElement.dataset.maxDigits);

          if (currentDigits.length < maxDigits) {
            this.inputElement.value = prefix + ' ' + e.key;
            this.formatPhoneNumber();
          }
        }
      }
      return;
    }

    // Permite doar cifre și taste speciale
    if (
      !/^\d$/.test(e.key) &&
      !['Backspace', 'Delete', 'ArrowLeft', 'ArrowRight', 'Home', 'End', 'Tab', 'Enter'].includes(
        e.key
      )
    ) {
      e.preventDefault();
    }

    // Verifică limita de cifre
    if (/^\d$/.test(e.key)) {
      const currentDigits = this.extractDigits(this.inputElement.value.substring(prefixLength));
      const maxDigits = parseInt(this.inputElement.dataset.maxDigits);

      if (currentDigits.length >= maxDigits) {
        e.preventDefault();
      }
    }
  }

  /**
   * Handler pentru input - formatează numărul
   * @param {Event} e
   */
  handleInput(e) {
    this.formatPhoneNumber();
    this._validateAndNotify();
  }

  /**
   * Handler pentru click - protejează prefixul
   * @param {MouseEvent} e
   */
  handleClick(e) {
    const prefix = this.inputElement.dataset.prefix;
    const prefixLength = prefix.length + 1;

    if (this.inputElement.selectionStart < prefixLength) {
      this.inputElement.setSelectionRange(prefixLength, prefixLength);
    }
  }

  /**
   * Handler pentru focus - poziționează cursorul
   * @param {FocusEvent} e
   */
  handleFocus(e) {
    const prefix = this.inputElement.dataset.prefix;
    const prefixLength = prefix.length + 1;

    if (this.inputElement.value === prefix || this.inputElement.value === prefix + ' ') {
      this.inputElement.setSelectionRange(prefixLength, prefixLength);
    }
  }

  /**
   * Handler pentru paste - cu detectare țară și formatare
   * @param {ClipboardEvent} e
   */
  handlePaste(e) {
    e.preventDefault();

    const pastedText = (e.clipboardData || window.clipboardData).getData('text');
    const digits = this.extractDigits(pastedText);

    this.log(`📋 Paste: ${pastedText} → Cifre: ${digits}`);

    if (!digits) {
      this.log('⚠️ Nu conține cifre valide');
      return;
    }

    // Încearcă să detecteze țara din număr
    const detectedCountry = this.getCountryFromPhone(digits);

    if (detectedCountry) {
      this.log(`🔍 Țară detectată: ${detectedCountry.name}`);
      this.setCountry(detectedCountry.code);

      // Extrage cifrele fără prefix
      let cleanDigits = digits;
      const dialCodeDigits = detectedCountry.dialCode.replace(/\+/g, '');
      if (cleanDigits.startsWith(dialCodeDigits)) {
        cleanDigits = cleanDigits.substring(dialCodeDigits.length);
      }

      const pattern = phonePatterns[detectedCountry.code];
      this.inputElement.value = pattern.dialCode + ' ' + cleanDigits;
      this.formatPhoneNumber();
    } else {
      this.log('⚠️ Nu pot detecta țara, folosesc țara curentă');

      const pattern = phonePatterns[this.currentCountryCode];
      const dialCodeDigits = pattern.dialCode.replace(/\+/g, '');

      let cleanDigits = digits;
      if (cleanDigits.startsWith(dialCodeDigits)) {
        cleanDigits = cleanDigits.substring(dialCodeDigits.length);
      }

      this.inputElement.value = pattern.dialCode + ' ' + cleanDigits;
      this.formatPhoneNumber();
    }

    this._validateAndNotify();
  }

  // ==================== CORE METHODS ====================

  /**
   * Setează țara și aplică mask-ul corespunzător
   * @param {string} countryCode - Cod țară (ex: 'RO')
   */
  setCountry(countryCode) {
    if (!phonePatterns[countryCode]) {
      this.log(`❌ Pattern lipsă pentru ${countryCode}`);
      return;
    }

    const oldCountry = this.currentCountryCode;
    this.currentCountryCode = countryCode;

    this.applyMask(countryCode);

    // Notificare schimbare țară
    if (oldCountry !== countryCode) {
      const country = euCountries.find((c) => c.code === countryCode);
      this._notifyCountryChange(country);
    }

    this.log(`✅ Țară setată: ${countryCode}`);
  }

  /**
   * Aplică mask-ul pentru țara selectată
   * @param {string} countryCode - Cod țară
   */
  applyMask(countryCode) {
    const pattern = phonePatterns[countryCode];

    if (!pattern) {
      this.log(`❌ Pattern lipsă pentru ${countryCode}`);
      return;
    }

    // Setează prefixul + un spațiu
    this.inputElement.value = `${pattern.dialCode} `;
    this.inputElement.dataset.prefix = pattern.dialCode;
    this.inputElement.dataset.pattern = pattern.pattern;
    this.inputElement.dataset.minDigits = pattern.minDigits;
    this.inputElement.dataset.maxDigits = pattern.maxDigits;

    // Poziționează cursorul după prefix
    const prefixLength = pattern.dialCode.length + 1;
    this.inputElement.setSelectionRange(prefixLength, prefixLength);

    this.log(
      `✅ Mask aplicat pentru ${countryCode}: ${pattern.dialCode} (${pattern.minDigits}-${pattern.maxDigits} cifre)`
    );
  }

  /**
   * Formatează numărul de telefon conform pattern-ului
   */
  formatPhoneNumber() {
    const prefix = this.inputElement.dataset.prefix;
    const pattern = this.inputElement.dataset.pattern;
    const prefixLength = prefix.length + 1;

    // Salvează poziția cursorului
    let cursorPos = this.inputElement.selectionStart;

    // Extrage doar cifrele (fără prefix)
    const allText = this.inputElement.value;
    const digitsOnly = this.extractDigits(allText.substring(prefixLength));

    if (!digitsOnly) {
      this.inputElement.value = prefix + ' ';
      this.inputElement.setSelectionRange(prefixLength, prefixLength);
      return;
    }

    // Aplică formatare cu spații conform pattern-ului
    let formatted = '';
    let digitIndex = 0;
    const maxDigits = parseInt(this.inputElement.dataset.maxDigits);

    for (
      let i = 0;
      i < pattern.length && digitIndex < digitsOnly.length && digitIndex < maxDigits;
      i++
    ) {
      if (pattern[i] === 'X') {
        formatted += digitsOnly[digitIndex];
        digitIndex++;
      } else {
        formatted += pattern[i];
      }
    }

    // Dacă mai sunt cifre rămase, le adăugăm
    while (digitIndex < digitsOnly.length && digitIndex < maxDigits) {
      formatted += digitsOnly[digitIndex];
      digitIndex++;
    }

    // Setează valoarea formatată
    this.inputElement.value = prefix + ' ' + formatted;

    // Ajustează poziția cursorului
    const oldSpaces = (allText.substring(0, cursorPos).match(/ /g) || []).length;
    const newSpaces = (this.inputElement.value.substring(0, cursorPos).match(/ /g) || []).length;

    if (newSpaces > oldSpaces) {
      cursorPos += newSpaces - oldSpaces;
    }

    if (cursorPos < prefixLength) {
      cursorPos = prefixLength;
    }

    this.inputElement.setSelectionRange(cursorPos, cursorPos);
  }

  // ==================== VALIDATION ====================

  /**
   * Validează numărul de telefon
   * @param {string} phone - Numărul de telefon (opțional, folosește input-ul dacă lipsește)
   * @param {string} countryCode - Codul țării (opțional, folosește țara curentă)
   * @returns {Object} { isValid, formatted, error, digitCount }
   */
  validate(phone = null, countryCode = null) {
    const phoneToValidate = phone || this.getFormattedPhone();
    const country = countryCode || this.currentCountryCode;

    if (!phoneToValidate) {
      return {
        isValid: false,
        formatted: '',
        error: 'Număr de telefon lipsă',
        digitCount: 0,
      };
    }

    const digits = this.extractDigits(phoneToValidate);
    const pattern = phonePatterns[country];

    if (!pattern) {
      return {
        isValid: false,
        formatted: phoneToValidate,
        error: 'Țară necunoscută',
        digitCount: digits.length,
      };
    }

    // Verifică lungimea
    const dialCodeDigits = pattern.dialCode.replace(/\+/g, '').length;
    const subscriberDigits = digits.length - dialCodeDigits;

    if (subscriberDigits < pattern.minDigits) {
      return {
        isValid: false,
        formatted: phoneToValidate,
        error: `Numărul trebuie să aibă minim ${pattern.minDigits} cifre`,
        digitCount: subscriberDigits,
      };
    }

    if (subscriberDigits > pattern.maxDigits) {
      return {
        isValid: false,
        formatted: phoneToValidate,
        error: `Numărul trebuie să aibă maxim ${pattern.maxDigits} cifre`,
        digitCount: subscriberDigits,
      };
    }

    // Valid!
    return {
      isValid: true,
      formatted: phoneToValidate,
      error: '',
      digitCount: subscriberDigits,
    };
  }

  /**
   * Verifică dacă numărul curent este valid și complet
   * @returns {boolean}
   */
  isValid() {
    const result = this.validate();
    return result.isValid;
  }

  // ==================== GETTERS ====================

  /**
   * Obține numărul de telefon complet formatat
   * @returns {string|null} Număr formatat (ex: +40723456789) sau null
   */
  getFormattedPhone() {
    const prefix = this.inputElement.dataset.prefix;
    const prefixLength = prefix.length + 1;

    const digits = this.extractDigits(this.inputElement.value.substring(prefixLength));

    if (!digits) {
      return null;
    }

    return prefix + digits;
  }

  /**
   * Detectează țara din cifrele unui număr
   * @param {string} phoneOrDigits - Număr de telefon sau cifre
   * @returns {Object|null} Obiect country sau null
   */
  getCountryFromPhone(phoneOrDigits) {
    const digits = this.extractDigits(phoneOrDigits);

    // Verifică fiecare țară
    for (const country of euCountries) {
      const dialCodeDigits = country.dialCode.replace(/\+/g, '');

      if (digits.startsWith(dialCodeDigits)) {
        return country;
      }
    }

    return null;
  }

  // ==================== UI HELPERS ====================

  /**
   * Setează steagul pentru un număr de telefon
   * @param {string} phone - Număr de telefon
   * @param {HTMLElement} iconElement - Element unde se va afișa steagul
   */
  setFlagForPhone(phone, iconElement) {
    if (!iconElement) {
      this.log('❌ iconElement lipsește');
      return;
    }

    const country = this.getCountryFromPhone(phone);

    if (country) {
      iconElement.innerHTML = `<span class="fi fi-${country.flag}"></span>`;
      this.log(`🏁 Steag setat: ${country.flag}`);
    } else {
      iconElement.innerHTML = '';
      this.log('⚠️ Nu s-a găsit țara pentru steag');
    }
  }

  // ==================== UTILS ====================

  /**
   * Extrage doar cifrele dintr-un string
   * @param {string} str
   * @returns {string}
   */
  extractDigits(str) {
    return str.replace(/\D/g, '');
  }

  /**
   * Resetează input-ul la starea inițială
   */
  reset() {
    this.applyMask(this.currentCountryCode);
    this.log('🔄 PhoneTools resetat');
  }

  /**
   * Curăță și distruge instanța
   */
  destroy() {
    // Șterge dataset-urile din input
    delete this.inputElement.dataset.prefix;
    delete this.inputElement.dataset.pattern;
    delete this.inputElement.dataset.minDigits;
    delete this.inputElement.dataset.maxDigits;

    // Curăță input
    this.inputElement.value = '';

    // Șterge referințele
    this.inputElement = null;
    this.eventBus = null;
    this.onValidation = null;
    this.onCountryChange = null;
    this.onComplete = null;

    this.log('🗑️ PhoneTools destroyed');
  }

  // ==================== PRIVATE METHODS ====================

  /**
   * Validează și notifică prin callbacks și eventBus
   * @private
   */
  _validateAndNotify() {
    const result = this.validate();

    // Callback validare
    this._notifyValidation(result);

    // Callback complet (doar dacă e valid)
    if (result.isValid && this.onComplete) {
      this.onComplete(result.formatted);
    }
  }

  /**
   * Notifică rezultatul validării
   * @private
   */
  _notifyValidation(result) {
    // Callback
    if (this.onValidation) {
      this.onValidation(result);
    }

    // EventBus
    if (this.eventBus) {
      this.eventBus.emit('PHONE_VALIDATION', {
        result,
        countryCode: this.currentCountryCode,
      });
    }
  }

  /**
   * Notifică schimbarea țării
   * @private
   */
  _notifyCountryChange(country) {
    // Callback
    if (this.onCountryChange) {
      this.onCountryChange(country);
    }

    // EventBus
    if (this.eventBus) {
      this.eventBus.emit('PHONE_COUNTRY_CHANGE', {
        country,
        countryCode: country.code,
      });
    }

    this.log(`📢 Notificare schimbare țară: ${country.name}`);
  }

  // ==================== LOGGING ====================

  /**
   * Logging helper
   * @private
   */
  log(message, data = null) {
    if (this.debugMode) {
      const now = new Date();
      const ts = `${now.toLocaleTimeString('en-GB', { hour12: false })}.${now
        .getMilliseconds()
        .toString()
        .padStart(3, '0')}`;
      console.log(
        `%c[${ts}] [PHONE_TOOLS] ${message}`,
        'color: #3b82f6; font-weight: bold;',
        data ?? ''
      );
    }
  }
}

export default PhoneTools;
