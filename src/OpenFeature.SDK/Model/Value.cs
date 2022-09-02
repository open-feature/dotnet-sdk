using System;
using System.Collections;
using System.Collections.Generic;

namespace OpenFeature.SDK.Model
{
    /// <summary>
    ///  Values serve as a return type for provider objects. Providers may deal in JSON, protobuf, XML or some other data-interchange format.
    ///  This intermediate representation provides a good medium of exchange.
    /// </summary>
    public class Value
    {
        private readonly object _innerValue;

        /// <summary>
        /// Creates a Value with the inner value set to null
        /// </summary>
        public Value() => this._innerValue = null;

        /// <summary>
        /// Creates a Value with the inner value to the inner value of the value param
        /// </summary>
        /// <param name="value"><see cref="Value">Value type</see></param>
        public Value(Value value) => this._innerValue = value._innerValue;

        /// <summary>
        /// Creates a Value with the inner set to bool type
        /// </summary>
        /// <param name="value"><see cref="bool">Bool type</see></param>
        public Value(bool value) => this._innerValue = value;

        /// <summary>
        /// Creates a Value with the inner set to int type
        /// </summary>
        /// <param name="value"><see cref="int">Int type</see></param>
        public Value(int value) => this._innerValue = value;

        /// <summary>
        /// Creates a Value with the inner set to double type
        /// </summary>
        /// <param name="value"><see cref="double">Double type</see></param>
        public Value(double value) => this._innerValue = value;

        /// <summary>
        /// Creates a Value with the inner set to string type
        /// </summary>
        /// <param name="value"><see cref="string">String type</see></param>
        public Value(string value) => this._innerValue = value;

        /// <summary>
        /// Creates a Value with the inner set to structure type
        /// </summary>
        /// <param name="value"><see cref="Structure">Structure type</see></param>
        public Value(Structure value) => this._innerValue = value;

        /// <summary>
        /// Creates a Value with the inner set to list type
        /// </summary>
        /// <param name="value"><see cref="List{T}">List type</see></param>
        public Value(IList<Value> value) => this._innerValue = value;

        /// <summary>
        /// Creates a Value with the inner set to DateTime type
        /// </summary>
        /// <param name="value"><see cref="DateTime">DateTime type</see></param>
        public Value(DateTime value) => this._innerValue = value;

        /// <summary>
        /// Determines if inner value is null
        /// </summary>
        /// <returns><see cref="bool">True if value is null</see></returns>
        public bool IsNull() => this._innerValue is null;

        /// <summary>
        /// Determines if inner value is int
        /// </summary>
        /// <returns><see cref="bool">True if value is int</see></returns>
        public bool IsInteger() => this._innerValue is int;

        /// <summary>
        /// Determines if inner value is bool
        /// </summary>
        /// <returns><see cref="bool">True if value is bool</see></returns>
        public bool IsBoolean() => this._innerValue is bool;

        /// <summary>
        /// Determines if inner value is double
        /// </summary>
        /// <returns><see cref="bool">True if value is double</see></returns>
        public bool IsDouble() => this._innerValue is double;

        /// <summary>
        /// Determines if inner value is string
        /// </summary>
        /// <returns><see cref="bool">True if value is string</see></returns>
        public bool IsString() => this._innerValue is string;

        /// <summary>
        /// Determines if inner value is <see cref="Structure">Structure</see>
        /// </summary>
        /// <returns><see cref="bool">True if value is <see cref="Structure">Structure</see></see></returns>
        public bool IsStructure() => this._innerValue is Structure;

        /// <summary>
        /// Determines if inner value is list
        /// </summary>
        /// <returns><see cref="bool">True if value is list</see></returns>
        public bool IsList() => this._innerValue is IList;

        /// <summary>
        /// Determines if inner value is DateTime
        /// </summary>
        /// <returns><see cref="bool">True if value is DateTime</see></returns>
        public bool IsDateTime() => this._innerValue is DateTime;

        /// <summary>
        /// Returns the underlying int value
        /// Value will be null if it isn't a integer
        /// </summary>
        /// <returns>Value as int</returns>
        public int? AsInteger() => this.IsInteger() ? (int?)this._innerValue : null;

        /// <summary>
        /// Returns the underlying bool value
        /// Value will be null if it isn't a bool
        /// </summary>
        /// <returns>Value as bool</returns>
        public bool? AsBoolean() => this.IsBoolean() ? (bool?)this._innerValue : null;

        /// <summary>
        /// Returns the underlying double value
        /// Value will be null if it isn't a double
        /// </summary>
        /// <returns>Value as int</returns>
        public double? AsDouble() => this.IsDouble() ? (double?)this._innerValue : null;

        /// <summary>
        /// Returns the underlying string value
        /// Value will be null if it isn't a string
        /// </summary>
        /// <returns>Value as string</returns>
        public string AsString() => this.IsString() ? (string)this._innerValue : null;

        /// <summary>
        /// Returns the underlying Structure value
        /// Value will be null if it isn't a Structure
        /// </summary>
        /// <returns>Value as Structure</returns>
        public Structure AsStructure() => this.IsStructure() ? (Structure)this._innerValue : null;

        /// <summary>
        /// Returns the underlying List value
        /// Value will be null if it isn't a List
        /// </summary>
        /// <returns>Value as List</returns>
        public IList<Value> AsList() => this.IsList() ? (IList<Value>)this._innerValue : null;

        /// <summary>
        /// Returns the underlying DateTime value
        /// Value will be null if it isn't a DateTime
        /// </summary>
        /// <returns>Value as DateTime</returns>
        public DateTime? AsDateTime() => this.IsDateTime() ? (DateTime?)this._innerValue : null;
    }
}
