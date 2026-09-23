// File: static/js/overlay-manager.js
/**
 * 🎭 OVERLAY MANAGER - Sistem centralizat pentru backdrop overlay
 *
 * Features:
 * - Un singur element DOM reutilizabil
 * - Stack de modale cu handlers independenți
 * - Suspend/Resume automat pentru modale inactive
 * - Integration cu ListenerTracker pentru cleanup
 * - Z-index management automat cu suport pentru stack vizual
 * - CSS classes dinamice pentru stilizare flexibilă
 * - Parent dinamic per modal
 * - Memory leak prevention cu cleanup automat
 * - Performance optimization cu z-index caching
 * - Metrics și monitoring pentru debugging
 *
 * @author Adelina Trandafir - Avatar Soft SRL
 * @version 2.0.0
 */

import ListenerTracker from '../listener-tracker/listener-tracker-mixin.js';
import { registerInstance, getInstance } from '../instances-registry.js';

class OverlayManager {
  constructor() {
    if (OverlayManager.instance) {
      this.log('⚠️ OverlayManager is singleton, returning existing instance');
      return OverlayManager.instance;
    }

    // 🎛️ Configurare
    this.config = {
      debugMode: false, // ← Schimbă în true pentru debugging
      maxStackSize: 10, // Max modale simultane
      modalTimeout: 300000, // 5 min - cleanup automat pentru modale uitate
      cleanupInterval: 60000, // 1 min - verificare periodică
      enableMetrics: true, // Metrics pentru monitoring
    };

    OverlayManager.instance = this;

    // 🎯 Aplicăm ListenerTracker pentru cleanup automat
    ListenerTracker.applyTo(this);

    // 📚 Stack de modale: [{ modal, handlers, parent, isActive }, ...]
    this.stack = [];

    // 🎨 Element DOM (creat de apelant, doar păstrăm referință)
    this.overlayElement = null;

    // 📍 Parent curent al overlay-ului
    this.currentParent = null;

    // 🎨 Clasele CSS custom aplicate (fără clasa base)
    this.currentClasses = new Set();

    // 🔒 Flag pentru a preveni re-entrancy
    this.isProcessing = false;

    // 📋 Clasa base (mereu aplicată)
    this.BASE_CLASS = 'main-overlay-base';

    // 🚀 Performance optimization - cache max z-index
    this._cachedMaxZIndex = 0;

    // 📊 Metrics pentru monitoring
    this.metrics = {
      totalSubscribes: 0,
      totalUnsubscribes: 0,
      currentStackSize: 0,
      maxStackSizeReached: 0,
      totalCleanups: 0,
      zIndexResets: 0,
    };

    // ⏱️ Cleanup interval timer
    this._cleanupTimer = null;

    registerInstance('overlayManager', this, {
      version: '2.0.0',
      description: 'Production-ready overlay manager with memory leak prevention',
    });

    this.log('🎭 OverlayManager inițializat (production-ready)');
  }

  init() {
    this.overlayElement = document.createElement('div');
    this.overlayElement.id = 'app-overlay';
    document.body.appendChild(this.overlayElement);

    // Salvează parent-ul inițial
    this.currentParent = document.body;

    // Verifică dacă setElement există (backwards compatibility)
    if (typeof this.setElement === 'function') {
      this.setElement(this.overlayElement, {
        defaultClass: 'main-overlay-light-blur',
      });
    } else {
      // Fallback: setează clasa direct
      this.overlayElement.classList.add(this.BASE_CLASS);
      this.overlayElement.classList.add('main-overlay-light-blur');
    }

    // Atașează listeners
    this._attachElementListeners();

    // Start periodic cleanup
    this._startPeriodicCleanup();

    window.overlay = this;

    this.log('✅ OverlayManager init: element DOM creat și atașat');

    return true;
  }

