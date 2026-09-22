/**
 * ========== CALENDAR-MANAGER.JS - MANAGER GLOBAL PENTRU CALENDARE ==========
 * Gestionează toate instanțele de calendar din aplicație
 * Configurări globale, persistență, API pentru alte module
 *
 * @version 1.0.0
 */

// //import '../../global-variables.js';
import eventBus, { EVENTS } from '../../event-bus/event-bus.js';
import ListenerTracker from '../../listener-tracker/listener-tracker-mixin.js';
import { Calendar } from './calendar.js';

export class CalendarManager {
  constructor() {
    // Singleton pattern
    if (CalendarManager.instance) {
      return CalendarManager.instance;
    }

    this.debugMode = false;

    // Aplică ListenerTracker mixin
    ListenerTracker.applyTo(this, {
      debugMode: this.debugMode || false,
      logPrefix: 'CalendarManager',
      trackPerformance: true,
    });

    // Management calendare
    this.calendars = new Map(); // Map<inputId, Calendar>
    this.activeCalendar = null;

    // Configurații globale per câmp
    this.globalFieldConfigurations = new Map();

    // Configurație default pentru câmpuri necunoscute
    this.defaultFieldConfig = {
      defaultTime: '09:00',
      timeStep: 15,
      minTime: '08:00',
      maxTime: '18:00',
      allowWeekends: true,
      allowPast: true,
      allowFuture: true,
      businessHoursOnly: false,
      validationRules: [],
      customMessages: {},
    };

    // Cache pentru date din server
    this.dataCache = new Map();
    this.dataCacheTimeout = 5 * 60 * 1000; // 5 minute

    this.lastEventRan = '';
    this.lastTimeEventWasRan = 0;

    CalendarManager.instance = this;
    this.init();
  }

  /**
   * ⚡ INIȚIALIZARE MANAGER
   */
  async init() {
    try {
      this.log('✅ CalendarManager inițializat cu succes');
    } catch (error) {
      this.handleError('Eroare la inițializarea CalendarManager', error);
    }
  }

  /**
   * 🕐 AFIȘEAZĂ SELECTORUL DE TIMP PENTRU UN CALENDAR
   * @param {HTMLElement|string} inputElement - Input-ul sau ID-ul acestuia
   */
  showTimeForCalendar(inputElement) {
    try {
      const inputId =
        typeof inputElement === 'string' ? inputElement : inputElement.id || inputElement.name;

      const calendar = this.calendars.get(inputId);

      if (calendar) {
        calendar.showTime();
        this.log(`🕐 Timp afișat pentru calendar: ${inputId}`);
      } else {
        this.log.error(`⚠️ Nu s-a găsit calendar pentru input: ${inputId}`);
      }
    } catch (error) {
      this.handleError('Eroare la showTimeForCalendar', error);
    }
  }

  /**
   * 🙈 ASCUNDE SELECTORUL DE TIMP PENTRU UN CALENDAR
   * @param {HTMLElement|string} inputElement - Input-ul sau ID-ul acestuia
   */
  hideTimeForCalendar(inputElement) {
    try {
      const inputId =
        typeof inputElement === 'string' ? inputElement : inputElement.id || inputElement.name;

      const calendar = this.calendars.get(inputId);

      if (calendar) {
        calendar.hideTime();
        this.log(`🙈 Timp ascuns pentru calendar: ${inputId}`);
      } else {
        this.log.error(`⚠️ Nu s-a găsit calendar pentru input: ${inputId}`);
      }
    } catch (error) {
      this.handleError('Eroare la hideTimeForCalendar', error);
    }
  }

  /**
   * 🔍 VERIFICĂ DACĂ INPUT-UL TREBUIE SĂ AIBĂ CALENDAR CUSTOM
   */
  shouldAttachCalendar(element) {
    return (
      element &&
      (element.type === 'date' ||
        element.type === 'datetime-local' ||
        element.classList.contains('use-custom-calendar')) &&
      !element.classList.contains('native-calendar') &&
      !element.disabled &&
      !element.readOnly
    );
  }

