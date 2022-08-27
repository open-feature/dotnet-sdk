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
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Value GetValue(string key) => this._structure.GetValue(key);

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        public void Remove(string key) => this._structure.Remove(key);

        /// <summary>
        ///
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
        ///
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
        ///
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
        ///
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
        ///
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
        ///
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
        ///
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
        ///
        /// </summary>
        public int Count => this._structure.Count;

        /// <summary>
        /// Merges provided evaluation context into this one
        ///
        /// Any duplicate keys will be overwritten
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
        ///
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<string, Value>> GetEnumerator()
        {
            return this._structure.GetEnumerator();
        }
    }
}