  /**
   * 🎨 Setează clase CSS (înlocuiește toate clasele custom)
   *
   * @param {string} classNames - Clase separate prin spațiu: 'main-overlay-heavy-blur main-overlay-blue'
   */
  setClass(classNames) {
    if (!this.overlayElement) {
      this.log.error('❌ setClass: element DOM nu e setat');
      return;
    }

    // Șterge toate clasele custom existente
    this.currentClasses.forEach((cls) => {
      this.overlayElement.classList.remove(cls);
    });
    this.currentClasses.clear();

    // Adaugă noile clase
    if (classNames && typeof classNames === 'string') {
      const classes = classNames.split(/\s+/).filter((cls) => cls.trim());
      classes.forEach((cls) => {
        this.overlayElement.classList.add(cls);
        this.currentClasses.add(cls);
      });

      this.log(`🎨 Clase CSS setate: ${classes.join(', ')}`);
    }
  }

  /**
   * 🎨 Adaugă clase CSS (păstrează cele existente)
   *
   * @param {string} classNames - Clase separate prin spațiu
   */
  addClass(classNames) {
    if (!this.overlayElement) {
      this.log.error('❌ addClass: element DOM nu e setat');
      return;
    }

    if (!classNames || typeof classNames !== 'string') return;

    const classes = classNames.split(/\s+/).filter((cls) => cls.trim());
    classes.forEach((cls) => {
      if (!this.currentClasses.has(cls)) {
        this.overlayElement.classList.add(cls);
        this.currentClasses.add(cls);
      }
    });

    this.log(`➕ Clase CSS adăugate: ${classes.join(', ')}`);
  }

  /**
   * 🎨 Elimină clase CSS
   *
   * @param {string} classNames - Clase separate prin spațiu
   */
  removeClass(classNames) {
    if (!this.overlayElement) {
      this.log.error('❌ removeClass: element DOM nu e setat');
      return;
    }

    if (!classNames || typeof classNames !== 'string') return;

    const classes = classNames.split(/\s+/).filter((cls) => cls.trim());
    classes.forEach((cls) => {
      if (this.currentClasses.has(cls)) {
        this.overlayElement.classList.remove(cls);
        this.currentClasses.delete(cls);
      }
    });

    this.log(`➖ Clase CSS eliminate: ${classes.join(', ')}`);
  }

  /**
   * 🎨 Returnează clasele CSS curente (fără base)
   */
  getCurrentClasses() {
    return Array.from(this.currentClasses);
  }

  /**
   * 🎧 Atașează listeners pe elementul overlay
   */
  _attachElementListeners() {
    if (!this.overlayElement) return;

    // Click pe overlay (backdrop)
    this.addDOMListener(this.overlayElement, 'click', (e) => {
      if (e.target !== this.overlayElement) return; // Click pe copil, nu pe overlay

      e.stopPropagation();
      e.preventDefault();

      const activeEntry = this._getActiveEntry();
      if (activeEntry?.handlers?.onClick) {
        this.log('🖱️ Overlay click → apel onClick handler');
        activeEntry.handlers.onClick(e);
      }
    });

    // ESC key
    this.addDOMListener(document, 'keydown', (e) => {
      if (e.key !== 'Escape') return;
      if (this.stack.length === 0) return;

      const activeEntry = this._getActiveEntry();
      if (activeEntry?.handlers?.onEscape) {
        this.log('⌨️ ESC pressed → apel onEscape handler');
        e.preventDefault();
        activeEntry.handlers.onEscape(e);
      }
    });

    // Scroll
    this.addDOMListener(
      window,
      'scroll',
      (e) => {
        if (this.stack.length === 0) return;

        const activeEntry = this._getActiveEntry();
        if (activeEntry?.handlers?.onScroll) {
          activeEntry.handlers.onScroll(e);
        }
      },
      { passive: true }
    );

    this.log('✅ Overlay listeners atașați');
  }

  /**
   * 🧹 Curăță listeners de pe element (pentru înlocuire element)
   */
  _cleanupElementListeners() {
    // ListenerTracker se ocupă automat de cleanup
    this.cleanupAllListeners('dom');
  }

