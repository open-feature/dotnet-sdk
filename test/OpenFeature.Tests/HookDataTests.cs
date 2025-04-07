using System;
using System.Collections.Generic;
using OpenFeature.Model;
using Xunit;

namespace OpenFeature.Tests;

public class HookDataTests
{
    private readonly HookData _commonHookData = new();

    public HookDataTests()
    {
        this._commonHookData.Set("bool", true);
        this._commonHookData.Set("string", "string");
        this._commonHookData.Set("int", 1);
        this._commonHookData.Set("double", 1.2);
        this._commonHookData.Set("float", 1.2f);
    }

    [Fact]
    public void HookData_Can_Set_And_Get_Data()
    {
        var hookData = new HookData();
        hookData.Set("bool", true);
        hookData.Set("string", "string");
        hookData.Set("int", 1);
        hookData.Set("double", 1.2);
        hookData.Set("float", 1.2f);
        var structure = Structure.Builder().Build();
        hookData.Set("structure", structure);

        Assert.True((bool)hookData.Get("bool"));
        Assert.Equal("string", hookData.Get("string"));
        Assert.Equal(1, hookData.Get("int"));
        Assert.Equal(1.2, hookData.Get("double"));
        Assert.Equal(1.2f, hookData.Get("float"));
        Assert.Same(structure, hookData.Get("structure"));
    }

    [Fact]
    public void HookData_Can_Chain_Set() {
        var structure = Structure.Builder().Build();

        var hookData = new HookData();
        hookData.Set("bool", true)
        .Set("string", "string")
        .Set("int", 1)
        .Set("double", 1.2)
        .Set("float", 1.2f)
        .Set("structure", structure);

        Assert.True((bool)hookData.Get("bool"));
        Assert.Equal("string", hookData.Get("string"));
        Assert.Equal(1, hookData.Get("int"));
        Assert.Equal(1.2, hookData.Get("double"));
        Assert.Equal(1.2f, hookData.Get("float"));
        Assert.Same(structure, hookData.Get("structure"));
    }

    [Fact]
    public void HookData_Can_Set_And_Get_Data_Using_Indexer()
    {
        var hookData = new HookData();
        hookData["bool"] = true;
        hookData["string"] = "string";
        hookData["int"] = 1;
        hookData["double"] = 1.2;
        hookData["float"] = 1.2f;
        var structure = Structure.Builder().Build();
        hookData["structure"] = structure;

        Assert.True((bool)hookData["bool"]);
        Assert.Equal("string", hookData["string"]);
        Assert.Equal(1, hookData["int"]);
        Assert.Equal(1.2, hookData["double"]);
        Assert.Equal(1.2f, hookData["float"]);
        Assert.Same(structure, hookData["structure"]);
    }

    [Fact]
    public void HookData_Can_Be_Enumerated()
    {
        var asList = new List<KeyValuePair<string, object>>();
        foreach (var kvp in this._commonHookData)
        {
            asList.Add(kvp);
        }

        asList.Sort((a, b) =>
            string.Compare(a.Key, b.Key, StringComparison.Ordinal));

        Assert.Equal([
            new KeyValuePair<string, object>("bool", true),
            new KeyValuePair<string, object>("double", 1.2),
            new KeyValuePair<string, object>("float", 1.2f),
            new KeyValuePair<string, object>("int", 1),
            new KeyValuePair<string, object>("string", "string")
        ], asList);
    }

    [Fact]
    public void HookData_Has_Count()
    {
        Assert.Equal(5, this._commonHookData.Count);
    }

    [Fact]
    public void HookData_Has_Keys()
    {
        Assert.Equal(5, this._commonHookData.Keys.Count);
        Assert.Contains("bool", this._commonHookData.Keys);
        Assert.Contains("double", this._commonHookData.Keys);
        Assert.Contains("float", this._commonHookData.Keys);
        Assert.Contains("int", this._commonHookData.Keys);
        Assert.Contains("string", this._commonHookData.Keys);
    }

    [Fact]
    public void HookData_Has_Values()
    {
        Assert.Equal(5, this._commonHookData.Values.Count);
        Assert.Contains(true, this._commonHookData.Values);
        Assert.Contains(1, this._commonHookData.Values);
        Assert.Contains(1.2f, this._commonHookData.Values);
        Assert.Contains(1.2, this._commonHookData.Values);
        Assert.Contains("string", this._commonHookData.Values);
    }

    [Fact]
    public void HookData_Can_Be_Converted_To_Dictionary()
    {
        var asDictionary = this._commonHookData.AsDictionary();
        Assert.Equal(5, asDictionary.Count);
        Assert.Equal(true, asDictionary["bool"]);
        Assert.Equal(1.2, asDictionary["double"]);
        Assert.Equal(1.2f, asDictionary["float"]);
        Assert.Equal(1, asDictionary["int"]);
        Assert.Equal("string", asDictionary["string"]);
    }

    [Fact]
    public void HookData_Get_Should_Throw_When_Key_Not_Found()
    {
        var hookData = new HookData();

        Assert.Throws<KeyNotFoundException>(() => hookData.Get("nonexistent"));
    }

    [Fact]
    public void HookData_Indexer_Should_Throw_When_Key_Not_Found()
    {
        var hookData = new HookData();

        Assert.Throws<KeyNotFoundException>(() => _ = hookData["nonexistent"]);
    }
}
