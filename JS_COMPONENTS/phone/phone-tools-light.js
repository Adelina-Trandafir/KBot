/**
 * 📱 PHONE TOOLS LIGHT
 * Variantă simplificată pentru formatare + steag automat
 * - Auto-insert element steag în DOM
 * - Auto-detect țară din număr
 * - Formatare live fără validare complexă
 *
 * @version 1.0.0
 */

import { phonePatterns, euCountries } from './phoneTools-data.js';
import { Combobox } from '../combobox/combobox.js';

export class PhoneToolsLight {
  /**
   * @param {HTMLInputElement} inputElement - Input element pentru telefon
   * @param {Object} options - Opțiuni
   * @param {string} options.defaultCountry - Țară default (ex: 'RO')
   * @param {boolean} options.autoFormat - Formatare automată (default: true)
   * @param {boolean} options.autoDetect - Detectare automată țară (default: true)
   * @param {Function} options.onCountryDetected - Callback când țara e detectată
   * @param {boolean} options.debugMode - Activează logging
   */
  constructor(inputElement, options = {}) {
    if (!inputElement || !(inputElement instanceof HTMLInputElement)) {
      //|| !(inputElement instanceof HTMLInputElement)) {
      throw new Error('PhoneToolsLight: inputElement invalid');
    }

    this.inputElement = inputElement;
    this.defaultCountry = options.defaultCountry || 'RO';
    this.autoFormat = options.autoFormat !== false;
    this.autoDetect = options.autoDetect !== false;
    this.onCountryDetected = options.onCountryDetected || null;
    this.debugMode = options.debugMode || false;
    this.flagAlwaysDisabled = options.flagAlwaysDisabled || false;
    this.ListenerTracker = options.ListenerTracker || null;

    // Daca e furnizat ListenerTracker, folosește-l
    if (this.ListenerTracker) {
      this.ListenerTracker.applyTo(this, {
        debugMode: this.debugMode || false,
        logPrefix: 'PHONE_TOOLS_LIGHT',
        trackPerformance: false,
      });
    } else {
      // Fallback la addEventListener normal
      this.addDOMListener = (el, ev, fn) => el.addEventListener(ev, fn);
    }

    // State
    this.comboboxContainer = null;
    this.countryCombobox = null;
    this.currentCountry = null;
    this.flagElement = null;
    this.parentElement = null;

    // Bind methods
    this.handleInput = this.handleInput.bind(this);
    this.handlePaste = this.handlePaste.bind(this);
    this.handleBlur = this.handleBlur.bind(this);

    // Inițializare
    this.init();
  }

  // ==================== INITIALIZATION ====================

  /**
   * Inițializare completă
   */
  init() {
    this.log('🚀 PhoneToolsLight init...');

    // 0. Încarcă CSS-ul pentru steaguri
    this.loadFlagsCSS()
      .then(() => {
        // 1. Creează și inserează combobox-ul pentru țări
        this.createCountryCombobox();

        // 2. Detectează țara din valoarea existentă sau folosește default
        this.detectInitialCountry();

        // 3. Atașează event listeners
        this.attachListeners();

        this.log('✅ PhoneToolsLight initialized');
      })
      .catch((err) => {
        this.log('❌ Eroare la încărcarea CSS-ului, inițializare oprită', err);
      });
  }

  async loadFlagsCSS() {
    if (document.getElementById('sessionFlagsCSS')) {
      this.log('✅ CSS deja încărcat');
      return;
    }

    this.log('🎨 Încarc CSS-ul flags...');

    return new Promise((resolve, reject) => {
      const link = document.createElement('link');
      link.id = 'sessionFlagsCSS';
      link.rel = 'stylesheet';
      // Incarcă fișierul CSS pentru steaguri din folderul curent
      link.href = new URL('./phone_tools_light.css', import.meta.url).href + '?v=' + Date.now();

      link.onload = () => {
        this.log('✅ CSS încărcat cu succes');
        resolve();
      };

      link.onerror = () => {
        this.log.error('❌ Eroare la încărcarea CSS-ului');
        reject(new Error('Failed to load CSS'));
      };

      document.head.appendChild(link);
    });
  }

