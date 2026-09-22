/**
 * 🌍 NATIONAL ID TOOLS - MODULAR
 * Orchestrator principal cu registry și logger
 *
 * @version 3.0.0
 */

import { initializationMixin } from './core/initialization.js';
import { eventHandlersMixin } from './core/event-handlers.js';
import { processingMixin } from './core/processing.js';
import { flagManagementMixin } from './core/flag-management.js';
import { validationUiMixin } from './core/validation-ui.js';
import { publicApiMixin } from './core/public-api.js';

export class NationalIdTools {
  /**
   * @param {HTMLInputElement} inputElement
   * @param {Object} options
   */
  constructor(inputElement, options = {}) {
    if (!inputElement || !(inputElement instanceof HTMLInputElement)) {
      throw new Error('NationalIdTools: inputElement invalid');
    }

    // Setup config
    this._setupConfig(inputElement, options);

    // Apply core mixins (automat)
    this._applyCoreMixins();

    // Registry pentru validatori și formatteri
    this._registerCountries();

    // Bind handlers
    this._bindHandlers();

    // Inițializare
    this.init();
  }

  // ==================== SETUP ====================

  _setupConfig(inputElement, options) {
    // Config
    this.inputElement = inputElement;
    this.autoFormat = options.autoFormat || false;
    this.autoValidate = options.autoValidate !== false;
    this.showError = options.showError || false;
    this.onValid = options.onValid || null;
    this.onInvalid = options.onInvalid || null;
    this.onChange = options.onChange || null;
    this.debugMode = options.debugMode || false;
    this.ListenerTracker = options.ListenerTracker || null;

    // Setup ListenerTracker
    if (this.ListenerTracker) {
      this.ListenerTracker.applyTo(this, {
        debugMode: this.debugMode || false,
        logPrefix: 'NAT_ID_TOOLS',
        trackPerformance: false,
      });
    } else {
      this.addDOMListener = (el, ev, fn) => el.addEventListener(ev, fn);
    }

    // Registry (lazy loading)
    this.validatorRegistry = new Map();
    this.formatterRegistry = new Map();
    this.loadedMixins = new Set();

    // State
    this.flagWrapper = null;
    this.flagElement = null;
    this.errorElement = null;
    this.parentElement = null;
    this.currentIdData = null;
    this.currentCountry = null;
  }

  _applyCoreMixins() {
    Object.assign(this, initializationMixin);
    Object.assign(this, eventHandlersMixin);
    Object.assign(this, processingMixin);
    Object.assign(this, flagManagementMixin);
    Object.assign(this, validationUiMixin);
    Object.assign(this, publicApiMixin);
  }

  _bindHandlers() {
    this.handleInput = this.handleInput.bind(this);
    this.handleBlur = this.handleBlur.bind(this);
    this.handleKeydown = this.handleKeydown.bind(this);
  }

  // ==================== LOGGER ====================

