using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace OpenFeature.Model;

/// <summary>
///  Values serve as a return type for provider objects. Providers may deal in JSON, protobuf, XML or some other data-interchange format.
///  This intermediate representation provides a good medium of exchange.
/// </summary>
[JsonConverter(typeof(ValueJsonConverter))]
public sealed class Value : IEquatable<Value>
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

    /// <summary>
    /// Determines whether the specified <see cref="Value"/> is equal to the current <see cref="Value"/>.
    /// </summary>
    /// <param name="other">The <see cref="Value"/> to compare with the current <see cref="Value"/>.</param>
    /// <returns>true if the specified <see cref="Value"/> is equal to the current <see cref="Value"/>; otherwise, false.</returns>
    public bool Equals(Value? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        // Both are null
        if (this.IsNull && other.IsNull) return true;

        // One is null, the other is not
        if (this.IsNull != other.IsNull) return false;

        // Different types
        if (this.GetValueType() != other.GetValueType()) return false;

        // Compare based on type
        return this.GetValueType() switch
        {
            ValueType.Boolean => this.AsBoolean == other.AsBoolean,
            ValueType.Number => this.AsDouble == other.AsDouble,
            ValueType.String => this.AsString == other.AsString,
            ValueType.DateTime => this.AsDateTime == other.AsDateTime,
            ValueType.Structure => this.StructureEquals(other),
            ValueType.List => this.ListEquals(other),
            _ => false
        };
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current <see cref="Value"/>.
    /// </summary>
    /// <param name="obj">The object to compare with the current <see cref="Value"/>.</param>
    /// <returns>true if the specified object is equal to the current <see cref="Value"/>; otherwise, false.</returns>
    public override bool Equals(object? obj) => this.Equals(obj as Value);

    /// <summary>
    /// Returns the hash code for this <see cref="Value"/>.
    /// </summary>
    /// <returns>A hash code for the current <see cref="Value"/>.</returns>
    public override int GetHashCode()
    {
        if (this.IsNull) return 0;

        return this.GetValueType() switch
        {
            ValueType.Boolean => this.AsBoolean!.GetHashCode(),
            ValueType.Number => this.AsDouble!.GetHashCode(),
            ValueType.String => this.AsString!.GetHashCode(),
            ValueType.DateTime => this.AsDateTime!.GetHashCode(),
            ValueType.Structure => this.GetStructureHashCode(),
            ValueType.List => this.GetListHashCode(),
            _ => 0
        };
    }

    /// <summary>
    /// Determines whether two <see cref="Value"/> instances are equal.
    /// </summary>
    /// <param name="left">The first <see cref="Value"/> to compare.</param>
    /// <param name="right">The second <see cref="Value"/> to compare.</param>
    /// <returns>true if the values are equal; otherwise, false.</returns>
    public static bool operator ==(Value? left, Value? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two <see cref="Value"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first <see cref="Value"/> to compare.</param>
    /// <param name="right">The second <see cref="Value"/> to compare.</param>
    /// <returns>true if the values are not equal; otherwise, false.</returns>
    public static bool operator !=(Value? left, Value? right) => !(left == right);

    /// <summary>
    /// Gets the type of the current value.
    /// </summary>
    /// <returns>The <see cref="ValueType"/> of the current value.</returns>
    private ValueType GetValueType()
    {
        if (this.IsNull) return ValueType.Null;
        if (this.IsBoolean) return ValueType.Boolean;
        if (this.IsNumber) return ValueType.Number;
        if (this.IsString) return ValueType.String;
        if (this.IsDateTime) return ValueType.DateTime;
        if (this.IsStructure) return ValueType.Structure;
        if (this.IsList) return ValueType.List;
        return ValueType.Unknown;
    }

    /// <summary>
    /// Compares two Structure values for equality.
    /// </summary>
    /// <param name="other">The other <see cref="Value"/> to compare.</param>
    /// <returns>true if the structures are equal; otherwise, false.</returns>
    private bool StructureEquals(Value other)
    {
        var thisStructure = this.AsStructure!;
        var otherStructure = other.AsStructure!;

        if (thisStructure.Count != otherStructure.Count) return false;

        foreach (var kvp in thisStructure)
        {
            if (!otherStructure.TryGetValue(kvp.Key, out var otherValue) || !kvp.Value.Equals(otherValue))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Compares two List values for equality.
    /// </summary>
    /// <param name="other">The other <see cref="Value"/> to compare.</param>
    /// <returns>true if the lists are equal; otherwise, false.</returns>
    private bool ListEquals(Value other)
    {
        var thisList = this.AsList!;
        var otherList = other.AsList!;

        if (thisList.Count != otherList.Count) return false;

        for (int i = 0; i < thisList.Count; i++)
        {
            if (!thisList[i].Equals(otherList[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Gets the hash code for a Structure value.
    /// </summary>
    /// <returns>The hash code of the structure.</returns>
    private int GetStructureHashCode()
    {
        var structure = this.AsStructure!;
        var hash = new HashCode();

        foreach (var kvp in structure)
        {
            hash.Add(kvp.Key);
            hash.Add(kvp.Value);
        }

        return hash.ToHashCode();
    }

    /// <summary>
    /// Gets the hash code for a List value.
    /// </summary>
    /// <returns>The hash code of the list.</returns>
    private int GetListHashCode()
    {
        var list = this.AsList!;
        var hash = new HashCode();

        foreach (var item in list)
        {
            hash.Add(item);
        }

        return hash.ToHashCode();
    }

    /// <summary>
    /// Represents the different types that a <see cref="Value"/> can contain.
    /// </summary>
    private enum ValueType
    {
        Null,
        Boolean,
        Number,
        String,
        DateTime,
        Structure,
        List,
        Unknown
    }
}
