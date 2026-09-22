export const CalendarContainerMixin = {
  /**
   * 📊 ÎNCARCĂ DATA/TIMPUL DIN INPUT
   */
  loadDateTimeFromInput() {
    if (!this.targetInput || !this.targetInput.value) return;

    const value = this.targetInput.value;

    if (this.inputType === 'datetime-local') {
      // Format: "2025-08-29T14:30"
      const [datePart, timePart] = value.split('T');

      if (datePart) {
        const date = new Date(datePart);
        if (!isNaN(date)) {
          this.currentDate = date;
          this.selectedDate = date;
        }
      }

      if (timePart) {
        this.selectedTime = timePart;
      }
    } else {
      // Format: "2025-08-29"
      const date = new Date(value);
      if (!isNaN(date)) {
        this.currentDate = date;
        this.selectedDate = date;
      }
    }

    this.log(`📊 Încărcat din input: ${value}`);
  },

  /**
   * 🎨 TRANSFORMĂ INPUT ÎN CONTROALE CUSTOM (DATE SAU DATETIME)
   * Ascunde input-ul original și creează controale separate pentru zi/lună/an și opțional oră/minut
   */
  transformToCustomControls() {
    if (!this.targetInput || !this.useCustomInput) {
      return;
    }

    // Ascunde input-ul original
    this.targetInput.style.display = 'none';

    // Creează container principal
    this.customContainer = document.createElement('div');
    this.customContainer.className = this.config.customDateTime
      ? 'custom-datetime-container'
      : 'custom-date-container';

    // Creează container pentru dată
    this.customDateContainer = document.createElement('div');
    this.customDateContainer.className = this.customContainer.className + '-date';

    // Controale pentru dată
    const dayInput = document.createElement('input');
    dayInput.type = 'number';
    dayInput.id = 'ziua_' + this.instanceId;
    dayInput.className = 'custom-date-day';
    dayInput.placeholder = 'ZZ';
    dayInput.maxLength = 2;
    dayInput.max = 31;
    dayInput.min = 1;

    const monthInput = document.createElement('input');
    monthInput.type = 'number';
    monthInput.id = 'luna_' + this.instanceId;
    monthInput.className = 'custom-date-month';
    monthInput.placeholder = 'LL';
    monthInput.maxLength = 2;
    monthInput.max = 12;
    monthInput.min = 1;

    const yearInput = document.createElement('input');
    yearInput.type = 'number';
    yearInput.id = 'an_' + this.instanceId;
    yearInput.className = 'custom-date-year';
    yearInput.placeholder = 'AAAA';
    yearInput.maxLength = 4;
    yearInput.min = 1900;
    yearInput.max = 2100;

    // Separatori pentru dată
    const sep1 = document.createElement('span');
    sep1.textContent = '/';

    const sep2 = document.createElement('span');
    sep2.textContent = '/';

    // Asamblează controalele de dată
    this.customDateContainer.appendChild(dayInput);
    this.customDateContainer.appendChild(sep1);
    this.customDateContainer.appendChild(monthInput);
    this.customDateContainer.appendChild(sep2);
    this.customDateContainer.appendChild(yearInput);

    // Adaugă partea de dată la container
    this.customContainer.appendChild(this.customDateContainer);

    // Buton calendar
    this.customButtonContainer = document.createElement('button');
    this.customButtonContainer.id = 'btn_' + this.instanceId;
    this.customButtonContainer.type = 'button';
    this.customButtonContainer.className = 'custom-calendar-btn';
    this.customButtonContainer.innerHTML = '📅';
    this.customButtonContainer.tabIndex = -1;

    // Adaugă butonul calendar
    this.customContainer.appendChild(this.customButtonContainer);

    // ✅ Salvează în cache pentru controale dată
    this.cacheContainerElements(true, false);

    // Controale pentru timp (doar dacă e datetime)
    if (this.config.customDateTime) {
      this.customTimeContainer = document.createElement('div');
      this.customTimeContainer.className = this.customContainer.className + '-time';

      const hourInput = document.createElement('input');
      hourInput.type = 'number';
      hourInput.id = 'ora_' + this.instanceId;
      hourInput.className = 'custom-time-hour';
      hourInput.placeholder = 'HH';
      hourInput.maxLength = 2;
      hourInput.min = 0;
      hourInput.max = 23;

      const minuteInput = document.createElement('input');
      minuteInput.type = 'number';
      minuteInput.id = 'minut_' + this.instanceId;
      minuteInput.className = 'custom-time-minute';
      minuteInput.placeholder = 'MM';
      minuteInput.maxLength = 2;
      minuteInput.min = 0;
      minuteInput.max = 59;

      const sepTime = document.createElement('span');
      sepTime.textContent = ':';

      // Asamblează controalele de timp
      this.customTimeContainer.appendChild(hourInput);
      this.customTimeContainer.appendChild(sepTime);
      this.customTimeContainer.appendChild(minuteInput);

      // Adaugă partea de timp după dată, dar înaintea butonului
      this.customContainer.insertBefore(this.customTimeContainer, this.customButtonContainer);

      // ✅ Salvează în cache pentru controale timp
      this.cacheContainerElements(false, true);
    }

    // Inserează după input-ul original
    this.targetInput.parentNode.insertBefore(this.customContainer, this.targetInput.nextSibling);

    if (this.isEnabled) {
      this.setupCustomControlsEvents(true, !!this.customTimeContainer);
    }

    this.log('🎨 Controale custom create și cache-uite în this.elements');
  },

  /**
   * 📅 ACTUALIZEAZĂ CONTROALELE CUSTOM CU O DATĂ
   * Setează valorile în input-urile custom și sincronizează cu input-ul ascuns
   * @param {string|Date} date - Data de setat (YYYY-MM-DD sau Date object)
   * @param {string} time - Timpul de setat (HH:mm)
   */
  updateCustomControls(date, time) {
    // Verifică dacă există controale de dată
    if (!this.elements.dateContainer) {
      this.log.error('updateCustomControls: dateContainer nu există');
      return;
    }

    // Actualizează controalele de dată
    if (date) {
      const d = date instanceof Date ? date : new Date(date);

      if (isNaN(d.getTime())) {
        this.log.error('updateCustomControls: Dată invalidă');
        return;
      }

      this.elements.dateContainer.day.value = d.getDate().toString().padStart(2, '0');
      this.elements.dateContainer.month.value = (d.getMonth() + 1).toString().padStart(2, '0');
      this.elements.dateContainer.year.value = d.getFullYear().toString();
    } else {
      // Golește controalele dacă nu avem dată
      this.elements.dateContainer.day.value = '';
      this.elements.dateContainer.month.value = '';
      this.elements.dateContainer.year.value = '';
    }

    // Actualizează controalele de timp dacă există
    if (this.elements.timeContainer) {
      if (time) {
        const [hours, minutes] = time.split(':');
        this.elements.timeContainer.hour.value = hours.padStart(2, '0');
        this.elements.timeContainer.minute.value = minutes.padStart(2, '0');
      } else {
        // Golește controalele dacă nu avem timp
        this.elements.timeContainer.hour.value = '';
        this.elements.timeContainer.minute.value = '';
      }
    }

    // Sincronizează cu input-ul ascuns
    this.updateHiddenInput();

    this.log(`📅 Controale custom actualizate: ${date} ${time || ''}`);
  },

  /**
   * 🧹 GOLESTE CONTROALELE CUSTOM
   */
  destroyCustomControls() {
    this.clearCustomControlEvents(true, true);

    if (this.customDateContainer) {
      this.elements.dateContainer.day.value = '';
      this.elements.dateContainer.month.value = '';
      this.elements.dateContainer.year.value = '';
    }

    if (this.customTimeContainer) {
      this.elements.timeContainer.hour.value = '';
      this.elements.timeContainer.minute.value = '';
    }

    this.clearContainerElements(true, true);
    this.customDateContainer = null;
    this.customTimeContainer = null;

    if (this.customContainer) {
      this.targetInput.classList.add('calendar-disabled');
      this.targetInput.classList.remove('calendar-managed');
      this.customContainer.remove();
    }
  },
};