  /**
   * 🔢 Găsește cel mai mare z-index din stack
   *
   * @returns {number} - Cel mai mare z-index sau 0 dacă stack-ul e gol
   */
  _getMaxZIndexFromStack() {
    if (this.stack.length === 0) return 0;

    let maxZIndex = 0;
    this.stack.forEach((entry) => {
      const zIndex = parseInt(entry.modal.style.zIndex) || 0;
      if (zIndex > maxZIndex) {
        maxZIndex = zIndex;
      }
    });

    return maxZIndex;
  }

  /**
   * 🧹 Curăță modale "moarte" (nu mai sunt în DOM)
   *
   * @returns {number} - Numărul de modale curățate
   */
  _cleanupDeadModals() {
    const initialLength = this.stack.length;

    this.stack = this.stack.filter((entry) => {
      // Verifică dacă modalul mai e în DOM
      if (!document.contains(entry.modal)) {
        this.log(`🧹 Cleanup modal mort: ${this._getModalName(entry.caller)}`);

        // Restaurează stilurile originale (dacă elementul încă există)
        if (entry.modal) {
          entry.modal.style.zIndex = entry.elementZIndex || '';
        }
        if (entry.parent) {
          entry.parent.style.position = entry.parentPosition || '';
        }

        return false; // Scoate din stack
      }

      // Verifică timeout (modale "uitate")
      if (this.config.modalTimeout > 0) {
        const age = Date.now() - entry.addedAt;
        if (age > this.config.modalTimeout) {
          this.log(
            `⏰ Cleanup modal expirat: ${this._getModalName(entry.caller)} (age: ${Math.round(age / 1000)}s)`
          );
          return false;
        }
      }

      return true;
    });

    const cleanedCount = initialLength - this.stack.length;

    if (cleanedCount > 0) {
      if (this.config.enableMetrics) {
        this.metrics.totalCleanups += cleanedCount;
        this.metrics.currentStackSize = this.stack.length;
      }

      // Dacă stack-ul e gol după cleanup, ascunde overlay
      if (this.stack.length === 0) {
        this._hideOverlay();
      } else {
        // Reactivează ultimul modal dacă era vreunul activ
        const hasActive = this.stack.some((entry) => entry.isActive);
        if (!hasActive && this.stack.length > 0) {
          this.stack[this.stack.length - 1].isActive = true;
          this._showOverlay();
        }
      }

      this.log(`🧹 Cleanup complet: ${cleanedCount} modale curățate`);
    }

    return cleanedCount;
  }

  /**
   * ⏱️ Start periodic cleanup pentru modale moarte
   */
  _startPeriodicCleanup() {
    if (this._cleanupTimer) {
      clearInterval(this._cleanupTimer);
    }

    if (this.config.cleanupInterval > 0) {
      this._cleanupTimer = setInterval(() => {
        this._cleanupDeadModals();
      }, this.config.cleanupInterval);

      this.log(`⏱️ Periodic cleanup started: ${this.config.cleanupInterval}ms`);
    }
  }

  /**
   * 🛑 Stop periodic cleanup
   */
  _stopPeriodicCleanup() {
    if (this._cleanupTimer) {
      clearInterval(this._cleanupTimer);
      this._cleanupTimer = null;
      this.log('🛑 Periodic cleanup stopped');
    }
  }

