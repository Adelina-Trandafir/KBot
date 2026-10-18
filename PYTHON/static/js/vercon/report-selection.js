import { el, paintTree } from './ui.js';

const FILTERS = [['supplier', 'Furnizori', 'suppliers'], ['receiver', 'Primitori', 'receivers'], ['document', 'Documente', 'documents']];

export function selectionNodes(response) {
  const nodes = [];
  if (response.account_filter && response.spec.filters.includes('category')) {
    nodes.push({ id: 'selection:use-accounts', label: 'Conturi în folosință', children: response.categories.map(item => ({ id: `category-filter:${item.id}`, label: item.label })) });
  }
  if (response.spec.filters.includes('variant')) {
    const categories = new Map();
    for (const item of response.variants) {
      const category = String(item.category ?? 'none');
      if (!categories.has(category)) categories.set(category, { id: `category:${category}`, label: item.account || 'Fără încadrare', children: [], products: new Map() });
      const bucket = categories.get(category), product = String(item.product ?? item.id);
      if (!bucket.products.has(product)) {
        const node = { id: `product:${category}:${product}`, label: `${item.name || item.label}${item.um ? ' / ' + item.um : ''}`, children: [] };
        bucket.products.set(product, node); bucket.children.push(node);
      }
      bucket.products.get(product).children.push({ id: `variant:${item.id}`, label: item.variant_label || item.label });
    }
    const children = [...categories.values()].map(({ products, ...node }) => node);
    if (children.length) nodes.push({ id: 'selection:products', label: 'Produse', children });
  }
  for (const [key, label, collection] of FILTERS) {
    if (!response.spec.filters.includes(key) || !response[collection]?.length) continue;
    const title = key !== 'receiver' ? label : response.spec.family === 'bs' ? 'Predători' : response.spec.ledger === 'custody' ? 'Deținători' : label;
    nodes.push({ id: `selection:${key}`, label: title, children: response[collection].map(item => ({ id: `${key}:${item.id}`, label: item.label })) });
  }
  return nodes;
}

