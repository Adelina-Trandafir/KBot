export const CalendarUIMixin = {
  /**
   * 📅 AFIȘEAZĂ CALENDARUL
   */
  show() {
    try {
      if (this.isVisible) {
        //this.hide();
        return;
      }

      // Adaugă clasa pentru datetime
      if (this.config.showTimeSelector && !this.timePickerElement) {
        this.calendarElement.classList.add('datetime-mode');
        this.setupTimeSelector();
      }

      this.renderCalendar();
      this.loadCalendarData();

      //this.overlayElement?.classList.remove('hidden');
      // this.overlayElement.style.zIndex = Math.floor(window.ZIndexManager.getNext()) + 10000;

      this.calendarElement.classList.add('drop-in-start');
      this.calendarElement.classList.remove('hidden')
      // this.calendarElement.classList.remove('hidden');
      
      // Chiar daca z-index-ul se stabileste in subscribe,
      // il adaug manual aici, pentru ca altfel, se deschide
      // in spatele elementului parinte pana la subscribe
      // this.calendarElement.style.zIndex = Math.floor(window.ZIndexManager.getNext());

      // Pozitionare reala pe baza inaltimii elementului
      this.positionCalendar();

      // IMPORTANT: Dublu requestAnimationFrame pentru timing corect!
      requestAnimationFrame(() => {
        requestAnimationFrame(() => {
          // Acum browser-ul a procesat display: block și drop-in-start
          this.calendarElement.classList.remove('drop-in-start');
          this.calendarElement.classList.add('drop-in-end');
        });
        
        // Subscribe DUPĂ ce animația a început
        window.overlay.subscribe(this, this.calendarElement, {
          onClick: () => this.hide(),
          onEscape: () => this.hide(),
          parent: document.getElementById('tableBodyScroll'),
        });
      });
  
      this.lastShownAt = Date.now();
      this.isVisible = true;

      this.log(`📅 Calendar afișat pentru ${this.fieldName}`);
    } catch (error) {
      this.handleError('Eroare la afișarea calendarului', error);
    }
  },

  /**
   * 🙈 ASCUNDE MODALUL
   */
  hideModal() {
    if (this.modalElement) {
      this.hide();
      this.log('🙈 Modal ascuns');
    }
  },

  /**
   * 🙈 ASCUNDE CALENDARUL
   */
  hide() {
    if (!this.isVisible) return;
    // 1. Aplică animația de închidere
    this.calendarElement.classList.remove('drop-in-end');
    this.calendarElement.classList.add('drop-out');

    // 2. Așteaptă să se termine animația (250ms conform CSS-ului tău)
    setTimeout(() => {
      // 3. Acum ascunde modalul complet
      this.calendarElement.classList.remove('drop-out');
      this.calendarElement.classList.add('hidden');

      // 5. ACUM face unsubscribe (la final!)
    }, 250); // Sync cu transition: 0.25s din .drop-out
    
    window.overlay.unsubscribe(this);
    
    this.isVisible = false;
    this.lastShownAt = 0;
  },

  /**
   * 📍 POZIȚIONEAZĂ CALENDARUL FAȚĂ DE INPUT
   */
  positionCalendar() {
    if (!this.targetInput) return;

    let inputRect = null;

    this.calendarElement.offsetHeight; // accesează o proprietate care forțează layout

    if (
      this.targetInput.hasAttribute('data-custom-datetime') ||
      this.targetInput.hasAttribute('data-custom-date')
    ) {
      // Daca este custom date/date-time, foloseste sub-elementul cu clasa
      // custom-datetime-container sau custom-date-container
      // din parintele lui targetInput
      inputRect =
        this.targetInput.parentElement
          .querySelector('.custom-datetime-container')
          ?.getBoundingClientRect() ||
        this.targetInput.parentElement
          .querySelector('.custom-date-container')
          ?.getBoundingClientRect();
    } else {
      inputRect = this.targetInput.getBoundingClientRect();
    }

    const calendarRect = document.querySelector('.custom-calendar').getBoundingClientRect();
    const viewportHeight = window.innerHeight;
    const viewportWidth = window.innerWidth;
    const scrollY = window.scrollY;
    const scrollX = window.scrollX;

    let top, left;

    // Calculează poziția orizontală
    left = inputRect.left + scrollX;

    // Verifică dacă calendarul iese din viewport pe dreapta
    if (left + calendarRect.width > viewportWidth) {
      left = viewportWidth - calendarRect.width - 10;
    }

    // Verifică dacă calendarul iese din viewport pe stânga
    if (left < 10) {
      left = 10;
    }

    // Calculează poziția verticală
    const spaceBelow = viewportHeight - (inputRect.bottom - scrollY);
    const spaceAbove = inputRect.top - scrollY;
    const calendarHeight = calendarRect.height || 100; // Estimare dacă nu e măsurat încă

    if (spaceBelow >= calendarHeight || spaceBelow > spaceAbove) {
      // Afișează sub input
      top = scrollY + inputRect.top ;
      this.calendarElement.classList.remove('position-above');
      this.calendarElement.classList.add('position-below');
    } else {
      // Afișează deasupra input
      top = scrollY + inputRect.bottom - 10 - calendarHeight;
      this.calendarElement.classList.remove('position-below');
      this.calendarElement.classList.add('position-above');
    }

    this.calendarElement.style.position = 'absolute !important';
    this.calendarElement.style.top = `${top}px`;
    this.calendarElement.style.left = `${left}px`;
    this.calendarElement.style.width = `${inputRect.width}px`;

    this.log(`📍 Calendar poziționat la ${left},${top}`);
  },
};
