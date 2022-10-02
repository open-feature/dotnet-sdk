﻿using OpenFeatureSDK;
using OpenFeatureSDK.Model;

namespace OpenFeature.ServiceCollection.Feature;

internal class FeatureCollection<T>:IFeatureCollection<T> where T : new()
{
    private readonly FeatureClient _featureClient;

    public FeatureCollection(FeatureClient featureClient)
    {
        this._featureClient = featureClient;
    }

    public async Task<T> GetValueAsync()
    {
        var featuresDto = new T();

        foreach (var propertyInfo in typeof(T).GetProperties())
        {
            if(propertyInfo.PropertyType==typeof(string))
                propertyInfo.SetValue(featuresDto,await this._featureClient.GetStringValue(propertyInfo.Name,(string?)propertyInfo.GetValue(featuresDto)));

            else if(propertyInfo.PropertyType==typeof(int))
                propertyInfo.SetValue(featuresDto,await this._featureClient.GetIntegerValue(propertyInfo.Name,(int)(propertyInfo.GetValue(featuresDto) ?? 0)));

            else if(propertyInfo.PropertyType==typeof(double))
                propertyInfo.SetValue(featuresDto,await this._featureClient.GetDoubleDetails(propertyInfo.Name,(double)(propertyInfo.GetValue(featuresDto) ?? 0)));

            else if(propertyInfo.PropertyType==typeof(bool))
                propertyInfo.SetValue(featuresDto,await this._featureClient.GetBooleanValue(propertyInfo.Name,(bool)(propertyInfo.GetValue(featuresDto) ?? false)));

            else if(propertyInfo.PropertyType==typeof(Value))
                propertyInfo.SetValue(featuresDto,await this._featureClient.GetObjectValue(propertyInfo.Name,new Value()));
        }

        return featuresDto;
    }
    public async Task<FlagEvaluationDetails<TPropertyType>?> GetDetailsAsync<TPropertyType>(string name)
    {
        if(typeof(TPropertyType)==typeof(string))
            return (await this._featureClient.GetStringDetails(name,String.Empty)) as FlagEvaluationDetails<TPropertyType>;

        if(typeof(TPropertyType)==typeof(int))
            return (await this._featureClient.GetIntegerDetails(name,0)) as FlagEvaluationDetails<TPropertyType>;

        if(typeof(TPropertyType)==typeof(double))
            return (await this._featureClient.GetDoubleDetails(name,0)) as FlagEvaluationDetails<TPropertyType>;

        if(typeof(TPropertyType)==typeof(bool))
            return (await this._featureClient.GetBooleanDetails(name,false)) as FlagEvaluationDetails<TPropertyType>;

        if(typeof(TPropertyType)==typeof(Value))
            return (await this._featureClient.GetObjectValue(name,new Value())) as FlagEvaluationDetails<TPropertyType>;

        if(typeof(TPropertyType)==typeof(string))
            return (await this._featureClient.GetStringDetails(name,String.Empty)) as FlagEvaluationDetails<TPropertyType>;


        throw new NotSupportedException($" Type {typeof(TPropertyType)} is not supported");
    }
}
