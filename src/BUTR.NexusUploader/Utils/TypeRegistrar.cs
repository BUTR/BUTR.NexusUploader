using Microsoft.Extensions.DependencyInjection;

using Spectre.Console.Cli;

using System;

namespace BUTR.NexusUploader.Utils;

internal sealed class TypeRegistrar : ITypeRegistrar
{
    private readonly IServiceCollection _provider;

    public TypeRegistrar(IServiceCollection provider) => _provider = provider;

    public ITypeResolver Build() => new TypeResolver(_provider.BuildServiceProvider());

    public void Register(Type service, Type implementation) => _provider.AddSingleton(service, implementation);

    public void RegisterInstance(Type service, object implementation) => _provider.AddSingleton(service, implementation);

    public void RegisterLazy(Type service, Func<object> factory) => _provider.AddSingleton(service, _ => factory());
}