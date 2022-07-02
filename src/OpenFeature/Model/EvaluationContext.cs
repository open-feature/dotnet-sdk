using System.Collections;
using System.Collections.Generic;

namespace OpenFeature.Model
{
    public class EvaluationContext : IEnumerable<object>
    {
        private readonly Dictionary<string, object> _internalContext = new Dictionary<string, object>();

        public void Add<T>(string key, T value)
        {
            _internalContext.Add(key, value);
        }
        
        public void Remove(string key)
        {
            _internalContext.Remove(key);
        }
        
        public T Get<T>(string key)
        {
            return (T)_internalContext[key];
        }
        
        public object this[string key]
        {
            get => _internalContext[key];
            set => _internalContext[key] = value;
        }

        public void Merge(EvaluationContext other)
        {
            foreach (var key in other._internalContext.Keys)
            {
                if (_internalContext.ContainsKey(key))
                {
                    _internalContext[key] = other._internalContext[key];
                }
                else
                {
                    _internalContext.Add(key, other._internalContext[key]);
                }
            }
        }
        
        public int Count => _internalContext.Count;

        public IEnumerator<object> GetEnumerator()
        {
            return _internalContext.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}