// calendar-elements-mixin.js
export const CalendarCacheMixin = {
  /**
   * 🧩 CACHE TOATE ELEMENTELE DIN DOM PENTRU ACCES RAPID
   */
  cacheElements() {
    this.elements = {
      calendar: {
        root: this.calendarElement,
        todayBtn: this.calendarElement.querySelector('.calendar-today-btn'),
        clearBtn: this.calendarElement.querySelector('.calendar-clear-btn'),
        closeBtn: this.calendarElement.querySelector('.calendar-close-btn'),
        hour: this.calendarElement.querySelector('.calendar-hour-select'),
        minute: this.calendarElement.querySelector('.calendar-minute-select'),
        presets: this.calendarElement.querySelectorAll('.calendar-time-preset'),
        nowBtn: this.calendarElement.querySelector('.calendar-time-now'),
      },
    };

    // Handle custom vs native input
    if (this.useCustomInput) {
      this.elements.inputContainer =
        this.targetInput.parentElement.querySelector('.custom-datetime-container') ||
        this.targetInput.parentElement.querySelector('.custom-date-container');
    } else {
      this.elements.input = this.targetInput;
    }
  },

  clearElements() {
    this.elements = {};
  },

  cacheModalElements() {
    this.elements = {
      modal: {
        root: this.modalElement,
        closeBtns: this.modalElement.querySelectorAll(
          '.calendar-modal-close, .calendar-modal-close-btn'
        ),
        //overlay: this.modalElement.querySelector('.overlay'),
      },
    };
  },

  clearModalElements() {
    if (this.elements.modal) {
      this.elements.modal = null;
    }
  },

  cacheCalendarButtonsElements() {
    this.elements = {
      calendar: {
        root: this.calendarElement,
        todayBtn: this.calendarElement.querySelector('.calendar-today-btn'),
        clearBtn: this.calendarElement.querySelector('.calendar-clear-btn'),
        closeBtn: this.calendarElement.querySelector('.calendar-close-btn'),
      },
    };
  },

  clearCalendarButtonsElements() {
    if (this.elements.calendar) {
      this.elements.calendar = null;
    }
  },

  cacheCalendarTimeElements() {
    this.elements = {
      time: {
        root: this.timePickerElement,
        hour: this.timePickerElement.querySelector('.calendar-hour-select'),
        minute: this.timePickerElement.querySelector('.calendar-minute-select'),
        presets: this.timePickerElement.querySelectorAll('.calendar-time-preset'),
        nowBtn: this.timePickerElement.querySelector('.calendar-time-now'),
      },
    };
  },

  clearTimeElements() {
    if (this.elements.time) {
      this.elements.time = null;
    }
  },

  /**
   * 🧩 CACHE ELEMENTELE CONTAINER ÎN this.elements
   * Adaugă referințe la controalele custom fără să suprascrie cache-urile existente
   * @param {boolean} forDate - Cache-ază controalele de dată
   * @param {boolean} forTime - Cache-ază controalele de timp
   */
  cacheContainerElements(forDate = true, forTime = false) {
    // Inițializează this.elements dacă nu există
    if (!this.elements) {
      this.elements = {};
    }

    // ✅ Cache controale dată (ADAUGĂ, nu suprascrie)
    if (forDate && this.customDateContainer) {
      this.elements.dateContainer = {
        root: this.customDateContainer,
        day: this.customDateContainer.querySelector('.custom-date-day'),
        month: this.customDateContainer.querySelector('.custom-date-month'),
        year: this.customDateContainer.querySelector('.custom-date-year'),
      };

      this.elements.button = {
        root: this.customButtonContainer,
      };

      this.log('🧩 Cache dată actualizat');
    }

    // ✅ Cache controale timp (ADAUGĂ, nu suprascrie)
    if (forTime && this.customTimeContainer) {
      this.elements.timeContainer = {
        root: this.customTimeContainer,
        hour: this.customTimeContainer.querySelector('.custom-time-hour'),
        minute: this.customTimeContainer.querySelector('.custom-time-minute'),
      };

      this.log('🧩 Cache timp actualizat');
    }
  },

  /**
   * 🧹 ȘTERGE REFERINȚELE DIN CACHE PENTRU CONTROALE CONTAINER
   * Șterge selectiv doar controalele container, păstrează celelalte cache-uri
   * @param {boolean} clearDate - Șterge cache-ul pentru controale dată
   * @param {boolean} clearTime - Șterge cache-ul pentru controale timp
   */
  clearContainerElements(clearDate = true, clearTime = false) {
    if (clearDate && this.elements.dateContainer) {
      this.elements.dateContainer = null;
    }

    if (clearDate && this.elements.button) {
      this.elements.button = null;
    }

    if (clearTime && this.elements.timeContainer) {
      this.elements.timeContainer = null;
    }

    // ✅ NU mai șterge complet this.elements
    // Păstrează calendar, modal, și alte cache-uri

    this.log('🧹 Cache controale container șters');
  },
};
