export const CalendarTimeMixin = {
  /**
   * 🕐 AFIȘEAZĂ SELECTORUL DE TIMP
   */
  showTime() {
    if (!this.targetInput) {
      this.log.error('Nu există targetInput pentru a afișa timpul');
      return;
    }

    if (this.customTimeContainer) return;

    // Marchează că trebuie să arate timp
    this.config.customDate = false;
    this.config.customDateTime = true;

    this.targetInput.setAttribute('data-custom-datetime', 'true');
    this.targetInput.removeAttribute('data-custom-date');
    this.targetInput.setAttribute('data-input-type', 'datetime-local');

    this.createCustomTimeContainer();
    this.setupCustomControlsEvents();
    // Setează o valoare default pentru timp dacă nu există
    // if (!this.customControls.hour.value) {
    //   const [hours, minutes] = this.config.defaultTime.split(':');
    //   this.customControls.hour.value = hours.padStart(2, '0');
    //   this.customControls.minute.value = minutes.padStart(2, '0');
    // }

    // Dacă calendarul e vizibil, adaugă time selector
    if (this.isVisible && !this.timePickerElement) {
      this.calendarElement.classList.add('datetime-mode');
      this.setupTimeSelector();
    }

    this.log('🕐 Selector de timp afișat');
  },

  /**
   * 🙈 ASCUNDE SELECTORUL DE TIMP
   */
  hideTime() {
    if (!this.targetInput) {
      this.log.error('Nu există targetInput pentru a ascunde timpul');
      return;
    }

    if (!this.customTimeContainer) return;

    this.clearCustomControlEvents(false, true);

    // Marchează că NU trebuie să arate timp
    this.config.showTimeSelector = false;
    this.targetInput.removeAttribute('data-custom-datetime');
    this.targetInput.setAttribute('data-custom-date', 'true');
    this.targetInput.setAttribute('data-input-type', 'date');

    // Șterge container-ul de timp custom
    this.customTimeContainer.remove();
    this.customTimeContainer = null;

    // Șterge time selector-ul din calendar dacă există
    if (this.timePickerElement) {
      this.timePickerElement.remove();
      this.timePickerElement = null;
    }

    if (this.isVisible) {
      this.calendarElement.classList.remove('datetime-mode');
    }
    // Actualizează controalele custom dacă există
    // if (this.customControls) {
    //   this.customControls.includeTime = false;

    //   // Sterge controalele de timp
    //   const timeContainer = this.customInputTime;
    //   if (timeContainer) {
    //     timeContainer.remove();
    //   }

    //   // Golește valorile de timp
    //   if (this.customControls.hour) this.customControls.hour.value = '';
    //   if (this.customControls.minute) this.customControls.minute.value = '';
    // }

    this.log('🙈 Selector de timp ascuns');
  },

  /**
   * 🔄 RECREEAZĂ CONTROALELE CUSTOM CU TIMP
   * Helper pentru showTime() când trebuie să adauge controale de timp
   */
  createCustomTimeContainer() {
    if (this.customTimeContainer) return;

    let hourInput, minuteInput;
    if (this.config.customDateTime) {
      this.customTimeContainer = document.createElement('div');
      this.customTimeContainer.className = this.customContainer.className + '-time';

      hourInput = document.createElement('input');
      hourInput.type = 'number';
      hourInput.id = 'ora_' + this.instanceId;
      hourInput.className = 'custom-time-hour';
      hourInput.placeholder = 'HH';
      hourInput.maxLength = 2;
      hourInput.min = 0;
      hourInput.max = 23;

      minuteInput = document.createElement('input');
      minuteInput.type = 'number';
      minuteInput.id = 'minut_' + this.instanceId;
      minuteInput.className = 'custom-time-minute';
      minuteInput.placeholder = 'MM';
      minuteInput.maxLength = 2;
      minuteInput.min = 0;
      minuteInput.max = 59;

      const sepTime = document.createElement('span');
      sepTime.textContent = ':';

      this.customTimeContainer.appendChild(hourInput);
      this.customTimeContainer.appendChild(sepTime);
      this.customTimeContainer.appendChild(minuteInput);

      // Adaugă partea de timp după dată, dar inaintea butonului
      this.customContainer.insertBefore(this.customTimeContainer, this.customButtonContainer);

      this.cacheContainerElements(false, true);
    }

    this.log('🔄 Controale de timp recreate');
  },

  /**
   * 🕐 SETUP TIME SELECTOR PENTRU CALENDAR
   */
  setupTimeSelector() {
    if (this.timePickerElement) return;

    const timeSelector = document.createElement('div');
    timeSelector.className = 'calendar-time-selector';
    timeSelector.innerHTML = this.getTimeSelectorTemplate();

    // Inserează înaintea footer-ului
    const footer = this.calendarElement.querySelector('.calendar-footer');
    footer.parentNode.insertBefore(timeSelector, footer);

    this.timePickerElement = timeSelector;
    this.setupTimeSelectorEvents();
    this.updateTimeDisplay();

    this.log('🕐 Time selector configurat');
  },

  /**
   * 🧩 TEMPLATE TIME SELECTOR
   */
  getTimeSelectorTemplate() {
    return `
      <label id="calendarHourLabel_${this.instanceId}">Ora:</label>
      <div id="calendarTimeSelector_${this.instanceId}" class="calendar-time-controls">
        <div id="calendarTimeInputGroup_${this.instanceId}" class="calendar-time-input-group">
          <select id="calendarHourSelect_${this.instanceId}" class="calendar-hour-select">
            ${this.generateHourOptions()}
          </select>
          <span class="calendar-time-separator">:</span>
          <select id="calendarMinuteSelect_${this.instanceId}" class="calendar-minute-select">
            ${this.generateMinuteOptions()}
          </select>
        </div>
      </div>
    `;
  },

  /**
   * 🔢 GENERATE HOUR OPTIONS
   */
  generateHourOptions() {
    const [minHour] = this.config.minTime.split(':').map(Number);
    const [maxHour] = this.config.maxTime.split(':').map(Number);

    let options = '';
    for (let hour = minHour; hour <= maxHour; hour++) {
      const value = hour.toString().padStart(2, '0');
      options += `<option value="${hour}">${value}</option>`;
    }
    return options;
  },

  /**
   * 🔢 GENERATE MINUTE OPTIONS
   */
  generateMinuteOptions() {
    const step = this.config.timeStep;
    let options = '';

    for (let minute = 0; minute < 60; minute += step) {
      const value = minute.toString().padStart(2, '0');
      options += `<option value="${minute}">${value}</option>`;
    }
    return options;
  },

  /**
   * 🎨 UPDATE TIME DISPLAY
   */
  updateTimeDisplay() {
    const display = this.timePickerElement?.querySelector('.calendar-selected-time');
    if (display) {
      display.textContent = this.selectedTime;
    }
  },

  /**
   * 🔧 PARSE TIMP ÎN MINUTE
   */
  parseTime(timeString) {
    const [hours, minutes] = timeString.split(':').map(Number);
    return hours * 60 + minutes;
  },
};
