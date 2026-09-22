/**
 * 🎯 EVENT HANDLERS MIXIN
 * Gestionează event handlers pentru input
 */

export const eventHandlersMixin = {
  /**
   * Handler pentru keydown
   */
  handleKeydown(e) {
    if (!this.currentCountry) return;

    const methodName = `handle_keydown_${this.currentCountry.toLowerCase()}_${
      this.formatterRegistry.get(this.currentCountry).mixinName.split('_')[1]
    }`;

    if (typeof this[methodName] === 'function') {
      this[methodName](e);
    }
  },

  /**
   * Handler pentru input - procesare live
   */
  async handleInput(e) {
    await this.processInput(e.target.value);
  },

  /**
   * Handler pentru blur - validare finală
   */
  async handleBlur(e) {
    const value = e.target.value.trim();

    if (!value) {
      this.hideFlag();
      this.hideError();
      return;
    }

    const result = await this.validate();

    if (result && result.valid) {
      this.showValidationState(true);
      if (this.onValid) {
        this.onValid(result);
      }
    } else {
      this.showValidationState(false, 'invalid');
      if (this.onInvalid) {
        this.onInvalid(value);
      }
    }
  },
};

export default eventHandlersMixin;