  /**
   * 📌 Subscribe: Adaugă modal în stack și arată overlay
   *
   * @param {Object} caller - Referință la instanța modalului
   * @param {HTMLElement} modalElement - Elementul DOM al modalului
   * @param {Object} handlers - { onClick, onEscape, onScroll, parent, transparent }
   */
  subscribe(caller, modalElement, handlers = {}) {
    // 🔒 Validări robuste
    if (!caller || !modalElement) {
      this.log.error('❌ subscribe: parametri invalidi (caller sau modalElement lipsă)');
      return;
    }

    if (!(modalElement instanceof HTMLElement)) {
      this.log.error('❌ subscribe: modalElement nu e HTMLElement');
      return;
    }

    if (!document.contains(modalElement)) {
      this.log.error('❌ subscribe: modalElement nu e în DOM');
      return;
    }

    // 🧹 Cleanup modale moarte înainte de subscribe
    this._cleanupDeadModals();

    // 🚫 Verifică limit stack size
    if (this.stack.length >= this.config.maxStackSize) {
      this.log.error(`❌ subscribe: Stack limit atins (${this.config.maxStackSize})`);
      return;
    }

    const transparent = handlers.transparent || false;

    // 🔍 Verifică dacă modalul (elementul) e deja în stack
    const existingByModal = this.stack.find((entry) => entry.modal === modalElement);
    if (existingByModal) {
      this.log('⚠️ Modal element deja subscris, ignor');
      return;
    }

    // 🔍 Verifică dacă caller-ul e deja în stack
    const existingIndex = this.stack.findIndex((entry) => entry.caller === caller);
    if (existingIndex !== -1) {
      this.log('⚠️ Caller deja în stack, îl readuc activ');
      this._makeActive(existingIndex);
      return;
    }

    // Suspendă modalul curent activ (dacă există)
    const currentActive = this._getActiveEntry();
    if (currentActive) {
      currentActive.isActive = false;
      this.log(`💤 Modal suspendat: ${this._getModalName(currentActive.caller)}`);
    }

    // Stabilește parent-ul pentru acest modal
    const modalParent = handlers.parent || modalElement.parentNode;

    // 🔒 Validare parent
    if (!modalParent || !document.contains(modalParent)) {
      this.log.error('❌ subscribe: parent invalid sau nu e în DOM');
      return;
    }

    // 🎯 CALCULARE Z-INDEX CORECT
    // Folosim cached value pentru performanță
    let maxStackZIndex = this._cachedMaxZIndex;

    // Safe access la ZIndexManager
    let nextZIndex = 1000; // Fallback
    if (window.ZIndexManager?.getNext) {
      nextZIndex = Math.floor(window.ZIndexManager.getNext());
    } else {
      this.log('⚠️ ZIndexManager nu e disponibil, folosesc fallback z-index');
    }

    // Noul modal trebuie să fie mai mare decât ambele valori
    const newModalZIndex = Math.max(maxStackZIndex + 2, nextZIndex);

    // Update cache
    this._cachedMaxZIndex = newModalZIndex;

    this.log(
      `🔢 Z-index calculat: ${newModalZIndex} (cached: ${maxStackZIndex}, ZIndexManager: ${nextZIndex})`
    );

    // Adaugă noul modal în stack
    const entry = {
      caller: caller,
      modal: modalElement,
      parent: modalParent,
      transparent: transparent,
      handlers: this._normalizeHandlers(handlers),
      isActive: true,
      elementZIndex: window.getComputedStyle(modalElement).zIndex || '',
      parentPosition: window.getComputedStyle(modalParent).position || '',
      addedAt: Date.now(),
    };

    this.stack.push(entry);

    // 📊 Update metrics
    if (this.config.enableMetrics) {
      this.metrics.totalSubscribes++;
      this.metrics.currentStackSize = this.stack.length;
      this.metrics.maxStackSizeReached = Math.max(
        this.metrics.maxStackSizeReached,
        this.metrics.currentStackSize
      );
    }

    this.log(
      `📌 Modal subscribe: ${this._getModalName(caller)} (stack size: ${this.stack.length})`
    );

    // Setează z-index-ul modalului
    modalElement.style.zIndex = newModalZIndex;

    // Setează parent-ul ca relative
    modalParent.style.position = 'relative';

    // Mută overlay-ul la parent-ul specificat (dacă e diferit de currentParent)
    if (modalParent !== this.currentParent) {
      this._moveToParent(modalParent);
    } else {
      this.log(`✅ Overlay deja în parent-ul corect pentru ${this._getModalName(caller)}`);
    }

    // Arată overlay-ul cu z-index corect
    this._showOverlay();
  }

