export const CalendarValidationMixin = {
  /**
   * ⚠️ SETEAZĂ REGULILE DE VALIDARE
   */
  setValidationRules(rules) {
    this.validationRules = Array.isArray(rules) ? rules : [];
    this.log(`⚠️ Reguli validare setate: ${this.validationRules.join(', ')}`);
  },

  /**
   * ✅ VALIDEAZĂ O DATĂ/TIMP
   */
  validateDateTime(dateString, timeString = null) {
    const errors = [];
    const date = new Date(dateString);
    const now = new Date();

    // Validare dată bazată pe reguli
    this.validationRules.forEach((rule) => {
      switch (rule) {
        case 'no-past':
          const today = new Date();
          today.setHours(0, 0, 0, 0);
          if (date < today) {
            errors.push('Nu poți selecta o dată din trecut');
          }
          break;

        case 'no-future':
          if (date > now) {
            errors.push('Nu poți selecta o dată din viitor');
          }
          break;

        case 'no-weekends':
          const dayOfWeek = date.getDay();
          if (dayOfWeek === 0 || dayOfWeek === 6) {
            errors.push('Nu se permit selecții în weekend');
          }
          break;

        case 'business-hours':
          if (timeString) {
            const [hours] = timeString.split(':').map(Number);
            const minHour = parseInt(this.config.minTime.split(':')[0]);
            const maxHour = parseInt(this.config.maxTime.split(':')[0]);
            if (hours < minHour || hours >= maxHour) {
              errors.push(
                `Timpul trebuie să fie între ${this.config.minTime}-${this.config.maxTime}`
              );
            }
          }
          break;
      }
    });

    // Validare timp specifică
    if (timeString && this.inputType === 'datetime-local') {
      const timeErrors = this.validateTime(timeString);
      errors.push(...timeErrors);
    }

    return {
      isValid: errors.length === 0,
      errors: errors,
    };
  },

  /**
   * 🕐 VALIDEAZĂ TIMPUL
   */
  validateTime(timeString) {
    const errors = [];
    const [hours, minutes] = timeString.split(':').map(Number);

    // Validări de bază
    if (hours < 0 || hours > 23) {
      errors.push('Ora trebuie să fie între 00-23');
      return errors;
    }
    if (minutes < 0 || minutes > 59) {
      errors.push('Minutele trebuie să fie între 00-59');
      return errors;
    }

    // Validare interval permis
    const minTime = this.parseTime(this.config.minTime);
    const maxTime = this.parseTime(this.config.maxTime);
    const currentTime = hours * 60 + minutes;

    if (currentTime < minTime || currentTime > maxTime) {
      errors.push(`Timpul trebuie să fie între ${this.config.minTime} - ${this.config.maxTime}`);
    }

    // Validare step
    if (minutes % this.config.timeStep !== 0) {
      errors.push(`Minutele trebuie să fie multiplu de ${this.config.timeStep}`);
    }

    return errors;
  },

  /**
   * ⚠️ AFIȘEAZĂ ERORI DE VALIDARE
   */
  showValidationErrors(errors) {
    if (!this.targetInput || !errors.length) return;

    this.clearValidationErrors();

    const errorContainer = document.createElement('div');
    errorContainer.className = 'calendar-validation-errors';
    errorContainer.innerHTML = errors
      .map((error) => `<div class="calendar-validation-error">⚠️ ${error}</div>`)
      .join('');

    this.targetInput.parentNode.appendChild(errorContainer);
    this.targetInput.classList.add('calendar-validation-invalid');

    setTimeout(() => {
      this.clearValidationErrors();
    }, 5000);

    this.log.error('Erori validare:', errors);
  },

  /**
   * ✅ AFIȘEAZĂ MESAJ DE SUCCES
   */
  showValidationSuccess() {
    if (!this.targetInput) return;

    this.clearValidationErrors();

    const successElement = document.createElement('div');
    successElement.className = 'calendar-validation-success';
    successElement.textContent = '✅ Selecție validă';

    this.targetInput.parentNode.appendChild(successElement);
    this.targetInput.classList.add('calendar-validation-valid');

    setTimeout(() => {
      successElement.remove();
      this.targetInput.classList.remove('calendar-validation-valid');
    }, 2000);
  },

  /**
   * 🧹 ȘTERGE ERORILE DE VALIDARE
   */
  clearValidationErrors() {
    if (!this.targetInput) return;

    const errorContainer = this.targetInput.parentNode.querySelector('.calendar-validation-errors');
    if (errorContainer) {
      errorContainer.remove();
    }

    this.targetInput.classList.remove('calendar-validation-invalid', 'calendar-validation-valid');
  },
};
