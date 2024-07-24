using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace OpenFeature.Model
{
    /// <summary>
    ///  Values serve as a return type for provider objects. Providers may deal in JSON, protobuf, XML or some other data-interchange format.
    ///  This intermediate representation provides a good medium of exchange.
    /// </summary>
    public sealed class Value
    {
        private readonly object? _innerValue;

        /// <summary>
        /// Creates a Value with the inner value set to null
        /// </summary>
        public Value() => this._innerValue = null;

        /// <summary>
        /// Creates a Value with the inner set to the object
        /// </summary>
        /// <param name="value"><see cref="Object">The object to set as the inner value</see></param>
        public Value(Object value)
        {
            if (value is IList<Value> list)
            {
                value = list.ToImmutableList();
            }
            // integer is a special case, convert those.
            this._innerValue = value is int ? Convert.ToDouble(value) : value;
            if (!(this.IsNull
                || this.IsBoolean
                || this.IsString
                || this.IsNumber
                || this.IsStructure
                || this.IsList
                || this.IsDateTime))
            {
                throw new ArgumentException("Invalid value type: " + value.GetType());
            }
        }


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
        /// Creates a Value by converting value to a double
        /// </summary>
        /// <param name="value"><see cref="int">Int type</see></param>
        public Value(int value) => this._innerValue = Convert.ToDouble(value);

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
        /// <param name="value"><see cref="IImmutableList{T}">List type</see></param>
        public Value(IList<Value> value) => this._innerValue = value.ToImmutableList();

        /// <summary>
        /// Creates a Value with the inner set to DateTime type
        /// </summary>
        /// <param name="value"><see cref="DateTime">DateTime type</see></param>
        public Value(DateTime value) => this._innerValue = value;

        /// <summary>
        /// Determines if inner value is null
        /// </summary>
        /// <returns><see cref="bool">True if value is null</see></returns>
        public bool IsNull => this._innerValue is null;

        /// <summary>
        /// Determines if inner value is bool
        /// </summary>
        /// <returns><see cref="bool">True if value is bool</see></returns>
        public bool IsBoolean => this._innerValue is bool;

        /// <summary>
        /// Determines if inner value is numeric
        /// </summary>
        /// <returns><see cref="bool">True if value is double</see></returns>
        public bool IsNumber => this._innerValue is double;

        /// <summary>
        /// Determines if inner value is string
        /// </summary>
        /// <returns><see cref="bool">True if value is string</see></returns>
        public bool IsString => this._innerValue is string;

        /// <summary>
        /// Determines if inner value is <see cref="Structure">Structure</see>
        /// </summary>
        /// <returns><see cref="bool">True if value is <see cref="Structure">Structure</see></see></returns>
        public bool IsStructure => this._innerValue is Structure;

        /// <summary>
        /// Determines if inner value is list
        /// </summary>
        /// <returns><see cref="bool">True if value is list</see></returns>
        public bool IsList => this._innerValue is IImmutableList<Value>;

        /// <summary>
        /// Determines if inner value is DateTime
        /// </summary>
        /// <returns><see cref="bool">True if value is DateTime</see></returns>
        public bool IsDateTime => this._innerValue is DateTime;

        /// <summary>
        /// Returns the underlying inner value as an object. Returns null if the inner value is null.
        /// </summary>
        /// <returns>Value as object</returns>
        public object? AsObject => this._innerValue;

        /// <summary>
        /// Returns the underlying int value.
        /// Value will be null if it isn't an integer
        /// </summary>
        /// <returns>Value as int</returns>
        public int? AsInteger => this.IsNumber ? Convert.ToInt32((double?)this._innerValue) : null;

        /// <summary>
        /// Returns the underlying bool value.
        /// Value will be null if it isn't a bool
        /// </summary>
        /// <returns>Value as bool</returns>
        public bool? AsBoolean => this.IsBoolean ? (bool?)this._innerValue : null;

        /// <summary>
        /// Returns the underlying double value.
        /// Value will be null if it isn't a double
        /// </summary>
        /// <returns>Value as int</returns>
        public double? AsDouble => this.IsNumber ? (double?)this._innerValue : null;

        /// <summary>
        /// Returns the underlying string value.
        /// Value will be null if it isn't a string
        /// </summary>
        /// <returns>Value as string</returns>
        public string? AsString => this.IsString ? (string?)this._innerValue : null;

        /// <summary>
        /// Returns the underlying Structure value.
        /// Value will be null if it isn't a Structure
        /// </summary>
        /// <returns>Value as Structure</returns>
        public Structure? AsStructure => this.IsStructure ? (Structure?)this._innerValue : null;

        /// <summary>
        /// Returns the underlying List value.
        /// Value will be null if it isn't a List
        /// </summary>
        /// <returns>Value as List</returns>
        public IImmutableList<Value>? AsList => this.IsList ? (IImmutableList<Value>?)this._innerValue : null;

        /// <summary>
        /// Returns the underlying DateTime value.
        /// Value will be null if it isn't a DateTime
        /// </summary>
        /// <returns>Value as DateTime</returns>
        public DateTime? AsDateTime => this.IsDateTime ? (DateTime?)this._innerValue : null;
    }
}
