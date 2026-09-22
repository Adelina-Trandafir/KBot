/**
 * 📡 PUBLIC API MIXIN
 * API public pentru interacțiune externă
 */

export const publicApiMixin = {
  /**
   * Validare cu auto-detect sau țară specificată
   * @param {string} value - ID de validat (opțional, ia din input dacă lipsește)
   * @param {string} country - Cod țară (opțional, auto-detect dacă lipsește)
   * @returns {Object|null}
   */
  async validate(value = null, country = null) {
    const valueToValidate = value || this.inputElement.value;
    const countryToUse = country || this.currentCountry || this.detectCountry(valueToValidate);

    if (!countryToUse) {
      this.log.error('❌ Nu s-a putut detecta țara pentru validare');
      return null;
    }

    // Încarcă mixinurile dacă nu sunt deja
    await this._loadMixinsForCountry(countryToUse);

    // Extrage valoarea curată
    const extractMethodName = `extract_${countryToUse.toLowerCase()}_${
      this.formatterRegistry.get(countryToUse).mixinName.split('_')[1]
    }`;
    const cleanValue =
      typeof this[extractMethodName] === 'function'
        ? this[extractMethodName](valueToValidate)
        : valueToValidate;

    // Validează
    const parseMethodName = `parse_${countryToUse.toLowerCase()}_${
      this.validatorRegistry.get(countryToUse).mixinName.split('_')[1]
    }`;

    if (typeof this[parseMethodName] === 'function') {
      return this[parseMethodName](cleanValue);
    }

    this.log.error(`❌ Metoda ${parseMethodName} nu există`);
    return null;
  },

  /**
   * Verifică dacă ID-ul curent este valid
   */
  async isValid() {
    const result = await this.validate();
    return result && result.valid;
  },

  /**
   * Setează un ID național
   * @param {string} value
   * @param {string} country - Cod țară (RO, UK, etc.) - OPȚIONAL (auto-detect)
   */
  async setNationalId(value, country = null) {
    const countryToUse = country || this.detectCountry(value);

    if (!countryToUse) {
      this.log.error('❌ Nu s-a putut detecta țara');
      return;
    }

    if (!this.validatorRegistry.has(countryToUse)) {
      this.log.error(`❌ Țara ${countryToUse} nu este suportată`);
      return;
    }

    this.currentCountry = countryToUse;
    await this._loadMixinsForCountry(countryToUse);

    this.inputElement.value = value;
    await this.processInput(value);
  },

  /**
   * Obține ID-ul curent
   * @returns {Object} - { value: string, country: string }
   */
  getNationalId() {
    if (!this.currentCountry) {
      return { value: this.inputElement.value, country: null };
    }

    const extractMethodName = `extract_${this.currentCountry.toLowerCase()}_${
      this.formatterRegistry.get(this.currentCountry).mixinName.split('_')[1]
    }`;
    const cleanValue =
      typeof this[extractMethodName] === 'function'
        ? this[extractMethodName](this.inputElement.value)
        : this.inputElement.value;

    return {
      value: cleanValue,
      country: this.currentCountry,
    };
  },

  /**
   * Obține datele complete ID
   */
  getNationalIdData() {
    return this.currentIdData;
  },

  /**
   * Resetează la starea inițială
   */
  reset() {
    this.inputElement.value = '';
    this.hideFlag();
    this.hideError();
    this.currentIdData = null;
    this.currentCountry = null;
    this.log('🔄 Reset');
  },

  /**
   * Activează/dezactivează input-ul
   */
  setEnabled(enabled) {
    this.inputElement.disabled = !enabled;

    if (enabled) {
      this.parentElement?.classList.remove('disabled');
    } else {
      this.parentElement?.classList.add('disabled');
    }
  },

  /**
   * Distruge instanța
   */
  destroy() {
    // Șterge event listeners
    if (this.addDOMListener === ((el, ev, fn) => el.addEventListener(ev, fn))) {
      this.inputElement.removeEventListener('input', this.handleInput);
      this.inputElement.removeEventListener('keydown', this.handleKeydown);
      this.inputElement.removeEventListener('blur', this.handleBlur);
    }

    // Șterge elementele din DOM
    if (this.flagWrapper && this.flagWrapper.parentElement) {
      this.flagWrapper.remove();
    }

    if (this.errorElement && this.errorElement.parentElement) {
      this.errorElement.remove();
    }

    // Curăță referințele
    this.inputElement = null;
    this.flagWrapper = null;
    this.flagElement = null;
    this.errorElement = null;
    this.parentElement = null;
    this.currentIdData = null;
    this.currentCountry = null;
    this.validatorRegistry.clear();
    this.formatterRegistry.clear();
    this.loadedMixins.clear();
    this.addDOMListener = null;

    this.log('🗑️ Destroyed');
  },
};

export default publicApiMixin;
