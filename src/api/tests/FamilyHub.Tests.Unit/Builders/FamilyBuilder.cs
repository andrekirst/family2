using FamilyHub.Modules.Auth.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Tests.Unit.Builders;

/// <summary>
/// Test builder for Family with timestamp control via public API.
/// </summary>
/// <remarks>
/// Since Entity properties have public setters (required for EF Core and interceptor),
/// we can set timestamps directly in tests. The InternalsVisibleTo attribute provides
/// additional test assembly access if needed for future scenarios.
/// </remarks>
public sealed class FamilyBuilder
{
    private FamilyName _name = FamilyName.From("Test Family");
    private UserId _ownerId = UserId.New();
    private DateTime? _createdAt;
    private DateTime? _updatedAt;

    public FamilyBuilder WithName(FamilyName name)
    {
        _name = name;
        return this;
    }

    public FamilyBuilder WithOwnerId(UserId ownerId)
    {
        _ownerId = ownerId;
        return this;
    }

    public FamilyBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public FamilyBuilder WithUpdatedAt(DateTime updatedAt)
    {
        _updatedAt = updatedAt;
        return this;
    }

    public Family Build()
    {
        var family = Family.Create(_name, _ownerId);

        // Set timestamps directly (public setters)
        if (_createdAt.HasValue)
        {
            family.CreatedAt = _createdAt.Value;
        }

        if (_updatedAt.HasValue)
        {
            family.UpdatedAt = _updatedAt.Value;
        }

        return family;
    }
}
