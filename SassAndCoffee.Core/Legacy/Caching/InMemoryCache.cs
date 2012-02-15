namespace SassAndCoffee.Core {
    using System;
    using System.Collections.Generic;

    public class InMemoryCache : ICompiledCache {
        readonly MemoizingMRUCache<string, object> _cache;
        readonly Dictionary<string, Func<string, object>> _delegateIndex = new Dictionary<string, Func<string, object>>();

        public InMemoryCache() {
            _cache = new MemoizingMRUCache<string, object>((file, _) => {
                Func<string, object> compiler = null;
                lock (_delegateIndex) {
                    compiler = _delegateIndex[file];
                }

                return compiler(file);
            }, 50);
        }

        public object GetOrAdd(string fileName, Func<string, object> compilationDelegate, string mimeType) {
            lock (_delegateIndex) {
                _delegateIndex[fileName] = compilationDelegate;
            }

            return _cache.Get(fileName);
        }

        public void Clear() {
            lock (_delegateIndex) { _delegateIndex.Clear(); }
            _cache.InvalidateAll();
        }
    }
}
