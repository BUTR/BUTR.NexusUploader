using Spectre.Console.Cli;

using System;

namespace BUTR.NexusUploader.Utils;

internal sealed class TypeResolver : ITypeResolver
{
    private readonly IServiceProvider _provider;

    public TypeResolver(IServiceProvider provider) => _provider = provider;

    public object? Resolve(Type? type)
    {
        if (type == null) return null;

        return _provider.GetService(type);
    }
}