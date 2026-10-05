/*
 * File: ArchitectureTests.cs
 * Purpose: Enforces the clean-architecture dependency rule: the Domain and Application layers
 *          (where all business logic lives) never reference MongoDB, ASP.NET or Infrastructure.
 * Project: Smart Solar Microgrid Trading System - Web Service (SolarGrid API)
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

using System.Reflection;
using SolarGrid.Application.Services;
using SolarGrid.Domain.Rules;

namespace SolarGrid.Tests
{
    public class ArchitectureTests
    {
        private static readonly string[] ForbiddenForCore =
        {
            "MongoDB.Driver", "MongoDB.Bson", "Microsoft.AspNetCore", "SolarGrid.Infrastructure", "SolarGrid.Api"
        };

        // Checks the Domain project references no other SolarGrid, MongoDB or ASP.NET assembly.
        [Fact]
        public void Domain_DependsOnNothingButTheFramework()
        {
            var references = Names(typeof(ReservationPolicy).Assembly);
            Assert.DoesNotContain(references, r => r.StartsWith("SolarGrid.") || ForbiddenForCore.Any(r.StartsWith));
        }

        // Checks the Application project references Domain but not MongoDB, ASP.NET or Infrastructure.
        [Fact]
        public void Application_DependsOnlyOnDomain()
        {
            var references = Names(typeof(ReservationService).Assembly);
            Assert.Contains("SolarGrid.Domain", references);
            Assert.DoesNotContain(references, r => ForbiddenForCore.Any(r.StartsWith));
        }

        // Lists the names of the assemblies an assembly references.
        private static List<string> Names(Assembly assembly) =>
            assembly.GetReferencedAssemblies().Select(a => a.Name ?? string.Empty).ToList();
    }
}
