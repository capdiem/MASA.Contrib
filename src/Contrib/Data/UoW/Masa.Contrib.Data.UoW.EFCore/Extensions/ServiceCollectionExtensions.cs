﻿// Copyright (c) MASA Stack All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

// ReSharper disable once CheckNamespace

namespace Microsoft.Extensions.DependencyInjection;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection UseUoW<TDbContext, TUserId>(
        this IServiceCollection services,
        string paramName,
        Action<MasaDbContextBuilder>? optionsBuilder = null,
        bool disableRollbackOnFailure = false,
        bool? useTransaction = null)
        where TDbContext : MasaDbContext, IMasaDbContext
        where TUserId : IComparable
    {
        MasaArgumentException.ThrowIfNull(services, paramName);

        if (services.Any(service => service.ImplementationType == typeof(UoWProvider)))
            return services;

        services.AddSingleton<UoWProvider>();
        services.TryAddScoped<IUnitOfWorkAccessor, UnitOfWorkAccessor>();
        services.TryAddSingleton<IUnitOfWorkManager, UnitOfWorkManager<TDbContext>>();
        services.TryAddScoped<IConnectionStringProvider, Masa.Contrib.Data.UoW.EFCore.DefaultConnectionStringProvider>();

        services.AddScoped<IUnitOfWork>(serviceProvider => new UnitOfWork<TDbContext>(serviceProvider)
        {
            DisableRollbackOnFailure = disableRollbackOnFailure,
            UseTransaction = useTransaction
        });
        if (services.All(service => service.ServiceType != typeof(MasaDbContextOptions<TDbContext>)))
            services.AddMasaDbContext<TDbContext, TUserId>(optionsBuilder);

        services.AddScoped<ITransaction, Transaction>();
        MasaApp.TrySetServiceCollection(services);
        return services;
    }

    private sealed class UoWProvider
    {
    }
}
