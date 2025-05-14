using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Graph;
using System;
using System.Threading.Tasks;


namespace PaulVAuthenticationHelpersLibrary
{
    public class GraphClientHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="clientId"></param>
        /// <param name="kvName"></param>
        /// <param name="secretName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public GraphServiceClient GetGraphClientUsingKeyVault(string tenantId, string clientId, string kvName, string secretName)
        {
            if (string.IsNullOrEmpty(tenantId))
                throw new ArgumentNullException($"{nameof(tenantId)} ");

            if (string.IsNullOrEmpty(clientId))
                throw new ArgumentNullException($"{nameof(clientId)} ");

            if (string.IsNullOrEmpty(kvName))
                throw new ArgumentNullException($"{nameof(kvName)} ");

            if (string.IsNullOrEmpty(secretName))
                throw new ArgumentNullException($"{nameof(secretName)} ");

            Uri accountUri = new Uri(string.Format("https://{0}.vault.azure.net/", kvName));
            // Use Managed Identity to access Key Vault
            var secretClient = new SecretClient(accountUri, new ManagedIdentityCredential());
            if (secretClient == null)
                throw new ArgumentNullException(nameof(secretClient));

            var secret = secretClient.GetSecret(secretName).Value.Value;
            if (string.IsNullOrEmpty(secret))
            {
                var appException = new ApplicationException("secret is not set in AppSettings or KeyVault");
                ApplicationInsightsHelper.TelemetryInstance.TrackException(appException);
                throw appException;
            }

            return GetGraphClientUsingClientSecret(tenantId, clientId, secret);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="clientId"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public GraphServiceClient GetGraphClientUsingClientSecret(string tenantId, string clientId, string secret)
        {
            if (string.IsNullOrEmpty(tenantId))
                throw new ArgumentNullException($"{nameof(tenantId)} ");

            if (string.IsNullOrEmpty(clientId))
                throw new ArgumentNullException($"{nameof(clientId)} ");

            if (string.IsNullOrEmpty(secret))
                throw new ArgumentNullException($"{nameof(secret)} ");

            var chainedCredential = new ChainedTokenCredential(
                               new ClientSecretCredential(tenantId, clientId, secret),
                               new DefaultAzureCredential());

            GraphServiceClient graphClient = new GraphServiceClient(
                chainedCredential,
                new string[] { "https://graph.microsoft.com/.default" },
                null);
            return graphClient;
        }

    }


    public interface IAuthProvider
    {
        Task<string> GetUserAccessTokenAsync();
    }
}
