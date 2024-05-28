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
            if (context.Value is string str && context.Parameter.ParameterType == typeof(int))
            {
                result = int.Parse(str);
                return true;
            }
            
            result = context.Value;
            return true;
        }

        if (context.Resolver.Resolve(typeof(IConfiguration)) is not IConfigurationRoot configuration)
        {
            result = null;
            return false;
        }

        var value = configuration.GetValue(context.Parameter.ParameterType, _name);
        if (value is string str2 && context.Parameter.ParameterType == typeof(int))
        {
            result = int.Parse(str2);
            return true;
        }
        
        result = value;
        return true;
    }
}