  /**
   * 📍 Unsubscribe: Scoate modal din stack
   *
   * @param {Object} caller - Referință la instanța modalului
   */
  unsubscribe(caller) {
    if (!caller) {
      this.log.error('❌ unsubscribe: caller invalid');
      return;
    }

    const index = this.stack.findIndex((entry) => {
      return entry.caller === caller;
    });
    if (index === -1) {
      this.log('⚠️ Modal nu e în stack, ignor unsubscribe');
      return;
    }

    const entry = this.stack[index];
    const wasActive = entry.isActive;
    const modalName = this._getModalName(caller);

    // Restaurează stilurile originale
    entry.parent.style.position = entry.parentPosition || '';
    entry.modal.style.zIndex = entry.elementZIndex || '';

    this.log(`📍 Modal unsubscribe: ${modalName} (stack size: ${this.stack.length - 1})`);

    // Scoate din stack
    this.stack.splice(index, 1);

    // 📊 Update metrics
    if (this.config.enableMetrics) {
      this.metrics.totalUnsubscribes++;
      this.metrics.currentStackSize = this.stack.length;
    }

    // Dacă era activ și mai sunt modale, activează ultimul
    if (wasActive && this.stack.length > 0) {
      const newActive = this.stack[this.stack.length - 1];
      newActive.isActive = true;
      this.log(`✨ Modal reactivat: ${this._getModalName(newActive.caller)}`);

      // Mută overlay-ul la parent-ul noului modal activ (dacă are unul)
      if (newActive.parent) {
        this._moveToParent(newActive.parent);
      }

      // Actualizează overlay-ul pentru noul modal activ
      this._showOverlay();
    }

    // Dacă stack-ul e gol, ascunde overlay-ul și resetează
    if (this.stack.length === 0) {
      this._hideOverlay();
      this._resetZIndexCache();
    }
  }

  /**
   * 🎨 Arată overlay-ul în DOM cu z-index corect
   */
  _showOverlay() {
    if (!this.overlayElement) return;

    const activeEntry = this._getActiveEntry();
    if (!activeEntry) return;

    const { caller, modal, parent, transparent } = activeEntry;

    // Setează transparența
    !transparent
      ? this.overlayElement.classList.remove('main-overlay-full-transparent')
      : this.overlayElement.classList.add('main-overlay-full-transparent');

    // Overlay-ul trebuie să fie cu 1 mai mic decât modalul activ
    const modalZIndex = parseInt(modal.style.zIndex) || 0;
    const overlayZIndex = modalZIndex - 1;

    this.overlayElement.style.zIndex = overlayZIndex;
    this.overlayElement.style.display = 'block';

    this.log(`🎨 Overlay arătat: z-index=${overlayZIndex} (modal activ z-index=${modalZIndex})`);
  }

  /**
   * 🚫 Ascunde overlay-ul din DOM
   */
  _hideOverlay() {
    if (!this.overlayElement) return;

    this.overlayElement.style.display = 'none';
    this.overlayElement.style.zIndex = '';

    this.log('🚫 Overlay ascuns (stack gol)');
  }

  /**
   * 🔄 Reset z-index cache și counters
   */
  _resetZIndexCache() {
    this._cachedMaxZIndex = 0;

    // 📊 Update metrics
    if (this.config.enableMetrics) {
      this.metrics.zIndexResets++;
    }

    // Opțional: notifică ZIndexManager să reseteze
    if (window.ZIndexManager?.reset) {
      try {
        window.ZIndexManager.reset();
        this.log('🔄 ZIndexManager reset success');
      } catch (error) {
        this.log.error('❌ ZIndexManager reset failed:', error);
      }
    }

    this.log('🔄 Z-index cache și counters reset');
  }

