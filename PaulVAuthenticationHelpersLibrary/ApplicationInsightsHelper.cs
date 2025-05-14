using Microsoft.ApplicationInsights;
using System;
using System.Configuration;

namespace PaulVAuthenticationHelpersLibrary
{

    /// <summary>
    /// ApplicationInsightsHelper is a helper class for Application Insights.
    /// Remember to set the instrumentation key in the web.config or app.config file as APPINSIGHTS_INSTRUMENTATIONKEY.
    /// </summary>
    public class ApplicationInsightsHelper : IApplicationErrorHandlerService
    {
        public static TelemetryClient TelemetryInstance { get; private set; }

        public ApplicationInsightsHelper()
        {
            CreateTelemetryInstance();
        }

        private void CreateTelemetryInstance()
        {
            if (ConfigurationManager.AppSettings["APPINSIGHTS_INSTRUMENTATIONKEY"] == null)
            {
                throw new Exception("APPINSIGHTS_INSTRUMENTATIONKEY is not set in the web.config or app.config file.");
            }

            var appInsightKey = ConfigurationManager.AppSettings["APPINSIGHTS_INSTRUMENTATIONKEY"];
            TelemetryInstance = new TelemetryClient(new Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration(appInsightKey));
        }

        public void TrackException(Exception exception)
        {
            CreateTelemetryInstance();
            TelemetryInstance.TrackException(exception);
        }
    }

    public interface IApplicationErrorHandlerService
    {
        void TrackException(Exception exception);
    }
}
