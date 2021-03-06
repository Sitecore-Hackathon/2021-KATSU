using Katsu.Feature.Hero.Models;
using Katsu.Feature.Hero.ViewModels;

namespace Katsu.Feature.Hero.Factories
{
    public interface IHeroViewModelFactory
    {
        HeroViewModel CreateHeroViewModel(IHero heroItemDataSource, bool isExperienceEditor);
    }
}
