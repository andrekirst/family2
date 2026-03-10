namespace FamilyHub.Common.Domain;

/// <summary>
/// Thrown when a purpose-built query method (e.g., IsCancelledAsync) is called
/// for an entity that does not exist. Validators should guard with ExistsByIdAsync first.
/// </summary>
public sealed class EntityNotFoundException<TEntity> : DomainException
{
    public EntityNotFoundException(object id)
        : base($"{typeof(TEntity).Name} with ID '{id}' was not found.", DomainErrorCodes.NotFound)
    {
    }
}
