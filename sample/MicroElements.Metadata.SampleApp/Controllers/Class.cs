using System.Collections.Generic;
using MicroElements.Metadata.Schema;

using Microsoft.AspNetCore.Mvc;

namespace MicroElements.Metadata.SampleApp.Controllers
{
    public class LPPrometheusRequestSchema : IStaticSchema
    {
        public static IProperty<string> MessageId = new Property<string>("MessageId");
        public static IProperty<string> CorrelationId = new Property<string>("CorrelationId");

        public static IProperty<PropertyContainer<RequestDataSchema>> Data = new Property<PropertyContainer<RequestDataSchema>>("Data");
    }

    public class RequestDataSchema : IStaticSchema, IOneOf
    {
        /// <inheritdoc />
        public IEnumerable<ISchema> OneOf()
        {
            yield return new GetAllTradesRoot().GetObjectSchema();
            yield return new GetClientsRoot().GetObjectSchema();
            yield return new GetTradesRoot().GetObjectSchema();
        }
    }
    public class GetAllTradesRoot : StaticSchema
    {
        public static IProperty<PropertyContainer<GetAllTrades>> GetAllTrades = new Property<PropertyContainer<GetAllTrades>>("GetAllTrades");
    }

    public class GetTradesRoot : StaticSchema
    {
        public static IProperty<PropertyContainer<GetTrades>> GetTrades = new Property<PropertyContainer<GetTrades>>("GetTrades");
    }

    public class GetClientsRoot : StaticSchema
    {
        public static IProperty<PropertyContainer<GetClients>> GetClients = new Property<PropertyContainer<GetClients>>("GetClients");
    }

    public class GetAllTrades : IStaticSchema
    {
        public static IProperty<string[]> TradeIds = new Property<string[]>("TradeIds");
    }

    public class GetTrades : IStaticSchema
    {
        public static IProperty<string[]> TradeIds = new Property<string[]>("TradeIds");
    }

    public class GetClients : IStaticSchema
    {
        public static IProperty<PropertyContainer<ClientRequest>> ClientRequests = new Property<PropertyContainer<ClientRequest>>("ClientRequests");
    }

    public class ClientRequest : IStaticSchema
    {
        public static IProperty<string> CrmId = new Property<string>("CrmId");
        public static IProperty<string> INN = new Property<string>("INN");
        public static IProperty<string> MurexId = new Property<string>("MurexId");
    }

    [ApiController]
    public class AAAController : Controller
    {
        [HttpGet("[action]")]
        public PropertyContainer<LPPrometheusRequestSchema> GetRequest()
        {
            return new PropertyContainer<LPPrometheusRequestSchema>();
        }

        [HttpPost("[action]")]
        public PropertyContainer<LPPrometheusRequestSchema> SendRequest(PropertyContainer<LPPrometheusRequestSchema> container)
        {
            return new PropertyContainer<LPPrometheusRequestSchema>();
        }
    }
}
