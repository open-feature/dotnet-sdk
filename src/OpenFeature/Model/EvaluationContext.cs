using System.Collections;
using System.Collections.Generic;

namespace OpenFeature.Model
{
    public class EvaluationContext : IEnumerable<KeyValuePair<string, object>>
    {
        private readonly Dictionary<string, object> _internalContext = new Dictionary<string, object>();

        public void Add<T>(string key, T value)
        {
            this._internalContext.Add(key, value);
        }

        public void Remove(string key)
        {
            this._internalContext.Remove(key);
        }

        public T Get<T>(string key)
        {
            return (T)this._internalContext[key];
        }

        public object this[string key]
        {
            get => this._internalContext[key];
            set => this._internalContext[key] = value;
        }

        public void Merge(EvaluationContext other)
        {
            foreach (var key in other._internalContext.Keys)
            {
                if (this._internalContext.ContainsKey(key))
                {
                    this._internalContext[key] = other._internalContext[key];
                }
                else
                {
                    this._internalContext.Add(key, other._internalContext[key]);
                }
            }
        }

        public int Count => this._internalContext.Count;
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return this._internalContext.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
