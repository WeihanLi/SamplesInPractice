using System;
using System.Collections.Generic;

namespace MiniAspNetCore
{
    public interface IFeatureCollection : IDictionary<Type, object> { }

    public class FeatureCollection : Dictionary<Type, object>, IFeatureCollection
    {
    }

    public static class FeatureExtensions
    {
        public static IFeatureCollection Set<TFeature>(this IFeatureCollection featureCollection, TFeature feature)
        {
            featureCollection[typeof(TFeature)] = feature;
            return featureCollection;
        }

        public static TFeature Get<TFeature>(this IFeatureCollection featureCollection)
        {
            var featureType = typeof(TFeature);
            return featureCollection.ContainsKey(featureType) ? (TFeature)featureCollection[featureType] : default(TFeature);
        }
    }
}
