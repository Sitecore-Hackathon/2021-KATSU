using Glass.Mapper.Sc.Pipelines.AddMaps;
using Katsu.Foundation.ORM.Extensions;

namespace Katsu.Foundation.ORM.Mappings
{
    public class RegisterMappings : AddMapsPipeline  {
        public void Process(AddMapsPipelineArgs args)
        {
            args.MapsConfigFactory.AddFluentMaps("Katsu.Foundation.ORM");
        }
    }
}
