using MassTransit;

namespace Athary.Domain.Entities;

public abstract class BaseEntity
{
    protected BaseEntity()
    {
        Id = NewId.NextSequentialGuid();
    }

    public Guid Id { get; set; }
}
