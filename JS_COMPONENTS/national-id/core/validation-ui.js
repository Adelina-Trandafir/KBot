/**
 * ✅ VALIDATION UI MIXIN
 * Gestionează UI-ul pentru validare
 */

export const validationUiMixin = {
  /**
   * Marchează starea validării în UI
   */
  showValidationState(isValid, errorMessage = 'invalid') {
    this.inputElement.classList.toggle('is-valid', isValid);
    this.inputElement.classList.toggle('is-invalid', !isValid);

    if (this.showError && this.errorElement) {
      if (isValid) {
        this.hideError();
      } else {
        this.showErrorMessage(errorMessage);
      }
    }
  },

  /**
   * Afișează mesaj de eroare
   */
  showErrorMessage(message) {
    if (this.errorElement) {
      this.errorElement.textContent = message;
      this.errorElement.style.display = 'block';
    }
  },

  /**
   * Ascunde mesajul de eroare
   */
  hideError() {
    if (this.errorElement) {
      this.errorElement.textContent = '';
      this.errorElement.style.display = 'none';
    }

    this.inputElement.classList.remove('is-valid', 'is-invalid');
  },
};

export default validationUiMixin;
