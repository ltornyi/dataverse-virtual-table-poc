using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;

namespace FacilityDataProvider
{
    public class RetrieveMultiple : IPlugin
    {
        string apihost = "\nhttps://XXXXXXXXXXXXXXXXXXXXX.azurewebsites.net";
        string apipath = "api/facility";
        string apikey = "XXXXXXXXXXXXXXXXX";
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            EntityCollection ec = new EntityCollection();

            tracingService.Trace("Starting to retrieve facility data");

            try
            {

                //Build query
                string clientId = "";
                string search = "";
                string top = "";
                string orderbyField = "";
                string orderbyDir = "";
                if (context.InputParameters.Contains("Query"))
                {
                    var thisQuery = context.InputParameters["Query"] as QueryExpression;
                    tracingService.Trace("query: {0}", JsonSerializer.Serialize(thisQuery));

                    var filters = thisQuery.Criteria.Filters;
                    var conditions = thisQuery.Criteria.Conditions;
                    tracingService.Trace("query filters: {0}", JsonSerializer.Serialize(filters));
                    tracingService.Trace("query conditions: {0}", JsonSerializer.Serialize(conditions));
                    //generate search parameter from query
                    search = generateSearchfromQuery(filters, conditions, tracingService);
                    //generate clientId parameter from query
                    clientId = generateClientIdfromQuery(filters, conditions, tracingService);
                    //top is not handled
                    //extract orderbyField and orderbyDir from query
                    var orders = thisQuery.Orders;
                    tracingService.Trace("query orders: {0}", JsonSerializer.Serialize(orders));
                    var ordering = generateOrderingfromQueryOrders(orders, tracingService);
                    orderbyField = ordering.orderbyField;
                    orderbyDir = ordering.orderbyDir;
                    
                }
                string requestUri = $"{apihost}/{apipath}?code={apikey}&clientId={clientId}&search={search}&top={top}&orderbyField={orderbyField}&orderbyDir={orderbyDir}";
                tracingService.Trace("generated URI:{0}", requestUri);

                var webRequest = WebRequest.Create(requestUri) as HttpWebRequest;

                if (webRequest != null)
                {
                    webRequest.ContentType = "application/json";
                    using (var s = webRequest.GetResponse().GetResponseStream())
                    {
                        using (var sr = new StreamReader(s))
                        {
                            var facilitiesAsJson = sr.ReadToEnd();
                            tracingService.Trace("Response: {0}", facilitiesAsJson);

                            List<Facility> facilities = null;
                            facilities = JsonSerializer.Deserialize<List<Facility>>(facilitiesAsJson);

                            if (facilities != null)
                            {
                                ec.Entities.AddRange(facilities.Select(f => f.ToEntity(tracingService)));
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                tracingService.Trace("Exception with message: {0}", e.Message);
            }

            // Set output parameter
            context.OutputParameters["BusinessEntityCollection"] = ec;
            tracingService.Trace("End of retrieve facility data");
        }

        private ConditionExpression findConditionExpressionOnAttribute(DataCollection<ConditionExpression> conditions, string attributeName)
        {
            ConditionExpression nameCondition = null;
            foreach (ConditionExpression ce in conditions)
            {
                if (ce.AttributeName.Equals(attributeName))
                {
                    nameCondition = ce;
                    break;
                }
            }
            return nameCondition;
        }

        private ConditionExpression findFirstConditionOnAttribute(DataCollection<FilterExpression> filters, DataCollection<ConditionExpression> conditions, ITracingService tracingService, string attributeName)
        {
            //see if there's a condition on the attribute - if yes, return it
            ConditionExpression condition = null;
            //first look for it in Filters
            foreach (FilterExpression fe in filters)
            {
                condition = findConditionExpressionOnAttribute(fe.Conditions, attributeName);
                if (condition != null)
                {
                    break;
                }
            }
            //if not found, look for it in Conditions:
            if (condition == null)
            {
                condition = findConditionExpressionOnAttribute(conditions, attributeName);
            }

            return condition;
        }

        private string generateSearch(ConditionExpression nc)
        {
            if (nc == null)
            {
                return "";
            }
            switch (nc.Operator)
            {
                case ConditionOperator.Like: //string "equals", "begins with", "ends with", "contains" are mapped to like
                    return nc.Values[0].ToString();
                //API only supports like
                case ConditionOperator.NotEqual: 
                case ConditionOperator.NotLike: //"does not contain", "does not begin with", "does not end with"
                case ConditionOperator.NotNull: //"contains data"
                case ConditionOperator.Null: //"does not contain data"
                    return "";
                default:
                    return nc.Values[0].ToString();
            }
        }

        private string generateSearchfromQuery(
            DataCollection<FilterExpression> filters,
            DataCollection<ConditionExpression> conditions,
            ITracingService tracingService)
        {
            //see if there's an condition on ltornyi_name - if yes, populate the search parameter
            ConditionExpression nameCondition = findFirstConditionOnAttribute(filters, conditions, tracingService, "ltornyi_name");
            //generate search parameter from the condition on name
            return generateSearch(nameCondition);
        }

        private string generateClientId(ConditionExpression nc)
        {
            if (nc == null)
            {
                return "";
            }
            switch (nc.Operator)
            {
                case ConditionOperator.Equal: //Guid "equals"
                    return nc.Values[0].ToString();
                //API only supports equal
                case ConditionOperator.NotEqual:
                case ConditionOperator.Like: //"contains", "begins with", "ends with"
                case ConditionOperator.NotLike: //"does not contain", "does not begin with", "does not end with"
                case ConditionOperator.NotNull: //"contains data"
                case ConditionOperator.Null: //"does not contain data"
                    return "";
                default:
                    return nc.Values[0].ToString();
            }
        }

        private string generateClientIdfromQuery(
            DataCollection<FilterExpression> filters,
            DataCollection<ConditionExpression> conditions,
            ITracingService tracingService)
        {
            //see if there's an condition on ltornyi_name - if yes, populate the search parameter
            ConditionExpression nameCondition = findFirstConditionOnAttribute(filters, conditions, tracingService, "ltornyi_clientid");
            //generate search parameter from the condition on name
            return generateClientId(nameCondition);
        }

        private class Ordering
        {
            public string orderbyField { get; set; }
            public string orderbyDir { get; set; }
        }

        private Ordering generateOrderingfromQueryOrders(
            DataCollection<OrderExpression> orders,
            ITracingService tracingService)
        {
            Ordering result = new Ordering();
            result.orderbyField = "";
            result.orderbyDir = "";
            if (orders.Any())
            {
                OrderExpression orderExpression = orders[0];
                switch (orderExpression.AttributeName)
                {
                    case "ltornyi_name": result.orderbyField = "name"; break;
                    case "ltornyi_facilitytype": result.orderbyField = "facility_type"; break;
                    case "ltornyi_address": result.orderbyField = "address"; break;
                    default:
                        break;
                }
                //determine if ASC or DESC but only if sorted by a supported field: 
                if (result.orderbyField != "")
                {
                    switch (orderExpression.OrderType)
                    {
                        case OrderType.Ascending: result.orderbyDir = "asc"; break;
                        case OrderType.Descending: result.orderbyDir = "desc"; break;
                    }
                }
                

            }
            return result;
        }
    }
}
