using Microsoft.Extensions.Configuration;

using Spectre.Console.Cli;

using System;

namespace BUTR.NexusUploader.Utils;

public sealed class EnvironmentVariableAttribute : ParameterValueProviderAttribute
{
    private readonly string _name;

    public EnvironmentVariableAttribute(string name) => _name = name ?? throw new ArgumentNullException(nameof(name));

    public override bool TryGetValue(CommandParameterContext context, out object? result)
    {
        if (context.Value is not null)
        {
            result = context.Value;
            return true;
        }

        if (context.Resolver.Resolve(typeof(IConfiguration)) is not IConfigurationRoot configuration)
        {
            result = null;
            return false;
        }

        result = configuration.GetValue(context.Parameter.ParameterType, _name);
        return true;
    }
}