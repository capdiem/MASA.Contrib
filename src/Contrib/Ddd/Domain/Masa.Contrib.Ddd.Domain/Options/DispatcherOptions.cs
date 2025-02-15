// Copyright (c) MASA Stack All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace Masa.Contrib.Ddd.Domain.Options;

public class DispatcherOptions : IDomainEventOptions
{
    public IServiceCollection Services { get; }

    public Assembly[] Assemblies { get; }

    private bool IsAggregateRootEntity(Type type)
        => type.IsClass && !type.IsGenericType && !type.IsAbstract && type != typeof(AggregateRoot) &&
            typeof(IAggregateRoot).IsAssignableFrom(type);

    private IEnumerable<Type> Types { get; set; }

    private IEnumerable<Type> GetTypes(Type type) => Types.Where(t => t.IsClass && type.IsAssignableFrom(t));

    internal List<Type> AllDomainServiceTypes { get; private set; }

    internal List<Type> AllAggregateRootTypes { get; private set; }

    private DispatcherOptions(IServiceCollection services) => Services = services;

    public DispatcherOptions(IServiceCollection services, Assembly[] assemblies)
        : this(services)
    {
        Assemblies = assemblies;
        Types = assemblies.SelectMany(assembly => assembly.GetTypes());
        AllDomainServiceTypes = GetTypes(typeof(DomainService)).ToList();
        AllAggregateRootTypes = GetTypes(typeof(IAggregateRoot)).Where(IsAggregateRootEntity).ToList();
    }
}
