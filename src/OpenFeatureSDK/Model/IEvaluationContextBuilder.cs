using System;

namespace OpenFeatureSDK.Model
{
    /// <summary>
    /// Interface for building immutable <see cref="EvaluationContext"/>.
    /// </summary>
    public interface IEvaluationContextBuilder
    {
        /// <summary>
        /// Set the key to the given <see cref="Value"/>.
        /// </summary>
        /// <param name="key">The key for the value</param>
        /// <param name="value">The value to set</param>
        /// <returns>This builder</returns>
        EvaluationContextBuilder Set(string key, Value value);

        /// <summary>
        /// Set the key to the given string.
        /// </summary>
        /// <param name="key">The key for the value</param>
        /// <param name="value">The value to set</param>
        /// <returns>This builder</returns>
        EvaluationContextBuilder Set(string key, string value);

        /// <summary>
        /// Set the key to the given int.
        /// </summary>
        /// <param name="key">The key for the value</param>
        /// <param name="value">The value to set</param>
        /// <returns>This builder</returns>
        EvaluationContextBuilder Set(string key, int value);

        /// <summary>
        /// Set the key to the given double.
        /// </summary>
        /// <param name="key">The key for the value</param>
        /// <param name="value">The value to set</param>
        /// <returns>This builder</returns>
        EvaluationContextBuilder Set(string key, double value);

        /// <summary>
        /// Set the key to the given long.
        /// </summary>
        /// <param name="key">The key for the value</param>
        /// <param name="value">The value to set</param>
        /// <returns>This builder</returns>
        EvaluationContextBuilder Set(string key, long value);

        /// <summary>
        /// Set the key to the given bool.
        /// </summary>
        /// <param name="key">The key for the value</param>
        /// <param name="value">The value to set</param>
        /// <returns>This builder</returns>
        EvaluationContextBuilder Set(string key, bool value);

        /// <summary>
        /// Set the key to the given <see cref="Structure"/>.
        /// </summary>
        /// <param name="key">The key for the value</param>
        /// <param name="value">The value to set</param>
        /// <returns>This builder</returns>
        EvaluationContextBuilder Set(string key, IStructure value);

        /// <summary>
        /// Set the key to the given DateTime.
        /// </summary>
        /// <param name="key">The key for the value</param>
        /// <param name="value">The value to set</param>
        /// <returns>This builder</returns>
        EvaluationContextBuilder Set(string key, DateTime value);

        /// <summary>
        /// Remove the given key from the context.
        /// </summary>
        /// <param name="key">The key to remove</param>
        /// <returns>This builder</returns>
        EvaluationContextBuilder Remove(string key);

        /// <summary>
        /// Incorporate an existing context into the builder.
        /// <para>
        /// Any existing keys in the builder will be replaced by keys in the context.
        /// </para>
        /// </summary>
        /// <param name="context">The context to add merge</param>
        /// <returns>This builder</returns>
        EvaluationContextBuilder Merge(IEvaluationContext context);

        /// <summary>
        /// Build an immutable <see cref="EvaluationContext"/>.
        /// </summary>
        /// <returns>An immutable <see cref="EvaluationContext"/></returns>
        IEvaluationContext Build();
    }
}