export function mountSelection(host, response, checked, onChange, owner, { loadBranch, onDocuments, onBusy } = {}) {
  const nodes = response.nodes;
  const chosen = new Set(checked), pending = new Map(), checking = new Set();
  const controller = new AbortController();
  let disposed = false, suppress = true, searchVersion = 0;
  const leaf = id => /^(variant|category-filter|supplier|receiver|document):/.test(id);
  const snapshot = items => items.map(node => ({ ...node, children: node.lazy
    ? [{ id: `waiting:${node.id}`, label: 'Deschideți ramura pentru încărcare.', selectable: false }]
    : node.children ? snapshot(node.children) : undefined }));
  const find = (id, items = nodes) => {
    for (const node of items) {
      if (node.id === id) return node;
      const child = node.children && find(id, node.children);
      if (child) return child;
    }
    return null;
  };
  const hasLazy = node => node.lazy || node.children?.some(hasLazy);
  host._vcExpanded = new Set();
  const tree = paintTree(host, snapshot(nodes), { title: 'Selectare date', collapsed: true,
    checkable: true, branchChecks: true,
    onToggle: (id, open) => { if (open) void load(id).catch(failed); },
    onCheckChange: ids => {
      if (suppress || disposed) return;
      // Selections in branches not loaded yet remain intact.
      for (const id of chosen) if (tree._checkIndex.has(id)) chosen.delete(id);
      ids.filter(leaf).forEach(id => chosen.add(id));
      onChange([...chosen]);
    },
  }, owner);
  const hint = el('p', { className: 'vc-hint', textContent: 'Deschideți ramurile pentru încărcare și bifați datele dorite. Fără bife într-o categorie de filtre se folosesc toate datele acesteia.' });
  const error = el('p', { className: 'vc-hint', role: 'status', hidden: true });
  host.append(hint, error);
  const failed = problem => {
    if (disposed || problem.name === 'AbortError') return;
    error.textContent = `${problem.message} Închideți și redeschideți ramura pentru a reîncerca.`;
    error.hidden = false;
  };
  const hasCheckbox = tree.hasCheckbox.bind(tree);
  tree.hasCheckbox = node => {
    const full = tree._checkIndex.get(String(node.id)) || node;
    return !String(full.id).startsWith('waiting:')
      && (full.children?.length ? hasCheckbox(full) : leaf(full.id));
  };

  function redraw() {
    const expanded = new Set(tree.expandedNodes), top = tree.treeContainer.scrollTop;
    const query = tree.searchInput?.value.trim().toLowerCase() || '';
    suppress = true;
    try {
      tree.setData(snapshot(nodes));
      tree.setCheckedLeaves([...chosen]);
      tree.expandedNodes = expanded;
      tree.treeContainer.replaceChildren(); tree.isTreeRendered = false;
      if (query.length >= 3) tree.renderTreeWithData(query);
      else tree.renderTree('');
      if (tree.searchInput) tree.searchInput.value = query;
      if (tree.searchClear) tree.searchClear.hidden = !query;
      tree.treeContainer.scrollTop = top;
    } finally { suppress = false; }
  }

  function load(id, recursive = false) {
    const node = find(id);
    if (disposed || !node || !(recursive ? hasLazy(node) : node.lazy)) return Promise.resolve();
    if (pending.has(id)) return pending.get(id).then(() => load(id, recursive));
    const task = (async () => {
      tree.treeElement.setAttribute('aria-busy', 'true');
      const result = await loadBranch(id, recursive, controller.signal);
      if (disposed) return;
      node.children = result.nodes; delete node.lazy;
      if (result.documents) onDocuments?.(result.documents);
      // A restored root is now complete: discard stale selections from the old period.
      if (recursive && nodes.includes(node)) {
        const prefix = id === 'selection:products' ? 'variant:'
          : id === 'selection:use-accounts' ? 'category-filter:' : id.slice('selection:'.length) + ':';
        const valid = new Set();
        const collect = items => items.forEach(item => { if (leaf(item.id)) valid.add(item.id); if (item.children) collect(item.children); });
        collect(node.children);
        let changed = false;
        for (const selected of chosen) if (selected.startsWith(prefix) && !valid.has(selected)) { chosen.delete(selected); changed = true; }
        if (changed) onChange([...chosen]);
      }
      error.hidden = true; redraw();
    })().finally(() => {
      pending.delete(id);
      if (!disposed) {
        tree.treeElement.setAttribute('aria-busy', String(pending.size > 0));
        onBusy?.(pending.size > 0 || checking.size > 0);
      }
    });
    pending.set(id, task);
    onBusy?.(true);
    return task;
  }

  const toggleCheck = tree.toggleCheckNode.bind(tree);
  tree.toggleCheckNode = id => {
    if (checking.has(id) || disposed) return;
    const node = find(id);
    if (!node || !hasLazy(node)) { toggleCheck(id); return; }
    checking.add(id);
    onBusy?.(true);
    void load(id, true).then(() => { if (!disposed) toggleCheck(id); })
      .catch(failed).finally(() => {
        checking.delete(id);
        if (!disposed) onBusy?.(pending.size > 0 || checking.size > 0);
      });
  };
  // Searching explicitly loads the remaining branches, then uses the common tree filter.
  tree.handleLocalSearch = event => {
    clearTimeout(tree.localSearchTimeout);
    const query = event.target.value.trim().toLowerCase(), version = ++searchVersion;
    tree.localSearchTimeout = setTimeout(async () => {
      if (disposed || version !== searchVersion) return;
      try {
        if (query.length >= 3) await Promise.all(nodes.map(node => load(node.id, true)));
        if (disposed || version !== searchVersion) return;
        tree.expandedNodes.clear(); tree.treeContainer.replaceChildren(); tree.isTreeRendered = false;
        if (query.length >= 3) tree.renderTreeWithData(query);
        else { tree.clearLocalSearch(false); tree.renderTree(''); }
        if (tree.searchClear) tree.searchClear.hidden = !query;
      } catch (problem) { failed(problem); }
    }, 300);
  };
  const cancelSearch = () => { ++searchVersion; clearTimeout(tree.localSearchTimeout); };
  tree.searchClear?.addEventListener('click', cancelSearch, { capture: true, signal: controller.signal });
  tree.searchInput?.addEventListener('keydown', event => {
    if (event.key === 'Escape') cancelSearch();
  }, { capture: true, signal: controller.signal });
  redraw();
  owner.own(() => { disposed = true; ++searchVersion; clearTimeout(tree.localSearchTimeout); controller.abort(); });
  const selectedRoots = nodes.filter(node => {
    const prefix = node.id === 'selection:products' ? 'variant:'
      : node.id === 'selection:use-accounts' ? 'category-filter:' : node.id.slice('selection:'.length) + ':';
    return [...chosen].some(id => id.startsWith(prefix));
  });
  return { getCheckedLeaves: () => [...chosen], ready: Promise.all(selectedRoots.map(node => load(node.id, true))) };
}

export function selectionFilters(ids) {
  const collected = {};
  for (const id of ids) {
    const [rawKey, value] = id.split(':');
    const key = rawKey === 'category-filter' ? 'category' : rawKey;
    if (!['variant', 'category', 'supplier', 'receiver', 'document'].includes(key)) continue;
    (collected[key] ||= []).push(value);
  }
  const filters = {};
  for (const [key, values] of Object.entries(collected)) {
    if (values.length === 1) filters[key] = values[0]; else filters[`${key}_ids`] = values.join(',');
  }
  return filters;
}

export function selectionError(spec, ids) {
  if (!spec) return 'Alegeți raportul.';
  for (const [key, label] of [['variant', 'produs / variantă'], ['supplier', 'furnizor'], ['document', 'document']]) {
    if (spec[`${key}_required`] && ids.filter(id => id.startsWith(key + ':')).length !== 1) return `Alegeți un singur ${label} pentru acest raport.`;
  }
  return '';
}
