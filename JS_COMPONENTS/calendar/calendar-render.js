export const CalendarRenderMixin = {
  /**
   * 🎨 CREEAZĂ ELEMENTUL PRINCIPAL AL CALENDARULUI
   */
  createCalendarElement() {
    this.calendarElement = document.createElement('div');
    this.calendarElement.id = `calendar_${this.instanceId}`;
    this.calendarElement.className = 'custom-calendar hidden';
    this.calendarElement.innerHTML = this.getCalendarTemplate();

    document.body.appendChild(this.calendarElement);

    this.cacheCalendarButtonsElements();

    this.setupCalendarNavigation();
    this.log('🎨 Element calendar creat');
  },

  /*
   * DEZINTEGREAZĂ ELEMENTUL CALENDAR DIN DOM
   */
  destroyCalendarElement() {
    if (this.calendarElement) {
      document.body.removeChild(this.calendarElement);
      this.calendarElement = null;
    }
  },

  /**
   * 🧩 TEMPLATE PRINCIPAL CALENDAR
   */
  getCalendarTemplate() {
    return `
    <div class="calendar-header">
      <button class="calendar-nav-btn" data-action="prev-year" title="Anul precedent">⏪</button>
      <button class="calendar-nav-btn" data-action="prev-month" title="Luna precedentă">◀</button>
      <div class="calendar-header-title"></div>
      <button class="calendar-nav-btn" data-action="next-month" title="Luna următoare">▶</button>
      <button class="calendar-nav-btn" data-action="next-year" title="Anul următor">⏩</button>
    </div>
    <div class="calendar-weekdays">
      ${this.weekDays
        .map((day, i) => `<div class="calendar-weekday ${i >= 5 ? 'weekend' : ''}">${day}</div>`)
        .join('')}
    </div>
    <div class="calendar-days-grid"></div>
    <div class="calendar-footer">
      <button class="calendar-today-btn">Astăzi</button>
      <button class="calendar-clear-btn">Șterge</button>
      <button class="calendar-close-btn">Închide</button>
    </div>
  `;
  },

  // createCalendarOverlay() {
  //   this.overlayElement = document.createElement('div');
  //   this.overlayElement.className = 'calendar-overlay hidden';
  //   this.overlayElement.id = `calendar_overlay_${this.instanceId}`;
  //   document.body.appendChild(this.overlayElement);
  //   this.setupOverlayListener();
  //   this.log('🎨 Overlay calendar creat');
  // },

  // destroyCalendarOverlay() {
  //   if (this.overlayElement) {
  //     document.body.removeChild(this.overlayElement);
  //     this.overlayElement = null;
  //   }
  // },

  /**
   * 🎭 CREEAZĂ MODALUL PENTRU DETALII ZI
   */
  createModalElement() {
    const modal = document.createElement('div');
    modal.id = `calendar_modal_${this.instanceId}`;
    modal.className = 'calendar-day-modal hidden';
    modal.innerHTML = this.getDayModalTemplate();

    document.body.appendChild(modal);

    this.modalElement = modal;

    this.cacheModalElements();

    this.setupModalListeners();
    this.log('🎭 Modal detalii zi creat');
  },

  destroyModalElement() {
    if (this.modalElement) {
      document.body.removeChild(this.modalElement);
      this.modalElement = null;
    }
  },

  /**
   * 🎭 TEMPLATE MODAL DETALII ZI
   */
  getDayModalTemplate() {
    return `
      <div class="calendar-modal-overlay">
        <div class="calendar-modal-content">
          <div class="calendar-modal-header">
            <h3 class="calendar-modal-title"></h3>
            <button class="calendar-modal-close">&times;</button>
          </div>
          <div class="calendar-modal-body">
            <div class="calendar-day-details">
              <!-- Detaliile zilei vor fi generate dinamic -->
            </div>
          </div>
          <div class="calendar-modal-footer">
            <button class="calendar-modal-close-btn">Închide</button>
          </div>
        </div>
      </div>
    `;
  },

  /**
   * 🎨 RENDERIZEAZĂ CALENDARUL
   */
  renderCalendar() {
    this.renderHeader();
    this.renderDays();
  },

  /**
   * 🎨 RENDERIZEAZĂ HEADER-UL
   */
  renderHeader() {
    const monthYearElement = this.calendarElement.querySelector('.calendar-header-title');
    if (monthYearElement) {
      monthYearElement.textContent = `${this.months[this.currentDate.getMonth()]} ${this.currentDate.getFullYear()}`;
    }
  },

  /**
   * 🎨 RENDERIZEAZĂ ZILELE
   */
  renderDays() {
    const daysGrid = this.calendarElement.querySelector('.calendar-days-grid');
    if (!daysGrid) return;

    daysGrid.innerHTML = '';

    const year = this.currentDate.getFullYear();
    const month = this.currentDate.getMonth();
    const today = new Date();
    const firstDay = new Date(year, month, 1);
    const lastDay = new Date(year, month + 1, 0);
    const daysInMonth = lastDay.getDate();
    const startingWeekDay = (firstDay.getDay() + 6) % 7; // Luni = 0

    // Adaugă zilele goale de la începutul lunii
    for (let i = 0; i < startingWeekDay; i++) {
      const emptyDay = document.createElement('div');
      emptyDay.className = 'calendar-day empty';
      daysGrid.appendChild(emptyDay);
    }

    // Adaugă zilele lunii
    for (let day = 1; day <= daysInMonth; day++) {
      const date = new Date(year, month, day);
      const dateString = this.formatDate(date);
      const dayElement = document.createElement('div');

      dayElement.className = 'calendar-day';
      dayElement.dataset.date = dateString;
      dayElement.innerHTML = `
        <span class="calendar-day-number">${day}</span>
        <span class="calendar-day-count" style="display: none;"></span>
      `;

      // Aplică clase CSS pentru diferite stări
      if (this.isSameDate(date, today)) {
        dayElement.classList.add('today');
      }

      if (this.selectedDate && this.isSameDate(date, this.selectedDate)) {
        dayElement.classList.add('selected');
      }

      if (this.isWeekend(date)) {
        dayElement.classList.add('weekend');
      }

      if (this.isDisabledDate(date) || (this.isWeekend(date) && !this.config.allowWeekends)) {
        dayElement.classList.add('disabled');
      }

      // Adaugă informații din cache
      const dayData = this.dataCache.get(dateString);
      if (dayData && dayData.count > 0) {
        dayElement.classList.add('has-data');
        const countElement = dayElement.querySelector('.calendar-day-count');
        countElement.textContent = dayData.count;
        countElement.style.display = 'block';
      }

      daysGrid.appendChild(dayElement);
    }

    this.log('🎨 Zile renderizate pentru', `${this.months[month]} ${year}`);
  },

  /**
   * 🔄 REFRESH DATE CALENDAR
   */
  refreshCalendarData() {
    this.dataCache.clear();
    this.loadCalendarData();
    this.log('🔄 Date calendar actualizate');
  },
};
