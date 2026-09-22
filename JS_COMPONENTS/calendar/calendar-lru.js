/**
 * ========== LRU CACHE PENTRU CALENDAR ==========
 * Previne memory leaks prin limitarea cache-ului
 * Least Recently Used - păstrează doar datele recente
 */

export class CalendarLRUCache {
  constructor(maxSize = 100, ttl = 5 * 60 * 1000) {
    this.maxSize = maxSize;
    this.ttl = ttl; // Time To Live în milliseconds
    this.cache = new Map();
    this.timestamps = new Map();
    this.accessOrder = [];

    // Pornește cleanup periodic
    this.startPeriodicCleanup();
  }

  /**
   * 🎯 SET - Adaugă sau actualizează o valoare
   */
  set(key, value) {
    // Verifică dacă trebuie să facem loc
    if (!this.cache.has(key) && this.cache.size >= this.maxSize) {
      this.evictLRU();
    }

    // Adaugă/actualizează valoarea
    this.cache.set(key, value);
    this.timestamps.set(key, Date.now());

    // Actualizează ordinea de acces
    this.updateAccessOrder(key);

    return this;
  }

  /**
   * 🎯 GET - Obține o valoare din cache
   */
  get(key) {
    // Verifică dacă există
    if (!this.cache.has(key)) {
      return undefined;
    }

    // Verifică TTL
    const timestamp = this.timestamps.get(key);
    if (Date.now() - timestamp > this.ttl) {
      // Expired
      this.delete(key);
      return undefined;
    }

    // Actualizează ordinea de acces
    this.updateAccessOrder(key);

    return this.cache.get(key);
  }

  /**
   * 🎯 HAS - Verifică dacă o cheie există și e validă
   */
  has(key) {
    if (!this.cache.has(key)) {
      return false;
    }

    // Verifică TTL
    const timestamp = this.timestamps.get(key);
    if (Date.now() - timestamp > this.ttl) {
      this.delete(key);
      return false;
    }

    return true;
  }

  /**
   * 🗑️ EVICT LRU - Elimină cel mai puțin recent folosit
   */
  evictLRU() {
    if (this.accessOrder.length === 0) return;

    const lruKey = this.accessOrder.shift(); // Primul e cel mai vechi
    this.cache.delete(lruKey);
    this.timestamps.delete(lruKey);

    console.log(`🗑️ Evicted LRU entry: ${lruKey}`);
  }

  /**
   * 🔄 PERIODIC CLEANUP - Curăță entries expirate
   */
  startPeriodicCleanup() {
    // Rulează la fiecare minut
    this.cleanupInterval = setInterval(() => {
      this.cleanupExpired();
    }, 60 * 1000);
  }

  /**
   * 🧹 CLEANUP EXPIRED - Șterge toate entries expirate
   */
  cleanupExpired() {
    const now = Date.now();
    const expiredKeys = [];

    this.timestamps.forEach((timestamp, key) => {
      if (now - timestamp > this.ttl) {
        expiredKeys.push(key);
      }
    });

    expiredKeys.forEach((key) => {
      this.delete(key);
    });

    if (expiredKeys.length > 0) {
      console.log(`🧹 Cleaned ${expiredKeys.length} expired entries`);
    }
  }

  /**
   * 📏 ESTIMATE MEMORY USAGE
   */
  estimateMemoryUsage() {
    let totalSize = 0;

    this.cache.forEach((value, key) => {
      // Estimare aproximativă
      totalSize += key.length * 2; // Unicode chars
      totalSize += JSON.stringify(value).length * 2;
    });

    // Convertește în KB
    return Math.round(totalSize / 1024) + ' KB';
  }

  /**
   * 🛑 DESTROY - Cleanup complet
   */
  destroy() {
    if (this.cleanupInterval) {
      clearInterval(this.cleanupInterval);
    }
    this.clear();
  }
}

/**
 * ========== CALENDAR DATA CACHE MIXIN ==========
 * Integrează LRU Cache în Calendar
 */
export const CalendarDataCacheMixin = {
  /**
   * 🎯 INIT DATA CACHE
   */
  initDataCache() {
    // Înlocuiește Map simplu cu LRU Cache
    this.dataCache = new CalendarLRUCache(
      100, // Max 100 entries
      5 * 60 * 1000 // 5 minute TTL
    );

    this.log('✅ LRU Cache inițializat');
  },

  /**
   * 📦 CACHE DATA
   */
  cacheData(dateString, data) {
    this.dataCache.set(dateString, data);
  },

  /**
   * 📦 GET CACHED DATA
   */
  getCachedData(dateString) {
    return this.dataCache.get(dateString);
  },

  /**
   * 📊 GET CACHE STATS
   */
  getCacheStats() {
    return this.dataCache.getStats();
  },

  /**
   * 🧹 CLEANUP CACHE
   */
  cleanupCache() {
    this.dataCache.cleanupExpired();

    const stats = this.getCacheStats();
    this.log(`📊 Cache stats: ${stats.size}/${stats.maxSize} entries, ${stats.memoryUsage}`);
  },

  delete(key) {
    this.cache.delete(key);
    this.timestamps.delete(key);

    const index = this.accessOrder.indexOf(key);
    if (index > -1) {
      this.accessOrder.splice(index, 1);
    }

    return true;
  },

  /**
   * 🧹 CLEAR - Golește tot cache-ul
   */
  clear() {
    this.cache.clear();
    this.timestamps.clear();
    this.accessOrder = [];
  },

  /**
   * 📊 GET STATS - Statistici despre cache
   */
  getStats() {
    return {
      size: this.cache.size,
      maxSize: this.maxSize,
      ttl: this.ttl,
      oldestEntry: this.accessOrder[0],
      newestEntry: this.accessOrder[this.accessOrder.length - 1],
      memoryUsage: this.estimateMemoryUsage(),
    };
  },

  /**
   * 🔄 UPDATE ACCESS ORDER
   * Mută cheia la sfârșitul listei (most recently used)
   */
  updateAccessOrder(key) {
    const index = this.accessOrder.indexOf(key);

    if (index > -1) {
      // Există deja, mută la final
      this.accessOrder.splice(index, 1);
    }

    this.accessOrder.push(key);
  },
};
