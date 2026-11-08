import { el, paintTree, CheckBox } from './ui.js';
import { searchDelay, SEARCH_MIN_CHARS } from './search-settings.js';
import { searchLength } from '../utils/search-text.js';

const FILTERS = [['receiver', 'Primitori', 'receivers'], ['document', 'Documente', 'documents']];

export function selectionHeader(owner, onChange) {
  const node = el('header', { className: 'vc-reports__selection-header' }, el('h4', { textContent: 'Selectare date' }));
  const check = owner.own(new CheckBox(node, { label: 'Selectează TOATE', disabled: true,
    className: 'vc-reports__select-all', onChange }));
  return { node, check };
}

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
      bucket.products.get(product).children.push({ id: `variant:${item.id}`, label: item.variant_label || item.label,
        sortType: 'number', sortValue: item.price, sortThen: [{ type: 'number', value: item.id }] });
    }
    const children = [...categories.values()].map(({ products, ...node }) => node);
    nodes.push(...children);
  }
  for (const [key, label, collection] of FILTERS) {
    if (!response.spec.filters.includes(key) || !response[collection]?.length) continue;
    const title = key !== 'receiver' ? label : response.spec.family === 'bs' ? 'Predători' : response.spec.ledger === 'custody' ? 'Deținători' : label;
    nodes.push({ id: `selection:${key}`, label: title, children: response[collection].map(item => ({ id: `${key}:${item.id}`, label: item.label })) });
  }
  return nodes;
}

