using Azure.Core;
using Azure.Identity;
using NotNullStrings;

namespace AppInsightsSupport;

public static class TokenManager
{
    private static readonly Dictionary<string, DefaultAzureCredential> CredentialCache = new();

    public static async Task<AccessToken> GetToken(string[] scopes)
    {
        var credential = GetCredentials(string.Empty);
        
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
}
