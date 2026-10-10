// Run with: node --experimental-vm-modules --test PYTHON/tests/portal_messages.test.mjs
import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import vm from 'node:vm';

async function environment(subscribed) {
  const emitted = [];
  const bus = {
    listenerCount: (name) => subscribed && name === 'ui:message' ? 1 : 0,
    emit: (...args) => emitted.push(args),
  };
  const context = vm.createContext({ Date });
  const source = await readFile(new URL('../static/js/portal/messages.js', import.meta.url), 'utf8');
  const module = new vm.SourceTextModule(source, { context });
  await module.link(() => new vm.SyntheticModule(['default'], function () {
    this.setExport('default', bus);
  }, { context }));
  await module.evaluate();
  const element = { textContent: '', hidden: true, setAttribute(name, value) { this[name] = value; } };
  return { messages: module.namespace, element, emitted };
}

test('messages display and remain in the journal without optional bus subscribers', async () => {
  const { messages, element, emitted } = await environment(false);
  messages.showMessage(element, 'Alegeți subunitatea în care lucrați.');
  assert.equal(element.textContent, 'Alegeți subunitatea în care lucrați.');
  assert.equal(element.hidden, false);
  assert.equal(element.role, 'status');
  assert.equal(messages.messageHistory()[0].message, element.textContent);
  assert.equal(emitted.length, 0);
  messages.showMessage(element, 'Eroare', 'error');
  assert.equal(element.role, 'alert');
  assert.equal(messages.messageHistory().length, 2);
  messages.showMessage(element, '');
  assert.equal(element.hidden, true);
  assert.equal(messages.messageHistory().length, 2);
});

test('subscribers still receive message levels without business data', async () => {
  const { messages, element, emitted } = await environment(true);
  messages.showMessage(element, 'Eroare', 'error');
  assert.equal(emitted.length, 1);
  assert.equal(emitted[0][0], 'ui:message');
  assert.deepEqual(Object.keys(emitted[0][1]), ['level']);
  assert.equal(emitted[0][1].level, 'error');
  messages.showMessage(element, '');
  assert.equal(emitted.length, 1);
});
