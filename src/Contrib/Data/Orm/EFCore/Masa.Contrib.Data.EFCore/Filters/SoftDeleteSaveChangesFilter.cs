// Copyright (c) MASA Stack All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore;

public sealed class SoftDeleteSaveChangesFilter<TDbContext, TUserId> : ISaveChangesFilter<TDbContext>
    where TDbContext : MasaDbContext, IMasaDbContext
    where TUserId : IComparable
{
    private readonly Type _userIdType;
    private readonly IUserContext? _userContext;
    private readonly TDbContext _context;
    private readonly MasaDbContextOptions<TDbContext> _masaDbContextOptions;

    public SoftDeleteSaveChangesFilter(
        MasaDbContextOptions<TDbContext> masaDbContextOptions,
        TDbContext dbContext,
        IUserContext? userContext = null)
    {
        _userIdType = typeof(TUserId);
        _masaDbContextOptions = masaDbContextOptions;
        _context = dbContext;
        _userContext = userContext;
    }

    public void OnExecuting(ChangeTracker changeTracker)
    {
        if (!_masaDbContextOptions.EnableSoftDelete)
            return;

        changeTracker.DetectChanges();
        var entries = changeTracker.Entries().Where(entry => entry.State == EntityState.Deleted && entry.Entity is ISoftDelete);
        foreach (var entity in entries)
        {
            var navigationEntries = entity.Navigations
                .Where(navigationEntry => navigationEntry.Metadata is not ISkipNavigation &&
                    !((IReadOnlyNavigation)navigationEntry.Metadata).IsOnDependent && navigationEntry.CurrentValue != null &&
                    entries.All(e => e.Entity != navigationEntry.CurrentValue));
            HandleNavigationEntry(navigationEntries);

            entity.State = EntityState.Modified;
            entity.CurrentValues[nameof(ISoftDelete.IsDeleted)] = true;

            if (entity.Entity.GetType().IsImplementerOfGeneric(typeof(IAuditEntity<>)))
            {
                entity.CurrentValues[nameof(IAuditEntity<TUserId>.ModificationTime)] =
                    DateTime.UtcNow; //The current time to change to localization after waiting for localization
            }

            if (entity.Entity is IAuditEntity<TUserId> && _userContext != null)
            {
                var userId = GetUserId(_userContext.UserId);
                if (userId != null) entity.CurrentValues[nameof(IAuditEntity<TUserId>.Modifier)] = userId;
            }
        }
    }

    private void HandleNavigationEntry(IEnumerable<NavigationEntry> navigationEntries)
    {
        foreach (var navigationEntry in navigationEntries)
        {
            if (navigationEntry is CollectionEntry collectionEntry)
            {
                foreach (var dependentEntry in collectionEntry.CurrentValue ?? new List<object>())
                {
                    HandleDependent(dependentEntry);
                }
            }
            else
            {
                var dependentEntry = navigationEntry.CurrentValue;
                if (dependentEntry != null)
                {
                    HandleDependent(dependentEntry);
                }
            }
        }
    }

    private void HandleDependent(object dependentEntry)
    {
        var entityEntry = _context.Entry(dependentEntry);
        entityEntry.State = EntityState.Modified;

        if (entityEntry.Entity is ISoftDelete)
            entityEntry.CurrentValues[nameof(ISoftDelete.IsDeleted)] = true;

        var navigationEntries = entityEntry.Navigations
            .Where(navigationEntry => navigationEntry.Metadata is not ISkipNavigation &&
                !((IReadOnlyNavigation)navigationEntry.Metadata).IsOnDependent && navigationEntry.CurrentValue != null);
        HandleNavigationEntry(navigationEntries);
    }

    private object? GetUserId(string? userId)
    {
        if (userId == null)
            return null;

        if (_userIdType == typeof(Guid))
            return Guid.Parse(userId);

        return Convert.ChangeType(userId, _userIdType);
    }
}
