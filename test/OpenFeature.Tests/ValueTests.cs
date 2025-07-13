using OpenFeature.Model;

namespace OpenFeature.Tests;

public class ValueTests
{
    class Foo
    {
    }

    [Fact]
    public void No_Arg_Should_Contain_Null()
    {
        Value value = new Value();
        Assert.True(value.IsNull);
    }

    [Fact]
    public void Object_Arg_Should_Contain_Object()
    {
        // int is a special case, see Int_Object_Arg_Should_Contain_Object()
        IList<Object> list = new List<Object>()
        {
            true,
            "val",
            .5,
            Structure.Empty,
            new List<Value>(),
            DateTime.Now
        };

        int i = 0;
        foreach (Object l in list)
        {
            Value value = new Value(l);
            Assert.Equal(list[i], value.AsObject);
            i++;
        }
    }

    [Fact]
    public void Int_Object_Arg_Should_Contain_Object()
    {
        try
        {
            int innerValue = 1;
            Value value = new Value(innerValue);
            Assert.True(value.IsNumber);
            Assert.Equal(innerValue, value.AsInteger);
        }
        catch (Exception)
        {
            Assert.Fail("Expected no exception.");
        }
    }

    [Fact]
    public void Invalid_Object_Should_Throw()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            return new Value(new Foo());
        });
    }

    [Fact]
    public void Bool_Arg_Should_Contain_Bool()
    {
        bool innerValue = true;
        Value value = new Value(innerValue);
        Assert.True(value.IsBoolean);
        Assert.Equal(innerValue, value.AsBoolean);
    }

    [Fact]
    public void Numeric_Arg_Should_Return_Double_Or_Int()
    {
        double innerDoubleValue = .75;
        Value doubleValue = new Value(innerDoubleValue);
        Assert.True(doubleValue.IsNumber);
        Assert.Equal(1, doubleValue.AsInteger); // should be rounded
        Assert.Equal(.75, doubleValue.AsDouble);

        int innerIntValue = 100;
        Value intValue = new Value(innerIntValue);
        Assert.True(intValue.IsNumber);
        Assert.Equal(innerIntValue, intValue.AsInteger);
        Assert.Equal(innerIntValue, intValue.AsDouble);
    }

    [Fact]
    public void String_Arg_Should_Contain_String()
    {
        string innerValue = "hi!";
        Value value = new Value(innerValue);
        Assert.True(value.IsString);
        Assert.Equal(innerValue, value.AsString);
    }

    [Fact]
    public void DateTime_Arg_Should_Contain_DateTime()
    {
        DateTime innerValue = new DateTime();
        Value value = new Value(innerValue);
        Assert.True(value.IsDateTime);
        Assert.Equal(innerValue, value.AsDateTime);
    }

    [Fact]
    public void Structure_Arg_Should_Contain_Structure()
    {
        string INNER_KEY = "key";
        string INNER_VALUE = "val";
        Structure innerValue = Structure.Builder().Set(INNER_KEY, INNER_VALUE).Build();
        Value value = new Value(innerValue);
        Assert.True(value.IsStructure);
        Assert.Equal(INNER_VALUE, value.AsStructure?.GetValue(INNER_KEY).AsString);
    }

    [Fact]
    public void List_Arg_Should_Contain_List()
    {
        string ITEM_VALUE = "val";
        IList<Value> innerValue = new List<Value>() { new Value(ITEM_VALUE) };
        Value value = new Value(innerValue);
        Assert.True(value.IsList);
        Assert.Equal(ITEM_VALUE, value.AsList?[0].AsString);
    }

    [Fact]
    public void Constructor_WhenCalledWithAnotherValue_CopiesInnerValue()
    {
        // Arrange
        var originalValue = new Value("testValue");

        // Act
        var copiedValue = new Value(originalValue);

        // Assert
        Assert.Equal(originalValue.AsObject, copiedValue.AsObject);
    }

    [Fact]
    public void AsInteger_WhenCalledWithNonIntegerInnerValue_ReturnsNull()
    {
        // Arrange
        var value = new Value("test");

        // Act
        var actualValue = value.AsInteger;

        // Assert
        Assert.Null(actualValue);
    }

    [Fact]
    public void AsBoolean_WhenCalledWithNonBooleanInnerValue_ReturnsNull()
    {
        // Arrange
        var value = new Value("test");

        // Act
        var actualValue = value.AsBoolean;

        // Assert
        Assert.Null(actualValue);
    }

    [Fact]
    public void AsDouble_WhenCalledWithNonDoubleInnerValue_ReturnsNull()
    {
        // Arrange
        var value = new Value("test");

        // Act
        var actualValue = value.AsDouble;

        // Assert
        Assert.Null(actualValue);
    }

    [Fact]
    public void AsString_WhenCalledWithNonStringInnerValue_ReturnsNull()
    {
        // Arrange
        var value = new Value(123);

        // Act
        var actualValue = value.AsString;

        // Assert
        Assert.Null(actualValue);
    }

    [Fact]
    public void AsStructure_WhenCalledWithNonStructureInnerValue_ReturnsNull()
    {
        // Arrange
        var value = new Value("test");

        // Act
        var actualValue = value.AsStructure;

        // Assert
        Assert.Null(actualValue);
    }

    [Fact]
    public void AsList_WhenCalledWithNonListInnerValue_ReturnsNull()
    {
        // Arrange
        var value = new Value("test");

        // Act
        var actualValue = value.AsList;

        // Assert
        Assert.Null(actualValue);
    }

    [Fact]
    public void AsDateTime_WhenCalledWithNonDateTimeInnerValue_ReturnsNull()
    {
        // Arrange
        var value = new Value("test");

        // Act
        var actualValue = value.AsDateTime;

        // Assert
        Assert.Null(actualValue);
    }

    #region Equality Tests

    [Fact]
    public void Equals_WithNullValue_ReturnsFalse()
    {
        // Arrange
        var value = new Value("test");

        // Act & Assert
        Assert.False(value.Equals((Value?)null));
    }

    [Fact]
    public void Equals_WithSameReference_ReturnsTrue()
    {
        // Arrange
        var value = new Value("test");

        // Act & Assert
        Assert.True(value.Equals(value));
    }

    [Fact]
    public void Equals_WithBothNull_ReturnsTrue()
    {
        // Arrange
        var value1 = new Value();
        var value2 = new Value();

        // Act & Assert
        Assert.True(value1.Equals(value2));
    }

    [Fact]
    public void Equals_WithOneNullOneNotNull_ReturnsFalse()
    {
        // Arrange
        var nullValue = new Value();
        var stringValue = new Value("test");

        // Act & Assert
        Assert.False(nullValue.Equals(stringValue));
        Assert.False(stringValue.Equals(nullValue));
    }

    [Fact]
    public void Equals_WithDifferentTypes_ReturnsFalse()
    {
        // Arrange
        var stringValue = new Value("test");
        var intValue = new Value(42);
        var boolValue = new Value(true);

        // Act & Assert
        Assert.False(stringValue.Equals(intValue));
        Assert.False(stringValue.Equals(boolValue));
        Assert.False(intValue.Equals(boolValue));
    }

    [Fact]
    public void Equals_WithSameStringValues_ReturnsTrue()
    {
        // Arrange
        var value1 = new Value("test");
        var value2 = new Value("test");

        // Act & Assert
        Assert.True(value1.Equals(value2));
    }

    [Fact]
    public void Equals_WithDifferentStringValues_ReturnsFalse()
    {
        // Arrange
        var value1 = new Value("test1");
        var value2 = new Value("test2");

        // Act & Assert
        Assert.False(value1.Equals(value2));
    }

    [Fact]
    public void Equals_WithSameBooleanValues_ReturnsTrue()
    {
        // Arrange
        var value1 = new Value(true);
        var value2 = new Value(true);
        var value3 = new Value(false);
        var value4 = new Value(false);

        // Act & Assert
        Assert.True(value1.Equals(value2));
        Assert.True(value3.Equals(value4));
    }

    [Fact]
    public void Equals_WithDifferentBooleanValues_ReturnsFalse()
    {
        // Arrange
        var value1 = new Value(true);
        var value2 = new Value(false);

        // Act & Assert
        Assert.False(value1.Equals(value2));
    }

    [Fact]
    public void Equals_WithSameNumberValues_ReturnsTrue()
    {
        // Arrange
        var value1 = new Value(42.5);
        var value2 = new Value(42.5);
        var intValue1 = new Value(42);
        var intValue2 = new Value(42);

        // Act & Assert
        Assert.True(value1.Equals(value2));
        Assert.True(intValue1.Equals(intValue2));
    }

    [Fact]
    public void Equals_WithDifferentNumberValues_ReturnsFalse()
    {
        // Arrange
        var value1 = new Value(42.5);
        var value2 = new Value(42.6);

        // Act & Assert
        Assert.False(value1.Equals(value2));
    }

    [Fact]
    public void Equals_WithSameDateTimeValues_ReturnsTrue()
    {
        // Arrange
        var dateTime = DateTime.Now;
        var value1 = new Value(dateTime);
        var value2 = new Value(dateTime);

        // Act & Assert
        Assert.True(value1.Equals(value2));
    }

    [Fact]
    public void Equals_WithDifferentDateTimeValues_ReturnsFalse()
    {
        // Arrange
        var value1 = new Value(DateTime.Now);
        var value2 = new Value(DateTime.Now.AddDays(1));

        // Act & Assert
        Assert.False(value1.Equals(value2));
    }

    [Fact]
    public void Equals_WithSameStructureValues_ReturnsTrue()
    {
        // Arrange
        var structure1 = Structure.Builder()
            .Set("key1", new Value("value1"))
            .Set("key2", new Value(42))
            .Build();
        var structure2 = Structure.Builder()
            .Set("key1", new Value("value1"))
            .Set("key2", new Value(42))
            .Build();
        var value1 = new Value(structure1);
        var value2 = new Value(structure2);

        // Act & Assert
        Assert.True(value1.Equals(value2));
    }

    [Fact]
    public void Equals_WithDifferentStructureValues_ReturnsFalse()
    {
        // Arrange
        var structure1 = Structure.Builder()
            .Set("key1", new Value("value1"))
            .Build();
        var structure2 = Structure.Builder()
            .Set("key1", new Value("value2"))
            .Build();
        var value1 = new Value(structure1);
        var value2 = new Value(structure2);

        // Act & Assert
        Assert.False(value1.Equals(value2));
    }

    [Fact]
    public void Equals_WithStructuresDifferentKeyCounts_ReturnsFalse()
    {
        // Arrange
        var structure1 = Structure.Builder()
            .Set("key1", new Value("value1"))
            .Build();
        var structure2 = Structure.Builder()
            .Set("key1", new Value("value1"))
            .Set("key2", new Value("value2"))
            .Build();
        var value1 = new Value(structure1);
        var value2 = new Value(structure2);

        // Act & Assert
        Assert.False(value1.Equals(value2));
    }

    [Fact]
    public void Equals_WithSameListValues_ReturnsTrue()
    {
        // Arrange
        var list1 = new List<Value> { new Value("test"), new Value(42), new Value(true) };
        var list2 = new List<Value> { new Value("test"), new Value(42), new Value(true) };
        var value1 = new Value(list1);
        var value2 = new Value(list2);

        // Act & Assert
        Assert.True(value1.Equals(value2));
    }

    [Fact]
    public void Equals_WithDifferentListValues_ReturnsFalse()
    {
        // Arrange
        var list1 = new List<Value> { new Value("test1"), new Value(42) };
        var list2 = new List<Value> { new Value("test2"), new Value(42) };
        var value1 = new Value(list1);
        var value2 = new Value(list2);

        // Act & Assert
        Assert.False(value1.Equals(value2));
    }

    [Fact]
    public void Equals_WithListsDifferentLengths_ReturnsFalse()
    {
        // Arrange
        var list1 = new List<Value> { new Value("test") };
        var list2 = new List<Value> { new Value("test"), new Value(42) };
        var value1 = new Value(list1);
        var value2 = new Value(list2);

        // Act & Assert
        Assert.False(value1.Equals(value2));
    }

    [Fact]
    public void Equals_WithObject_CallsTypedEquals()
    {
        // Arrange
        var value1 = new Value("test");
        var value2 = new Value("test");
        object obj = value2;

        // Act & Assert
        Assert.True(value1.Equals(obj));
    }

    [Fact]
    public void Equals_WithNonValueObject_ReturnsFalse()
    {
        // Arrange
        var value = new Value("test");
        object obj = "test";

        // Act & Assert
        Assert.False(value.Equals(obj));
    }

    #endregion

    #region Operator Tests

    [Fact]
    public void OperatorEquals_WithBothNull_ReturnsTrue()
    {
        // Arrange
        Value? value1 = null;
        Value? value2 = null;

        // Act & Assert
        Assert.True(value1 == value2);
    }

    [Fact]
    public void OperatorEquals_WithOneNull_ReturnsFalse()
    {
        // Arrange
        Value? value1 = null;
        Value value2 = new Value("test");

        // Act & Assert
        Assert.False(value1 == value2);
        Assert.False(value2 == value1);
    }

    [Fact]
    public void OperatorEquals_WithEqualValues_ReturnsTrue()
    {
        // Arrange
        var value1 = new Value("test");
        var value2 = new Value("test");

        // Act & Assert
        Assert.True(value1 == value2);
    }

    [Fact]
    public void OperatorEquals_WithDifferentValues_ReturnsFalse()
    {
        // Arrange
        var value1 = new Value("test1");
        var value2 = new Value("test2");

        // Act & Assert
        Assert.False(value1 == value2);
    }

    [Fact]
    public void OperatorNotEquals_WithEqualValues_ReturnsFalse()
    {
        // Arrange
        var value1 = new Value("test");
        var value2 = new Value("test");

        // Act & Assert
        Assert.False(value1 != value2);
    }

    [Fact]
    public void OperatorNotEquals_WithDifferentValues_ReturnsTrue()
    {
        // Arrange
        var value1 = new Value("test1");
        var value2 = new Value("test2");

        // Act & Assert
        Assert.True(value1 != value2);
    }

    #endregion

    #region GetHashCode Tests

    [Fact]
    public void GetHashCode_WithNullValue_ReturnsZero()
    {
        // Arrange
        var value = new Value();

        // Act
        var hashCode = value.GetHashCode();

        // Assert
        Assert.Equal(0, hashCode);
    }

    [Fact]
    public void GetHashCode_WithEqualValues_ReturnsSameHashCode()
    {
        // Arrange
        var value1 = new Value("test");
        var value2 = new Value("test");

        // Act
        var hashCode1 = value1.GetHashCode();
        var hashCode2 = value2.GetHashCode();

        // Assert
        Assert.Equal(hashCode1, hashCode2);
    }

    [Fact]
    public void GetHashCode_WithBooleanValues_ReturnsConsistentHashCode()
    {
        // Arrange
        var value1 = new Value(true);
        var value2 = new Value(true);
        var value3 = new Value(false);

        // Act
        var hashCode1 = value1.GetHashCode();
        var hashCode2 = value2.GetHashCode();
        var hashCode3 = value3.GetHashCode();

        // Assert
        Assert.Equal(hashCode1, hashCode2);
        Assert.NotEqual(hashCode1, hashCode3);
    }

    [Fact]
    public void GetHashCode_WithNumberValues_ReturnsConsistentHashCode()
    {
        // Arrange
        var value1 = new Value(42.5);
        var value2 = new Value(42.5);
        var value3 = new Value(42.6);

        // Act
        var hashCode1 = value1.GetHashCode();
        var hashCode2 = value2.GetHashCode();
        var hashCode3 = value3.GetHashCode();

        // Assert
        Assert.Equal(hashCode1, hashCode2);
        Assert.NotEqual(hashCode1, hashCode3);
    }

    [Fact]
    public void GetHashCode_WithStringValues_ReturnsConsistentHashCode()
    {
        // Arrange
        var value1 = new Value("test");
        var value2 = new Value("test");
        var value3 = new Value("different");

        // Act
        var hashCode1 = value1.GetHashCode();
        var hashCode2 = value2.GetHashCode();
        var hashCode3 = value3.GetHashCode();

        // Assert
        Assert.Equal(hashCode1, hashCode2);
        Assert.NotEqual(hashCode1, hashCode3);
    }

    [Fact]
    public void GetHashCode_WithDateTimeValues_ReturnsConsistentHashCode()
    {
        // Arrange
        var dateTime = DateTime.Now;
        var value1 = new Value(dateTime);
        var value2 = new Value(dateTime);
        var value3 = new Value(dateTime.AddDays(1));

        // Act
        var hashCode1 = value1.GetHashCode();
        var hashCode2 = value2.GetHashCode();
        var hashCode3 = value3.GetHashCode();

        // Assert
        Assert.Equal(hashCode1, hashCode2);
        Assert.NotEqual(hashCode1, hashCode3);
    }

    [Fact]
    public void GetHashCode_WithStructureValues_ReturnsConsistentHashCode()
    {
        // Arrange
        var structure1 = Structure.Builder()
            .Set("key1", new Value("value1"))
            .Set("key2", new Value(42))
            .Build();
        var structure2 = Structure.Builder()
            .Set("key1", new Value("value1"))
            .Set("key2", new Value(42))
            .Build();
        var value1 = new Value(structure1);
        var value2 = new Value(structure2);

        // Act
        var hashCode1 = value1.GetHashCode();
        var hashCode2 = value2.GetHashCode();

        // Assert
        Assert.Equal(hashCode1, hashCode2);
    }

    [Fact]
    public void GetHashCode_WithListValues_ReturnsConsistentHashCode()
    {
        // Arrange
        var list1 = new List<Value> { new Value("test"), new Value(42) };
        var list2 = new List<Value> { new Value("test"), new Value(42) };
        var value1 = new Value(list1);
        var value2 = new Value(list2);

        // Act
        var hashCode1 = value1.GetHashCode();
        var hashCode2 = value2.GetHashCode();

        // Assert
        Assert.Equal(hashCode1, hashCode2);
    }

    #endregion

    #region Complex Nested Tests

    [Fact]
    public void Equals_WithNestedStructuresAndLists_ReturnsTrue()
    {
        // Arrange
        var innerList1 = new List<Value> { new Value("nested"), new Value(123) };
        var innerList2 = new List<Value> { new Value("nested"), new Value(123) };

        var innerStructure1 = Structure.Builder()
            .Set("nested_key", new Value("nested_value"))
            .Set("nested_list", new Value(innerList1))
            .Build();
        var innerStructure2 = Structure.Builder()
            .Set("nested_key", new Value("nested_value"))
            .Set("nested_list", new Value(innerList2))
            .Build();

        var outerStructure1 = Structure.Builder()
            .Set("outer_key", new Value("outer_value"))
            .Set("inner", new Value(innerStructure1))
            .Build();
        var outerStructure2 = Structure.Builder()
            .Set("outer_key", new Value("outer_value"))
            .Set("inner", new Value(innerStructure2))
            .Build();

        var value1 = new Value(outerStructure1);
        var value2 = new Value(outerStructure2);

        // Act & Assert
        Assert.True(value1.Equals(value2));
        Assert.Equal(value1.GetHashCode(), value2.GetHashCode());
    }

    [Fact]
    public void Equals_WithDeeplyNestedDifferences_ReturnsFalse()
    {
        // Arrange
        var innerList1 = new List<Value> { new Value("nested"), new Value(123) };
        var innerList2 = new List<Value> { new Value("nested"), new Value(124) }; // Different value

        var innerStructure1 = Structure.Builder()
            .Set("nested_key", new Value("nested_value"))
            .Set("nested_list", new Value(innerList1))
            .Build();
        var innerStructure2 = Structure.Builder()
            .Set("nested_key", new Value("nested_value"))
            .Set("nested_list", new Value(innerList2))
            .Build();

        var outerStructure1 = Structure.Builder()
            .Set("outer_key", new Value("outer_value"))
            .Set("inner", new Value(innerStructure1))
            .Build();
        var outerStructure2 = Structure.Builder()
            .Set("outer_key", new Value("outer_value"))
            .Set("inner", new Value(innerStructure2))
            .Build();

        var value1 = new Value(outerStructure1);
        var value2 = new Value(outerStructure2);

        // Act & Assert
        Assert.False(value1.Equals(value2));
    }

    #endregion
}
