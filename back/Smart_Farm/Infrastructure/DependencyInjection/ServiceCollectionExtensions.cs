using Smart_Farm.Application.Abstractions;
using Smart_Farm.Application.Services;
using Smart_Farm.Infrastructure.BackgroundServices;
using Smart_Farm.Infrastructure.External;
using Smart_Farm.Infrastructure.Persistence;

namespace Smart_Farm.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddScoped<IAIDiagnosisService, AIDiagnosisService>();
        services.AddScoped<IWaterBalanceService, WaterBalanceService>();
        return services;
    }

    public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services)
    {
        services.AddScoped<IAIDiagnosisRepository, AIDiagnosisRepository>();
        services.AddScoped<IPlantSpeciesIdentifier, PlantNetSpeciesIdentifier>();
        services.AddScoped<IPlantDiseaseIdentifier, PlantNetDiseaseIdentifier>();
        services.AddScoped<IAgriculturalReportGenerator, GrogAgriculturalReportGenerator>();
        services.AddScoped<IIrrigationAdviceGenerator, GrogIrrigationAdviceGenerator>();
        services.AddHttpClient("OpenMeteo");
        services.AddHttpClient("Nominatim");
        services.AddScoped<ILocationGeocodingService, NominatimLocationGeocodingService>();
        services.AddScoped<IWeatherProvider, OpenMeteoWeatherProvider>();
        services.AddHostedService<WeatherTaskGeneratorService>();
        return services;
    }
}