  log = (() => {
    const fn = (message, data = null) => {
      if (this.debugMode) {
        const now = new Date();
        const ts = `${now.toLocaleTimeString('en-GB', { hour12: false })}.${now
          .getMilliseconds()
          .toString()
          .padStart(3, '0')}`;
        const CPN = `NAT_ID_TOOLS_[${this.inputElement.name}]`.padEnd(25);
        console.log(
          `%c[${ts}] [${CPN}] ${message}`,
          'color: #3498db; font-weight: bold;',
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
      const CPN = `NAT_ID_TOOLS_[${this.inputElement.name}]`.padEnd(25);
      console.error(
        `%c[${ts}] [${CPN}] ${message}`,
        'color: #e74c3c; font-weight: bold;',
        data ?? ''
      );
    };

    fn.warn = (message, data = null) => {
      if (this.debugMode) {
        const now = new Date();
        const ts = `${now.toLocaleTimeString('en-GB', { hour12: false })}.${now
          .getMilliseconds()
          .toString()
          .padStart(3, '0')}`;
        const CPN = `NAT_ID_TOOLS_[${this.inputElement.name}]`.padEnd(25);
        console.warn(
          `%c[${ts}] [${CPN}] ${message}`,
          'color: #f39c12; font-weight: bold;',
          data ?? ''
        );
      }
    };

    return fn;
  })();

  // ==================== REGISTRY ====================

  /**
   * Înregistrare țări suportate (lazy - doar metadata)
   */
  _registerCountries() {
    // RO - CNP
    this.validatorRegistry.set('RO', {
      loaded: false,
      mixinName: 'ro_cnp_validator',
      modulePath: './mixins/ro-cnp-validator.js',
      detectionPattern: /^[1-9]/,
      validLength: 13,
      flagCode: 'ro',
    });

    this.formatterRegistry.set('RO', {
      loaded: false,
      mixinName: 'ro_cnp_formatter',
      modulePath: './mixins/ro-cnp-formatter.js',
    });

    // UK - NINO
    this.validatorRegistry.set('UK', {
      loaded: false,
      mixinName: 'uk_nino_validator',
      modulePath: './mixins/uk-nino-validator.js',
      detectionPattern: /^[A-Za-z]/,
      validLength: 9,
      flagCode: 'gb',
    });

    this.formatterRegistry.set('UK', {
      loaded: false,
      mixinName: 'uk_nino_formatter',
      modulePath: './mixins/uk-nino-formatter.js',
    });

    this.log('✅ Registry inițializat (lazy)');
  }

  /**
   * Încarcă și aplică mixinul pentru o țară (lazy loading)
   * @param {string} country - Cod țară (RO, UK)
   */
  async _loadMixinsForCountry(country) {
    const validatorMeta = this.validatorRegistry.get(country);
    const formatterMeta = this.formatterRegistry.get(country);

    if (!validatorMeta || !formatterMeta) {
      this.log.error(`❌ Țara ${country} nu este înregistrată`);
      return false;
    }

    // Dacă deja sunt încărcate, skip
    if (validatorMeta.loaded && formatterMeta.loaded) {
      this.log(`✅ Mixins pentru ${country} deja încărcate`);
      return true;
    }

    try {
      // Încarcă validatorul
      if (!validatorMeta.loaded) {
        const validatorModule = await import(validatorMeta.modulePath);
        const validatorMixin = validatorModule.default || validatorModule[validatorMeta.mixinName];

        // Aplică mixinul PE INSTANȚA CURENTĂ
        Object.assign(this, validatorMixin);
        validatorMeta.loaded = true;
        this.loadedMixins.add(validatorMeta.mixinName);
        this.log(`✅ Validator ${country} încărcat:`, validatorMeta.mixinName);
      }

      // Încarcă formatterul
      if (!formatterMeta.loaded) {
        const formatterModule = await import(formatterMeta.modulePath);
        const formatterMixin = formatterModule.default || formatterModule[formatterMeta.mixinName];

        // Aplică mixinul PE INSTANȚA CURENTĂ
        Object.assign(this, formatterMixin);
        formatterMeta.loaded = true;
        this.loadedMixins.add(formatterMeta.mixinName);
        this.log(`✅ Formatter ${country} încărcat:`, formatterMeta.mixinName);
      }

      return true;
    } catch (err) {
      this.log.error(`❌ Eroare la încărcarea mixins pentru ${country}:`, err);
      return false;
    }
  }

  /**
   * Detectează țara pe baza valorii introduse
   * @param {string} value
   * @returns {string|null} - Cod țară (RO, UK, etc.)
   */
  detectCountry(value) {
    if (!value || value.trim().length === 0) return null;

    for (const [country, meta] of this.validatorRegistry.entries()) {
      if (meta.detectionPattern.test(value)) {
        return country;
      }
    }

    return null;
  }
}

export default NationalIdTools;
