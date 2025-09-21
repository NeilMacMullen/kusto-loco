using Azure.Core;
using Azure.Identity;
using NotNullStrings;
using System.Security.Cryptography;

namespace AppInsightsSupport;

public static class TokenManager
{
    private static readonly Dictionary<string, DefaultAzureCredential> CredentialCache = new();

    public static async Task<AccessToken> GetToken(string tenant,string[] scopes)
    {
        var credential = GetCredentials(tenant);
        return  await credential.GetTokenAsync(new TokenRequestContext(scopes));
    }

    public static DefaultAzureCredential GetCredentials(string tenantId)
    {
        if (CredentialCache.TryGetValue(tenantId, out var cred))
            return cred;

        var credentialOptions = tenantId.IsBlank()
            ? new DefaultAzureCredentialOptions()
            : new DefaultAzureCredentialOptions()
            {
                TenantId = tenantId
            };
        cred = new DefaultAzureCredential(credentialOptions);
        CredentialCache[tenantId] = cred;

        return cred;
    }

    public static (string tenant, string rid) ExtractTenantFromResourcePath(string resourcePath)
    {
        var i = resourcePath.IndexOf(":");
        var tenant = string.Empty;
        if (i >= 0 && i < (resourcePath.Length - 1))
        {
            tenant = resourcePath[..i];
            resourcePath = resourcePath[(i + 1)..];
        }
        return (tenant, resourcePath);
    }
}