export function mountSelection(host, response, checked, onChange, owner, { loadBranch, onDocuments, onBusy, onError } = {}) {
  let nodes = response.nodes, all = new Set(), complete = false;
  const documentDetails = new Map((response.documents || []).map(item => [String(item.id), item]));
  const chosen = new Set(checked.filter(id => !id.startsWith('supplier:'))), pending = new Map();
  const controller = new AbortController();
  let disposed = false, searchVersion = 0, selecting = false;
  const leaf = id => /^(variant|category-filter|receiver|document|receipt-product|receipt-document-product|outgoing-item|mf-item):/.test(id);
  const prefixOf = node => node.id.startsWith('mf-branch:') ? `mf-item:${node.id.split(':')[1]}:`
    : node.id.startsWith('out-account:') ? `outgoing-item:account:${node.id.split(':')[1]}:`
    : node.id.startsWith('out-receiver:') ? `outgoing-item:receiver:${node.id.split(':')[1]}:`
    : node.id.startsWith('out-document:') ? node.id.replace('out-document:', 'outgoing-item:') + ':'
    : node.id.startsWith('outgoing-item:') ? node.id
    : node.id.startsWith('receipt-supplier:') ? `receipt-product:${node.id.split(':')[1]}:`
    : node.id.startsWith('nir-supplier:') ? `receipt-document-product:${node.id.split(':')[1]}:`
    : node.id.startsWith('document-supplier:') ? 'document:'
    : node.id.startsWith('category:') || node.id === 'selection:products' ? 'variant:'
    : node.id === 'selection:use-accounts' ? 'category-filter:' : node.id.slice('selection:'.length) + ':';
  const leaves = (items, result = new Set()) => {
    for (const node of items) {
      if (leaf(node.id)) result.add(node.id);
      if (node.children) leaves(node.children, result);
    }
    return result;
  };
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
  const tree = paintTree(host, [], { title: 'Selectare date', collapsed: true,
    checkable: true, branchChecks: true,
    onToggle: (id, open) => {
      if (open) void load(id).catch(problem => failed(problem, 'Închideți și redeschideți ramura pentru a reîncerca.'));
    },
    onCheckChange: ids => {
      if (disposed) return;
      // Selections in branches not loaded yet remain intact.
      for (const id of chosen) if (tree._checkIndex.has(id)) chosen.delete(id);
      ids.filter(leaf).forEach(id => chosen.add(id));
      updateAll();
      onChange([...chosen]);
    },
  }, owner);
  // Node labels are already visible; suppress both native and custom tooltips here.
  tree.treeContainer.setAttribute('data-kbot-tooltip', 'off');
  const header = selectionHeader(owner, selectAll);
  host.firstElementChild.replaceWith(header.node);
  const loader = el('div', { className: 'vc-reports__selection-loading', hidden: true, role: 'status' },
    el('span', { className: 'report-loading-spinner', 'aria-hidden': 'true' }),
    el('span', { textContent: 'Se încarcă selecția…' }));
  tree.container.append(loader);
  const hint = el('p', { className: 'vc-hint', textContent: 'Deschideți ramurile pentru încărcare și bifați datele dorite. Fără bife într-o categorie de filtre se folosesc toate datele acesteia.' });
  host.append(hint);
  const failed = (problem, retry = 'Încercați din nou.') => {
    if (disposed || problem.name === 'AbortError') return;
    onError?.(new Error(`${problem.message} ${retry}`));
  };
  const hasCheckbox = tree.hasCheckbox.bind(tree);
  tree.hasCheckbox = node => {
    const full = tree._checkIndex.get(String(node.id)) || node;
    return !String(full.id).startsWith('waiting:')
      && (/^(category|document-supplier|receipt-supplier|nir-supplier|out-account|out-receiver|out-document|mf-branch):/.test(String(full.id)) && (full.lazy || full.children?.length)
        || (full.children?.length ? hasCheckbox(full) : leaf(full.id)));
  };

  function updateAll() {
    header.check.checked = complete && all.size > 0 && chosen.size >= all.size && [...all].every(id => chosen.has(id));
    header.check.input.indeterminate = chosen.size > 0 && !header.check.checked;
  }

  function busy() {
    if (disposed) return;
    const loading = pending.size > 0 || selecting;
    tree.treeElement.setAttribute('aria-busy', String(loading));
    tree.treeElement.inert = selecting;
    loader.hidden = !selecting;
    header.check.setEnabled(nodes.length > 0 && !loading);
    onBusy?.(loading);
  }

  async function selectAfterLoad(id, apply) {
    selecting = true;
    busy();
    try {
      await load(id, true);
      if (!disposed) apply();
    } catch (problem) { failed(problem); }
    finally {
      selecting = false;
      if (!disposed) { updateAll(); busy(); }
    }
  }

  function selectAll(checked) {
    if (disposed || selecting || pending.size) return;
    const apply = () => {
      chosen.clear();
      tree.setCheckedLeaves(checked ? all : []);
    };
    if (checked && !complete) void selectAfterLoad('root', apply);
    else apply();
  }

  function redraw() {
    const top = tree.treeContainer.scrollTop, query = tree.localSearchQuery;
    all = leaves(nodes);
    complete = !nodes.some(hasLazy);
    tree.setData(snapshot(nodes), { render: false });
    tree.setCheckedLeaves(chosen, { refresh: false });
    if (query) tree.renderTreeWithData(query);
    else tree.renderTree('');
    if (!tree.isTreeRendered) tree.renderNoResults();
    tree.treeContainer.scrollTop = top;
    updateAll();
  }

  function load(id, recursive = false) {
    if (disposed) return Promise.resolve();
    if (pending.has('root')) return pending.get('root');
    const wholeTree = id === 'root', node = wholeTree ? null : find(id);
    if (wholeTree ? complete : !node || !(recursive ? hasLazy(node) : node.lazy)) return Promise.resolve();
    if (pending.has(id)) return pending.get(id).then(() => load(id, recursive));
    // A bulk load replaces the tree. Let existing branch loads finish before replacing it.
    const earlier = wholeTree ? [...pending.values()] : [];
    const task = (async () => {
      if (earlier.length) await Promise.allSettled(earlier);
      if (disposed || wholeTree && complete) return;
      const result = await loadBranch(id, recursive, controller.signal);
      if (disposed) return;
      if (wholeTree) {
        if (result.nodes.some(hasLazy)) throw new Error('Serverul nu a încărcat toate ramurile selecției.');
        nodes = result.nodes;
      } else {
        if (find(id) !== node) return;
        node.children = result.nodes; delete node.lazy;
      }
      if (result.documents) {
        if (wholeTree) documentDetails.clear();
        result.documents.forEach(item => documentDetails.set(String(item.id), item));
        onDocuments?.([...documentDetails.values()]);
      }
      redraw();
    })().finally(() => {
      pending.delete(id);
      busy();
    });
    pending.set(id, task);
    busy();
    return task;
  }

  const toggleCheck = tree.toggleCheckNode.bind(tree);
  tree.toggleCheckNode = id => {
    if (disposed || selecting) return;
    const node = find(id);
    if (!node || !hasLazy(node)) { toggleCheck(id); return; }
    void selectAfterLoad(id, () => toggleCheck(id));
  };
  // Searching explicitly loads the remaining branches, then uses the common tree filter.
  tree.handleLocalSearch = event => {
    clearTimeout(tree.localSearchTimeout);
    const query = event.target.value.trim().toLowerCase(), version = ++searchVersion;
    const enough = searchLength(query) >= SEARCH_MIN_CHARS;   // spaces do not count
    tree.localSearchTimeout = setTimeout(async () => {
      if (disposed || version !== searchVersion) return;
      try {
        if (enough) await load('root', true);
        if (disposed || version !== searchVersion) return;
        tree.expandedNodes.clear(); tree.treeContainer.replaceChildren(); tree.isTreeRendered = false;
        if (enough) tree.renderTreeWithData(query);
        else { tree.clearLocalSearch(false); tree.renderTree(''); }
        if (!tree.isTreeRendered) tree.renderNoResults();
        if (tree.searchClear) tree.searchClear.hidden = !query;
      } catch (problem) { failed(problem); }
    }, searchDelay());
  };
  const cancelSearch = () => { ++searchVersion; clearTimeout(tree.localSearchTimeout); };
  tree.searchClear?.addEventListener('click', cancelSearch, { capture: true, signal: controller.signal });
  tree.searchInput?.addEventListener('keydown', event => {
    if (event.key === 'Escape') cancelSearch();
  }, { capture: true, signal: controller.signal });
  redraw();
  busy();
  owner.own(() => { disposed = true; ++searchVersion; clearTimeout(tree.localSearchTimeout); controller.abort(); });
  const restored = [...chosen], lazyRoots = nodes.filter(hasLazy);
  const selectedRoots = lazyRoots.filter(node => {
    const prefix = prefixOf(node);
    return restored.some(id => id.startsWith(prefix));
  });
  const restore = selectedRoots.length > 1 && selectedRoots.length === lazyRoots.length
    ? load('root', true) : Promise.all(selectedRoots.map(node => load(node.id, true)));
  const ready = restore.then(() => {
    if (disposed) return;
    // Accounts share the variant filter: reconcile against their combined leaves.
    let changed = false;
    for (const id of chosen) if (!all.has(id)) { chosen.delete(id); changed = true; }
    if (changed) { updateAll(); onChange([...chosen]); }
  });
  return { getCheckedLeaves: () => [...chosen],
    getFilters: (spec = response.spec) => selectionFilters([...chosen].filter(id => {
      if (id.startsWith('mf-item:')) return !!spec.mf_selection && !header.check.checked;
      if (/^receipt-(product|document-product):/.test(id)) return spec.incoming_selection && !header.check.checked;
      if (id.startsWith('outgoing-item:')) return (spec.outgoing_selection || spec.outgoing_filters) && !header.check.checked;
      const key = id.startsWith('category-filter:') ? 'category' : id.split(':')[0];
      return spec.filters.includes(key) && (!header.check.checked || spec[key + '_required']);
    })), ready };
}

export function selectionFilters(ids) {
  const collected = {};
  const receipts = [];
  const outgoing = [];
  const fixedAssets = [];
  for (const id of ids) {
    if (id.startsWith('mf-item:')) { fixedAssets.push(id); continue; }
    if (/^receipt-(product|document-product):/.test(id)) { receipts.push(id); continue; }
    if (id.startsWith('outgoing-item:')) { outgoing.push(id); continue; }
    const [rawKey, value] = id.split(':');
    const key = rawKey === 'category-filter' ? 'category' : rawKey;
    if (!['variant', 'category', 'receiver', 'document'].includes(key)) continue;
    (collected[key] ||= []).push(value);
  }
  const filters = {};
  if (receipts.length) filters.receipt_selection = receipts.join(',');
  if (outgoing.length) filters.outgoing_selection = outgoing.join(',');
  if (fixedAssets.length) filters.mf_selection = fixedAssets.join(',');
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
