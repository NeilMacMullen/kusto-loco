# 01-test-infra-packages: test-infra-packages

Update test-infrastructure packages: `Microsoft.NET.Test.Sdk` (18.9.0→18.11.0), `MSTest.TestFramework`/`MSTest.TestAdapter` (4.4.0→4.5.2), `xunit.runner.visualstudio` (3.1.5→4.0.1), `xunit.v3` (3.2.2→4.0.2), `coverlet.collector` (10.1.0→10.1.0 if changed), `AwesomeAssertions` (confirm current), `Moq` (4.20.72→4.21.0). No breaking API changes reported for these.

**Done when**: Directory.Packages.props updated, solution builds, all test projects build and tests pass.
