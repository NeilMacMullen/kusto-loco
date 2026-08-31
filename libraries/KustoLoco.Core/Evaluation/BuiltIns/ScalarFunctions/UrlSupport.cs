using System;
using System.Text.Json.Nodes;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

// Shared helpers for parse_url / parse_urlquery. Parsed literally (not via System.Uri, which would fill in default
// ports and normalise the path) to match ADX's component breakdown.
internal static class UrlSupport
{
    public static JsonNode ParseUrl(string? url)
    {
        var scheme = "";
        var host = "";
        var port = "";
        var path = "";
        var username = "";
        var password = "";
        var fragment = "";
        var query = "";
        var rest = url ?? "";

        var schemeIdx = rest.IndexOf("://", StringComparison.Ordinal);
        if (schemeIdx >= 0) { scheme = rest.Substring(0, schemeIdx); rest = rest.Substring(schemeIdx + 3); }

        var hashIdx = rest.IndexOf('#');
        if (hashIdx >= 0) { fragment = rest.Substring(hashIdx + 1); rest = rest.Substring(0, hashIdx); }

        var qIdx = rest.IndexOf('?');
        if (qIdx >= 0) { query = rest.Substring(qIdx + 1); rest = rest.Substring(0, qIdx); }

        var slashIdx = rest.IndexOf('/');
        var authority = rest;
        if (slashIdx >= 0) { path = rest.Substring(slashIdx); authority = rest.Substring(0, slashIdx); }

        var atIdx = authority.IndexOf('@');
        if (atIdx >= 0)
        {
            var userinfo = authority.Substring(0, atIdx);
            authority = authority.Substring(atIdx + 1);
            var colon = userinfo.IndexOf(':');
            if (colon >= 0) { username = userinfo.Substring(0, colon); password = userinfo.Substring(colon + 1); }
            else username = userinfo;
        }

        var portColon = authority.LastIndexOf(':');
        if (portColon >= 0) { host = authority.Substring(0, portColon); port = authority.Substring(portColon + 1); }
        else host = authority;

        return new JsonObject
        {
            ["Scheme"] = scheme,
            ["Host"] = host,
            ["Port"] = port,
            ["Path"] = path,
            ["Username"] = username,
            ["Password"] = password,
            ["Query Parameters"] = ParseQuery(query),
            ["Fragment"] = fragment,
        };
    }

    public static JsonObject ParseQuery(string? query)
    {
        var qp = new JsonObject();
        if (string.IsNullOrEmpty(query)) return qp;
        // a full URL or leading '?' is tolerated: keep only the query segment.
        var q = query;
        var qIdx = q.IndexOf('?');
        if (qIdx >= 0) q = q.Substring(qIdx + 1);
        var hashIdx = q.IndexOf('#');
        if (hashIdx >= 0) q = q.Substring(0, hashIdx);
        foreach (var pair in q.Split('&'))
        {
            if (pair.Length == 0) continue;
            var eq = pair.IndexOf('=');
            if (eq >= 0) qp[pair.Substring(0, eq)] = pair.Substring(eq + 1);
            else qp[pair] = "";
        }
        return qp;
    }
}
