using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SistemaVentasMotorrepuestos.API.Servicios.Workers;

namespace SistemaVentasMotorrepuestos.API.Configuracion
{
    public static class FacturacionWorkersServiceCollectionExtensions
    {
        public static IServiceCollection AddFacturacionHostedWorkers(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var section = configuration.GetSection(FacturacionWorkersOptions.SectionName);

            services.AddOptions<FacturacionWorkersOptions>()
                .Bind(section)
                .ValidateOnStart();

            var options = section.Get<FacturacionWorkersOptions>()
                ?? new FacturacionWorkersOptions();

            if (options.Enabled)
            {
                // services.AddHostedService<FacturacionEmisorWorker>();
                // services.AddHostedService<FacturacionConsultorWorker>();
            }

            return services;
        }
    }
}
