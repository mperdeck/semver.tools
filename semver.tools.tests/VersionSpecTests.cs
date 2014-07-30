using System;
using System.Collections.Generic;
using NUnit;
using NUnit.Framework;
using semver.tools;

namespace semver.tools.tests
{
    public class VersionSpecTests
    {
        [TestCase("(,)")]
        [TestCase("[,]")]
        [TestCase("[,)")]
        [TestCase("(,]")]
        [TestCase("(,1.3..2]")]
        [TestCase("(1.2.3.4.5,1.2]")]
        public void ParseVersionThrowsIfVersionSpecInvalid(string versionSpec)
        {
            // Assert
            Assert.Throws<ArgumentException>(delegate { VersionSpec.ParseNuGet(versionSpec); });
        }

        [Test]
        public void ParseVersionParsesTokensVersionsCorrectly()
        {
            foreach(object[] tuple in VersionSpecData)
            {
                string versionString = tuple[0].ToString();
                VersionSpec versionSpec = (VersionSpec)tuple[1];

                // Act
                var actual = VersionSpec.ParseNuGet(versionString);

                // Assert
                Assert.AreEqual(versionSpec.IsMinInclusive, actual.IsMinInclusive);
                Assert.AreEqual(versionSpec.IsMaxInclusive, actual.IsMaxInclusive);
                Assert.AreEqual(versionSpec.MinVersion, actual.MinVersion);
                Assert.AreEqual(versionSpec.MaxVersion, actual.MaxVersion);
            }
        }

        public static IEnumerable<object[]> VersionSpecData
        {
            get
            {
                yield return new object[] { "(1.2.3.4, 3.2)", new VersionSpec(minVersion: SemanticVersion.ParseNuGet("1.2.3.4"), maxVersion: SemanticVersion.ParseNuGet("3.2"), isMinInclusive: false, isMaxInclusive: false ) };
                yield return new object[] { "(1.2.3.4, 3.2]", new VersionSpec ( minVersion: SemanticVersion.ParseNuGet("1.2.3.4"), maxVersion: SemanticVersion.ParseNuGet("3.2"), isMinInclusive: false, isMaxInclusive: true ) };
                yield return new object[] { "[1.2, 3.2.5)", new VersionSpec ( minVersion: SemanticVersion.ParseNuGet("1.2"), maxVersion: SemanticVersion.ParseNuGet("3.2.5"), isMinInclusive: true, isMaxInclusive: false ) };
                yield return new object[] { "[2.3.7, 3.2.4.5]", new VersionSpec ( minVersion: SemanticVersion.ParseNuGet("2.3.7"), maxVersion: SemanticVersion.ParseNuGet("3.2.4.5"), isMinInclusive: true, isMaxInclusive: true ) };
                yield return new object[] { "(, 3.2.4.5]", new VersionSpec ( minVersion: null, maxVersion: SemanticVersion.ParseNuGet("3.2.4.5"), isMinInclusive: false, isMaxInclusive: true ) };
                yield return new object[] { "(1.6, ]", new VersionSpec ( minVersion: SemanticVersion.ParseNuGet("1.6"), maxVersion: null, isMinInclusive: false, isMaxInclusive: true ) };
                yield return new object[] { "(1.6)", new VersionSpec ( minVersion: SemanticVersion.ParseNuGet("1.6"), maxVersion: SemanticVersion.ParseNuGet("1.6"), isMinInclusive: false, isMaxInclusive: false ) };
                yield return new object[] { "[2.7]", new VersionSpec ( minVersion: SemanticVersion.ParseNuGet("2.7"), maxVersion: SemanticVersion.ParseNuGet("2.7"), isMinInclusive: true, isMaxInclusive: true ) };
            }
        }
    }
}