  /**
   * 👀 SETUP OBSERVER PENTRU INPUT-URI NOI
   */
  setupInputObserver() {
    const observer = new MutationObserver((mutations) => {
      mutations.forEach((mutation) => {
        mutation.addedNodes.forEach((node) => {
          if (node.nodeType === Node.ELEMENT_NODE) {
            // Verifică node-ul însuși
            if (this.shouldAttachCalendar(node)) {
              node.classList.add('calendar-managed');
              this.log(`👀 Input nou detectat: ${node.id || node.name}`);
            }

            // Verifică copiii node-ului
            const dateInputs =
              node.querySelectorAll &&
              node.querySelectorAll(
                'input[type="date"], input[type="datetime-local"], .use-custom-calendar'
              );
            if (dateInputs) {
              dateInputs.forEach((input) => {
                if (this.shouldAttachCalendar(input)) {
                  input.classList.add('calendar-managed');
                  this.log(`👀 Input nou detectat în subtree: ${input.id || input.name}`);
                }
              });
            }
          }
        });
      });
    });

    observer.observe(document.body, {
      childList: true,
      subtree: true,
    });

    this.observer = observer;
    this.log('👀 Observer pentru input-uri noi configurat');
  }

  /**
   * 📅 AFIȘEAZĂ CALENDAR PENTRU INPUT
   */
  showCalendarForInput(inputElement) {
    try {
      const inputId = inputElement.id || `input_${Date.now()}`;

      // Ascunde calendarul activ dacă există
      if (this.activeCalendar && this.activeCalendar !== this.calendars.get(inputId)) {
        this.activeCalendar.hide();
      }

      // Creează sau refolosește calendarul pentru acest input
      let calendar = this.calendars.get(inputId);

      if (!calendar) {
        calendar = this.createCalendarForInput(inputElement);
        this.calendars.set(inputId, calendar);
      }

      // Setează ca activ și afișează
      this.activeCalendar = calendar;
      calendar.show();

      this.log(`📅 Calendar afișat pentru input: ${inputElement.name || inputId}`);
    } catch (error) {
      this.handleError('Eroare la afișarea calendarului', error);
    }
  }

  /**
   * 🏭 CREEAZĂ CALENDAR PENTRU INPUT
   */
  createCalendarForInput(inputElement, fieldConfig = null, isEnabled = true) {
    if (!fieldConfig) {
      fieldConfig = this.defaultFieldConfig;
    }

    // Creează calendarul cu configurația specifică
    const calendar = new Calendar(inputElement, fieldConfig);

    // Activeaza / dezactiveaza inputul parinte pentru calendar
    calendar.setEnabled(isEnabled);

    // Configurează validările
    calendar.setValidationRules(fieldConfig.validationRules || []);

    //calendar.setupEventListeners();

    // Ataseaza clasa la inptul curent
    if (this.shouldAttachCalendar(inputElement)) {
      // Marchează input-ul
      inputElement.classList.add('calendar-managed');
    }

    this.log(`🏭 Calendar creat pentru câmpul: ${inputElement.name}`);

    // Adauga in cache this.calendars
    this.calendars.set(inputElement.id || inputElement.name, calendar);

    return calendar;
  }

  /**
   * Elimină calendarul asociat cu un input
   * @param {*} inputElement
   */
  removeCalendarForInput(inputElement) {
    try {
      const inputId = inputElement.id || inputElement.name;
      const calendar = this.calendars.get(inputId);
      if (calendar) {
        calendar.destroy();
        this.calendars.delete(inputId);
        this.log(`🗑️ Calendar eliminat pentru input: ${inputId}`);
      }
    } catch (error) {
      this.handleError('Eroare la eliminarea calendarului', error);
    }
  }

  /**
   * 🔒 ACTIVEAZA/DEZACTIVEAZA CALENDAR PENTRU UN INPUT
   */
  setCalendarEnabled(inputElement, isEnabled = true) {
    try {
      const inputId = inputElement.id || inputElement.name;
      const calendar = this.calendars.get(inputId);

      if (calendar) {
        calendar.setEnabled(isEnabled);
        this.log(`🔧 Calendar ${isEnabled ? 'activat' : 'dezactivat'} pentru input: ${inputId}`);
      } else {
        this.log(`⚠️ Nu s-a gasit calendar pentru input: ${inputId}`);
      }
    } catch (error) {
      this.handleError('Eroare la setCalendarEnabled', error);
    }
  }

