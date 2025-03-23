using Application.Commom.Interfaces;
using Domain.ValueObjects;
using Infrastructure.Data;
using MongoDB.Driver;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // services.AddScoped<IRepositoryMongo, RepositoryMongo>();
        ConfigureSettings(services, configuration);
        var mongoSettings = DIMongo(services, configuration);
        services.AddScoped<IUnitOfWork>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return new UnitOfWork(client, mongoSettings.DatabaseName);
        });
        
        return services;
    }

    public static MongoSettings DIMongo(IServiceCollection services, IConfiguration configuration)
    {
        // Lấy cấu hình MongoSettings từ appsettings.json
        var mongoSettings = configuration.GetSection(MongoSettings.SectionName).Get<MongoSettings>();
        services.AddSingleton<IMongoClient>(sp =>
        {
            return new MongoClient(mongoSettings.ConnectionString);
        });
        
        return mongoSettings!;
    }

    public static void ConfigureSettings(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoSettings>(configuration.GetSection(MongoSettings.SectionName));
    }
}