using System;
using System.Collections.Generic;
using OpenFeature.SDK.Model;
using Xunit;

namespace OpenFeature.SDK.Tests
{
    public class ValueTests
    {
        [Fact]
        public void No_Arg_Should_Contain_Null()
        {
            Value value = new Value();
            Assert.True(value.IsNull());
        }

        [Fact]
        public void Bool_Arg_Should_Contain_Bool()
        {
            bool innerValue = true;
            Value value = new Value(innerValue);
            Assert.True(value.IsBoolean());
            Assert.Equal(innerValue, value.AsBoolean());
        }

        [Fact]
        public void Int_Arg_Should_Contain_Int()
        {
            int innerValue = 99;
            Value value = new Value(innerValue);
            Assert.True(value.IsInteger());
            Assert.Equal(innerValue, value.AsInteger());
        }

        [Fact]
        public void Double_Arg_Should_Contain_Double()
        {
            double innerValue = .5;
            Value value = new Value(innerValue);
            Assert.True(value.IsDouble());
            Assert.Equal(innerValue, value.AsDouble());
        }

        [Fact]
        public void String_Arg_Should_Contain_String()
        {
            string innerValue = "hi!";
            Value value = new Value(innerValue);
            Assert.True(value.IsString());
            Assert.Equal(innerValue, value.AsString());
        }


        [Fact]
        public void DateTime_Arg_Should_Contain_DateTime()
        {
            DateTime innerValue = new DateTime();
            Value value = new Value(innerValue);
            Assert.True(value.IsDateTime());
            Assert.Equal(innerValue, value.AsDateTime());
        }

        [Fact]
        public void Structure_Arg_Should_Contain_Structure()
        {
            string INNER_KEY = "key";
            string INNER_VALUE = "val";
            Structure innerValue = new Structure().Add(INNER_KEY, INNER_VALUE);
            Value value = new Value(innerValue);
            Assert.True(value.IsStructure());
            Assert.Equal(INNER_VALUE, value.AsStructure().GetValue(INNER_KEY).AsString());
        }

        [Fact]
        public void List_Arg_Should_Contain_List()
        {
            string ITEM_VALUE = "val";
            IList<Value> innerValue = new List<Value>()
            {
                new Value(ITEM_VALUE)
            };
            Value value = new Value(innerValue);
            Assert.True(value.IsList());
            Assert.Equal(ITEM_VALUE, value.AsList()[0].AsString());
        }
    }
}
