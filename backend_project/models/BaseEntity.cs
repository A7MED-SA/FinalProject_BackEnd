using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MassTransit;

namespace backend_project.Models;

/// <summary>
/// Base entity with Sequential GUID as Primary Key using MassTransit.NewId
/// </summary>
public abstract class BaseEntity
{
    protected BaseEntity()
    {
        // Generate Sequential GUID using MassTransit.NewId (UUIDv7-like)
        Id = NewId.NextSequentialGuid();
    }

    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }
}
