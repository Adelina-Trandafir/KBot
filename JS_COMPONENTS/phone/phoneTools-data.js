/**
 * 📊 PHONE TOOLS - DATA MODULE
 * Phone patterns and EU countries data
 *
 * @version 1.0.0
 */

/**
 * Phone patterns pentru țările EU
 * minDigits/maxDigits = cifre DUPĂ dial code
 */
export const phonePatterns = {
  // Țări cu lungimi FIXE
  RO: { dialCode: '+40', pattern: 'XXX XXX XXX', minDigits: 9, maxDigits: 9 },
  BE: { dialCode: '+32', pattern: 'XXX XXX XXX', minDigits: 9, maxDigits: 9 },
  BG: { dialCode: '+359', pattern: 'XX XXX XXXX', minDigits: 9, maxDigits: 9 },
  HR: { dialCode: '+385', pattern: 'XX XXX XXXX', minDigits: 9, maxDigits: 9 },
  CY: { dialCode: '+357', pattern: 'XX XXXXXX', minDigits: 8, maxDigits: 8 },
  CZ: { dialCode: '+420', pattern: 'XXX XXX XXX', minDigits: 9, maxDigits: 9 },
  DK: { dialCode: '+45', pattern: 'XX XX XX XX', minDigits: 8, maxDigits: 8 },
  EE: { dialCode: '+372', pattern: 'XXXX XXXX', minDigits: 7, maxDigits: 8 },
  FI: { dialCode: '+358', pattern: 'XX XXX XXXX', minDigits: 9, maxDigits: 9 },
  FR: { dialCode: '+33', pattern: 'X XX XX XX XX', minDigits: 9, maxDigits: 9 },
  GR: { dialCode: '+30', pattern: 'XXX XXX XXXX', minDigits: 10, maxDigits: 10 },
  HU: { dialCode: '+36', pattern: 'XX XXX XXXX', minDigits: 8, maxDigits: 9 },
  IE: { dialCode: '+353', pattern: 'XX XXX XXXX', minDigits: 9, maxDigits: 9 },
  IT: { dialCode: '+39', pattern: 'XXX XXX XXXX', minDigits: 9, maxDigits: 10 },
  LV: { dialCode: '+371', pattern: 'XX XXX XXX', minDigits: 8, maxDigits: 8 },
  LT: { dialCode: '+370', pattern: 'XXX XXXXX', minDigits: 8, maxDigits: 8 },
  LU: { dialCode: '+352', pattern: 'XXX XXX XXX', minDigits: 9, maxDigits: 9 },
  MT: { dialCode: '+356', pattern: 'XX XX XX XX', minDigits: 8, maxDigits: 8 },
  NL: { dialCode: '+31', pattern: 'X XXXX XXXX', minDigits: 9, maxDigits: 9 },
  PL: { dialCode: '+48', pattern: 'XXX XXX XXX', minDigits: 9, maxDigits: 9 },
  PT: { dialCode: '+351', pattern: 'XXX XXX XXX', minDigits: 9, maxDigits: 9 },
  SK: { dialCode: '+421', pattern: 'XXX XXX XXX', minDigits: 9, maxDigits: 9 },
  SI: { dialCode: '+386', pattern: 'XX XXX XXX', minDigits: 8, maxDigits: 8 },
  ES: { dialCode: '+34', pattern: 'XXX XXX XXX', minDigits: 9, maxDigits: 9 },
  SE: { dialCode: '+46', pattern: 'XX XXX XX XX', minDigits: 9, maxDigits: 9 },

  // Țări cu lungimi VARIABILE
  AT: { dialCode: '+43', pattern: 'XXXX XXXXXX', minDigits: 4, maxDigits: 13 },
  DE: { dialCode: '+49', pattern: 'XXX XXXXXXXX', minDigits: 10, maxDigits: 11 },
};

/**
 * Lista țărilor UE cu flags și dial codes
 */
export const euCountries = [
  { code: 'RO', dialCode: '+40', flag: 'ro', name: 'România' },
  { code: 'AT', dialCode: '+43', flag: 'at', name: 'Austria' },
  { code: 'BE', dialCode: '+32', flag: 'be', name: 'Belgia' },
  { code: 'BG', dialCode: '+359', flag: 'bg', name: 'Bulgaria' },
  { code: 'HR', dialCode: '+385', flag: 'hr', name: 'Croația' },
  { code: 'CY', dialCode: '+357', flag: 'cy', name: 'Cipru' },
  { code: 'CZ', dialCode: '+420', flag: 'cz', name: 'Cehia' },
  { code: 'DK', dialCode: '+45', flag: 'dk', name: 'Danemarca' },
  { code: 'EE', dialCode: '+372', flag: 'ee', name: 'Estonia' },
  { code: 'FI', dialCode: '+358', flag: 'fi', name: 'Finlanda' },
  { code: 'FR', dialCode: '+33', flag: 'fr', name: 'Franța' },
  { code: 'DE', dialCode: '+49', flag: 'de', name: 'Germania' },
  { code: 'GR', dialCode: '+30', flag: 'gr', name: 'Grecia' },
  { code: 'HU', dialCode: '+36', flag: 'hu', name: 'Ungaria' },
  { code: 'IE', dialCode: '+353', flag: 'ie', name: 'Irlanda' },
  { code: 'IT', dialCode: '+39', flag: 'it', name: 'Italia' },
  { code: 'LV', dialCode: '+371', flag: 'lv', name: 'Letonia' },
  { code: 'LT', dialCode: '+370', flag: 'lt', name: 'Lituania' },
  { code: 'LU', dialCode: '+352', flag: 'lu', name: 'Luxemburg' },
  { code: 'MT', dialCode: '+356', flag: 'mt', name: 'Malta' },
  { code: 'NL', dialCode: '+31', flag: 'nl', name: 'Olanda' },
  { code: 'PL', dialCode: '+48', flag: 'pl', name: 'Polonia' },
  { code: 'PT', dialCode: '+351', flag: 'pt', name: 'Portugalia' },
  { code: 'SK', dialCode: '+421', flag: 'sk', name: 'Slovacia' },
  { code: 'SI', dialCode: '+386', flag: 'si', name: 'Slovenia' },
  { code: 'ES', dialCode: '+34', flag: 'es', name: 'Spania' },
  { code: 'SE', dialCode: '+46', flag: 'se', name: 'Suedia' },
];
