// ========== FILE: /static/js/dashboard/session-data.js ==========
/**
 * 📊 SESSION DATA - Simple storage pentru date de sesiune
 * Înlocuiește window globals cu registry pattern
 */

import { registerInstance } from '../instances-registry.js';

class SessionData {
  constructor() {
    // Singleton check
    if (SessionData.instance) {
      this.log.error('⚠️ SessionData is singleton, returning existing instance');
      return SessionData.instance;
    }

    this.data = new Map();
    this.debugMode = false;
    this.storageKey = 'app_session_data';

    // Încarcă datele existente din sessionStorage
    this.loadFromStorage();

    SessionData.instance = this;

    registerInstance('sessionData', this, {
      version: '1.0.0',
      description: 'Session data storage - replaces window globals',
    });

    this.log('📊 SessionData initialized');
  }

  /**
   * 📥 ÎNCARCĂ DATELE DIN SESSIONSTORAGE
   */
  loadFromStorage() {
    try {
      const storedData = sessionStorage.getItem(this.storageKey);
      if (storedData) {
        const parsed = JSON.parse(storedData);
        this.data = new Map(Object.entries(parsed));
        this.log('📥 Date încărcate din sessionStorage:', this.data.size);
      }
    } catch (error) {
      this.log.error('❌ Eroare la încărcarea din storage:', error);
      this.data = new Map();
    }
  }

  /**
   * 💾 SALVEAZĂ DATELE ÎN SESSIONSTORAGE
   */
  saveToStorage() {
    try {
      const dataObject = Object.fromEntries(this.data);
      sessionStorage.setItem(this.storageKey, JSON.stringify(dataObject));
      this.log('💾 Date salvate în sessionStorage');
    } catch (error) {
      this.log.error('❌ Eroare la salvarea în storage:', error);
    }
  }

  /**
   * 💾 SET DATA - cu persistență automată
   */
  set(key, value) {
    this.data.set(key, value);
    this.saveToStorage(); // Salvează automat
    this.log(`💾 Set data: ${key}`, value);
  }

  /**
   * 📤 GET DATA
   */
  get(key) {
    return this.data.get(key);
  }

  /**
   * 🗑️ DELETE DATA - cu persistență automată
   */
  delete(key) {
    this.log(`🗑️ Delete data: ${key}`);
    const result = this.data.delete(key);
    this.saveToStorage(); // Salvează automat
    return result;
  }

  /**
   * 🧹 CLEAR ALL - cu persistență automată
   */
  clear() {
    this.log('🧹 Clear all data');
    this.data.clear();
    sessionStorage.removeItem(this.storageKey);
  }

  /**
   * ❓ HAS DATA
   */
  has(key) {
    return this.data.has(key);
  }

  /**
   * 📋 GET ALL KEYS
   */
  keys() {
    return Array.from(this.data.keys());
  }

  /**
   * 📊 GET SIZE
   */
  size() {
    return this.data.size;
  }

  /**
   * 📝 LOG pentru debugging (PĂSTRAT EXACT)
   */
  log = (() => {
    const fn = (message, data = null) => {
      if (this.debugMode) {
        const now = new Date();
        const ts = `${now.toLocaleTimeString('en-GB', { hour12: false })}.${now
          .getMilliseconds()
          .toString()
          .padStart(3, '0')}`;
        const CPN = 'SessionData'.padEnd(15);
        console.log(
          `%c[${ts}] [${CPN}] ${message}`,
          'color: #fff89dff; font-weight: bold;',
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
      const CPN = 'SessionData'.padEnd(15);
      console.error(
        `%c[${ts}] [${CPN}] ${message}`,
        'color: #ef4444; font-weight: bold;',
        data ?? ''
      );
    };

    return fn;
  })();
}

// Creează instanța singleton
const sessionData = new SessionData();

// Export pentru ES6 modules
export default sessionData;

// Pentru debugging pune pe window (opțional)
window.sessionData = sessionData;
