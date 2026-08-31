using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using KustoLoco.Core;

namespace BasicTests;

/// <summary>
/// The engine's built-in <c>externaldata</c> fetcher. Every test drives a fake transport, so the suite makes no
/// network calls: what is under test is the resolver's POLICY (scheme, address, redirect, size) and its parsing,
/// not connectivity.
/// </summary>
[TestClass]
public class HttpExternalDataResolverTests
{
    // A transport that answers from a script instead of the network.
    private sealed class FakeHandler : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> _responses;
        public List<Uri> Requested { get; } = new();

        public FakeHandler(params HttpResponseMessage[] responses) => _responses = new Queue<HttpResponseMessage>(responses);

        // ResolveRows is synchronous, so the resolver uses HttpClient.Send — the fake must answer that path too.
        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken ct)
        {
            Requested.Add(request.RequestUri!);
            var response = _responses.Dequeue();
            response.RequestMessage = request;
            return response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) =>
            Task.FromResult(Send(request, ct));
    }

    private static HttpResponseMessage Ok(string body) =>
        new(HttpStatusCode.OK) { Content = new StringContent(body, Encoding.UTF8) };

    private static HttpResponseMessage Redirect(string to)
    {
        var response = new HttpResponseMessage(HttpStatusCode.Found);
        response.Headers.Location = new Uri(to);
        return response;
    }

    [TestMethod]
    public void FetchesAndParsesCsv()
    {
        using var resolver = new HttpExternalDataResolver(handler: new FakeHandler(Ok("alice,30\nbob,40\n")));
        var rows = resolver.ResolveRows("https://example.com/data.csv", "csv");
        rows.Should().HaveCount(2);
        rows[1][0].Should().Be("bob");
        rows[1][1].Should().Be("40");
    }

    [TestMethod]
    public void HonoursTheDeclaredFormat()
    {
        using var resolver = new HttpExternalDataResolver(handler: new FakeHandler(Ok("alice\t30\n")));
        var rows = resolver.ResolveRows("https://example.com/data.tsv", "tsv");
        rows[0][1].Should().Be("30"); // split on TAB, not comma
    }

    [TestMethod]
    public void RefusesPlainHttp()
    {
        using var resolver = new HttpExternalDataResolver(handler: new FakeHandler());
        var act = () => resolver.ResolveRows("http://example.com/data.csv", "csv");
        act.Should().Throw<InvalidOperationException>().WithMessage("*https*");
    }

    [TestMethod]
    public void RefusesLoopbackAndPrivateAddresses()
    {
        using var resolver = new HttpExternalDataResolver(handler: new FakeHandler());
        foreach (var host in new[] { "127.0.0.1", "10.0.0.5", "192.168.1.10", "172.16.0.3", "169.254.169.254" })
        {
            var act = () => resolver.ResolveRows($"https://{host}/data.csv", "csv");
            act.Should().Throw<InvalidOperationException>().WithMessage("*non-public*", $"{host} must be refused");
        }
    }

    [TestMethod]
    public void EnforcesAnAllowList()
    {
        using var resolver = new HttpExternalDataResolver(
            allowedHosts: new[] { "trusted.example" }, handler: new FakeHandler(Ok("a\n")));
        var act = () => resolver.ResolveRows("https://other.example/data.csv", "csv");
        act.Should().Throw<InvalidOperationException>().WithMessage("*allow-list*");
    }

    [TestMethod]
    public void RevalidatesEveryRedirectHop()
    {
        // The SSRF case that a naive fetcher misses: a public URL that redirects to an internal address.
        var handler = new FakeHandler(Redirect("https://169.254.169.254/latest/meta-data"));
        using var resolver = new HttpExternalDataResolver(handler: handler);
        var act = () => resolver.ResolveRows("https://example.com/data.csv", "csv");
        act.Should().Throw<InvalidOperationException>().WithMessage("*non-public*");
    }

    [TestMethod]
    public void EnforcesTheByteCap()
    {
        using var resolver = new HttpExternalDataResolver(maxBytes: 8, handler: new FakeHandler(Ok(new string('x', 100))));
        var act = () => resolver.ResolveRows("https://example.com/data.csv", "csv");
        act.Should().Throw<InvalidOperationException>().WithMessage("*limit*");
    }

    [TestMethod]
    public void ReportsHttpFailuresLoudly()
    {
        // A broken feed must fail the query, never look like a clean no-match.
        using var resolver = new HttpExternalDataResolver(
            handler: new FakeHandler(new HttpResponseMessage(HttpStatusCode.NotFound)));
        var act = () => resolver.ResolveRows("https://example.com/data.csv", "csv");
        act.Should().Throw<InvalidOperationException>().WithMessage("*404*");
    }

    [TestMethod]
    public void StripsAByteOrderMark()
    {
        // Published CSV exports often carry a BOM; a retained one would corrupt the first cell of the first row.
        using var resolver = new HttpExternalDataResolver(handler: new FakeHandler(Ok("﻿alice,30\n")));
        var rows = resolver.ResolveRows("https://example.com/data.csv", "csv");
        rows[0][0].Should().Be("alice");
    }

    [TestMethod]
    public void DecompressesGzipContent()
    {
        using var memory = new MemoryStream();
        using (var gz = new GZipStream(memory, CompressionLevel.Optimal, leaveOpen: true))
        using (var writer = new StreamWriter(gz, Encoding.UTF8))
            writer.Write("alice,30\n");
        var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(memory.ToArray()) };
        response.Content.Headers.ContentEncoding.Add("gzip");

        using var resolver = new HttpExternalDataResolver(handler: new FakeHandler(response));
        var rows = resolver.ResolveRows("https://example.com/data.csv.gz", "csv");
        rows[0][0].Should().Be("alice");
    }
}
