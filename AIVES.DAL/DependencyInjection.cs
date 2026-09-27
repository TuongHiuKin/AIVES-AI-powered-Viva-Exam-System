using AIVES.DAL.Context;
using AIVES.DAL.Repositories.Implementations;
using AIVES.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AIVES.DAL;

public static class DependencyInjection
{
    public static IServiceCollection AddAivesDataAccess(this IServiceCollection services, string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        services.AddDbContext<AIVESDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<ISystemAccountRepository, SystemAccountRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<INewsArticleRepository, NewsArticleRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        // Stateless DAOs are accessed through their thread-safe Instance properties.
        return services;
    }
}
