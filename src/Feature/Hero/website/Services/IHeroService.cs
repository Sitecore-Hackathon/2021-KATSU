using Katsu.Feature.Hero.Models;
using Katsu.Foundation.Search.Models;

namespace Katsu.Feature.Hero.Services
{
    public interface IHeroService
    {
        IHero GetHeroItems();
        BaseSearchResultItem GetHeroImagesSearch();
        bool IsExperienceEditor { get; }
    }
}