  /**
   * Creează și inserează combobox-ul pentru țări în DOM
   */
  createCountryCombobox() {
    // Găsește elementul părinte (div-ul form-group)
    this.parentElement = this.inputElement.parentElement;

    if (!this.parentElement) {
      this.log('❌ Parent element not found');
      return;
    }

    // Creează container pentru combobox
    this.comboboxContainer = document.createElement('div');
    this.comboboxContainer.className = 'phone-country-selector';

    // Găsește label-ul
    const label = this.parentElement.querySelector('label');

    if (label) {
      // Inserează DUPĂ label, ÎNAINTE de input
      label.insertAdjacentElement('afterend', this.comboboxContainer);
      this.log('✅ Combobox container inserted after label');
    } else {
      // Dacă nu există label, inserează la început
      this.parentElement.insertBefore(this.comboboxContainer, this.inputElement);
      this.log('✅ Combobox container inserted before input');
    }

    // Pregătește datele pentru combobox (cu HTML pentru steaguri)
    const countryData = euCountries.map((country) => ({
      value: country.code,
      label: `<span class="fi fi-${country.flag} country-flag"></span> ${country.name}`,
      data: country,
    }));

    // Creează combobox cu prefixIcon
    this.countryCombobox = new Combobox(this.comboboxContainer, {
      placeholder: '🌍',
      readonly: true,
      staticData: countryData,
      allowHtml: true,
      prefixIcon: true,
      showOnlyIcon: true,
      onSelect: (value, text, data) => {
        this.handleCountrySelect(value, text, data);
      },
    });

    if (this.flagAlwaysDisabled) this.countryCombobox.setEnabled(false);

    this.log('✅ Combobox creat cu steaguri');
  }

  /**
   * Creează și inserează elementul de steag în DOM
   */
  createFlagElement() {
    // Găsește elementul părinte (div-ul form-group)
    this.parentElement = this.inputElement.parentElement;

    if (!this.parentElement) {
      this.log('❌ Parent element not found');
      return;
    }

    // Creează container pentru steag
    const flagWrapper = document.createElement('div');
    flagWrapper.className = 'phone-flag-wrapper';

    // Creează elementul steag
    this.flagElement = document.createElement('span');
    this.flagElement.className = 'phone-flag-icon fi';

    flagWrapper.appendChild(this.flagElement);

    // Găsește label-ul
    const label = this.parentElement.querySelector('label');

    if (label) {
      // Inserează DUPĂ label, ÎNAINTE de input
      label.insertAdjacentElement('afterend', flagWrapper);
      this.log('✅ Flag element inserted after label');
    } else {
      // Dacă nu există label, inserează la început
      this.parentElement.insertBefore(flagWrapper, this.inputElement);
      this.log('✅ Flag element inserted before input');
    }

    // Ajustează styling pentru input (dacă e nevoie)
    this.adjustInputStyling();
  }

  /**
   * Ajustează styling-ul input-ului pentru a se potrivi cu steagul
   */
  adjustInputStyling() {
    // Verifică dacă input-ul are styling care ar putea fi stricat
    const computedStyle = window.getComputedStyle(this.inputElement);

    // Dacă input-ul e block, păstrează-l block
    if (computedStyle.display === 'block') {
      // Adaugă un mic padding-left pentru distanță față de steag
      if (!this.inputElement.style.paddingLeft) {
        this.inputElement.style.paddingLeft = '8px';
      }
    }

    this.log('✅ Input styling adjusted');
  }

