using Glass.Mapper.Sc.Pipelines.AddMaps;
using Katsu.Foundation.ORM.Extensions;

namespace Katsu.Feature.Hero.ORM
{
    public class RegisterMappings : AddMapsPipeline  {
        public void Process(AddMapsPipelineArgs args)
        {
            args.MapsConfigFactory.AddFluentMaps("Katsu.Feature.Hero");
        }
    }
}
