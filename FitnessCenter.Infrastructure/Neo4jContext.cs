using Microsoft.Extensions.Configuration;
using Neo4j.Driver;

namespace FitnessCenterr.Infrastructure.Data;

public class Neo4jContext : IAsyncDisposable
{
    private readonly IDriver _driver;

    public Neo4jContext(IConfiguration configuration)
    {
        var uri      = configuration["Neo4j:Uri"]!;
        var user     = configuration["Neo4j:Username"]!;
        var password = configuration["Neo4j:Password"]!;
        _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
    }

    public IAsyncSession OpenSession() => _driver.AsyncSession();

    public async ValueTask DisposeAsync() => await _driver.DisposeAsync();
}
