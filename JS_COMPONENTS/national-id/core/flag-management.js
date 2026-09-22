/**
 * 🚩 FLAG MANAGEMENT MIXIN
 * Gestionează afișarea steagurilor
 */

export const flagManagementMixin = {
  /**
   * Actualizează steagul
   */
  updateFlag(cleanValue) {
    if (!this.currentCountry || !cleanValue) {
      this.hideFlag();
      return;
    }

    const validatorMeta = this.validatorRegistry.get(this.currentCountry);
    let flagCode = null;

    // Pentru RO: determină steagul din prima cifră
    if (this.currentCountry === 'RO' && cleanValue.length >= 1) {
      const getFlagMethodName = `get_flag_from_first_digit_ro_cnp`;
      if (typeof this[getFlagMethodName] === 'function') {
        flagCode = this[getFlagMethodName](cleanValue[0]);
      }
    }
    // Pentru alte țări: folosește steagul standard
    else {
      flagCode = validatorMeta.flagCode;
    }

    if (flagCode) {
      this.showFlag(flagCode);
    } else {
      this.hideFlag();
    }
  },

  /**
   * Afișează steagul
   */
  showFlag(flagCode) {
    this.flagElement.className = `national-id-flag-icon fi fi-${flagCode}`;
    this.flagWrapper.classList.remove('hidden');
    this.flagWrapper.classList.add('visible');
    this.log(`🚩 Steag afișat: ${flagCode}`);
  },

  /**
   * Ascunde steagul
   */
  hideFlag() {
    this.flagWrapper.classList.add('hidden');
    this.flagWrapper.classList.remove('visible');
  },
};

export default flagManagementMixin;
