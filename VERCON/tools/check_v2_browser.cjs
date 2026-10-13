// Isolated browser checks. Every request is served from files/fixtures; no app or DB.
const fs = require('node:fs');
const path = require('node:path');
const assert = require('node:assert/strict');
const crypto = require('node:crypto');
const { chromium } = require(path.join(process.argv[2], 'playwright'));
const root = path.resolve(__dirname, '../..');
const staticRoot = path.join(root, 'PYTHON/static');
const cssRoot = path.join(staticRoot, 'css');
const hashes = {
  vercon: 'aab5d4de5b2e676843c56afb4a9b2497aad47fcb685e928b9623883ab132fc4e',
  adechit: 'bba38b067b26ee1d9ad5e126377bfed2af28055e8b9c243508736738c366b8e8',
  portal: '576a961b26953c0b45eb91e1002a7ec7b9594e28580c90eef66ff1bc07ae0fcd',
};
for (const [name, expected] of Object.entries(hashes)) {
  const entry = fs.readFileSync(path.join(cssRoot, name + '.css'), 'utf8');
  const imports = [...entry.matchAll(/@import url\("([^"\n]+)"\);/g)].map((match) => match[1]);
  assert(imports.length > 1);
  const additions = new Set(['vercon-stock.css', 'vercon-mf.css']);
  const original = Buffer.concat(imports.filter((file) => !additions.has(file)).map((file) => fs.readFileSync(path.join(cssRoot, file))));
  assert.equal(crypto.createHash('sha256').update(original).digest('hex'), expected);
  console.log(`${name}: original CSS contents/order preserved`);
}

(async () => {
  const browser = await chromium.launch({ headless: true, executablePath: process.argv[3] });
  const failures = [], errors = [];
  const page = await browser.newPage({ viewport: { width: 1600, height: 1000 } });
  page.on('pageerror', (error) => errors.push(error.message));
  await page.route('http://vercon-local/**', async (route) => {
    const url = new URL(route.request().url());
    if (url.pathname === '/stock-fixture') {
      await route.fulfill({ contentType: 'text/html', body: '<!doctype html><html><head><link rel="stylesheet" href="/static/css/variables.css"><link rel="stylesheet" href="/static/site/site.css"><link rel="stylesheet" href="/static/css/portal.css"><link rel="stylesheet" href="/static/css/vercon.css"><link rel="stylesheet" href="/static/css/dgv.css"><link rel="stylesheet" href="/static/css/treeview/treeview.css"><link rel="stylesheet" href="/static/css/combobox.css"><link rel="stylesheet" href="/static/css/datepicker.css"></head><body class="portal vc-page"><main id="fixture"></main></body></html>' }); return;
    }
    if (/^\/(portal|adechit|vercon)$/.test(url.pathname)) {
      const html = fs.readFileSync(path.join(staticRoot, url.pathname.slice(1) + '.html'), 'utf8');
      await route.fulfill({ contentType: 'text/html', body: html.replace(/<script\b[^>]*>[\s\S]*?<\/script>/gi, '') }); return;
    }
    const file = path.resolve(staticRoot, decodeURIComponent(url.pathname).replace(/^\/static\//, ''));
    if (!file.startsWith(staticRoot + path.sep) || !fs.existsSync(file) || !fs.statSync(file).isFile()) {
      failures.push(url.pathname); await route.fulfill({ status: 404, body: 'Not found' }); return;
    }
    const type = { '.css': 'text/css', '.js': 'application/javascript', '.svg': 'image/svg+xml', '.png': 'image/png' }[path.extname(file)] || 'application/octet-stream';
    await route.fulfill({ contentType: type, body: fs.readFileSync(file) });
  });
  try {
    for (const name of ['portal', 'adechit', 'vercon']) {
      await page.goto('http://vercon-local/' + name);
      const loaded = await page.evaluate(() => {
        let imports = 0;
        function visit(sheet) {
          for (const rule of sheet.cssRules) if (rule.type === CSSRule.IMPORT_RULE) {
            if (!rule.styleSheet || !rule.styleSheet.cssRules.length) throw new Error('Empty import: ' + rule.href);
            imports++; visit(rule.styleSheet);
          }
        }
        for (const sheet of document.styleSheets) visit(sheet);
        return imports;
      });
      assert(loaded > 1);
      console.log(`${name}: stylesheet imports loaded in Chromium (${loaded})`);
    }
    await page.goto('http://vercon-local/stock-fixture');
    await page.evaluate(async () => {
      await import('/static/js/utils/css.js');
      await import('/static/js/utils/z-max.js');
      const { mountStock } = await import('/static/js/vercon/stock.js');
      const quantities = (closing) => ({ opening: '1.00', entries: '2.00', returns: '0.00', transfers_in: '0.00',
        issues: '0.00', storno: '0.00', transfers_out: '0.00', scrapping: '0.00', closing });
      window.requests = [];
      window.stockEditor = mountStock(document.querySelector('#fixture'), {
        signal: new AbortController().signal,
        sel: () => ({ subunit: 4, gestiune: 8, year: 2026, type: 'MATERIALE' }), canOpen: () => false, openForm: () => {},
        api: async (url) => {
          window.requests.push(url);
          const query = new URL(url, location.href).searchParams;
          return { start: query.get('start'), end: query.get('end'), scale: 2, zec: 2,
            categories: [{ id: 9, label: '302.01 Materiale' }], value_totals: { closing: '600.0000' },
            rows: [1, 2].map((variant) => ({ variant, holder: null, holder_name: '', name: variant === 1 ? 'Hârtie' : 'Toner',
              um: 'buc', code: String(variant), category_name: 'Materiale', price: '100.00',
              quantities: quantities('3.00'), values: { closing: '300.0000' } })),
            statement: [{ variant: 2, holder: null, date: '2026-10-01', kind: 'nir', number: '44',
              document_id: 44, line_id: 3, quantity: '2.00', balance: '3.00', balance_value: '300.0000' }] };
        },
      });
    });
    await page.getByText('Hârtie', { exact: true }).first().waitFor();
    await page.getByText('Toner', { exact: true }).first().click();
    await page.locator('.vc-stock__details h4').filter({ hasText: 'Toner' }).waitFor();
    assert.equal(await page.locator('.vc-stock__movements').getByText('44', { exact: true }).count(), 1);
    await page.getByText('Ianuarie', { exact: true }).click();
    await page.waitForFunction(() => window.requests.at(-1).includes('start=2026-01-01'));
    assert.equal(await page.getByLabel('De la', { exact: true }).last().inputValue(), '01.01.2026');
    await page.evaluate(() => window.stockEditor.search('toner'));
    assert.equal(await page.locator('.vc-stock__grid').getByText('Hârtie', { exact: true }).count(), 0);
    for (const width of [390, 1280]) {
      await page.setViewportSize({ width, height: 1000 });
      await page.evaluate(() => document.documentElement.dataset.theme = 'dark');
      assert.equal(await page.locator('.vc-stock').isVisible(), true);
    }
    await page.evaluate(() => window.stockEditor.destroy());
    assert.deepEqual(failures, []);
    assert.deepEqual(errors, []);
    console.log('Stock fixture: selection, movements, month DatePicker sync, search, responsive/dark and cleanup passed');
  } finally { await browser.close(); }
})().catch((error) => { console.error(error); process.exitCode = 1; });
