// Searchable tree selector with viewport positioning shared by forms and grid cells.
import { TreeView } from '../treeview/treeview.js';

export class ComboTree extends TreeView {
  constructor(host, options = {}) {
    super(host, { autoCollapse: false, autoResize: false, selectableLevel: options.checkable ? null : 2, ...options });
    this.createElements();
    this.container.classList.add('combotree');
    this.arrow.textContent = '▾';
    this.treeElement.classList.add('combotree-popup');
    this.dropdown = this.treeElement;
    this.overlay = this.overlayElement;
    const surface = host.closest('dialog') || document.body;
    surface.append(this.treeElement, this.overlayElement);
    this.setData(options.nodes || []);
    if (options.checkable) {
      const hasCheckbox = this.hasCheckbox.bind(this);
      this.hasCheckbox = node => hasCheckbox(node) && node._depth > 1;
    }
    const show = this.show.bind(this);
    this.show = (...args) => {
      if (!this.isVisible) {
        const surface = this.container.closest('dialog') || document.body;
        if (this.treeElement.parentElement !== surface) surface.append(this.treeElement, this.overlayElement);
        this.expandedNodes = new Set(options.expanded || []);
        const label = this.input.value;
        this.input.value = '';
        show(...args);
        this.input.value = label;
      }
    };
    this.positionDropdown = () => {
      const rect = this.input.getBoundingClientRect(), gap = 6, margin = 8;
      this.rect = rect;
      const below = Math.max(0, innerHeight - rect.bottom - gap - margin);
      const above = Math.max(0, rect.top - gap - margin);
      const minimum = options.minHeight || 220, desired = options.dropdownHeight || 320;
      const down = below >= minimum || below >= above;
      const height = Math.min(Math.max(minimum, desired), down ? below : above);
      const width = Math.min(Math.max(rect.width, options.minWidth || 300), innerWidth - 2 * margin);
      Object.assign(this.treeElement.style, { height: `${height}px`, width: `${width}px`,
        left: `${Math.max(margin, Math.min(rect.left, innerWidth - width - margin))}px`,
        top: `${down ? rect.bottom + gap : rect.top - gap - height}px`,
        zIndex: String(Number(window.ZIndexManager?.getMax?.() || 1000) + 10000) });
      this.overlayElement.style.zIndex = String(Number(this.treeElement.style.zIndex) - 1);
      this.fitsBelowContainer = down;
    };
    this.resizeDropdown = () => { if (this.isVisible) this.positionDropdown(); };
    this.performSearch = async query => {
      if (!this.isVisible) this.show();
      this.currentQuery = query;
      if (this.searchInput) this.searchInput.value = query;
      this.treeContainer.replaceChildren();
      this.highlightedIndex = -1;
      this.flattenedItems = [];
      this.renderTreeWithData(query);
      if (!this.flattenedItems.length) this.renderNoResults();
      if (this.searchClear) this.searchClear.hidden = !query;
      this.positionDropdown();
    };
    this.handleLocalSearch = event => {
      clearTimeout(this.localSearchTimeout);
      const query = event.target.value;
      this.localSearchTimeout = setTimeout(() => this.performSearch(query), this.options.searchDelay);
    };
    const clearSearch = this.clearLocalSearch.bind(this);
    this.clearLocalSearch = (...args) => {
      clearSearch(...args);
      this.expandedNodes = new Set(options.expanded || []);
    };
    this._popupAbort = new AbortController();
    for (const event of ['resize', 'scroll']) window.addEventListener(event, () => {
      if (this.isVisible) this.positionDropdown();
    }, { capture: true, signal: this._popupAbort.signal });
  }

  destroy() { this._popupAbort.abort(); super.destroy(); }
}