  // setCalendarEnabled(inputElement, isEnabled = true) {
  //   try {
  //     const inputId = inputElement.id || inputElement.name;
  //     const calendar = this.calendars.get(inputId);

  //     if (calendar) {
  //       calendar.setEnabled(isEnabled);
  //       this.log(`🔧 Calendar ${isEnabled ? 'activat' : 'dezactivat'} pentru input: ${inputId}`);
  //     } else {
  //       this.log(`⚠️ Nu s-a gasit calendar pentru input: ${inputId}`);
  //     }
  //   } catch (error) {
  //     this.handleError('Eroare la setCalendarEnabled', error);
  //   }
  // }

  /**
   * 📡 SETUP EVENT LISTENERS PENTRU UN CALENDAR
   */
  setupCalendarEventListeners(calendar) {
    // Listener pentru selecția de dată
    calendar.addBusListener(calendar.customEvents.CALENDAR_DATE_SELECTED, (eventData) => {
      const { fieldName, datetime, isValid } = eventData.data;

      if (isValid) {
        this.log(`📅 Dată selectată: ${fieldName} = ${datetime}`);

        // Emit eveniment global pentru alte module
        eventBus.emit(EVENTS.CALENDAR_DATE_SELECTED, {
          fieldName,
          value: datetime,
          source: 'calendar-manager',
          timestamp: Date.now(),
        });
      }
    });

    // Listener pentru right-click pe zi
    calendar.addBusListener(calendar.customEvents.CALENDAR_DAY_RIGHT_CLICK, (eventData) => {
      const { date, hasData, fieldName } = eventData.data;

      this.log(`👆 Right-click pe ziua ${date} din câmpul ${fieldName}`);

      // Emit eveniment global
      eventBus.emit(EVENTS.CALENDAR_DAY_CONTEXT_MENU, {
        date,
        hasData,
        fieldName,
        source: 'calendar-manager',
        timestamp: Date.now(),
      });
    });

    // Listener pentru ascunderea calendarului
    calendar.addBusListener(calendar.customEvents.CALENDAR_HIDDEN, () => {
      if (this.activeCalendar === calendar) {
        this.activeCalendar = null;
      }
    });
  }

  /**
   * 📋 OBȚINE CONFIGURAȚIA PENTRU UN CÂMP
   */
  getFieldConfiguration(fieldName) {
    // Încearcă să găsească configurația exactă
    if (this.globalFieldConfigurations.has(fieldName)) {
      return { ...this.globalFieldConfigurations.get(fieldName) };
    }

    // Încearcă să găsească prin pattern matching
    for (const [configuredField, config] of this.globalFieldConfigurations) {
      if (
        fieldName.toLowerCase().includes(configuredField.toLowerCase()) ||
        configuredField.toLowerCase().includes(fieldName.toLowerCase())
      ) {
        this.log(`📋 Configurație găsită prin pattern: ${fieldName} -> ${configuredField}`);
        return { ...config };
      }
    }

    // Returnează configurația default
    this.log(`📋 Folosesc configurația default pentru: ${fieldName}`);
    return { ...this.defaultFieldConfig };
  }

  /**
   * ➕ API PENTRU CONFIGURĂRI DINAMICE
   */

  addFieldConfiguration(fieldName, config) {
    const fullConfig = { ...this.defaultFieldConfig, ...config };
    this.globalFieldConfigurations.set(fieldName, fullConfig);

    // Actualizează calendarul existent dacă există
    this.calendars.forEach((calendar) => {
      if (calendar.fieldName === fieldName) {
        calendar.updateConfig(fullConfig);
        calendar.setValidationRules(fullConfig.validationRules || []);
      }
    });

    if (this.globalFieldConfigurations.has(fieldName)) {
      return { ...this.globalFieldConfigurations.get(fieldName) };
    }
  }

  updateFieldConfiguration(fieldName, configUpdate) {
    const existingConfig = this.globalFieldConfigurations.get(fieldName) || {
      ...this.defaultFieldConfig,
    };
    const newConfig = { ...existingConfig, ...configUpdate };

    this.globalFieldConfigurations.set(fieldName, newConfig);

    // Actualizează calendarul existent
    this.calendars.forEach((calendar) => {
      if (calendar.fieldName === fieldName) {
        calendar.updateConfig(newConfig);
        calendar.setValidationRules(newConfig.validationRules || []);
      }
    });

    this.log(`🔄 Configurația pentru ${fieldName} actualizată`);
  }

