using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using OpenFeature.Model;
using Xunit;

namespace OpenFeature.Tests
{
    public class StructureTests
    {
        [Fact]
        public void No_Arg_Should_Contain_Empty_Attributes()
        {
            Structure structure = Structure.Empty;
            Assert.Equal(0, structure.Count);
            Assert.Empty(structure.AsDictionary());
        }

        [Fact]
        public void Dictionary_Arg_Should_Contain_New_Dictionary()
        {
            string KEY = "key";
            IDictionary<string, Value> dictionary = new Dictionary<string, Value>() { { KEY, new Value(KEY) } };
            Structure structure = new Structure(dictionary);
            Assert.Equal(KEY, structure.AsDictionary()[KEY].AsString);
            Assert.NotSame(structure.AsDictionary(), dictionary); // should be a copy
        }

        [Fact]
        public void Add_And_Get_Add_And_Return_Values()
        {
            String BOOL_KEY = "bool";
            String STRING_KEY = "string";
            String INT_KEY = "int";
            String DOUBLE_KEY = "double";
            String DATE_KEY = "date";
            String STRUCT_KEY = "struct";
            String LIST_KEY = "list";
            String VALUE_KEY = "value";

            bool BOOL_VAL = true;
            String STRING_VAL = "val";
            int INT_VAL = 13;
            double DOUBLE_VAL = .5;
            DateTime DATE_VAL = DateTime.Now;
            Structure STRUCT_VAL = Structure.Empty;
            IList<Value> LIST_VAL = new List<Value>();
            Value VALUE_VAL = new Value();

            var structureBuilder = Structure.Builder();
            structureBuilder.Set(BOOL_KEY, BOOL_VAL);
            structureBuilder.Set(STRING_KEY, STRING_VAL);
            structureBuilder.Set(INT_KEY, INT_VAL);
            structureBuilder.Set(DOUBLE_KEY, DOUBLE_VAL);
            structureBuilder.Set(DATE_KEY, DATE_VAL);
            structureBuilder.Set(STRUCT_KEY, STRUCT_VAL);
            structureBuilder.Set(LIST_KEY, ImmutableList.CreateRange(LIST_VAL));
            structureBuilder.Set(VALUE_KEY, VALUE_VAL);
            var structure = structureBuilder.Build();

            Assert.Equal(BOOL_VAL, structure.GetValue(BOOL_KEY).AsBoolean);
            Assert.Equal(STRING_VAL, structure.GetValue(STRING_KEY).AsString);
            Assert.Equal(INT_VAL, structure.GetValue(INT_KEY).AsInteger);
            Assert.Equal(DOUBLE_VAL, structure.GetValue(DOUBLE_KEY).AsDouble);
            Assert.Equal(DATE_VAL, structure.GetValue(DATE_KEY).AsDateTime);
            Assert.Equal(STRUCT_VAL, structure.GetValue(STRUCT_KEY).AsStructure);
            Assert.Equal(LIST_VAL, structure.GetValue(LIST_KEY).AsList);
            Assert.True(structure.GetValue(VALUE_KEY).IsNull);
        }

        [Fact]
        public void TryGetValue_Should_Return_Value()
        {
            String KEY = "key";
            String VAL = "val";

            var structure = Structure.Builder()
                .Set(KEY, VAL).Build();
            Value? value;
            Assert.True(structure.TryGetValue(KEY, out value));
            Assert.Equal(VAL, value?.AsString);
        }

        [Fact]
        public void Values_Should_Return_Values()
        {
            String KEY = "key";
            Value VAL = new Value("val");

            var structure = Structure.Builder()
                .Set(KEY, VAL).Build();
            Assert.Single(structure.Values);
        }

        [Fact]
        public void Keys_Should_Return_Keys()
        {
            String KEY = "key";
            Value VAL = new Value("val");

            var structure = Structure.Builder()
                .Set(KEY, VAL).Build();
            Assert.Single(structure.Keys);
            Assert.Equal(0, structure.Keys.IndexOf(KEY));
        }

        [Fact]
        public void GetEnumerator_Should_Return_Enumerator()
        {
            string KEY = "key";
            string VAL = "val";

            var structure = Structure.Builder()
                .Set(KEY, VAL).Build();
            IEnumerator<KeyValuePair<string, Value>> enumerator = structure.GetEnumerator();
            enumerator.MoveNext();
            Assert.Equal(VAL, enumerator.Current.Value.AsString);
        }
    }
}
