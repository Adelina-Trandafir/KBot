export const CalendarEventsHandlersMixin = {
  /**
   * 🔄 ACTUALIZEAZĂ INPUT-UL ASCUNS CU VALORILE DIN CONTROALE
   * ConstruiEște valoarea ISO din controalele custom și o setează în targetInput
   * ✅ Actualizează și starea internă a calendarului (selectedDate, currentDate, selectedTime)
   */
  updateHiddenInput() {
    // Verifică dacă există controale custom
    if (!this.elements.dateContainer) {
      this.log.error('updateHiddenInput: dateContainer nu există');
      return;
    }

    const { day, month, year } = this.elements.dateContainer;

    // Verifică dacă avem valori valide pentru dată
    if (!day?.value || !month?.value || !year?.value) {
      this.log('updateHiddenInput: Valori incomplete pentru dată');
      return;
    }

    // ✅ PARSEAZĂ VALORILE CA NUMERE
    const dayNum = parseInt(day.value, 10);
    const monthNum = parseInt(month.value, 10);
    const yearNum = parseInt(year.value, 10);

    // ✅ VALIDARE DE BUN SIMȚ
    if (dayNum < 1 || dayNum > 31 || monthNum < 1 || monthNum > 12 || yearNum < 1900) {
      this.log('updateHiddenInput: Valori invalide pentru dată');
      return;
    }

    // ✅ CREEAZĂ OBIECTUL DATE PENTRU STAREA INTERNĂ
    const dateObj = new Date(yearNum, monthNum - 1, dayNum);

    // Verifică dacă data e validă (ex: 31 februarie devine 3 martie)
    if (dateObj.getDate() !== dayNum || dateObj.getMonth() !== monthNum - 1) {
      this.log.error('updateHiddenInput: Data invalida (ex: 31 februarie)');
      return;
    }

    // ✅ ACTUALIZEAZĂ STAREA INTERNĂ A CALENDARULUI
    this.selectedDate = dateObj;
    this.currentDate = new Date(dateObj); // Pozitionează calendarul pe luna corectă

    // Construiește data în format ISO (YYYY-MM-DD)
    const isoDate = `${year.value}-${month.value.padStart(2, '0')}-${day.value.padStart(2, '0')}`;

    // Verifică dacă avem și controale de timp
    const hasTimeControls = !!this.elements.timeContainer;

    if (hasTimeControls) {
      const { hour, minute } = this.elements.timeContainer;

      if (hour?.value && minute?.value) {
        // ✅ PARSEAZĂ VALORILE DE TIMP
        const hourNum = parseInt(hour.value, 10);
        const minuteNum = parseInt(minute.value, 10);

        // Validare timp
        if (hourNum >= 0 && hourNum <= 23 && minuteNum >= 0 && minuteNum <= 59) {
          // Construiește timpul în format ISO (HH:mm)
          const isoTime = `${hour.value.padStart(2, '0')}:${minute.value.padStart(2, '0')}`;

          // ✅ ACTUALIZEAZĂ TIMPUL SELECTAT
          this.selectedTime = isoTime;

          // Setează datetime complet
          this.targetInput.value = `${isoDate}T${isoTime}`;
        } else {
          this.log.error('updateHiddenInput: Valori invalide pentru timp');
          return;
        }
      } else {
        // Are controale de timp dar sunt goale - setează doar data
        this.targetInput.value = isoDate;
      }
    } else {
      // Nu are controale de timp - setează doar data
      this.targetInput.value = isoDate;
    }

    // Trigger change event pentru validări/listeners externi
    this.targetInput.dispatchEvent(new Event('change', { bubbles: true }));

    this.log(
      `🔄 Hidden input actualizat: ${this.targetInput.value} | selectedDate: ${this.selectedDate.toISOString()}`
    );
  },
  /**
   * 🔧 MODIFICA VALOAREA PENTRU UN INPUT
   */
  changeValue(direction, input) {
    const current = parseInt(input.value) || 1;
    let next = current + direction;
    next = Math.max(input.min ? parseInt(input.min) : 1, Math.min(parseInt(input.max), next));
    input.value = next.toString().padStart(2, '0');
  },

  /**
   * ⬅️ NAVIGEAZĂ LA LUNA PRECEDENTĂ/URMĂTOARE
   */
  navigateMonth(direction) {
    const newDate = new Date(this.currentDate);
    newDate.setMonth(newDate.getMonth() + direction);
    this.currentDate = newDate;

    this.renderCalendar();
    this.loadCalendarData();

    this.log(
      `⬅️ Navigat la ${direction > 0 ? 'următoare' : 'precedentă'}: ${this.months[newDate.getMonth()]} ${newDate.getFullYear()}`
    );
  },

  /**
   * ⬅️ NAVIGEAZĂ LA ANUL PRECEDENT/URMĂTOR
   */
  navigateYear(direction) {
    const newDate = new Date(this.currentDate);
    newDate.setFullYear(newDate.getFullYear() + direction);
    this.currentDate = newDate;

    this.renderCalendar();
    this.loadCalendarData();

    this.log(
      `⬅️ Navigat la anul ${direction > 0 ? 'următor' : 'precedent'}: ${newDate.getFullYear()}`
    );
  },

  /**
   * 📅 MERGI LA ZIUA DE ASTĂZI
   */
  goToToday() {
    this.currentDate = new Date();

    if (this.inputType === 'datetime-local') {
      const now = new Date();
      const currentTime = `${now.getHours().toString().padStart(2, '0')}:${now.getMinutes().toString().padStart(2, '0')}`;
      this.setTime(currentTime);
    }

    this.renderCalendar();
    this.loadCalendarData();
    this.log('📅 Navigat la astăzi');
  },

  /**
   * 🗑️ ȘTERGE SELECȚIA
   */
  clearSelection() {
    this.selectedDate = null;
    this.selectedTime = this.config.defaultTime;

    if (this.targetInput) {
      this.targetInput.value = '';
      this.targetInput.dispatchEvent(new Event('change'));
    }

    if (this.timePickerElement) {
      this.setTime(this.config.defaultTime);
    }

    this.renderCalendar();
    this.clearValidationErrors();
    this.log('🗑️ Selecție ștearsă');
  },

  /**
   * ✅ SELECTEAZĂ O DATĂ (cu validare completă)
   */
  selectDate(dateString) {
    const date = new Date(dateString);

    const validation = this.validateDateTime(dateString, this.selectedTime);
    if (!validation.isValid) {
      this.showValidationErrors(validation.errors);
      return false;
    }

    this.selectedDate = date;

    // ADAUGĂ: Actualizează controalele custom dacă există
    if (this.elements.dateContainer) {
      this.updateCustomControls(dateString, this.selectedTime);
    } else {
      // Comportament normal pentru input standard
      let finalValue;
      if (this.inputType === 'datetime-local') {
        finalValue = `${dateString}T${this.selectedTime}`;
      } else {
        finalValue = dateString;
      }

      if (this.targetInput) {
        this.targetInput.value = finalValue;
        this.targetInput.dispatchEvent(new Event('change'));
      }
    }

    this.renderCalendar();

    // Ascunde pentru date simple
    if (this.inputType === 'date') {
      setTimeout(() => this.hide(), 150);
    }

    this.eventBus.emit('calendar-date-selected', {
      date: dateString,
      time: this.selectedTime,
      fieldName: this.fieldName,
      timestamp: Date.now(),
      instanceId: this.instanceId,
    });
    return true;
  },

  /**
   * 🎭 AFIȘEAZĂ DETALIILE UNEI ZILE
   */
  showDayDetails(dateString) {
    const dayData = this.dataCache.get(dateString);
    const date = new Date(dateString);

    const title = this.modalElement.querySelector('.calendar-modal-title');
    title.textContent = `Detalii pentru ${date.getDate()} ${this.months[date.getMonth()]} ${date.getFullYear()}`;

    const detailsContainer = this.modalElement.querySelector('.calendar-day-details');

    if (dayData && dayData.count > 0) {
      detailsContainer.innerHTML = `
        <div class="calendar-day-summary">
          <strong>Total înregistrări: ${dayData.count}</strong>
        </div>
        <div class="calendar-day-list">
          ${dayData.details
            .map(
              (detail) => `
            <div class="calendar-day-item">
              <span class="calendar-day-time">${detail.time}</span>
              <span class="calendar-day-description">${detail.description}</span>
            </div>
          `
            )
            .join('')}
        </div>
      `;
    } else {
      detailsContainer.innerHTML = `
        <div class="calendar-no-data">
          Nu există înregistrări pentru această zi.
        </div>
      `;
    }

    this.modalElement.classList.remove('hidden');

    eventBus.emit(this.customEvents.CALENDAR_DAY_RIGHT_CLICK, {
      date: dateString,
      hasData: !!(dayData && dayData.count > 0),
      fieldName: this.fieldName,
      timestamp: Date.now(),
    });

    this.log(`🎭 Afișez detalii pentru ziua: ${dateString}`);
  },

  /**
   * 🔄 UPDATE SELECTED TIME
   */
  updateSelectedTime() {
    const hourSelect = this.timePickerElement.querySelector('.calendar-hour-select');
    const minuteSelect = this.timePickerElement.querySelector('.calendar-minute-select');

    const hour = hourSelect.value.padStart(2, '0');
    const minute = minuteSelect.value.padStart(2, '0');

    this.selectedTime = `${hour}:${minute}`;
    this.updateTimeDisplay();

    this.log(`🕐 Timp actualizat: ${this.selectedTime}`);
  },

  /**
   * 🕐 SET TIME
   */
  setTime(timeString) {
    this.selectedTime = timeString;

    const [hour, minute] = timeString.split(':');
    const hourSelect = this.timePickerElement?.querySelector('.calendar-hour-select');
    const minuteSelect = this.timePickerElement?.querySelector('.calendar-minute-select');

    if (hourSelect) hourSelect.value = parseInt(hour);
    if (minuteSelect) minuteSelect.value = parseInt(minute);

    this.updateTimeDisplay();
  },
};
