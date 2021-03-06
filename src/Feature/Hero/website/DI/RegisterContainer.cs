using Katsu.Feature.Hero.Factories;
using Katsu.Feature.Hero.Mediators;
using Katsu.Feature.Hero.Services;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.DependencyInjection;

namespace Katsu.Feature.Hero.DI
{
    public class RegisterContainer : IServicesConfigurator
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IHeroMediator, HeroMediator>();
            serviceCollection.AddTransient<IHeroService, HeroService>();
            serviceCollection.AddTransient<IHeroViewModelFactory, HeroViewModelFactory>();
        }
    }
}
