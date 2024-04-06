// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.PowerPlatform.PowerApps.Persistence.PaYaml.Models.PowerFx;
using Microsoft.PowerPlatform.PowerApps.Persistence.PaYaml.Models.SchemaV2_2;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Microsoft.PowerPlatform.PowerApps.Persistence.PaYaml.Serialization;

public record PaYamlSerializerOptions
{
    internal static readonly PaYamlSerializerOptions Default = new();

    public string NewLine { get; init; } = "\n";

    public PFxExpressionYamlFormattingOptions PFxExpressionYamlFormatting { get; init; } = new();

    public Action<DeserializerBuilder>? AdditionalDeserializerConfiguration { get; init; }

    public Action<SerializerBuilder>? AdditionalSerializerConfiguration { get; init; }

    internal void ApplyToDeserializerBuilder(DeserializerBuilder builder, SerializationContext serializationContext)
    {
        builder
            .WithDuplicateKeyChecking()
            ;
        AddTypeConverters(builder, serializationContext);
        AdditionalDeserializerConfiguration?.Invoke(builder);
    }

    internal void ApplyToSerializerBuilder(SerializerBuilder builder, SerializationContext serializationContext)
    {
        // TODO: Can we control indentation chars? e.g. to be explicitly set to 2 spaces?
        builder
            .WithQuotingNecessaryStrings()
            .WithNewLine(NewLine)
            .DisableAliases()
            .WithEnumNamingConvention(PascalCaseNamingConvention.Instance)
            .WithIndentedSequences() // to match VS Code's default formatting settings
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitEmptyCollections | DefaultValuesHandling.OmitNull | DefaultValuesHandling.OmitDefaults)
            ;
        AddTypeConverters(builder, serializationContext);
        AdditionalSerializerConfiguration?.Invoke(builder);
    }

    private void AddTypeConverters<TBuilder>(BuilderSkeleton<TBuilder> builder, SerializationContext serializationContext)
        where TBuilder : BuilderSkeleton<TBuilder>
    {
        builder.WithTypeConverter(new PFxExpressionYamlConverter(PFxExpressionYamlFormatting));
        builder.WithTypeConverter(new NamedObjectYamlConverter<ControlInstance>(serializationContext));
        builder.WithTypeConverter(new NamedObjectYamlConverter<PFxFunctionParameter>(serializationContext));
    }
}