using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MicroElements.Metadata.Schema;

using Microsoft.AspNetCore.Mvc;

namespace MicroElements.Metadata.SampleApp.Controllers
{
    public class LPPrometheusRequestSchema : IStaticSchema, IStaticPropertySet
    {
        public static IProperty<string> MessageId = new Property<string>("MessageId");
        public static IProperty<string> CorrelationId = new Property<string>("CorrelationId");


        public static IProperty<PropertyContainer<DataSchema>> Data = new Property<PropertyContainer<DataSchema>>("Data");
    }

    public class DataSchema : IStaticSchema, IStaticPropertySet, IOneOf
    {
        private static IProperty<PropertyContainer<GetAllTrades>> GetAllTrades = new Property<PropertyContainer<GetAllTrades>>("GetAllTrades");
        private static IProperty<PropertyContainer<GetClients>> GetClients = new Property<PropertyContainer<GetClients>>("GetClients");
        private static IProperty<PropertyContainer<GetTrades>> GetTrades = new Property<PropertyContainer<GetTrades>>("GetTrades");

        private static GetAllTradesRequest AllTradesRequest = new GetAllTradesRequest();

        public static ISchema DataSchema2 = new Schema.DataSchema("Data")
            .OneOf(new Schema.DataSchema("GetAllTradesRequest"));


        private static IObjectSchema GetAllTrades2 = new ObjectSchema<PropertyContainer<GetAllTrades>>();

        /// <inheritdoc />
        public IEnumerable<ISchema> OneOf()
        {
            yield return new MutableObjectSchema(name: "GetAllTradesRequest", properties: new IProperty[] { new Property<PropertyContainer<GetAllTrades>>("GetAllTrades") });
            yield return GetAllTrades;
            yield return GetClients;
            yield return GetTrades;
        }
    }

    public class GetAllTradesRequest : IStaticSchema, IStaticPropertySet
    {
        public static IProperty<PropertyContainer<GetAllTrades>> GetAllTrades = new Property<PropertyContainer<GetAllTrades>>("GetAllTrades");
    }

    public class GetAllTrades : IStaticSchema, IStaticPropertySet
    {
        public static IProperty<string[]> TradeIds = new Property<string[]>("TradeIds");
    }

    public class GetClients : IStaticSchema, IStaticPropertySet
    {
        public static IProperty<PropertyContainer<ClientRequest>> ClientRequests = new Property<PropertyContainer<ClientRequest>>("ClientRequests");
    }

    public class ClientRequest : IStaticSchema, IStaticPropertySet
    {
        public static IProperty<string> CrmId = new Property<string>("CrmId");
        public static IProperty<string> INN = new Property<string>("INN");
        public static IProperty<string> MurexId = new Property<string>("MurexId");
    }

    public class GetTrades : IStaticSchema, IStaticPropertySet
    {
        public static IProperty<string[]> TradeIds = new Property<string[]>("TradeIds");
    }

    [ApiController]
    public class AAAController : Controller
    {
        [HttpGet("[action]")]
        public PropertyContainer<LPPrometheusRequestSchema> GetRequest()
        {
            return new PropertyContainer<LPPrometheusRequestSchema>();
        }
    }
}
