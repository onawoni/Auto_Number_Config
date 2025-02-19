using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace AutoNumberConfig 
{
    public class AutoNumber : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Obtain the tracing service
            ITracingService tracingService =
            (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.  
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            // The InputParameters collection contains all the data passed in the message request.  
            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {
                // Obtain the target entity from the input parameters.  
                Entity contact = (Entity)context.InputParameters["Target"];
                Entity autonoconfig = new Entity();

                // Obtain the IOrganizationService instance which you will need for  
                // web service calls.  
                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                try
                {
                    // Plug-in business logic goes here.
                   StringBuilder autoNumber = new StringBuilder();
                    string prefix, suffix, separator, currentNumber;
                    DateTime currentDate = DateTime.Now;
                    string year = currentDate.Year.ToString();
                    string month = currentDate.Month.ToString("00");
                    string day = currentDate.Day.ToString("00");
                    

                    QueryExpression query = new QueryExpression("syntek_autonumberconfiguration");
                    query.ColumnSet = new ColumnSet(new string[] { "syntek_name", 
                                                                    "syntek_prefix", 
                                                                    "syntek_suffix", 
                                                                    "syntek_separator", 
                                                                    "syntek_currentnumber" });
                    //query.Criteria.AddCondition("syntek_name", ConditionOperator.Equal, "Syntek Automation Config".ToLower());
                    EntityCollection autoNumberConfigCollection = service.RetrieveMultiple(query);
                   if (autoNumberConfigCollection.Entities.Count == 0)
                    {
                        throw new InvalidPluginExecutionException("No matching record found");
                    }
                   foreach( Entity entity in autoNumberConfigCollection.Entities)
                    {
                        if (entity.Attributes["syntek_name"].ToString().ToLower() == "syntek automation config")
                        {
                            prefix = entity.GetAttributeValue<string>("syntek_name");
                            suffix = entity.GetAttributeValue<string>("syntek_suffix");
                            separator = entity.GetAttributeValue<string>("syntek_separator");
                            string previousNumber = entity.GetAttributeValue<string>("syntek_currentnumber");
                            int intCurrentNumber = int.Parse(previousNumber);
                            intCurrentNumber ++;
                            currentNumber = intCurrentNumber.ToString("000000");
                            autonoconfig.Id = entity.Id;
                            autonoconfig["syntek_currentnumber"] = intCurrentNumber;
                            autoNumber.Append(prefix + separator +day + month + year + separator + suffix + currentNumber);

                        }
                        break;
                    }
                    contact["jobtitle"] = autoNumber;

                
                }

                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in AutoNumberCOnfigPlugin.", ex);
                }

                catch (Exception ex)
                {
                    tracingService.Trace("FollowUpPlugin: {0}", ex.ToString());
                    throw;
                }
            }
        }
    }
}


