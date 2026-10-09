import test from 'node:test';
import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
const source = await readFile(new URL('../static/js/dgv/selection.js', import.meta.url), 'utf8');
const { selectRows } = await import(`data:text/javascript;base64,${Buffer.from(source).toString('base64')}`);

test('Shift selects the sorted range including offscreen rows, Ctrl toggles individual rows', () => {
  const rows = ['Z', 'A', 'C', 'B'];
  let selected = selectRows(new Set(), rows, 'A', null);
  selected = selectRows(selected, rows, 'B', 'A', { shiftKey: true });
  assert.deepEqual([...selected], ['A', 'C', 'B']);
  selected = selectRows(selected, rows, 'C', 'A', { ctrlKey: true });
  assert.deepEqual([...selected], ['A', 'B']);
  selected = selectRows(selected, rows, 'Z', 'A', { toggle: true });
  assert.deepEqual([...selected], ['A', 'B', 'Z']);
  assert.deepEqual([...selectRows(selected, rows, 'C', 'A')], ['C']);
});

test('Reverse range and missing filtered anchor select valid rows only', () => {
  assert.deepEqual([...selectRows(new Set(), [1, 2, 3, 4], 1, 4, { shiftKey: true })], [1, 2, 3, 4]);
  assert.deepEqual([...selectRows(new Set(), [1, 4], 4, 2, { shiftKey: true })], [4]);
});