  /**
   * 🔄 Face un modal din mijlocul stack-ului activ
   *
   * Strategie:
   * - Verifică dacă modalul care devine activ are z-index mai mic decât alte modale
   * - Dacă da, îi dă un z-index nou (mai mare decât toate)
   * - Overlay-ul se poziționează la activeModal.zIndex - 1
   */
  _makeActive(index) {
    if (index < 0 || index >= this.stack.length) return;

    // Dezactivează toate
    this.stack.forEach((entry) => (entry.isActive = false));

    // Activează cel specificat
    this.stack[index].isActive = true;

    const activeEntry = this.stack[index];
    const modalName = this._getModalName(activeEntry.caller);

    this.log(`✨ Modal făcut activ: ${modalName}`);

    // Verifică dacă modalul activ are z-index mai mic decât alte modale din stack
    const activeZIndex = parseInt(activeEntry.modal.style.zIndex) || 0;
    const maxStackZIndex = this._getMaxZIndexFromStack();

    if (activeZIndex < maxStackZIndex) {
      // Modalul activ are z-index mai mic, trebuie să-i dăm unul nou
      const newZIndex = maxStackZIndex + 2;
      activeEntry.modal.style.zIndex = newZIndex;

      // Update cache
      this._cachedMaxZIndex = newZIndex;

      this.log(`🔼 Z-index actualizat pentru modal activ: ${activeZIndex} → ${newZIndex}`);
    }

    // Mută overlay-ul la parent-ul modalului activ (dacă are)
    if (activeEntry.parent) {
      this._moveToParent(activeEntry.parent);
    }

    // Actualizează overlay-ul cu z-index corect
    this._showOverlay();
  }

  /**
   * 📍 Mută overlay-ul într-un nou parent
   *
   * @param {HTMLElement} newParent - Noul părinte
   */
  _moveToParent(newParent) {
    if (!this.overlayElement || !newParent) {
      this.log.error('❌ _moveToParent: element sau parent invalid');
      return;
    }

    // Verifică dacă e deja în parent-ul corect
    if (this.currentParent === newParent) {
      this.log(
        `✅ Overlay deja în parent-ul corect: ${newParent.tagName}#${newParent.id || 'NoID'}`
      );
      return;
    }

    // Validează că newParent e în DOM
    if (!document.contains(newParent)) {
      this.log.error('❌ _moveToParent: parent-ul nu e în DOM');
      return;
    }

    try {
      // Mută overlay-ul
      newParent.appendChild(this.overlayElement);
      this.currentParent = newParent;

      this.log(`📍 Overlay mutat în: ${newParent.tagName}#${newParent.id || 'NoID'}`);
    } catch (error) {
      this.log.error('❌ Eroare la mutarea overlay-ului:', error);
    }
  }

  /**
   * 📊 Returnează entry-ul activ din stack
   */
  _getActiveEntry() {
    return this.stack.find((entry) => entry.isActive);
  }

  /**
   * 🔧 Normalizează handlers (asigură că sunt funcții sau null)
   */
  _normalizeHandlers(handlers) {
    return {
      onClick: typeof handlers.onClick === 'function' ? handlers.onClick : null,
      onEscape: typeof handlers.onEscape === 'function' ? handlers.onEscape : null,
      onScroll: typeof handlers.onScroll === 'function' ? handlers.onScroll : null,
    };
  }

  /**
   * 🏷️ Extrage numele modalului pentru logging
   */
  _getModalName(caller) {
    return caller?.constructor?.name || caller?.toString() || 'UnknownModal';
  }

  /**
   * 📊 API PUBLIC - Obține metrics pentru monitoring
   *
   * @returns {Object} - Obiect cu metrics și statistici
   */
  getMetrics() {
    const leakedModals = this.stack.filter((entry) => !document.contains(entry.modal));

    return {
      ...this.metrics,
      leakedModals: leakedModals.length,
      leakedModalDetails: leakedModals.map((entry) => ({
        name: this._getModalName(entry.caller),
        age: Math.round((Date.now() - entry.addedAt) / 1000),
      })),
      cachedMaxZIndex: this._cachedMaxZIndex,
      config: { ...this.config },
    };
  }

