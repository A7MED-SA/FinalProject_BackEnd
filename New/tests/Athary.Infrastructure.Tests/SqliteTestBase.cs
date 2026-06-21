using Mapster;
using MapsterMapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Athary.Infrastructure.Data;

namespace Athary.Infrastructure.Tests;

public abstract class SqliteTestBase : IDisposable
{
    private readonly SqliteConnection _connection;
    protected readonly ApplicationDbContext Context;
    protected static readonly IMapper Mapper;

    static SqliteTestBase()
    {
        var config = new TypeAdapterConfig();
        config.Scan(typeof(Athary.Application.Mappings.IAssemblyMarker).Assembly);
        Mapper = new Mapper(config);
    }

    protected SqliteTestBase()
    {
        _connection = new SqliteConnection("DataSource=:memory:;Foreign Keys=False");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new ApplicationDbContext(options);
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
        _connection.Close();
        _connection.Dispose();
    }
}
