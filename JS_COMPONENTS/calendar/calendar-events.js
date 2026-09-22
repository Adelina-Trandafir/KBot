/**
 * ========== CALENDAR-EVENTS.JS - MIXIN PENTRU EVENIMENTE CONTROALE CUSTOM ==========
 * Injectează metode pentru gestionarea evenimentelor în clasa Calendar
 *
 * @version 1.0.0
 */

export const CalendarEventsMixin = {
  clearAllEventsListeners() {
    this.clearCustomControlEvents();
    // this.clearOverlayEvents();
    this.clearModalListenersEvents();
    this.clearCalendarNavigationEvents();
    this.clearTimeSelectorEvents();
  },

  clearCustomControlEvents(clearDateEvents = true, clearTimeEvents = true) {
    if (!this.areCustomDateEventsSet && !this.areCustomTimeEventsSet) return;

    if (clearDateEvents && this.areCustomDateEventsSet) {
      if (!this.customDateContainer) {
        this.log.error('customDateContainer este null, dar areCustomDateEventsSet = true');
        this.areCustomDateEventsSet = false;
        return;
      }

      const { day, month, year } = this.customDateContainer;
      const button = this.customButtonContainer;

      if (day) this.removeDOMListener(day);
      if (month) this.removeDOMListener(month);
      if (year) this.removeDOMListener(year);
      if (button) this.removeDOMListener(button);

      this.areCustomDateEventsSet = false;
    }

    if (clearTimeEvents && this.areCustomTimeEventsSet) {
      if (!this.customTimeContainer) {
        this.log.error('customTimeContainer este null, dar areCustomTimeEventsSet = true');
        this.areCustomTimeEventsSet = false;
        return;
      }

      const { hour, minute } = this.customTimeContainer;

      if (hour) this.removeDOMListener(hour);
      if (minute) this.removeDOMListener(minute);

      this.areCustomTimeEventsSet = false;
    }
  },

  // clearOverlayEvents() {
  //   if (!this.areOverlayEventsSet) return;
  //   this.removeDOMListener(this.overlayElement);
  //   this.areOverlayEventsSet = false;
  // },

  clearModalListenersEvents() {
    if (!this.areModalEventsSet) return;
    this.removeDOMListener(this.elements.modal.closeBtns);
    this.areModalEventsSet = false;
  },

  clearCalendarNavigationEvents() {
    if (!this.areNavigationEventsSet) return;
    if (!this.calendarElement) return;
    if (!this.elements || !this.elements.calendar) return;
    this.removeDOMListener(this.elements.calendar.prevYearBtn);
    this.removeDOMListener(this.elements.calendar.nextYearBtn);
    this.removeDOMListener(this.elements.calendar.prevMonthBtn);
    this.removeDOMListener(this.elements.calendar.nextMonthBtn);
    this.removeDOMListener(this.elements.calendar.todayBtn);
    this.removeDOMListener(this.elements.calendar.clearBtn);
    this.removeDOMListener(this.elements.calendar.closeBtn);
    this.removeDOMListener(this.calendarElement);
    this.removeDOMListener(document);
    this.areNavigationEventsSet = false;
  },

  clearTimeSelectorEvents() {
    if (!this.timePickerElement) return;
    this.removeDOMListener(this.timePickerElement);
    this.areCustomTimeEventsSet = false;
  },

  /**
   * 📡 SETUP EVENT LISTENERS PENTRU CONTROALE CUSTOM
   * Setează evenimente pentru input-urile custom de dată/timp
   * @param {boolean} forDate - Setează evenimente pentru controale dată
   * @param {boolean} forTime - Setează evenimente pentru controale timp
   */
  setupCustomControlsEvents(forDate = true, forTime = true) {
    // ✅ Verifică și ajustează flag-urile individual
    if (forDate && this.areCustomDateEventsSet) {
      forDate = false; // Skip - deja setate
    }

    if (forTime && this.areCustomTimeEventsSet) {
      forTime = false; // Skip - deja setate
    }

    // Dacă ambele sunt false, nu mai e nimic de făcut
    if (!forDate && !forTime) {
      this.log('📡 Evenimente deja setate, skip');
      return;
    }

    let day, month, year, hour, minute, button;

    // Destructurare controale custom pentru dată și buton
    if (forDate && this.elements.dateContainer) {
      ({ day, month, year } = this.elements.dateContainer);
      button = this.customButtonContainer;
    }

    // Destructurare controale custom pentru timp dacă e cazul
    if (forTime && this.elements.timeContainer) {
      ({ hour, minute } = this.elements.timeContainer);
    }

    let touchStartY = 0;

    // Configurare generică pentru toate câmpurile
    const fieldConfigs = [
      { el: day, maxLen: 2, next: month },
      { el: month, maxLen: 2, next: year },
      { el: year, maxLen: 4, next: hour },
      { el: hour, maxLen: 2, next: minute },
      { el: minute, maxLen: 2, next: null },
    ];

    // Setare evenimente pentru fiecare câmp existent
    fieldConfigs.forEach(({ el, maxLen, next }) => {
      if (!el) return;

      // input -> auto-advance + hidden update
      this.addDOMListener(el, 'input', (e) => {
        if (maxLen && e.target.value.length === maxLen && next) next.focus();
      });

      // DEZACTIVAT - prea sensibil - daca vreau scroll pe fiecare element al datei/timpului
      // // wheel -> increment/decrement
      // this.addDOMListener(el, 'wheel', (e) => {
      //   e.preventDefault();
      //   this.changeValue(e.deltaY < 0 ? 1 : -1, el);
      // });

      // // swipe -> increment/decrement
      // this.addDOMListener(el, 'touchstart', (e) => {
      //   touchStartY = e.touches[0].clientY;
      // });

      // this.addDOMListener(el, 'touchend', (e) => {
      //   const deltaY = touchStartY - e.changedTouches[0].clientY;
      //   if (Math.abs(deltaY) >= 20) {
      //     this.changeValue(deltaY > 0 ? 1 : -1, el);
      //   }
      // });

      // numeric validation
      this.addDOMListener(el, 'keypress', (e) => {
        if (!/\d/.test(e.key) && e.key !== 'Backspace' && e.key !== 'Tab') {
          e.preventDefault();
        }
      });

      // ✅ FOCUS - selectează tot textul + styling
      this.addDOMListener(el, 'focus', (e) => {
        el.style.borderColor = '#80bdff';
        e.target.select(); // Selectează automat tot textul
      });

      // ✅ CLICK - selectează textul (pentru siguranță pe toate browserele)
      this.addDOMListener(el, 'click', (e) => {
        e.target.select();
      });

      // BLUR - remove styling
      this.addDOMListener(el, 'blur', () => {
        this.updateHiddenInput();
        el.style.borderColor = '#ced4da';
        el.style.boxShadow = 'none';
      });
    });

    // Calendar button
    if (button) {
      this.addDOMListener(button, 'click', () => {
        this.show();
      });
    }

    // ✅ Setează flag-urile individual
    if (forDate) {
      this.areCustomDateEventsSet = true;
      this.log('📡 Evenimente controale dată setate');
    }

    if (forTime) {
      this.areCustomTimeEventsSet = true;
      this.log('📡 Evenimente controale timp setate');
    }
  },

  setupModalListeners() {
    // Pentru fereastra care afiseaza detaliile pentru data selectata
    this.elements.modal.closeBtns.forEach((btn) => {
      this.addDOMListener(btn, 'click', () => this.hideModal());
    });
    this.areModalEventsSet = true;
  },

  setupCalendarNavigation() {
    // ✅ FIX: UN SINGUR listener de click pentru tot calendarul
    this.addDOMListener(this.calendarElement, 'click', (e) => {
      // Verifică butoanele de navigare
      const action = e.target.closest('[data-action]')?.dataset.action;
      if (action) {
        switch (action) {
          case 'prev-year':
            this.navigateYear(-1);
            return;
          case 'next-year':
            this.navigateYear(1);
            return;
          case 'prev-month':
            this.navigateMonth(-1);
            return;
          case 'next-month':
            this.navigateMonth(1);
            return;
        }
      }

      // Verifică zilele calendarului
      const dayElement = e.target.closest('.calendar-day');
      if (dayElement && !dayElement.classList.contains('disabled')) {
        this.selectDate(dayElement.dataset.date);
        this.hide();
        return;
      }
    });

    // Butoanele din footer
    this.addDOMListener(this.elements.calendar.todayBtn, 'click', () => this.goToToday());
    this.addDOMListener(this.elements.calendar.clearBtn, 'click', () => this.clearSelection());
    this.addDOMListener(this.elements.calendar.closeBtn, 'click', () => this.hide());

    // Right-click pe zilele calendarului
    this.addDOMListener(this.calendarElement, 'contextmenu', (e) => {
      const dayElement = e.target.closest('.calendar-day');
      if (dayElement && !dayElement.classList.contains('disabled')) {
        e.preventDefault();
        this.showDayDetails(dayElement.dataset.date);
      }
    });

    // Esc key to close
    this.addDOMListener(document, 'keydown', (e) => {
      if (e.key === 'Escape' && this.isVisible) {
        e.preventDefault();
        e.stopPropagation();
        this.hide();
      }
    });

    this.areNavigationEventsSet = true;
  },

  setupTimeSelectorEvents() {
    const hourSelect = this.timePickerElement.querySelector('.calendar-hour-select');
    const minuteSelect = this.timePickerElement.querySelector('.calendar-minute-select');
    const presets = this.timePickerElement.querySelectorAll('.calendar-time-preset');
    const nowBtn = this.timePickerElement.querySelector('.calendar-time-now');

    // Set current values
    if (hourSelect && minuteSelect) {
      const [currentHour, currentMinute] = this.selectedTime.split(':');
      hourSelect.value = parseInt(currentHour);
      minuteSelect.value = parseInt(currentMinute);
    }

    // Hour selection
    this.addDOMListener(hourSelect, 'change', () => {
      this.updateSelectedTime();
    });

    // Minute selection
    this.addDOMListener(minuteSelect, 'change', () => {
      this.updateSelectedTime();
    });

    // Preset buttons
    presets.forEach((preset) => {
      this.addDOMListener(preset, 'click', (e) => {
        e.preventDefault();
        const time = preset.dataset.time;
        this.setTime(time);
      });
    });
  },

  // setupOverlayListener() {
  //   if (this.areOverlayEventsSet) return;
  //   this.addDOMListener(this.overlayElement, 'click', () => {
  //     this.hide();
  //   });
  //   this.areOverlayEventsSet = true;
  // },
};
