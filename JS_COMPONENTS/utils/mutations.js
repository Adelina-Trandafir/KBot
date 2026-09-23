const watchTreeview = (() => {
  // log propriu, independent
  const log = (() => {
    const fn = (message, data = null) => {
      const now = new Date();
      const ts = `${now.toLocaleTimeString('en-GB', { hour12: false })}.${now
        .getMilliseconds()
        .toString()
        .padStart(3, '0')}`;
      const CPN = 'TREEVIEW-WATCH'.padEnd(15);
      console.log(
        `%c[${ts}] [${CPN}] ${message}`,
        'color: #77ff87ff; font-weight: bold;',
        data ?? ''
      );
    };

    fn.error = (message, data = null) => {
      const now = new Date();
      const ts = `${now.toLocaleTimeString('en-GB', { hour12: false })}.${now
        .getMilliseconds()
        .toString()
        .padStart(3, '0')}`;
      const CPN = 'TREEVIEW-WATCH'.padEnd(15);
      console.error(
        `%c[${ts}] [${CPN}] ${message}`,
        'color: #ef4444; font-weight: bold;',
        data ?? ''
      );
    };

    return fn;
  })();

  // wrapper pentru a detecta schimbările prin classList și className
  const observeClassChanges = (el) => {
    const originalAdd = el.classList.add;
    const originalRemove = el.classList.remove;

    el.classList.add = function (...args) {
      log(`classList.add called: ${args.join(', ')}`, el);
      console.trace();
      return originalAdd.apply(this, args);
    };

    el.classList.remove = function (...args) {
      log(`classList.remove called: ${args.join(', ')}`, el);
      console.trace();
      return originalRemove.apply(this, args);
    };

    Object.defineProperty(el, 'className', {
      set: function (value) {
        log(`className set to: ${value}`, el);
        console.trace();
        this.setAttribute('class', value);
      },
      get: function () {
        return this.getAttribute('class');
      },
    });

    const classObserver = new MutationObserver((mutations) => {
      mutations.forEach((mutation) => {
        if (mutation.attributeName === 'class') {
          //log('Clasa "treeview" s-a schimbat (MutationObserver):', el.className);
        }
      });
    });
    classObserver.observe(el, { attributes: true });
  };

  // observăm DOM-ul pentru elemente noi .treeview
  const domObserver = new MutationObserver((mutations) => {
    mutations.forEach((mutation) => {
      mutation.addedNodes.forEach((node) => {
        if (node.nodeType === 1 && node.classList.contains('treeview')) {
          log('Elementul treeview a apărut în DOM!', node);
          observeClassChanges(node);
        }
      });
    });
  });

  domObserver.observe(document.body, { childList: true, subtree: true });

  // pentru elementele deja existente
  document.querySelectorAll('.treeview').forEach((el) => {
    log('Element treeview deja prezent în DOM', el);
    observeClassChanges(el);
  });

  return log; // pentru a putea fi folosit și extern dacă vrei
})();