  removeFieldConfiguration(fieldName) {
    const removed = this.globalFieldConfigurations.delete(fieldName);

    if (removed) {
      this.log(`🗑️ Configurația pentru ${fieldName} eliminată`);
    }

    return removed;
  }

  getConfigurationsAsObject() {
    const configs = {};
    for (const [fieldName, config] of this.globalFieldConfigurations) {
      configs[fieldName] = config;
    }
    return configs;
  }

  /**
   * 🔄 REFRESH TOATE CALENDARELE
   */
  refreshAllCalendars() {
    this.calendars.forEach((calendar) => {
      if (calendar.refreshCalendarData) {
        calendar.refreshCalendarData();
      }
    });

    // Refresh cache de date
    this.dataCache.clear();

    this.log('🔄 Toate calendarele au fost actualizate');
  }

  /**
   * 🧹 CLEANUP CALENDARE NEFOLOSITE
   */
  cleanupUnusedCalendars() {
    const toRemove = [];

    this.calendars.forEach((calendar, inputId) => {
      const inputElement =
        document.getElementById(inputId) || document.querySelector(`input[name="${inputId}"]`);

      if (!inputElement || !inputElement.isConnected) {
        calendar.destroy();
        toRemove.push(inputId);
      }
    });

    toRemove.forEach((inputId) => {
      this.calendars.delete(inputId);
    });

    if (toRemove.length > 0) {
      this.log(`🧹 ${toRemove.length} calendare nefolosite eliminate`);
    }
  }

  setWorkingHours(fieldName, startTime, endTime) {
    this.updateFieldConfiguration(fieldName, {
      minTime: startTime,
      maxTime: endTime,
      validationRules: ['business-hours'],
    });
  }

  setWeekendPolicy(fieldName, allowWeekends) {
    const currentRules = this.getFieldConfiguration(fieldName).validationRules || [];
    const newRules = allowWeekends
      ? currentRules.filter((rule) => rule !== 'no-weekends')
      : [...currentRules.filter((rule) => rule !== 'no-weekends'), 'no-weekends'];

    this.updateFieldConfiguration(fieldName, {
      allowWeekends: allowWeekends,
      validationRules: newRules,
    });
  }

  /**
   * 🔄 RESETEAZĂ LA CONFIGURAȚII DEFAULT
   */
  resetToDefaults() {
    this.globalFieldConfigurations.clear();
    localStorage.removeItem('calendar_field_configurations');

    this.loadDefaultConfigurations();

    // Actualizează toate calendarele existente
    this.calendars.forEach((calendar) => {
      const fieldConfig = this.getFieldConfiguration(calendar.fieldName);
      calendar.updateConfig(fieldConfig);
      calendar.setValidationRules(fieldConfig.validationRules || []);
    });

    this.log('🔄 Configurații resetate la default');
  }

  /**
   * 📊 LOGGING
   */
  log = (() => {
    const fn = (message, data = null) => {
      if (this.debugMode) {
        const now = new Date();
        const ts = `${now.toLocaleTimeString('en-GB', { hour12: false })}.${now
          .getMilliseconds()
          .toString()
          .padStart(3, '0')}`;
        const CPN = 'CalendarManager'.padEnd(15);
        console.log(
          `%c[${ts}] [${CPN}] ${message}`,
          'color: #9b59b6; font-weight: bold;',
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
      const CPN = 'CalendarManager'.padEnd(15);
      console.error(
        `%c[${ts}] [${CPN}] ${message}`,
        'color: #e74c3c; font-weight: bold;',
        data ?? ''
      );
    };

    return fn;
  })();

  /**
   * 🚨 GESTIONARE ERORI
   */
  handleError(message, error) {
    this.log.error(message, error);
  }

  /**
   * 🧹 CLEANUP COMPLET
   */
  destroy() {
    // Distruge toate calendarele
    this.calendars.forEach((calendar) => calendar.destroy());
    this.calendars.clear();

    // Cleanup observer
    if (this.observer) {
      this.observer.disconnect();
    }

    // Cleanup ListenerTracker
    this.cleanup();

    this.log('🧹 CalendarManager destroyed');
  }
}
