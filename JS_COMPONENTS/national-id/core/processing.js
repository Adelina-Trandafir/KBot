/**
 * ⚙️ PROCESSING MIXIN
 * Gestionează procesarea input-ului
 */

export const processingMixin = {
  /**
   * Procesează input-ul (detectare țară + formatare + steag)
   */
  async processInput(value) {
    // Callback onChange
    if (this.onChange) {
      this.onChange(value);
    }

    if (!value || value.trim().length === 0) {
      this.reset();
      return;
    }

    // Detectează țara
    const detectedCountry = this.detectCountry(value);

    if (!detectedCountry) {
      this.log.warn('⚠️ Nu s-a putut detecta țara');
      this.hideFlag();
      return;
    }

    // Dacă țara s-a schimbat, încarcă mixinurile
    if (detectedCountry !== this.currentCountry) {
      this.currentCountry = detectedCountry;
      await this._loadMixinsForCountry(detectedCountry);
      this.log(`🌍 Țară detectată: ${detectedCountry}`);
    }

    // Extrage valoarea curată
    const extractMethodName = `extract_${detectedCountry.toLowerCase()}_${
      this.formatterRegistry.get(detectedCountry).mixinName.split('_')[1]
    }`;
    const cleanValue =
      typeof this[extractMethodName] === 'function' ? this[extractMethodName](value) : value;

    // Afișează steagul
    this.updateFlag(cleanValue);

    // Formatare (dacă e activată)
    if (this.autoFormat && cleanValue) {
      const formatMethodName = `apply_format_${detectedCountry.toLowerCase()}_${
        this.formatterRegistry.get(detectedCountry).mixinName.split('_')[1]
      }`;

      if (typeof this[formatMethodName] === 'function') {
        this[formatMethodName](cleanValue);
      }
    }

    // Validare parțială (pentru feedback instant)
    const validatorMeta = this.validatorRegistry.get(detectedCountry);
    if (cleanValue.length === validatorMeta.validLength) {
      const result = await this.validate(cleanValue, detectedCountry);
      this.currentIdData = result;
    }
  },
};

export default processingMixin;
