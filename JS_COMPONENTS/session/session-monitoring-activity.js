// js/session/session-monitoring-activity.js
/**
 * 👆 SESSION MONITORING ACTIVITY MIXIN
 * Gestionează activitatea utilizatorului și event listeners
 *
 * @version 3.0.0
 */

export const sessionMonitoringActivityMixin = {
  /**
   * 👆 Gestionează activitatea utilizatorului
   */
  handleUserActivity() {
    const now = Date.now();

    // Emit event pentru activitatea utilizatorului
    this.eventBus.emit(this.EVENTS.USER_ACTIVITY, {
      timestamp: now,
      inGracePeriod: this.inGracePeriod,
      extensionCount: this.extensionCount,
    });

    // DOAR în perioada de grație: încearcă prelungirea automată
    if (this.inGracePeriod && this.extensionCount < this.SESSION_CONFIG.MAX_EXTENSIONS) {
      // Ascunde warning-ul dacă e vizibil
      if (this.warningShown) {
        const modal = document.getElementById('session-warning-modal');
        if (modal) {
          const intervalId = modal.dataset.countdownInterval;
          if (intervalId) clearInterval(intervalId);
          modal.remove();
          this.warningShown = false;
        }
      }

      // Încearcă prelungirea automată
      this.attemptAutoExtension();
      this.lastActivitySent = now;
      return;
    }

    // ÎN AFARA perioadei de grație: doar înregistrează activitatea, NU reseta timerul
    if (!this.inGracePeriod) {
      // Ascunde warning-ul dacă e vizibil (pentru cazuri speciale)
      if (this.warningShown) {
        const modal = document.getElementById('session-warning-modal');
        if (modal) {
          const intervalId = modal.dataset.countdownInterval;
          if (intervalId) clearInterval(intervalId);
          modal.remove();
          this.warningShown = false;
        }
      }

      // DOAR actualizează timpul ultimei activități - NU reseta sessionStartTime
      this.lastActivitySent = now;

      this.log(
        `🔍 Activitate înregistrată la: ${new Date(now).toLocaleTimeString()} (timer continuă să curgă)`
      );

      // NU reprogramează warning-ul și expirarea - lasă timerul să curgă normal
    }
  },

  /**
   * 🎯 Setup activity listeners
   */
  setupActivityListeners() {
    this.SESSION_CONFIG.ACTIVITY_EVENTS.forEach((event) => {
      this.addDOMListener(document, event, this.handleUserActivity.bind(this), { passive: true });
    });

    this.log('✅ Activity listeners configurați');
  },

  /**
   * 🧹 Remove activity listeners
   */
  removeActivityListeners() {
    this.SESSION_CONFIG.ACTIVITY_EVENTS.forEach((event) => {
      document.removeEventListener(event, this.handleUserActivity.bind(this));
    });

    this.log('🧹 Activity listeners eliminați');
  },
};
