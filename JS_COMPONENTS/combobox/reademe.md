# Combobox Component

> Enterprise-grade autocomplete/dropdown component with advanced search capabilities, automatic memory management, and modular architecture.

---

## 📑 Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Features](#features)
- [Quick Start](#quick-start)
- [API Reference](#api-reference)
  - [Constructor Options](#constructor-options)
  - [Public Methods](#public-methods)
  - [Events & Callbacks](#events--callbacks)
- [Module Structure](#module-structure)
- [State Management](#state-management)
- [UI Positioning](#ui-positioning)
- [Styling & Customization](#styling--customization)
- [Performance Considerations](#performance-considerations)
- [Browser Support](#browser-support)
- [Troubleshooting](#troubleshooting)
- [Integration Notes](#integration-notes)

---

## Overview

**Combobox** is a production-ready, modular autocomplete component designed for enterprise applications. Built with a mixin-based architecture, it provides seamless integration with the project's **ListenerTracker** system for automatic memory management and leak prevention.

### Key Characteristics

- **Modular Design**: Split into 5 specialized mixins for separation of concerns
- **Memory Safe**: Automatic listener cleanup via ListenerTracker integration
- **Flexible**: Supports both static and dynamic (async) data sources
- **Responsive**: Elastic width with viewport-aware positioning
- **Accessible**: Full keyboard navigation with highlight tracking
- **Performance**: Debounced search, lazy rendering, optimized DOM operations

---

## Architecture

### Component Structure

```
┌─────────────────────────────────────────────────────────┐
│                    Combobox (Core)                      │
│  - Instance management                                  │
│  - ListenerTracker integration                          │
│  - Lifecycle coordination                               │
└──────────────────┬──────────────────────────────────────┘
                   │
      ┌────────────┼────────────┬──────────────┬──────────┐
      │            │            │              │          │
┌─────▼─────┐ ┌───▼────┐ ┌─────▼──────┐ ┌────▼─────┐ ┌──▼────────┐
│   State   │ │   UI   │ │   Events   │ │  Search  │ │ Listeners │
│   Mixin   │ │  Mixin │ │   Mixin    │ │  Mixin   │ │  Tracker  │
└───────────┘ └────────┘ └────────────┘ └──────────┘ └───────────┘
```

### Mixin Responsibilities

| Mixin      | File                 | Responsibility                                        |
| ---------- | -------------------- | ----------------------------------------------------- |
| **State**  | `combobox-state.js`  | Internal state management, result tracking            |
| **UI**     | `combobox-ui.js`     | DOM creation, rendering, positioning                  |
| **Events** | `combobox-events.js` | Event binding, keyboard navigation, user interactions |
| **Search** | `combobox-search.js` | Search logic (static/dynamic), result handling        |
| **Core**   | `combobox.js`        | Component orchestration, lifecycle management         |

### ListenerTracker Integration

The component leverages the project's **ListenerTracker** mixin for automatic memory management:

- All event listeners are tracked automatically
- Cleanup happens via `cleanupAllListeners()` on destroy
- Prevents memory leaks in SPA contexts
- Provides debugging capabilities for listener inspection

---

## Features

### Core Features

- ✅ **Autocomplete Search** - Real-time filtering with highlight
- ✅ **Static & Dynamic Data** - Supports pre-loaded arrays or async fetch
- ✅ **Keyboard Navigation** - Arrow keys, Enter, Escape, Tab
- ✅ **Readonly Mode** - Click-to-open dropdown without typing
- ✅ **Debounced Search** - Configurable delay to reduce API calls
- ✅ **Min Search Length** - Trigger search only after N characters
- ✅ **Max Results** - Limit displayed results for performance
- ✅ **Match Highlighting** - Visual emphasis on matching text
- ✅ **Viewport Awareness** - Auto-position above/below based on space
- ✅ **Loader Indication** - Visual feedback during async operations

### Advanced Features

- 🔧 **Memory Management** - Automatic cleanup via ListenerTracker
- 🔧 **Z-Index Management** - Integration with global ZIndexManager
- 🔧 **Elastic Width** - Responsive container with min/max constraints
- 🔧 **HTML Escaping** - XSS protection on all rendered content
- 🔧 **Instance Isolation** - Unique IDs prevent conflicts
- 🔧 **Error Handling** - Graceful degradation on search failures

---

## Quick Start

### Basic Usage

```javascript
import { Combobox } from './components/combobox/combobox.js';

const container = document.getElementById('my-combobox');
const combobox = new Combobox(container, {
  placeholder: 'Search users...',
  onSearch: async (query) => {
    // Async search function
    const response = await fetch(`/api/users?q=${query}`);
    const data = await response.json();
    return data.map((user) => ({
      value: user.id,
      label: user.name,
    }));
  },
  onSelect: (value, text) => {
    console.log('Selected:', value, text);
  },
});
```

### Readonly Mode (Dropdown)

```javascript
const combobox = new Combobox(container, {
  readonly: true,
  staticData: [
    { value: '1', label: 'Option 1' },
    { value: '2', label: 'Option 2' },
  ],
  onSelect: (value, text) => {
    console.log('Selected:', value, text);
  },
});
```

---

## API Reference

### Constructor Options

#### Required Parameters

| Parameter   | Type          | Description                                     |
| ----------- | ------------- | ----------------------------------------------- |
| `container` | `HTMLElement` | DOM element where the combobox will be rendered |

#### Configuration Options

| Option             | Type       | Default          | Description                                      |
| ------------------ | ---------- | ---------------- | ------------------------------------------------ |
| `placeholder`      | `string`   | `'Selectați...'` | Input placeholder text                           |
| `searchDelay`      | `number`   | `300`            | Debounce delay in milliseconds                   |
| `onSearch`         | `function` | `null`           | Async search handler `(query) => Promise<Array>` |
| `onSelect`         | `function` | `null`           | Selection callback `(value, text) => void`       |
| `showLoader`       | `boolean`  | `true`           | Show loading indicator during search             |
| `minSearchLength`  | `number`   | `0`              | Minimum characters before search triggers        |
| `maxResults`       | `number`   | `50`             | Maximum results to display                       |
| `highlightMatches` | `boolean`  | `true`           | Highlight matching text in results               |
| `allowEmpty`       | `boolean`  | `true`           | Allow clearing the selection                     |
| `readonly`         | `boolean`  | `false`          | Enable readonly/dropdown mode                    |
| `staticData`       | `Array`    | `[]`             | Pre-loaded data for readonly mode                |

#### Data Format

Results from `onSearch` or `staticData` must follow this structure:

```javascript
[
  {
    value: 'unique-id', // Stored value (e.g., database ID)
    label: 'Display Text', // Displayed in dropdown
  },
];
```

---

### Public Methods

#### Value Management

##### `setValue(value, text)`

Sets the current value and display text.

**Parameters:**

- `value` (string): The value to store (e.g., ID)
- `text` (string): The text to display in input

**Returns:** `void`

---

##### `getInputValue()`

Gets the current input display value.

**Returns:** `string` - Current input text

---

##### `getSelectedValue()`

Gets the stored value (ID from database).

**Returns:** `string` - Selected value/ID

---

##### `getSelectedText()`

Gets the display text of selected item.

**Returns:** `string` - Selected display text

---

##### `getIndex()`

Gets the currently selected index in results array.

**Returns:** `number` - Selected index (-1 if none)

---

##### `clear()`

Clears the current selection and hides dropdown.

**Returns:** `void`

---

#### State Management

##### `setEnabled(enabled)`

Enables or disables the combobox.

**Parameters:**

- `enabled` (boolean): `true` to enable, `false` to disable

**Returns:** `void`

**Effect:** Disables input and prevents interaction when `false`

---

#### Lifecycle

##### `destroy()`

Destroys the combobox instance and cleans up all resources.

**Process:**

1. Clears any pending timeouts
2. Calls `cleanupAllListeners()` (ListenerTracker)
3. Removes DOM elements
4. Removes CSS classes

**Returns:** `void`

**⚠️ Important:** Always call `destroy()` when removing combobox from DOM to prevent memory leaks.

---

### Events & Callbacks

#### `onSearch(query)`

Called when user types and search is triggered.

**Parameters:**

- `query` (string): Search query entered by user

**Returns:** `Promise<Array>` - Array of result objects `{ value, label }`

**When triggered:**

- After `searchDelay` milliseconds of inactivity
- Only if `query.length >= minSearchLength`
- Not triggered in readonly mode with staticData

**Example:**

```javascript
onSearch: async (query) => {
  const results = await fetchFromAPI(query);
  return results.map((item) => ({
    value: item.id,
    label: item.name,
  }));
};
```

---

#### `onSelect(value, text)`

Called when user selects an item from dropdown.

**Parameters:**

- `value` (string): The stored value (ID)
- `text` (string): The display text

**Returns:** `void`

**When triggered:**

- Click on dropdown option
- Enter key on highlighted option
- Programmatic selection via `setValue()`

**Example:**

```javascript
onSelect: (value, text) => {
  console.log('Selected ID:', value);
  console.log('Display text:', text);
  // Update form, trigger API call, etc.
};
```

---

## Module Structure

### Core Module (`combobox.js`)

**Responsibilities:**

- Instance creation and initialization
- ListenerTracker application
- Mixin orchestration
- Lifecycle management (init, destroy)

**Key Methods:**

- `constructor(container, options)`
- `init()`
- `destroy()`

---

### State Mixin (`combobox-state.js`)

**Responsibilities:**

- Internal state initialization
- Result management
- Selection tracking

**State Properties:**

- `searchTimeout`: Debounce timer reference
- `currentQuery`: Current search query
- `selectedText`: Display text of selected item
- `selectedValue`: Stored value (ID)
- `selectedIndex`: Highlighted result index
- `results`: Array of search results
- `isVisible`: Dropdown visibility state
- `disabled`: Enabled/disabled state
- `lastShownAt`: Timestamp of last show
- `instanceId`: Unique instance identifier

**Key Methods:**

- `initializeState()`
- `updateResults(results, query)`
- `hasResults()`
- `getSelectedResult()`

---

### UI Mixin (`combobox-ui.js`)

**Responsibilities:**

- DOM element creation
- Rendering and positioning
- Visual state updates

**Key Methods:**

- `createOverlayElement()`: Creates fixed overlay element
- `createElements()`: Builds input, loader, dropdown structure
- `show()`: Opens dropdown with z-index management
- `hide()`: Closes dropdown and resets state
- `positionDropdown()`: Calculates viewport-aware position
- `renderResults(query)`: Renders result list with highlighting
- `updateHighlight()`: Updates selected option visual state
- `showLoader()` / `hideLoader()`: Loader state control
- `highlightMatch(text, query)`: Highlights matching substrings
- `escapeHtml(text)`: XSS protection
- `showError(message)`: Displays error message

---

### Events Mixin (`combobox-events.js`)

**Responsibilities:**

- Event listener binding (via ListenerTracker)
- User interaction handling
- Keyboard navigation

**Key Methods:**

- `bindEvents()`: Binds all event listeners
- `handleInput(e)`: Input change handler with debounce
- `handleReadonlyClick()`: Readonly mode click handler
- `handleKeydown(e)`: Keyboard navigation (arrows, enter, escape)
- `handleFocus()`: Focus event handler
- `handleBlur(e)`: Blur event with delayed close
- `handleDropdownClick(e)`: Dropdown item click handler
- `selectValue(value, text)`: Handles selection logic
- `navigateDown()` / `navigateUp()`: Arrow key navigation
- `selectHighlighted()`: Enter key selection
- `showAllOptions()`: Displays all options for readonly mode

---

### Search Mixin (`combobox-search.js`)

**Responsibilities:**

- Search execution (static/dynamic)
- Async operation handling
- Result processing

**Key Methods:**

- `performSearch(query)`: Executes search and updates UI

**Search Flow:**

1. Validates query length (unless readonly)
2. Shows loader
3. Calls `onSearch` callback or filters `staticData`
4. Updates results via state mixin
5. Renders results via UI mixin
6. Hides loader
7. Shows/hides dropdown based on results

---

## State Management

### State Lifecycle

```
┌─────────────┐
│  INIT       │ - selectedIndex: -1
│             │ - results: []
│             │ - isVisible: false
└──────┬──────┘
       │
       ▼
┌─────────────┐
│  SEARCHING  │ - Loader visible
│             │ - Debounce timer active
└──────┬──────┘
       │
       ▼
┌─────────────┐
│  RESULTS    │ - Dropdown visible
│  SHOWN      │ - Navigation enabled
└──────┬──────┘
       │
       ▼
┌─────────────┐
│  SELECTED   │ - Value stored
│             │ - Dropdown hidden
│             │ - onSelect triggered
└─────────────┘
```

### State Transitions

| Event           | From State    | To State      | Side Effects                      |
| --------------- | ------------- | ------------- | --------------------------------- |
| User types      | INIT          | SEARCHING     | Start debounce timer, show loader |
| Search complete | SEARCHING     | RESULTS_SHOWN | Render results, show dropdown     |
| Arrow key       | RESULTS_SHOWN | RESULTS_SHOWN | Update `selectedIndex`, highlight |
| Enter key       | RESULTS_SHOWN | SELECTED      | Call `onSelect`, hide dropdown    |
| Escape key      | RESULTS_SHOWN | INIT          | Hide dropdown, reset state        |
| Click outside   | RESULTS_SHOWN | INIT          | Hide dropdown                     |
| Destroy         | ANY           | DESTROYED     | Cleanup listeners, remove DOM     |

---

## UI Positioning

### Overlay System

The combobox uses a fixed overlay element (`combobox-overlay`) for proper z-index layering:

```javascript
this.overlay = document.createElement('div');
this.overlay.className = 'combobox-overlay';
document.body.appendChild(this.overlay);
```

**Z-Index Management:**

- Integrates with global `window.ZIndexManager` if available
- Overlay: `ZIndexManager.getNext() + 9999`
- Dropdown: `ZIndexManager.getNext() + 10000`

### Viewport-Aware Positioning

The dropdown intelligently positions itself:

**Algorithm:**

1. Calculate input `getBoundingClientRect()`
2. Check available space below (`viewportHeight - rect.bottom`)
3. If insufficient space (< 210px):
   - Position **above** input
   - Add `.show-above` class
   - Calculate `top = rect.top - dropdownHeight`
4. Otherwise:
   - Position **below** input (default)
   - Add `.show-below` class
   - Calculate `top = rect.bottom`

**Constraints:**

- Minimum top position: 10px (prevents clipping at viewport top)
- Width: Matches input width exactly
- Max height: 200px with scroll

---

## Styling & Customization

### CSS Variables

The component supports theming via CSS custom properties:

```css
.combobox-option:hover,
.combobox-option.highlighted {
  background: linear-gradient(to right, #ffffff, var(--hover-color, #f5f5f5)) !important;
}
```

**Override example:**

```css
.my-custom-combobox {
  --hover-color: #e3f2fd;
}
```

### CSS Classes

#### Container States

| Class                 | Applied When     | Purpose                |
| --------------------- | ---------------- | ---------------------- |
| `.combobox-container` | Always           | Base container         |
| `.readonly`           | `readonly: true` | Readonly mode styling  |
| `.disabled`           | `disabled: true` | Disabled state styling |

#### Dropdown States

| Class                | Applied When             | Purpose              |
| -------------------- | ------------------------ | -------------------- |
| `.combobox-dropdown` | Always                   | Base dropdown        |
| `.visible`           | Dropdown open            | Display dropdown     |
| `.hidden`            | Dropdown closed          | Hide dropdown        |
| `.show-above`        | Insufficient space below | Position above input |
| `.show-below`        | Sufficient space below   | Position below input |

#### Option States

| Class              | Applied When         | Purpose                  |
| ------------------ | -------------------- | ------------------------ |
| `.combobox-option` | Always               | Base option              |
| `.highlighted`     | Keyboard navigation  | Highlight current option |
| `.selected`        | (Not used currently) | Mark selected option     |

### Responsive Design

The component is fully responsive with elastic width constraints:

```css
.combobox-container {
  width: 100%;
  min-width: 0; /* Allows compression below 200px */
  max-width: 300px;
}

.combobox-dropdown {
  max-width: clamp(180px, 100%, 320px);
}
```

**Mobile optimizations:**

- `@media (max-width: 400px)`: Reduced font size (11px)
- `@media (max-width: 350px)`: Max width constrained to viewport

### Custom Scrollbar

```css
.combobox-dropdown::-webkit-scrollbar {
  width: 6px;
}
.combobox-dropdown::-webkit-scrollbar-thumb {
  background: #c1c1c1;
  border-radius: 3px;
}
```

---

## Performance Considerations

### Optimization Strategies

#### 1. Debounced Search

- Default delay: 300ms
- Prevents excessive API calls
- Clears previous timeout before setting new one

```javascript
this.searchTimeout = this.addTimeout(() => {
  this.performSearch(query);
}, this.options.searchDelay);
```

#### 2. Result Limiting

- `maxResults` option (default: 50)
- Only renders visible results
- Reduces DOM operations

```javascript
this.results.slice(0, this.options.maxResults);
```

#### 3. Lazy Positioning

- Dropdown positioned only when shown
- Uses `getBoundingClientRect()` for accurate measurements
- Event listeners for scroll/resize (optional)

#### 4. Memory Management

- ListenerTracker ensures no leaked listeners
- Automatic cleanup on destroy
- Timeout/interval tracking

### Performance Metrics

| Operation         | Typical Time | Notes                            |
| ----------------- | ------------ | -------------------------------- |
| `init()`          | < 10ms       | DOM creation, event binding      |
| `show()`          | < 5ms        | Z-index calculation, positioning |
| `renderResults()` | 5-50ms       | Depends on result count          |
| `hide()`          | < 2ms        | Class removal, state reset       |
| `destroy()`       | < 15ms       | Full cleanup                     |

### Best Practices

1. **Limit Search Results**: Use `maxResults` to cap rendered items
2. **Increase Search Delay**: For slow networks, use 500-800ms delay
3. **Cache Static Data**: Pre-load `staticData` for readonly mode
4. **Destroy Instances**: Always call `destroy()` when removing from DOM
5. **Debounce External**: If `onSearch` hits external API, add server-side debouncing

---

## Browser Support

### Minimum Requirements

| Browser | Minimum Version | Notes        |
| ------- | --------------- | ------------ |
| Chrome  | 90+             | Full support |
| Firefox | 88+             | Full support |
| Safari  | 14+             | Full support |
| Edge    | 90+             | Full support |
| Opera   | 76+             | Full support |

### Required Features

- ✅ ES6 Modules (`import`/`export`)
- ✅ ES6 Classes
- ✅ Async/Await
- ✅ Arrow Functions
- ✅ Template Literals
- ✅ `getBoundingClientRect()`
- ✅ CSS Grid/Flexbox
- ✅ CSS Custom Properties
- ✅ `addEventListener` / `removeEventListener`

### Polyfills Not Required

Modern browsers support all used features natively. No polyfills needed for target environments.

---

## Troubleshooting

### Common Issues

#### Issue: Dropdown doesn't appear

**Symptoms:**

- No dropdown shown after typing
- Console errors about missing elements

**Solutions:**

1. ✅ Verify container exists: `document.getElementById('container')`
2. ✅ Check `minSearchLength` option - query must meet minimum
3. ✅ Ensure `onSearch` returns valid array format `[{value, label}]`
4. ✅ Check console for JavaScript errors
5. ✅ Verify CSS is loaded (check for `.combobox-dropdown` styles)

---

#### Issue: Dropdown positioned incorrectly

**Symptoms:**

- Dropdown appears off-screen
- Overlaps with input

**Solutions:**

1. ✅ Check parent containers for `overflow: hidden`
2. ✅ Verify overlay element is appended to `document.body`
3. ✅ Ensure no conflicting CSS `position` rules
4. ✅ Check z-index conflicts with other components

---

#### Issue: Search not triggering

**Symptoms:**

- No loader shown
- `onSearch` callback not called

**Solutions:**

1. ✅ Verify `onSearch` is provided (required for dynamic search)
2. ✅ Check query length vs `minSearchLength` option
3. ✅ Look for JavaScript errors in `onSearch` callback
4. ✅ Test with simplified `onSearch` that returns static data

---

#### Issue: Selection not working

**Symptoms:**

- Click on option does nothing
- `onSelect` not called

**Solutions:**

1. ✅ Check event listeners are bound (inspect with DevTools)
2. ✅ Verify result format has `value` and `label` properties
3. ✅ Look for `mousedown` event blocking click
4. ✅ Check for JavaScript errors in `onSelect` callback

---

#### Issue: Memory leaks / listeners not cleaned up

**Symptoms:**

- Performance degradation over time
- Increasing memory usage

**Solutions:**

1. ✅ Always call `destroy()` before removing from DOM
2. ✅ Check ListenerTracker is properly applied
3. ✅ Use browser DevTools → Memory → Heap Snapshot to verify cleanup
4. ✅ Enable debug mode: `debugMode: true` to inspect listeners

**Debug commands:**

```javascript
// Global debugging functions
window.debugAllListeners(); // Shows all tracked listeners
window.checkAllListenersHealth(); // Health check
```

---

#### Issue: Readonly mode not showing options

**Symptoms:**

- Click does nothing in readonly mode
- Dropdown stays closed

**Solutions:**

1. ✅ Verify `readonly: true` is set in options
2. ✅ Ensure `staticData` array is populated
3. ✅ Check data format: `[{value: '1', label: 'Text'}]`
4. ✅ Verify `onSearch` is NOT required for staticData mode

---

### Debugging Tools

#### Enable Debug Mode

```javascript
const combobox = new Combobox(container, {
  debugMode: true, // Enables ListenerTracker logging
  ...options,
});
```

#### Global Debug Functions

The project provides global debugging utilities:

```javascript
// List all combobox instances with listeners
window.debugAllListeners();

// Get statistics for all listeners
window.getAllListenerStats();

// Check health of listeners
window.checkAllListenersHealth();

// Find listeners on specific element
window.findListenersByElement(container);
```

#### Browser DevTools

**Event Listeners:**

1. Select combobox input in Elements panel
2. Event Listeners tab → View all bound events
3. Verify `input`, `keydown`, `focus`, `blur` are present

**Performance:**

1. Performance tab → Record interaction
2. Look for long tasks > 50ms
3. Check for excessive reflows

**Memory:**

1. Memory tab → Take heap snapshot
2. Search for "Combobox" instances
3. Verify count matches expected (no duplicates)

---

## Integration Notes

### Project Context

The Combobox component is part of a larger enterprise ap
