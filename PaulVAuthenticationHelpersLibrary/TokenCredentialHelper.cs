using Azure.Core;
using Azure.Identity;
using System;

namespace PaulVAuthenticationHelpersLibrary
{
    public class TokenCredentialHelper
    {


        public TokenCredentialHelper(string tenantId, string clientId, string keyVaultName = null, ETokenCredentialAuthOption authOption = ETokenCredentialAuthOption.Chained)
        {
            TenantId = tenantId;
            ClientId = clientId;
            KeyVaultName = keyVaultName;
            TokenCredentialAuthOption = authOption;
        }

        public TokenCredentialHelper(string tenantId, string clientId, string keyVaultName = null, string clientSecret = null, ETokenCredentialAuthOption authOption = ETokenCredentialAuthOption.Chained) : this(tenantId, clientId, keyVaultName, authOption)
        {
            ClientSecretKey = clientSecret;
        }


        private string TenantId { get; set; }

        private string ClientId { get; set; }

        private string ClientSecretKey { get; set; }

        private ETokenCredentialAuthOption TokenCredentialAuthOption { get; set; }

        private string KeyVaultName { get; set; }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public TokenCredential GetTokenCredential(ETokenCredentialAuthOption? forcedAuthOption = null)
        {
            var credentialOptions = new DefaultAzureCredentialOptions();
            credentialOptions.ExcludeVisualStudioCredential = true;

            var credentialOption = TokenCredentialAuthOption;

            // if we have a forced option then use it
            if (forcedAuthOption.HasValue)
            {
                credentialOption = forcedAuthOption.Value;
                ApplicationInsightsHelper.TelemetryInstance.TrackTrace("GetTokenCredential"
                , new System.Collections.Generic.Dictionary<string, string> { { "UsedForcedAuthOption", true.ToString() } });
            }

            // Trace the credential option
            ApplicationInsightsHelper.TelemetryInstance.TrackTrace("GetTokenCredential"
            , new System.Collections.Generic.Dictionary<string, string> { { "CredentialOption", credentialOption.ToString() } });

            // based on the selected credentialOption use the appropriate credential (Secret, Managed Identity, default)
            switch (credentialOption)
            {
                case ETokenCredentialAuthOption.Secret:
                    return GetDefaultTokenCredential(credentialOptions, true);
                case ETokenCredentialAuthOption.ManagedIdentity:
                    return GetDefaultTokenCredential(credentialOptions, false);
                case ETokenCredentialAuthOption.Chained:
                    credentialOptions.ExcludeManagedIdentityCredential = true;
                    credentialOptions.ExcludeEnvironmentCredential = true;
                    credentialOptions.ExcludeSharedTokenCacheCredential = true;
                    credentialOptions.ExcludeVisualStudioCredential = true;
                    return new ChainedTokenCredential(
                        new ManagedIdentityCredential(),
                        new ClientSecretCredential(TenantId, ClientId, ClientSecretKey),
                        new DefaultAzureCredential(credentialOptions));
                default:
                    throw new Exception("No valid credential option found");
            }

        }

        private TokenCredential GetDefaultTokenCredential(DefaultAzureCredentialOptions credentialOptions, bool useClientSecret)
        {
            if (useClientSecret)
            {
                // This uses Secrets.xml (right-click WebApp project, Manage user secrets) within Dev Environment to replace "ClientSecret" AppSetting 
                string clientSecret = ClientSecretKey;
                if (string.IsNullOrEmpty(clientSecret) && !string.IsNullOrEmpty(KeyVaultName))
                {
                    //var kvm = new KeyVaultManager(kvName);
                    //if (kvm != null && !string.IsNullOrEmpty(kvm.GetSecret("GraphAPI-ClientSecret")))
                    //    clientSecret = kvm.GetSecret("GraphAPI-ClientSecret");
                    throw new NotImplementedException("KeyVault is not implemented yet");
                }

                if (string.IsNullOrEmpty(clientSecret))
                {
                    var appException = new ApplicationException("ClientSecret is not set in AppSettings or KeyVault");
                    ApplicationInsightsHelper.TelemetryInstance.TrackException(appException);
                    throw appException;
                }

                credentialOptions.ExcludeVisualStudioCredential = false;
                credentialOptions.ExcludeEnvironmentCredential = true;
                credentialOptions.ExcludeManagedIdentityCredential = true;
                ClientSecretCredential secretCredential = new ClientSecretCredential(
                    TenantId
                    , ClientId  // "e4bc46e6-1758-4651-88d1-510e093c54f7"
                    , clientSecret
                    , new TokenCredentialOptions()
                    {
                        AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
                    }
                    );
                return secretCredential;
            }
            else
            {
                return new DefaultAzureCredential(credentialOptions);
            }
        }
    }
}
