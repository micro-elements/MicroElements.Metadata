using System.Collections.Generic;
using MicroElements.Metadata.Schema;

using Microsoft.AspNetCore.Mvc;

namespace MicroElements.Metadata.SampleApp.Controllers
{
    public class RequestSchema : IStaticSchema
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
            yield return new GetClientsRoot().GetObjectSchema();
            yield return new GetTradesRoot().GetObjectSchema();
        }
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
        public static IProperty<string> ClientId = new Property<string>("ClientId");
        public static IProperty<string> Name = new Property<string>("Name");

    }

    [ApiController]
    public class AAAController : Controller
    {
        [HttpGet("[action]")]
        public PropertyContainer<RequestSchema> GetRequest()
        {
            return new PropertyContainer<RequestSchema>();
        }

        [HttpPost("[action]")]
        public PropertyContainer<RequestSchema> SendRequest(PropertyContainer<RequestSchema> container)
        {
            return new PropertyContainer<RequestSchema>();
        }
    }
}
