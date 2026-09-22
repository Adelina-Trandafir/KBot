/**
 * 🇷🇴 RO CNP VALIDATOR MIXIN
 * Mixin pentru validare CNP românesc
 * Metode cu prefix: ro_cnp
 *
 * @version 3.0.0
 */

export const roCnpValidatorMixin = {
  /**
   * Mapare județe (cod JJ -> cod 2 litere)
   */
  _judete_ro_cnp: {
    1: 'AB',
    2: 'AR',
    3: 'AG',
    4: 'BC',
    5: 'BH',
    6: 'BN',
    7: 'BT',
    8: 'BV',
    9: 'BR',
    10: 'BZ',
    11: 'CS',
    12: 'CJ',
    13: 'CT',
    14: 'CV',
    15: 'DB',
    16: 'DJ',
    17: 'GL',
    18: 'GJ',
    19: 'HR',
    20: 'HD',
    21: 'IL',
    22: 'IS',
    23: 'IF',
    24: 'MM',
    25: 'MH',
    26: 'MS',
    27: 'NT',
    28: 'OT',
    29: 'PH',
    30: 'SM',
    31: 'SJ',
    32: 'SB',
    33: 'SV',
    34: 'TR',
    35: 'TM',
    36: 'TL',
    37: 'VS',
    38: 'VL',
    39: 'VN',
    40: 'B',
    41: 'B',
    42: 'B',
    43: 'B',
    44: 'B',
    45: 'B',
    46: 'B',
    51: 'CL',
    52: 'GR',
  },

  /**
   * Extrage doar cifrele dintr-un string
   * @param {string} str
   * @returns {string}
   */
  _extract_digits_ro_cnp(str) {
    return str.replace(/\D/g, '');
  },

  /**
   * Verifică validitatea unei date
   * @param {number} year
   * @param {number} month (1-12)
   * @param {number} day (1-31)
   * @returns {boolean}
   */
  _is_valid_date_ro_cnp(year, month, day) {
    const d = new Date(year, month - 1, day);
    return d.getFullYear() === year && d.getMonth() === month - 1 && d.getDate() === day;
  },

  /**
   * Formatează o dată ca ISO string (YYYY-MM-DD)
   * @param {number} year
   * @param {number} month
   * @param {number} day
   * @returns {string|null}
   */
  _format_date_iso_ro_cnp(year, month, day) {
    if (!this._is_valid_date_ro_cnp(year, month, day)) return null;
    return new Date(year, month - 1, day).toISOString().slice(0, 10);
  },

  /**
   * Parsează și validează un CNP românesc
   * @param {string} cnp - CNP de 13 cifre
   * @returns {Object|null}
   */
  parse_ro_cnp(cnp) {
    // Validare format
    if (!/^\d{13}$/.test(cnp)) {
      this.log('❌ Format invalid (nu are 13 cifre)');
      return null;
    }

    const digits = cnp.split('').map(Number);
    const S = digits[0];
    const AA = parseInt(cnp.substr(1, 2), 10);
    const LL = parseInt(cnp.substr(3, 2), 10);
    const ZZ = parseInt(cnp.substr(5, 2), 10);
    const JJ = parseInt(cnp.substr(7, 2), 10);

    // Validări de bază
    if (S < 1 || S > 9) {
      this.log('❌ Prima cifră (S) invalidă:', S);
      return null;
    }

    if (LL < 1 || LL > 12) {
      this.log('❌ Luna (LL) invalidă:', LL);
      return null;
    }

    if (ZZ < 1 || ZZ > 31) {
      this.log('❌ Ziua (ZZ) invalidă:', ZZ);
      return null;
    }

    if (JJ < 1 || JJ > 52) {
      this.log('❌ Județul (JJ) invalid:', JJ);
      return null;
    }

    // Validare cifră de control
    if (!this.validate_checksum_ro_cnp(cnp)) {
      this.log('❌ Cifră de control invalidă');
      return null;
    }

    // Determinare an
    let year = null;
    if (S === 1 || S === 2) year = 1900 + AA;
    else if (S === 3 || S === 4) year = 1800 + AA;
    else if (S === 5 || S === 6) year = 2000 + AA;
    else if (S === 7 || S === 8 || S === 9) year = null; // non-rezident

    // Validare dată (doar pentru rezidenți)
    if (year && !this._is_valid_date_ro_cnp(year, LL, ZZ)) {
      this.log('❌ Dată invalidă:', { year, month: LL, day: ZZ });
      return null;
    }

    // Sex
    const sex = [1, 3, 5, 7].includes(S) ? 'M' : 'F';

    // Categorie
    let categorie = 'Necunoscut';
    if (S >= 1 && S <= 6) categorie = 'Rezident';
    else if (S === 7 || S === 8) categorie = 'Rezident străin';
    else if (S === 9) categorie = 'Non-rezident';

    // Data nașterii
    const dataNasterii = year ? this._format_date_iso_ro_cnp(year, LL, ZZ) : null;

    // Steag (7-9 = non-rezident = UN, altfel RO)
    const steag = S >= 7 && S <= 9 ? 'un' : 'ro';

    const result = {
      valid: true,
      country: 'RO',
      value: cnp,
      sex,
      categorie,
      judet: this._judete_ro_cnp[JJ] || '??',
      dataNasterii,
      anulNasterii: year,
      flagCode: steag,
    };

    this.log('✅ CNP valid:', result);
    return result;
  },

  /**
   * Validează cifra de control
   * @param {string} cnp
   * @returns {boolean}
   */
  validate_checksum_ro_cnp(cnp) {
    const digits = cnp.split('').map(Number);
    const controlKey = '279146358279';
    let sum = 0;

    for (let i = 0; i < 12; i++) {
      sum += digits[i] * parseInt(controlKey[i], 10);
    }

    const controlDigit = sum % 11 === 10 ? 1 : sum % 11;
    return controlDigit === digits[12];
  },

  /**
   * Determină steagul pe baza primei cifre (pentru feedback instant)
   * @param {string} firstDigit
   * @returns {string|null}
   */
  get_flag_from_first_digit_ro_cnp(firstDigit) {
    const digit = parseInt(firstDigit, 10);
    if (digit >= 1 && digit <= 6) return 'ro';
    if (digit >= 7 && digit <= 9) return 'un';
    return null;
  },
};

export default roCnpValidatorMixin;
