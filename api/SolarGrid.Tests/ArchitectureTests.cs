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

        [Fact]
        public void Domain_DependsOnNothingButTheFramework()
        {
            var references = Names(typeof(ReservationPolicy).Assembly);
            Assert.DoesNotContain(references, r => r.StartsWith("SolarGrid.") || ForbiddenForCore.Any(r.StartsWith));
        }

        [Fact]
        public void Application_DependsOnlyOnDomain()
        {
            var references = Names(typeof(ReservationService).Assembly);
            Assert.Contains("SolarGrid.Domain", references);
            Assert.DoesNotContain(references, r => ForbiddenForCore.Any(r.StartsWith));
        }

        private static List<string> Names(Assembly assembly) =>
            assembly.GetReferencedAssemblies().Select(a => a.Name ?? string.Empty).ToList();
    }
}
