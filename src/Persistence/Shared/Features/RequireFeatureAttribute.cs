namespace Persistence.Shared.Features;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class RequireFeatureAttribute<TFeature> : Attribute where TFeature : IFeature
{
    public Type FeatureType { get; } = typeof(TFeature);

    /// <summary>
    /// Gets the IFeature types declared via RequireFeatureAttribute on the specified types
    /// </summary>
    public static IEnumerable<Type> GetRequiredFeatureTypes(IEnumerable<Type> types)
    {
        var requiredFeatures = new HashSet<Type>();

        foreach (var type in types)
        {
            var attributes = type.GetCustomAttributes(inherit: true);

            foreach (var attr in attributes)
            {
                var attrType = attr.GetType();
                if (!attrType.IsGenericType || attrType.GetGenericTypeDefinition() != typeof(RequireFeatureAttribute<>))
                {
                    continue;
                }

                var featureType = attrType.GetProperty(nameof(FeatureType))?.GetValue(attr) as Type;
                if (featureType == null)
                {
                    continue;
                }

                requiredFeatures.Add(featureType);
            }
        }

        return requiredFeatures;
    }

    /// <summary>
    /// Gets the IFeature instances declared via RequireFeatureAttribute on the specified types
    /// </summary>
    public static IEnumerable<IFeature> GetRequiredFeatures(IEnumerable<Type> types)
    {
        var requiredFeatureTypes = GetRequiredFeatureTypes(types).ToList();
        var features = new List<IFeature>();

        foreach (var featureType in requiredFeatureTypes)
        {
            if (Activator.CreateInstance(featureType) is not IFeature feature)
            {
                throw new InvalidOperationException($"Failed to instantiate feature type {featureType.FullName}. Ensure it has a parameterless constructor.");
            }

            features.Add(feature);
        }

        return features;
    }
}
