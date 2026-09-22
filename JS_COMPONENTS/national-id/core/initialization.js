/**
 * 🚀 INITIALIZATION MIXIN
 * Gestionează inițializarea componentei
 */

export const initializationMixin = {
  /**
   * Inițializare completă
   */
  async init() {
    this.log('🚀 NationalIdTools init...');

    try {
      // 1. Încarcă CSS pentru steaguri
      await this.loadFlagsCSS();

      // 1.1 Încarcă CSS pentru componenta National ID Tools
      await this.loadComponentCSS();

      // 2. Crează element steag
      this.createFlagElement();

      // 3. Atașează event listeners
      this.attachListeners();

      // 4. Procesează valoarea existentă (dacă există)
      if (this.inputElement.value.trim()) {
        await this.processInput(this.inputElement.value);
      }

      this.log('✅ NationalIdTools initialized');
    } catch (err) {
      this.log.error('❌ Eroare la inițializare', err);
    }
  },

  /**
   * Încarcă CSS pentru componenta National ID Tools
   */
  async loadComponentCSS() {
    if (document.getElementById('nationalIdToolsCSS')) {
      this.log('✅ Component CSS deja încărcat');
      return;
    }

    this.log('🎨 Încarc CSS-ul componentei...');

    return new Promise((resolve, reject) => {
      const link = document.createElement('link');
      link.id = 'nationalIdToolsCSS';
      link.rel = 'stylesheet';
      link.href = '/static/js/components/national-id/national_id_tools.css'; // ajustează calea după structura ta

      link.onload = () => {
        this.log('✅ Component CSS încărcat cu succes');
        resolve();
      };

      link.onerror = () => {
        this.log.error('❌ Eroare la încărcarea CSS-ului componentei');
        reject(new Error('Failed to load component CSS'));
      };

      document.head.appendChild(link);
    });
  },

  /**
   * Încarcă CSS pentru steaguri
   */
  async loadFlagsCSS() {
    if (document.getElementById('nationalIdFlagsCSS')) {
      this.log('✅ CSS deja încărcat');
      return;
    }

    this.log('🎨 Încarc CSS-ul flags...');

    return new Promise((resolve, reject) => {
      const link = document.createElement('link');
      link.id = 'nationalIdFlagsCSS';
      link.rel = 'stylesheet';
      link.href = 'https://cdn.jsdelivr.net/npm/flag-icons@7.2.3/css/flag-icons.min.css';

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
  },

  /**
   * Crează și inserează elementul de steag în DOM
   */
  createFlagElement() {
    this.parentElement = this.inputElement.parentElement;

    if (!this.parentElement) {
      this.log.error('❌ Parent element not found');
      return;
    }

    // Crează wrapper pentru steag
    this.flagWrapper = document.createElement('div');
    this.flagWrapper.className = 'national-id-flag-wrapper hidden';

    // Crează elementul steag
    this.flagElement = document.createElement('span');
    this.flagElement.className = 'national-id-flag-icon fi';

    this.flagWrapper.appendChild(this.flagElement);

    // Găsește label-ul
    const label = this.parentElement.querySelector('label');

    if (label) {
      label.insertAdjacentElement('afterend', this.flagWrapper);
      this.log('✅ Flag element inserted after label');
    } else {
      this.parentElement.insertBefore(this.flagWrapper, this.inputElement);
      this.log('✅ Flag element inserted before input');
    }

    // Crează element pentru erori
    if (this.showError) {
      this.errorElement = document.createElement('div');
      this.errorElement.className = 'national-id-error-message';
      this.inputElement.insertAdjacentElement('afterend', this.errorElement);
    }
  },

  /**
   * Atașează event listeners
   */
  attachListeners() {
    this.addDOMListener(this.inputElement, 'input', this.handleInput);
    this.addDOMListener(this.inputElement, 'keydown', this.handleKeydown);

    if (this.autoValidate) {
      this.addDOMListener(this.inputElement, 'blur', this.handleBlur);
    }

    this.log('✅ Event listeners attached');
  },
};

export default initializationMixin;
