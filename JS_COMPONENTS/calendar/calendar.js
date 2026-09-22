/**
 * ========== CALENDAR.JS - COMPONENTA DE BAZĂ PENTRU CALENDAR ==========
 * Calendar individual - o instanță pentru un input specific
 * Conține toată logica de UI, validare și interacțiune
 *
 * @version 1.0.0
 */

//import '../../global-variables.js';

import eventBus, { EVENTS } from '../../event-bus/event-bus.js';
import ListenerTracker from '../../listener-tracker/listener-tracker-mixin.js';

import { CalendarCacheMixin } from './calendar-elements.js';
import { CalendarEventsMixin } from './calendar-events.js';
import { CalendarUIMixin } from './calendar-ui.js';
import { CalendarRenderMixin } from './calendar-render.js';
import { CalendarContainerMixin } from './calendar-container.js';
import { CalendarValidationMixin } from './calendar-validation.js';
import { CalendarTimeMixin } from './calendar-time.js';
import { CalendarEventsHandlersMixin } from './calendar-events-handlers.js';

export class Calendar {
  constructor(targetInput, options = {}) {
    this.debugMode = true;
    this.eventBus = eventBus;
    // Aplică ListenerTracker mixin
    ListenerTracker.applyTo(this, {
      debugMode: this.debugMode || false,
      logPrefix: 'Calendar',
      trackPerformance: true,
    });

    // 🎯 APLICĂ CALENDAR EVENTS MIXIN
    Object.assign(this, CalendarCacheMixin);
    Object.assign(this, CalendarEventsMixin);
    Object.assign(this, CalendarUIMixin);
    Object.assign(this, CalendarRenderMixin);
    Object.assign(this, CalendarContainerMixin);
    Object.assign(this, CalendarValidationMixin);
    Object.assign(this, CalendarTimeMixin);
    Object.assign(this, CalendarEventsHandlersMixin);

    const baseConfig = {
      dateFormat: 'YYYY-MM-DD',
      timeFormat: 'HH:mm',
      datetimeFormat: 'YYYY-MM-DD HH:mm',
      showTimeSelector: false, // se va seta mai jos
      defaultTime: '09:00',
      timeStep: 5,
      minTime: '08:00',
      maxTime: '18:00',
      showSeconds: false,
      minDate: null,
      maxDate: null,
      disabledDates: [],
      highlightToday: true,
      highlightWeekend: true,
      showWeekNumbers: false,
      customDate: false,
      customDateTime: false,
    };

    // Merge base config cu options primite
    this.config = {
      ...baseConfig,
      ...(options || {}),
    };

    // Setează showTimeSelector bazat pe tipul inputului (poate fi suprascris de options)
    if (!options?.hasOwnProperty('showTimeSelector')) {
      this.config.showTimeSelector = this.inputType === 'datetime-local';
    }

    // Input-ul țintă pentru acest calendar
    this.targetInput = targetInput; // Element DOM care tine data reală
    this.inputType = targetInput?.type || 'date'; // Tipul lui targetInput (text pentru data)
    this.fieldName = targetInput?.name || targetInput?.id || 'unknown'; // Nume câmp din MariaDB
    //this.isCustom_Date_Or_DateTime = options.customDateTime || options.customDate;

    // State management
    this.isVisible = false;
    this.currentDate = new Date();
    this.selectedDate = null;
    this.selectedTime = this.config.defaultTime;
    this.calendarElement = null;
    this.timePickerElement = null;
    this.modalElement = null;
    // this.overlayElement = null;
    this.dataCache = new Map();
    this.validationRules = [];
    this.areWindowEventListenersSet = false;
    this.areCustomDateEventsSet = false;
    this.areCustomTimeEventsSet = false;
    // this.areOverlayEventsSet = false;
    this.areModalEventsSet = false;
    this.areNavigationEventsSet = false;
    this.lastShownAt = 0;

    // Container variables
    this.useCustomInput = options?.customDateTime || options?.customDate || false;
    this.customContainer = null;
    this.customDateContainer = null;
    this.customTimeContainer = null;
    this.customButtonContainer = null;
    this.elements = {}; // pentru elemente create dinamic

    // Identificator unic pentru acest calendar
    this.instanceId = `calendar_${this.fieldName}_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;

    // Constante pentru luni și zile
    this.months = [
      'Ianuarie',
      'Februarie',
      'Martie',
      'Aprilie',
      'Mai',
      'Iunie',
      'Iulie',
      'August',
      'Septembrie',
      'Octombrie',
      'Noiembrie',
      'Decembrie',
    ];
    this.weekDays = ['L', 'M', 'M', 'J', 'V', 'S', 'D'];

    // Inițializare
    this.init();
  }

  /**
   * ⚡ INIȚIALIZARE CALENDAR
   */
  init() {
    try {
      this.log(`🚀 Inițializez calendar pentru ${this.fieldName} (${this.inputType})`);

      //this.createModalElement(); // nu il creez de la inceput
      this.loadDateTimeFromInput();
      // this.createCalendarOverlay();
      this.createCalendarElement();

      if (!this.customControls) this.transformToCustomControls();

      this.log('✅ Calendar inițializat cu succes');
    } catch (error) {
      this.log.error('Eroare la inițializarea calendarului', error);
    }
  }

  /**
   * ⚙️ ACTUALIZEAZĂ CONFIGURAȚIA
   */
  updateConfig(newConfig) {
    this.config = { ...this.config, ...newConfig };

    // Re-configurează pentru datetime dacă e necesar
    if (this.config.showTimeSelector) {
      this.customControls.hour.value = '00';
      this.customControls.minute.value = '00';
      //hide time selector
      if (this.timePickerElement) {
        this.timePickerElement.remove();
        this.timePickerElement = null;
      } else {
        // hide time selector and clear data
        this.customControls.container.classList.remove('datetime-mode');
        this.customControls.hour.value = '';
        this.customControls.minute.value = '';
      }
    }

    this.log(`⚙️ Configurație actualizată pentru ${this.fieldName}`);
  }

  /**
   * 🧹 ȘTERGE DATA SELECTATĂ
   */
  clearDate() {
    this.selectedDate = null;
    this.selectedTime = null;
  }

  /**
   * 📅 SETEAZĂ DATA PROGRAMATIC
   * Poziționează calendarul la data specificată și actualizează input-ul
   * @param {Date|string} date - Data de setat (Date object, DD/MM/YYYY, YYYY-MM-DD sau datetime formats)
   */
  setDate(date) {
    try {
      // Convertește parametrul în Date object
      let dateObj;
      let timeString = null;

      if (typeof date === 'string') {
        if (date.includes('T')) {
          // Format ISO cu timp: '2025-09-01T13:39:12'
          const [datePart, timePart] = date.split('T');
          dateObj = new Date(datePart);

          // Extrage doar HH:MM din timePart (ignoră secundele)
          const timeComponents = timePart.split(':');
          timeString = `${timeComponents[0].padStart(2, '0')}:${timeComponents[1].padStart(2, '0')}`;
        } else {
          // Format ISO doar dată: '2025-09-01'
          dateObj = new Date(date);
        }
      } else if (date instanceof Date) {
        dateObj = new Date(date);
        // Extrage timpul dacă e datetime-local
        if (this.inputType === 'datetime-local') {
          const hours = dateObj.getHours().toString().padStart(2, '0');
          const minutes = dateObj.getMinutes().toString().padStart(2, '0');
          timeString = `${hours}:${minutes}`;
        }
      } else {
        this.log.error('setDate: Parametru invalid - trebuie să fie Date sau string');
        return false;
      }

      // Verifică validitatea datei
      if (isNaN(dateObj.getTime())) {
        this.log.error('setDate: Dată invalidă după parsare');
        return false;
      }

      // Setează data selectată
      this.selectedDate = dateObj;

      // Setează luna curentă pentru a afișa data selectată
      this.currentDate = new Date(dateObj);

      // Setează timpul dacă e cazul
      if (this.targetInput.hasAttribute('data-custom-datetime')) {
        // Folosește timpul specificat sau timpul curent dacă nu e specificat
        if (timeString) {
          this.selectedTime = timeString;
        } else if (!this.selectedTime) {
          // Dacă nu avem timp selectat, folosește defaultTime din config
          this.selectedTime = this.config.defaultTime || '09:00';
        }

        // Actualizează selectoarele de timp dacă sunt vizibile
        if (this.timePickerElement) {
          this.setTime(this.selectedTime);
        }
      }

      // ✅ NOUA LOGICĂ: Actualizează controalele custom dacă există
      if (this.elements.dateContainer) {
        // Populează controalele de dată
        this.elements.dateContainer.day.value = dateObj.getDate().toString().padStart(2, '0');
        this.elements.dateContainer.month.value = (dateObj.getMonth() + 1)
          .toString()
          .padStart(2, '0');
        this.elements.dateContainer.year.value = dateObj.getFullYear();

        // Populează controalele de timp dacă există
        if (this.elements.timeContainer && this.selectedTime) {
          const [hours, minutes] = this.selectedTime.split(':');
          this.elements.timeContainer.hour.value = parseInt(hours, 10).toString().padStart(2, '0');

          // Pentru minute, verificăm dacă există în dropdown sau îl afișăm ca text
          const minuteValue = parseInt(minutes, 10).toString().padStart(2, '0');
          const minuteControl = this.elements.timeContainer.minute;

          if (minuteControl.tagName === 'SELECT') {
            // Dacă e dropdown, încearcă să selecteze valoarea
            const optionExists = Array.from(minuteControl.options).some(
              (option) => parseInt(option.value) === parseInt(minuteValue)
            );

            if (optionExists) {
              minuteControl.value = minuteValue;
            } else {
              // Dacă valoarea nu există în dropdown, o adaugă temporar
              const tempOption = document.createElement('option');
              tempOption.value = minuteValue;
              tempOption.textContent = minutes.padStart(2, '0');
              tempOption.selected = true;
              tempOption.style.color = '#007bff'; // Culoare diferită pentru a indica că nu e standard
              minuteControl.appendChild(tempOption);

              this.log(
                `⚠️ Valoarea ${minuteValue} pentru minute nu există în dropdown - adăugată temporar`
              );
            }
          } else {
            // Dacă e input normal
            minuteControl.value = minuteValue;
          }
        }

        // Construiește valoarea finală pentru input-ul ascuns (ÎNTOTDEAUNA în format ISO)
        let finalValue;
        if (this.inputType === 'datetime-local') {
          const dateString = this.formatDate(dateObj);
          finalValue = `${dateString}T${this.selectedTime}`;
        } else {
          finalValue = this.formatDate(dateObj);
        }

        // Actualizează input-ul ascuns (pentru compatibilitate cu restul sistemului)
        if (this.targetInput) {
          this.targetInput.value = finalValue;

          // Pentru afișare custom, poți adăuga un atribut data
          this.targetInput.setAttribute(
            'data-ro-date',
            this.formatDateRomanian(dateObj, this.selectedTime)
          );

          // Declanșează evenimentele necesare
          this.targetInput.dispatchEvent(new Event('input', { bubbles: true }));
          this.targetInput.dispatchEvent(new Event('change', { bubbles: true }));
        }
      } else {
        // Logica pentru input-uri normale (fără controale custom)
        let finalValue;
        if (this.inputType === 'datetime-local') {
          const dateString = this.formatDate(dateObj);
          finalValue = `${dateString}T${this.selectedTime}`;
        } else {
          finalValue = this.formatDate(dateObj);
        }

        // Actualizează input-ul țintă
        if (this.targetInput) {
          this.targetInput.value = finalValue;
          this.targetInput.setAttribute(
            'data-ro-date',
            this.formatDateRomanian(dateObj, this.selectedTime)
          );
          this.targetInput.dispatchEvent(new Event('input', { bubbles: true }));
          this.targetInput.dispatchEvent(new Event('change', { bubbles: true }));
        }
      }

      // Re-renderizează calendarul dacă e vizibil
      if (this.isVisible) {
        this.renderCalendar();

        // Actualizează și time picker-ul dacă există
        if (this.timePickerElement && this.selectedTime) {
          this.updateTimeDisplay();
        }
      }

      // Construiește valoarea finală pentru eveniment
      const finalEventValue =
        this.inputType === 'datetime-local'
          ? `${this.formatDate(dateObj)}T${this.selectedTime}`
          : this.formatDate(dateObj);

      this.log(
        `📅 Dată setată programatic: ${finalEventValue} (RO: ${this.formatDateRomanian(dateObj, this.selectedTime)})`
      );
      return true;
    } catch (error) {
      this.log.error('Eroare la setDate', error);
      return false;
    }
  }

  /**
   * 📤 OBȚINE VALOAREA CURENTĂ
   * Returnează valoarea din input-ul ascuns în format ISO
   */
  getValue() {
    if (!this.targetInput || !this.targetInput.value) {
      return null;
    }
    return this.targetInput.value;
  }

  /**
   * 📅 FORMATEAZĂ DATA ÎN FORMAT ROMÂNESC
   * Helper pentru afișare în format DD/MM/YYYY HH:mm
   */
  formatDateRomanian(date, timeString = null) {
    const day = date.getDate().toString().padStart(2, '0');
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const year = date.getFullYear();

    let formatted = `${day}/${month}/${year}`;

    if (timeString) {
      formatted += ` ${timeString}`;
    }

    return formatted;
  }

  /**
   * 📊 ÎNCARCĂ DATELE PENTRU CALENDAR
   */
  async loadCalendarData() {
    try {
      const year = this.currentDate.getFullYear();
      const month = this.currentDate.getMonth() + 1;

      const startDate = `${year}-${month.toString().padStart(2, '0')}-01`;
      const endDate = `${year}-${month.toString().padStart(2, '0')}-${new Date(year, month, 0).getDate()}`;

      this.log(`📊 Încarc date pentru perioada: ${startDate} - ${endDate}`);

      // TODO: Request către Python prin API
      // Pentru moment, simulează datele
      //const mockData = this.generateMockData(year, month);
      //this.processCalendarData(mockData);

      this.log('✅ Date calendar încărcate');
    } catch (error) {
      this.log.error('Eroare la încărcarea datelor calendar', error);
    }
  }

  /**
   * 📊 PROCESEAZĂ DATELE DIN API
   */
  processCalendarData(data) {
    this.dataCache.clear();

    data.forEach((item) => {
      this.dataCache.set(item.date, {
        count: item.count,
        details: item.details || [],
      });
    });

    // Re-renderizează zilele
    this.renderDays();
  }

  /**
   * 📅 FORMATEAZĂ DATA
   */
  formatDate(date) {
    const year = date.getFullYear();
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const day = date.getDate().toString().padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  /**
   * 🔍 HELPER FUNCTIONS
   */
  isSameDate(date1, date2) {
    return this.formatDate(date1) === this.formatDate(date2);
  }

  isWeekend(date) {
    const day = date.getDay();
    return day === 0 || day === 6;
  }

  isDisabledDate(date) {
    if (this.config.minDate && date < this.config.minDate) return true;
    if (this.config.maxDate && date > this.config.maxDate) return true;
    return this.config?.disabledDates?.some((disabledDate) =>
      this.isSameDate(date, new Date(disabledDate))
    );
  }

  /**
   * 🔒 ACTIVEAZĂ/DEZACTIVEAZĂ CALENDARUL
   */
  setEnabled(isEnabled = true) {
    try {
      if (isEnabled && this.targetInput.disabled === false) {
        this.log(`⚠️ Calendarul este deja activat pentru ${this.fieldName}`);
        return;
      } else if (!isEnabled && this.targetInput.disabled === true) {
        this.log(`⚠️ Calendarul este deja dezactivat pentru ${this.fieldName}`);
        return;
      }

      if (isEnabled) this.hide();

      this.isEnabled = isEnabled;

      // Activează calendarul
      this.targetInput.disabled = !isEnabled;
      this.targetInput.readOnly = !isEnabled;
      this.targetInput.classList.remove(`calendar-${isEnabled ? 'managed' : 'disabled'}`);
      this.targetInput.classList.add(`calendar-${!isEnabled ? 'managed' : 'disabled'}`);

      // Pentru custom controls container
      if (this.customContainer) {
        this.customContainer.classList.remove(`calendar-${!isEnabled ? 'managed' : 'disabled'}`);

        const inputs = this.customDateContainer.querySelectorAll('input');
        inputs.forEach((el) => {
          el.disabled = !isEnabled;
        });

        this.customButtonContainer.disabled = !isEnabled;

        if (isEnabled && !this.areCustomDateEventsSet) this.setupCustomControlsEvents(true, false);
        if (!isEnabled && this.areCustomDateEventsSet) this.clearCustomControlEvents(true, false);

        // Pentru controalele de timp, dacă există
        if (this.config.showTimeSelector && this.customTimeContainer) {
          const inputs = this.customTimeContainer.querySelectorAll('input');
          inputs.forEach((el) => {
            if (el?.nodeType) el.disabled = !isEnabled;
          });

          if (isEnabled && !this.areCustomTimeEventsSet)
            this.setupCustomControlsEvents(false, true);
          if (!isEnabled && this.areCustomTimeEventsSet) this.clearCustomControlEvents(false, true);
        }
      }

      this.log(`🔓 Calendar ${isEnabled ? 'activat' : 'dezactivat'} pentru ${this.fieldName}`);
    } catch (error) {
      this.log.error('Eroare la setEnabled', error);
    }
  }

  /**
   * 🧹 CLEANUP
   */
  // În calendar.js
  destroy() {
    this.targetInput.style.display = '';
    this.clearAllEventsListeners();
    this.clearDate();
    this.destroyCustomControls();
    this.destroyCalendarElement();
    this.destroyModalElement();
    // this.cleanup(); // Asta șterge TOȚI DOM listeners tracked

    this.log('🧹 Calendar destroyed');
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
        const CPN = `Calendar[${this.fieldName}]`.padEnd(15);
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
      const CPN = `Calendar[${this.fieldName}]`.padEnd(15);
      console.error(
        `%c[${ts}] [${CPN}] ${message}`,
        'color: #e74c3c; font-weight: bold;',
        data ?? ''
      );
    };

    return fn;
  })();
}

//import './calendar-events.js';
