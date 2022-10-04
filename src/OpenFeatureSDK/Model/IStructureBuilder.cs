using System;
using System.Collections.Generic;

namespace OpenFeatureSDK.Model
{
    /// <summary>
    /// Interface for building immutable <see cref="Structure"/> objects.
    /// </summary>
    public interface IStructureBuilder
    {
        /// <summary>
        /// Set the key to the given <see cref="Value"/>.
        /// </summary>
        /// <param name="key">The key for the value</param>
        /// <param name="value">The value to set</param>
        /// <returns>This builder</returns>
        StructureBuilder Set(string key, Value value);

        /// <summary>
        /// Set the key to the given string.
        /// </summary>
        /// <param name="key">The key for the value</param>
        /// <param name="value">The value to set</param>
        /// <returns>This builder</returns>
        StructureBuilder Set(string key, string value);

        /// <summary>
        /// Set the key to the given int.
        /// </summary>
        /// <param name="key">The key for the value</param>
        /// <param name="value">The value to set</param>
        /// <returns>This builder</returns>
        StructureBuilder Set(string key, int value);

        /// <summary>
        /// Set the key to the given double.
        /// </summary>
        /// <param name="key">The key for the value</param>
        /// <param name="value">The value to set</param>
        /// <returns>This builder</returns>
        StructureBuilder Set(string key, double value);

        /// <summary>
        /// Set the key to the given long.
        /// </summary>
        /// <param name="key">The key for the value</param>
        /// <param name="value">The value to set</param>
        /// <returns>This builder</returns>
        StructureBuilder Set(string key, long value);

        /// <summary>
        /// Set the key to the given bool.
        /// </summary>
        /// <param name="key">The key for the value</param>
        /// <param name="value">The value to set</param>
        /// <returns>This builder</returns>
        StructureBuilder Set(string key, bool value);

        /// <summary>
        /// Set the key to the given <see cref="IStructure"/>.
        /// </summary>
        /// <param name="key">The key for the value</param>
        /// <param name="value">The value to set</param>
        /// <returns>This builder</returns>
        StructureBuilder Set(string key, IStructure value);

        /// <summary>
        /// Set the key to the given DateTime.
        /// </summary>
        /// <param name="key">The key for the value</param>
        /// <param name="value">The value to set</param>
        /// <returns>This builder</returns>
        StructureBuilder Set(string key, DateTime value);

        /// <summary>
        /// Set the key to the given list.
        /// </summary>
        /// <param name="key">The key for the value</param>
        /// <param name="value">The value to set</param>
        /// <returns>This builder</returns>
        StructureBuilder Set(string key, IList<Value> value);

        /// <summary>
        /// Remove the specified key.
        /// </summary>
        /// <param name="key">The key to remove</param>
        /// <returns>This builder</returns>
        StructureBuilder Remove(string key);

        /// <summary>
        /// Build an immutable <see cref="Structure"/>/
        /// </summary>
        /// <returns>The built <see cref="Structure"/></returns>
        IStructure Build();
    }
}
