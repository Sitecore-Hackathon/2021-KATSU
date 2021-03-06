using Katsu.Feature.Hero.ViewModels;
using Katsu.Foundation.Core.Models;

namespace Katsu.Feature.Hero.Mediators
{
    public interface IHeroMediator
    {
        MediatorResponse<HeroViewModel> RequestHeroViewModel();
    }
}