  /**
   * Detectează țara inițială din valoarea input-ului
   */
  detectInitialCountry() {
    const currentValue = this.inputElement.value.trim();

    if (currentValue) {
      const country = this.detectCountryFromValue(currentValue);

      if (country) {
        this.setCountry(country);
        this.log(`🔍 Țară detectată din valoare: ${country.name}`);
        return;
      }
    }

    // Folosește țara default
    const defaultCountry = euCountries.find((c) => c.code === this.defaultCountry);
    if (defaultCountry) {
      this.setCountry(defaultCountry);
      this.log(`🏁 Țară default setată: ${defaultCountry.name}`);
    }
  }

  /**
   * Atașează event listeners
   */
  attachListeners() {
    this.addDOMListener(this.inputElement, 'input', this.handleInput);
    this.addDOMListener(this.inputElement, 'paste', this.handlePaste);
    this.addDOMListener(this.inputElement, 'blur', this.handleBlur);

    this.log('✅ Event listeners attached');
  }

  // ==================== EVENT HANDLERS ====================
  /**
   * Handler pentru selectarea țării din combobox
   */
  handleCountrySelect(value, text, data) {
    if (!data || !data.data) {
      this.log('❌ Date țară invalide din combobox');
      return;
    }

    this.isUpdatingFromCombobox = true;
    const country = data.data;

    this.log(`🌍 Țară selectată din combobox: ${country.name}`);

    // Setează țara (va actualiza și steagul)
    this.setCountry(country, true); // true = notifică

    // Resetează input-ul cu noul prefix
    this.inputElement.value = '';
    this.applyPhonePrefix(country.code);

    this.isUpdatingFromCombobox = false;
  }

  /**
   * Handler pentru input
   */
  handleInput(e) {
    if (!this.autoFormat) return;

    const value = e.target.value;

    // Detectează țara din valoare
    if (this.autoDetect) {
      this.detectAndUpdateCountry(value);
    }

    // Formatează numărul
    this.formatNumber(value);
  }

  /**
   * Handler pentru paste
   */
  handlePaste(e) {
    if (!this.autoFormat) return;

    setTimeout(() => {
      const value = this.inputElement.value;

      if (this.autoDetect) {
        this.detectAndUpdateCountry(value);
      }

      this.formatNumber(value);
    }, 10);
  }

  /**
   * Handler pentru blur - finalizează formatarea
   */
  handleBlur(e) {
    const value = e.target.value;

    if (this.autoDetect) {
      this.detectAndUpdateCountry(value);
    }

    this.formatNumber(value);
  }

  // ==================== CORE METHODS ====================
  /**
   * Aplică prefixul pentru țara curentă (fără formatare)
   */
  applyPhonePrefix(countryCode) {
    const pattern = phonePatterns[countryCode];

    if (!pattern) {
      this.log(`❌ Pattern lipsă pentru ${countryCode}`);
      return;
    }

    // Setează doar prefixul
    this.inputElement.value = `${pattern.dialCode} `;
    this.inputElement.focus();

    // Poziționează cursorul la sfârșit
    const prefixLength = pattern.dialCode.length + 1;
    this.inputElement.setSelectionRange(prefixLength, prefixLength);
  }

  /**
   * Detectează țara din valoare și actualizează steagul
   */
  detectAndUpdateCountry(value) {
    if (!value) return;

    const country = this.detectCountryFromValue(value);

    if (country && (!this.currentCountry || this.currentCountry.code !== country.code)) {
      this.setCountry(country);
      this.log(`🔄 Țară actualizată: ${country.name}`);

      // Callback
      if (this.onCountryDetected) {
        this.onCountryDetected(country);
      }
    }
  }

  /**
   * Detectează țara din valoarea unui număr
   * @param {string} value - Valoarea input-ului
   * @returns {Object|null} Country object sau null
   */
  detectCountryFromValue(value) {
    const digits = this.extractDigits(value);

    if (!digits) return null;

    // Încearcă să detecteze după dial code
    for (const country of euCountries) {
      const dialCodeDigits = country.dialCode.replace(/\+/g, '');

      // Verifică dacă începe cu dial code
      if (digits.startsWith(dialCodeDigits)) {
        return country;
      }
    }

    // Verifică pentru România - cazuri speciale
    if (value.startsWith('0') && digits.length >= 10) {
      return euCountries.find((c) => c.code === 'RO');
    }

    return null;
  }