  /**
   * 📊 API PUBLIC - Obține info despre stack-ul curent
   *
   * @returns {Array} - Array cu info despre fiecare modal din stack
   */
  getStackInfo() {
    return this.stack.map((entry, index) => ({
      index,
      name: this._getModalName(entry.caller),
      isActive: entry.isActive,
      zIndex: parseInt(entry.modal.style.zIndex) || 0,
      transparent: entry.transparent,
      age: Math.round((Date.now() - entry.addedAt) / 1000),
      inDOM: document.contains(entry.modal),
    }));
  }

  /**
   * 🧹 API PUBLIC - Force cleanup pentru modale moarte
   *
   * @returns {number} - Numărul de modale curățate
   */
  forceCleanup() {
    this.log('🧹 Force cleanup trigger manual');
    return this._cleanupDeadModals();
  }

  /**
   * 🎛️ API PUBLIC - Update configurare
   *
   * @param {Object} newConfig - Obiect cu setări noi
   */
  updateConfig(newConfig) {
    if (!newConfig || typeof newConfig !== 'object') {
      this.log.error('❌ updateConfig: configurare invalidă');
      return;
    }

    const oldDebugMode = this.config.debugMode;

    // Merge config
    Object.assign(this.config, newConfig);

    // Restart cleanup dacă intervalul s-a schimbat
    if (newConfig.cleanupInterval !== undefined) {
      this._stopPeriodicCleanup();
      this._startPeriodicCleanup();
    }

    // Log doar dacă debug e activat (cu noua setare)
    if (this.config.debugMode) {
      this.log('🎛️ Configurare actualizată:', this.config);
    } else if (oldDebugMode) {
      // Ultima log înainte de a opri debug mode
      console.log('%c[OVERLAY] 🎛️ Debug mode oprit', 'color: #515151ff; font-weight: bold;');
    }
  }

  /**
   * 🧹 Cleanup complet
   */
  destroy() {
    this.log('🗑️ OverlayManager destroy...');

    // Stop periodic cleanup
    this._stopPeriodicCleanup();

    // Curăță stack-ul
    this.stack.forEach((entry) => {
      // Restaurează stilurile originale
      if (entry.modal) {
        entry.modal.style.zIndex = entry.elementZIndex || '';
      }
      if (entry.parent) {
        entry.parent.style.position = entry.parentPosition || '';
      }
    });
    this.stack = [];

    // Curăță clasele custom
    this.currentClasses.clear();

    // Ascunde overlay
    this._hideOverlay();

    // Cleanup listeners prin ListenerTracker
    this.cleanupAllListeners();

    // Reset cache
    this._cachedMaxZIndex = 0;

    // Reset metrics
    if (this.config.enableMetrics) {
      this.metrics = {
        totalSubscribes: 0,
        totalUnsubscribes: 0,
        currentStackSize: 0,
        maxStackSizeReached: 0,
        totalCleanups: 0,
        zIndexResets: 0,
      };
    }

    // Șterge referința la element și parent
    this.overlayElement = null;
    this.currentParent = null;

    this.log('✅ OverlayManager distrus');
  }

  /**
   * 📊 LOGGING cu suport debug mode
   */
  log = (() => {
    const fn = (message, data = null) => {
      if (this.config.debugMode) {
        const now = new Date();
        const ts = `${now.toLocaleTimeString('en-GB', { hour12: false })}.${now.getMilliseconds().toString().padStart(3, '0')}`;
        console.log(
          `%c[${ts}] [OVERLAY] ${message}`,
          'color: #515151ff; font-weight: bold;',
          data ?? ''
        );
      }
    };
    fn.error = (message, data = null) => {
      // Errors se loggă întotdeauna, indiferent de debug mode
      const now = new Date();
      const ts = `${now.toLocaleTimeString('en-GB', { hour12: false })}.${now.getMilliseconds().toString().padStart(3, '0')}`;
      console.error(
        `%c[${ts}] [OVERLAY] ${message}`,
        'color: #ef4444; font-weight: bold;',
        data ?? ''
      );
    };
    return fn;
  })();
}

// Export pentru utilizare
const overlayManager = new OverlayManager();
export default overlayManager;
