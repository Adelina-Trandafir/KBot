# 🚀 Session Monitoring - Structură Modulară ES6

## 📁 Structura Fișierelor

```
js/components/session-monitoring/
├── session-monitoring-manager.js       # Core + orchestrator
├── session-monitoring-timers.js        # Programare timere
├── session-monitoring-extension.js     # Prelungire + verificare server
├── session-monitoring-ui.js            # UI: warnings, notifications, CSS loading
└── session-monitoring-activity.js      # User activity + event listeners

static/css/
└── monitoring.css                      # Stiluri CSS separate
```

## 🎯 Folosire

### Import în aplicație

```javascript
// În main.js sau app-init.js
import sessionMonitoring from './components/session-monitoring/session-monitoring-manager.js';

// La login
await sessionMonitoring.init();
```

### DOAR manager.js face import-uri

Restul modulelor sunt mixins aplicați cu `Object.assign()`:

```javascript
// În manager.js
import { sessionMonitoringTimersMixin } from './session-monitoring-timers.js';
Object.assign(this, sessionMonitoringTimersMixin);
```

### Access metodelor în mixins

Toate mixinurile folosesc `this.` pentru a accesa:

- Proprietăți: `this.sessionStartTime`, `this.inGracePeriod`
- Metode: `this.log()`, `this.showNotification()`
- Config: importat în fiecare mixin cu `import { SESSION_CONFIG }`

## 📋 Funcționalități per Modul

### 1. **session-monitoring-manager.js**

- ✅ Singleton pattern
- ✅ ListenerTracker integration
- ✅ Registry registration
- ✅ Mixin orchestration
- ✅ `init()`, `cleanup()`, `resetSessionState()`
- ✅ `handleSessionExpired()`, `getStatus()`

### 2. **session-monitoring-timers.js**

- ⏰ `scheduleWarningAndExpiry()` - programare timere warning/expiry

### 3. **session-monitoring-extension.js**

- 🔍 `verifySessionBeforeWarning()` - verificare server
- 🔄 `attemptAutoExtension()` - prelungire automată

### 4. **session-monitoring-ui.js**

- 🎨 `loadMonitoringCSS()` - loading CSS dinamic
- ⚠️ `showSessionWarning()` - modal warning
- ⚠️ `showFinalWarning()` - warning final
- 💬 `showNotification()` - toast notifications

### 5. **session-monitoring-activity.js**

- 👆 `handleUserActivity()` - gestionare evenimente
- 🎯 `setupActivityListeners()` - adaugă listeners
- 🧹 `removeActivityListeners()` - cleanup listeners

## 🔧 Config

`SESSION_CONFIG` este în **manager.js** și exportat pentru mixins:

```javascript
const SESSION_CONFIG = {
  SESSION_DURATION: null,
  WARNING_TIME: 60000,
  ACTIVITY_EVENTS: ['click', 'keypress', 'scroll', 'touchstart'],
  DEBUG_MODE: true,
  ACTIVITY_DEBOUNCE: 5000,
  CHECK_BEFORE_WARNING: 10000,
  MAX_EXTENSIONS: 2,
};
```

## 🎨 CSS Loading

CSS-ul se încarcă dinamic la `init()`:

- Path: `/static/css/monitoring.css`
- ID element: `sessionMonitoringCSS`
- Promise-based cu error handling

### Clase CSS pentru Header Warning

```css
.header-warning              /* Header roșu cu gradient */
.session-header-warning      /* Container warning text */
.countdown-pulse             /* Countdown cu pulsing animation */
```

### Animații

- **headerSlideIn** - tranziție background roșu (0.5s)
- **slideDown** - slide down warning text (0.5s)
- **pulse** - pulsing countdown (1s infinite)
- **slideIn/slideOut** - notificări (0.3s)

## 🎨 Warning Flow

```
SESIUNE ACTIVĂ
     │
     ├─ Mai sunt prelungiri (1-2/2)
     │   └─> ⚠️ MODAL BLOCANT cu countdown
     │       └─> Utilizator activ → Prelungire automată → SESIUNE ACTIVĂ
     │
     └─ NU mai sunt prelungiri (0/2)
         └─> 🔴 HEADER WARNING (NON-BLOCANT)
             │   - Header devine roșu
             │   - Warning în mijloc cu countdown pulsing
             │   - Utilizator poate continua să lucreze
             └─> Expirare → Logout
```

### Modal Warning (când mai sunt prelungiri)

Când utilizatorul **mai are prelungiri disponibile** (1-2), apare un **modal blocant** cu:

- Countdown
- Număr prelungiri rămase
- Mesaj "Sesiunea se va prelungi automat dacă continuați să lucrați"

### Header Warning (când NU mai sunt prelungiri)

Când utilizatorul **NU mai are prelungiri** (0/2 folosite), apare un **warning NON-BLOCANT** în header:

- Text de warning **în mijloc în header** (între logo și user-info)
- **Tot header-ul devine roșu** (gradient roșu)
- **Countdown cu pulsing animation**
- **NU blochează interfața** - utilizatorul poate continua să lucreze
- **Slide-down animation** la afișare

Exemplu HTML generat:

```html
<div class="header header-warning">
  <div class="logo">🏢 SVN ROMANIA</div>
  <div class="session-header-warning">
    ⚠️ Sesiunea va expira în <strong class="countdown-pulse">45</strong> secunde! Salvați-vă lucrul!
  </div>
  <div class="user-info">...</div>
</div>
```

Prin `eventBus`:

- `SESSION_WARNING` - warning afișat
- `SESSION_EXTENDED` - sesiune prelungită
- `SESSION_FINAL_WARNING` - ultima avertizare
- `SESSION_EXPIRED` - sesiune expirată
- `SESSION_ERROR` - eroare
- `SESSION_CLEANUP` - cleanup efectuat
- `USER_ACTIVITY` - activitate utilizator

## 🔍 Debugging

```javascript
// Status complet
const status = sessionMonitoring.getStatus();
console.log(status);

// Log în console (dacă DEBUG_MODE: true)
sessionMonitoring.log('Test message');
```

## ✅ Avantaje Structură

1. **Modularitate** - fiecare funcționalitate într-un fișier
2. **Refolosibilitate** - mixins pot fi refolosiți
3. **Mentenabilitate** - cod organizat, ușor de modificat
4. **Testabilitate** - fiecare mixin poate fi testat independent
5. **Singleton** - o singură instanță garantată
6. **Clean exports** - doar instanța exportată

## 🚀 Migration de la versiunea veche

Diferențe față de versiunea monolitică:

| Veche                      | Nouă                                    |
| -------------------------- | --------------------------------------- |
| Un fișier `monitoring.js`  | 5 fișiere modulare                      |
| CSS inline în JS           | CSS extern `/static/css/monitoring.css` |
| `window.sessionMonitoring` | ES6 import                              |
| Direct în DOM              | Loading dinamic CSS                     |

**API rămâne identic** - nu trebuie modificat codul care folosește `sessionMonitoring`!