  /**
   * Setează țara și actualizează steagul
   * @param {Object} country - Obiect country din euCountries
   */
  /**
   * Setează țara și actualizează steagul + combobox
   * @param {Object} country - Obiect country din euCountries
   * @param {boolean} notify - Dacă trebuie să notifice callback-ul
   */
  setCountry(country, notify = true) {
    const oldCountry = this.currentCountry;
    this.currentCountry = country;

    // Actualizează combobox-ul
    if (this.countryCombobox) {
      // Setează valoarea în combobox (doar numele țării, fără HTML)
      this.countryCombobox.setValue(country.code, country.name);

      // Setează steagul în prefix icon
      this.countryCombobox.setPrefixIcon(`<span class="fi fi-${country.flag}"></span>`);
    }

    this.log(`🏁 Țară setată: ${country.code} (${country.name})`);

    // Notifică callback doar dacă e cerut și țara s-a schimbat
    if (notify && oldCountry && oldCountry.code !== country.code) {
      if (this.onCountryChange) {
        this.onCountryChange(country);
      }
    }
  }

  /**
   * Formatează numărul în input
   * @param {string} value - Valoarea de formatat
   */
  formatNumber(value) {
    if (!this.currentCountry) return;

    const digits = this.extractDigits(value);

    if (!digits) {
      return;
    }

    const pattern = phonePatterns[this.currentCountry.code];

    if (!pattern) {
      this.log(`⚠️ Pattern lipsă pentru ${this.currentCountry.code}`);
      return;
    }

    // Extrage doar cifrele relevante (fără dial code)
    const dialCodeDigits = pattern.dialCode.replace(/\+/g, '');
    let cleanDigits = digits;

    // Dacă începe cu dial code, îl eliminăm
    if (cleanDigits.startsWith(dialCodeDigits)) {
      cleanDigits = cleanDigits.substring(dialCodeDigits.length);
    }

    // Pentru România - elimină 0 de la început
    if (this.currentCountry.code === 'RO' && cleanDigits.startsWith('0')) {
      cleanDigits = cleanDigits.substring(1);
    }

    // Limitează la max digits
    if (cleanDigits.length > pattern.maxDigits) {
      cleanDigits = cleanDigits.substring(0, pattern.maxDigits);
    }

    // Construiește numărul formatat
    let formatted = pattern.dialCode + ' ';
    let digitIndex = 0;

    // Aplică pattern-ul
    for (let i = 0; i < pattern.pattern.length && digitIndex < cleanDigits.length; i++) {
      if (pattern.pattern[i] === 'X') {
        formatted += cleanDigits[digitIndex];
        digitIndex++;
      } else {
        formatted += pattern.pattern[i];
      }
    }

    // Adaugă cifrele rămase (pentru țări cu lungimi variabile)
    while (digitIndex < cleanDigits.length) {
      formatted += cleanDigits[digitIndex];
      digitIndex++;
    }

    // Salvează poziția cursorului
    const cursorPos = this.inputElement.selectionStart;
    const oldLength = this.inputElement.value.length;

    // Setează valoarea formatată
    this.inputElement.value = formatted;

    // Recalculează poziția cursorului
    const newLength = formatted.length;
    const newCursorPos = cursorPos + (newLength - oldLength);
    this.inputElement.setSelectionRange(newCursorPos, newCursorPos);
  }

  // ==================== PUBLIC API ====================

