global using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PerformanceTracing.Tests;

[TestClass]
public sealed class TestInit
{
	[AssemblyInitialize]
	public static void ClassInitialize( TestContext context )
	{
		Sandbox.Application.InitUnitTest();
	}
}
