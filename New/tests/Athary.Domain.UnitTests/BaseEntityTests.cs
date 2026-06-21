using Athary.Domain.Entities;
using FluentAssertions;

namespace Athary.Domain.UnitTests;

public class BaseEntityTests
{
    [Fact]
    public void BaseEntity_ShouldHaveGeneratedId_WhenCreated()
    {
        var entity = new Course();
        entity.Id.Should().NotBeEmpty();
    }
}
