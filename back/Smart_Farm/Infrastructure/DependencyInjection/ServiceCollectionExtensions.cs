using Smart_Farm.Application.Abstractions;
using Smart_Farm.Application.Services;
using Smart_Farm.Infrastructure.External;
using Smart_Farm.Infrastructure.Persistence;

namespace Smart_Farm.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddScoped<IAIDiagnosisService, AIDiagnosisService>();
        return services;
    }

    public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services)
    {
        services.AddScoped<IAIDiagnosisRepository, AIDiagnosisRepository>();
        services.AddScoped<IPlantDiseaseIdentifier, PlantNetDiseaseIdentifier>();
        services.AddScoped<IAgriculturalReportGenerator, GeminiAgriculturalReportGenerator>();
        return services;
    }
}
