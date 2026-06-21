using Athary.Application.Common;
using FluentAssertions;

namespace Athary.Application.UnitTests;

public class PagedListTests
{
    [Fact]
    public void PagedList_ShouldCalculateTotalPages_WhenCreated()
    {
        var items = new List<int> { 1, 2, 3, 4, 5 };
        var result = new PagedList<int>(items, 25, 1, 10);

        result.TotalPages.Should().Be(3);
        result.HasNextPage.Should().BeTrue();
        result.HasPreviousPage.Should().BeFalse();
    }
}
