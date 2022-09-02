using System;
using System.Collections.Generic;

namespace OpenFeature.SDK.Model
{
    /// <summary>
    /// A KeyValuePair with a string key and object value that is used to apply user defined properties
    /// to the feature flag evaluation context.
    /// </summary>
    /// <seealso href="https://github.com/open-feature/spec/blob/main/specification/evaluation-context.md">Evaluation context</seealso>
    public class EvaluationContext
    {
        private readonly Structure _structure = new Structure();

        /// <summary>
        /// Gets the Value at the specified key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Value GetValue(string key) => this._structure.GetValue(key);

        /// <summary>
        /// Bool indicating if the specified key exists in the evaluation context
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(string key) => this._structure.ContainsKey(key);

        /// <summary>
        /// Removes the Value at the specified key
        /// </summary>
        /// <param name="key"></param>
        public void Remove(string key) => this._structure.Remove(key);

        /// <summary>
        /// Gets the value associated with the specified key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(string key, out Value value) => this._structure.TryGetValue(key, out value);

        /// <summary>
        /// Gets all values as a Dictionary
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, Value> AsDictionary()
        {
            return new Dictionary<string, Value>(this._structure.AsDictionary());
        }

        /// <summary>
        /// Add a new bool Value to the evaluation context
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public EvaluationContext Add(string key, bool value)
        {
            this._structure.Add(key, value);
            return this;
        }

        /// <summary>
        /// Add a new string Value to the evaluation context
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public EvaluationContext Add(string key, string value)
        {
            this._structure.Add(key, value);
            return this;
        }

        /// <summary>
        /// Add a new int Value to the evaluation context
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public EvaluationContext Add(string key, int value)
        {
            this._structure.Add(key, value);
            return this;
        }

        /// <summary>
        /// Add a new double Value to the evaluation context
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public EvaluationContext Add(string key, double value)
        {
            this._structure.Add(key, value);
            return this;
        }

        /// <summary>
        /// Add a new DateTime Value to the evaluation context
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public EvaluationContext Add(string key, DateTime value)
        {
            this._structure.Add(key, value);
            return this;
        }

        /// <summary>
        /// Add a new Structure Value to the evaluation context
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public EvaluationContext Add(string key, Structure value)
        {
            this._structure.Add(key, value);
            return this;
        }

        /// <summary>
        /// Add a new List Value to the evaluation context
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public EvaluationContext Add(string key, List<Value> value)
        {
            this._structure.Add(key, value);
            return this;
        }

        /// <summary>
        /// Add a new Value to the evaluation context
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public EvaluationContext Add(string key, Value value)
        {
            this._structure.Add(key, value);
            return this;
        }

        /// <summary>
        /// Return a count of all values
        /// </summary>
        public int Count => this._structure.Count;

        /// <summary>
        /// Merges provided evaluation context into this one.
        /// Any duplicate keys will be overwritten.
        /// </summary>
        /// <param name="other"><see cref="EvaluationContext"/></param>
        public void Merge(EvaluationContext other)
        {
            foreach (var key in other._structure.Keys)
            {
                if (this._structure.ContainsKey(key))
                {
                    this._structure[key] = other._structure[key];
                }
                else
                {
                    this._structure.Add(key, other._structure[key]);
                }
            }
        }

        /// <summary>
        /// Return an enumerator for all values
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<string, Value>> GetEnumerator()
        {
            return this._structure.GetEnumerator();
        }
    }
}
