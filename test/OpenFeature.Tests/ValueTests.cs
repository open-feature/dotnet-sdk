using System;
using System.Collections.Generic;
using OpenFeature.Model;
using Xunit;

namespace OpenFeature.Tests
{
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
    }
}
