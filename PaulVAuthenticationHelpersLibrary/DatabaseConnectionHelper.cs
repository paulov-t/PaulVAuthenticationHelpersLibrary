using Microsoft.Data.SqlClient;
using System;
using System.Threading;

namespace PaulVAuthenticationHelpersLibrary
{
    public static class DatabaseConnectionHelper
    {
        /// <summary>
        /// GetConnection returns a SqlConnection object using the connection string provided in the web.config or app.config file OR the connStr param.
        /// </summary>
        /// <param name="connStr"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static SqlConnection GetConnection(string tenantId, string clientId, string connStr, ETokenCredentialAuthOption authOption = ETokenCredentialAuthOption.Secret)
        {
            if (string.IsNullOrEmpty(connStr))
                throw new ArgumentNullException("Unable to obtain Connection String for SqlConnection. Provide one via Parameter or Web.Config [SQLServer]");

            if (connStr.Contains("(local)"))
            {
                // If the connection string contains (local), we are using a local SQL Server instance
                // and we don't need to use Azure AD authentication.
                return new SqlConnection(connStr);
            }

            TokenCredentialHelper tokenCredentialService = new TokenCredentialHelper(tenantId, clientId, keyVaultName: null, authOption: authOption);
            var token = tokenCredentialService.GetTokenCredential(ETokenCredentialAuthOption.Secret);

            var trc = new Azure.Core.TokenRequestContext(new[] { "https://database.windows.net/.default" });
            return new SqlConnection(connStr) { AccessToken = token.GetToken(trc, default(CancellationToken)).Token };
        }

        /// <summary>
        /// GetConnection returns a SqlConnection object using the connection string provided in the web.config or app.config file OR the connStr param.
        /// </summary>
        /// <param name="connStr"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static SqlConnection GetConnectionUsingClientSecret(string tenantId, string clientId, string clientSecret, string connStr)
        {
            if (string.IsNullOrEmpty(connStr))
                throw new ArgumentNullException("Unable to obtain Connection String for SqlConnection. Provide one via Parameter or Web.Config [SQLServer]");

            if (connStr.Contains("(local)"))
            {
                // If the connection string contains (local), we are using a local SQL Server instance
                // and we don't need to use Azure AD authentication.
                return new SqlConnection(connStr);
            }

            TokenCredentialHelper tokenCredentialService = new TokenCredentialHelper(tenantId: tenantId, clientId: clientId, clientSecret: clientSecret, authOption: ETokenCredentialAuthOption.Secret);
            var token = tokenCredentialService.GetTokenCredential(ETokenCredentialAuthOption.Secret);

            var trc = new Azure.Core.TokenRequestContext(new[] { "https://database.windows.net/.default" });
            return new SqlConnection(connStr) { AccessToken = token.GetToken(trc, default(CancellationToken)).Token };
        }

    }

}