  /**
   * Setează un număr de telefon
   *
   * @param {*} phoneNumber
   * @return {*}
   * @memberof PhoneToolsLight
   */
  setNumber(phoneNumber) {
    if (typeof phoneNumber !== 'string') {
      this.log('❌ setNumber: phoneNumber trebuie să fie string');
      return;
    }
    this.inputElement.value = phoneNumber.trim();

    // Detectează țara și formatează
    if (this.autoDetect) {
      this.detectAndUpdateCountry(this.inputElement.value);
    }

    if (this.autoFormat) {
      this.formatNumber(this.inputElement.value);
    }

    this.log(`🔢 Număr setat manual: ${this.inputElement.value}`);
  }

  /**
   * Obține numărul formatat curent
   * @returns {string|null}
   */
  getFormattedPhone() {
    const value = this.inputElement.value.trim();
    return value || null;
  }

  /**
   * Obține țara curentă
   * @returns {Object|null}
   */
  getCurrentCountry() {
    return this.currentCountry;
  }

  /**
   * Setează manual o țară
   * @param {string} countryCode - Cod țară (ex: 'DE')
   */
  setCountryByCode(countryCode) {
    const country = euCountries.find((c) => c.code === countryCode);

    if (country) {
      this.setCountry(country);
    } else {
      this.log(`❌ Țară necunoscută: ${countryCode}`);
    }
  }

  /**
   * Resetează la starea inițială
   */
  reset() {
    this.inputElement.value = '';
    this.detectInitialCountry();
    this.log('🔄 Reset');
  }

  /**
   * Distruge instanța și curăță DOM-ul
   */
  destroy() {
    // Șterge event listeners daca addDOMListener este cel standard
    // altfel gestionarea este facuta in evenBus-ul personalizat
    if (this.addDOMListener === ((el, ev, fn) => el.addEventListener(ev, fn))) {
      this.inputElement.removeEventListener('input', this.handleInput);
      this.inputElement.removeEventListener('paste', this.handlePaste);
      this.inputElement.removeEventListener('blur', this.handleBlur);
    }

    // Distruge combobox-ul
    if (this.countryCombobox) {
      this.countryCombobox.destroy();
      this.countryCombobox = null;
    }

    // Șterge containerul din DOM
    if (this.comboboxContainer && this.comboboxContainer.parentElement) {
      this.comboboxContainer.remove();
    }

    // Curăță referințele
    this.inputElement = null;
    this.comboboxContainer = null;
    this.parentElement = null;
    this.currentCountry = null;
    this.addDOMListener = null;

    this.log('🗑️ Destroyed');
  }

  // ==================== UTILS ====================

  /**
   * Extrage doar cifrele dintr-un string
   */
  extractDigits(str) {
    return str.replace(/\D/g, '');
  }

  setEnabled(enabled) {
    this.inputElement.disabled = !enabled;

    if (!this.flagAlwaysDisabled) this.countryCombobox.setEnabled(enabled);
    if (enabled) {
      this.parentElement.classList.remove('disabled');
    } else {
      this.parentElement.classList.add('disabled');
    }
  }
  /**
   * Logging helper
   */
  log = (() => {
    const fn = (message, data = null) => {
      if (this.debugMode) {
        const now = new Date();
        const ts = `${now.toLocaleTimeString('en-GB', { hour12: false })}.${now
          .getMilliseconds()
          .toString()
          .padStart(3, '0')}`;
        const CPN = 'PHONE_LIGHT'.padEnd(15);
        console.log(
          `%c[${ts}] [${CPN}] ${message}`,
          'color: #9b59b6; font-weight: bold;',
          data ?? ''
        );
      }
    };

    fn.error = (message, data = null) => {
      const now = new Date();
      const ts = `${now.toLocaleTimeString('en-GB', { hour12: false })}.${now
        .getMilliseconds()
        .toString()
        .padStart(3, '0')}`;
      const CPN = 'PHONE_LIGHT'.padEnd(15);
      console.error(
        `%c[${ts}] [${CPN}] ${message}`,
        'color: #e74c3c; font-weight: bold;',
        data ?? ''
      );
    };

    return fn;
  })();
}

export default PhoneToolsLight;
