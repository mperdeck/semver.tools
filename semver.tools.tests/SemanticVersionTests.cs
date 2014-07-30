using System;
using NUnit;
using NUnit.Framework;
using semver.tools;
using System.Collections.Generic;
using semver.tools;
using System.Globalization;

namespace semver.tools.tests
{
    [TestFixture]
    public class SemanticVersionTests
    {
        [TestCase("1.0.0", 1,0,0,0, "")]
        [TestCase("2.3-alpha", 2,3,0,0, "alpha")]
        [TestCase("3.4.0.3-RC-3", 3,4,0,3, "RC-3")]
        [TestCase("1.022-Beta", 1, 22, 0, 0, "Beta")]
        [TestCase("23.2.3-Alpha", 23, 2, 3, 0, "Alpha")]
        [TestCase("1.3.42.10133-PreRelease", 1, 3, 42, 10133, "PreRelease")]
        [TestCase("1.3.42.200930-RC-2", 1, 3, 42, 200930, "RC-2")]
        [TestCase("  1.022-Beta", 1, 22, 0, 0, "Beta")]
        [TestCase("23.2.3-Alpha  ", 23, 2, 3, 0, "Alpha")]
        [TestCase("    1.3.42.10133-PreRelease  ", 1, 3, 42, 10133, "PreRelease")]
        public void StringConstructorParsesValuesCorrectly(string version, int major, int minor, int build, int revision, string preReleaseName)
        {
            // Act
            SemanticVersion parsedSemanticVersion = SemanticVersion.ParseNuGet(version);
            SemanticVersion expectedSemanticVersion = new SemanticVersion(major, minor, build, revision, preReleaseName);

            // Assert
            Assert.AreEqual(parsedSemanticVersion, expectedSemanticVersion);
        }

        [Test]
        public void TryParseStrictParsesStrictVersion()
        {
            // Arrange
            var versionString = "1.3.2-CTP-2-Refresh-Alpha";

            // Act
            SemanticVersion parsedSemanticVersion;
            bool result = SemanticVersion.TryParse(versionString, out parsedSemanticVersion);

            SemanticVersion expectedSemanticVersion = new SemanticVersion(1, 3, 2, "CTP-2-Refresh-Alpha");


            // Assert
            Assert.True(result);
            Assert.AreEqual(parsedSemanticVersion, expectedSemanticVersion);
        }

        [Test]
        public void ParseThrowsIfStringIsNullOrEmpty()
        {
            Assert.Throws<ArgumentNullException>(delegate { SemanticVersion.Parse(null); } );
            Assert.Throws<ArgumentNullException>(delegate { SemanticVersion.Parse(String.Empty); } );
            Assert.Throws<ArgumentNullException>(delegate { SemanticVersion.ParseNuGet(null); });
            Assert.Throws<ArgumentNullException>(delegate { SemanticVersion.ParseNuGet(String.Empty); });
        }

        [TestCase("1")]
        [TestCase("1beta")]
        [TestCase("1.2Av^c")]
        [TestCase("1.2..")]
        [TestCase("1.2.3.4.5")]
        [TestCase("1.2.3.Beta")]
        [TestCase("1.2.3.4This version is full of awesomeness!!")]
        [TestCase("So.is.this")]
        [TestCase("1.34.2Alpha")]
        [TestCase("1.34.2Release Candidate")]
        [TestCase("1.4.7-")]
        public void ParseThrowsIfStringIsNotAValidSemVer(string versionString)
        {
            Assert.Throws<ArgumentException>(delegate { SemanticVersion.ParseNuGet(versionString); });
        }

        [TestCase("1.0", "1.0.1")]
        [TestCase("1.23", "1.231")]
        [TestCase("1.4.5.6", "1.45.6")]
        [TestCase("1.4.5.6", "1.4.5.60")]
        [TestCase("1.01", "1.10")]
        [TestCase("1.01-alpha", "1.10-beta")]
        [TestCase("1.01.0-RC-1", "1.10.0-rc-2")]
        [TestCase("1.01-RC-1", "1.01")]
        [TestCase("1.01", "1.2-preview")]
        public void SemVerLessThanAndGreaterThanOperatorsWorks(string versionA, string versionB)
        {
            // Arrange
            var itemA = SemanticVersion.ParseNuGet(versionA);
            var itemB = SemanticVersion.ParseNuGet(versionB);
            object objectB = itemB;

            // Act and Assert
            Assert.True(itemA < itemB);
            Assert.True(itemA <= itemB);
            Assert.True(itemB > itemA);
            Assert.True(itemB >= itemA);
            Assert.False(itemA.Equals(itemB));
            Assert.False(itemA.Equals(objectB));
        }

        public void EqualsReturnsFalseIfComparingANonSemVerType()
        {
            // Arrange
            var semVer = SemanticVersion.Parse("1.0.0");

            // Act and Assert
            object other = new object[] { 1 };
            Assert.False(semVer.Equals(other));

            other = new object[] { "1.0.0" };
            Assert.False(semVer.Equals(other));

            other = new object[] { new object[0] };
            Assert.False(semVer.Equals(other));
        }

        [Test]
        public void SemVerThrowsIfLeftHandExpressionForCompareOperatorsIsNull()
        {
            // Arrange
            SemanticVersion itemA = null;
            SemanticVersion itemB = new SemanticVersion(1, 0, 0);

            // Disable this warning since it complains on mono
#pragma warning disable 0219
            // Act and Assert
            Assert.Throws<ArgumentNullException>(delegate { bool val = itemA < itemB; });
            Assert.Throws<ArgumentNullException>(delegate { bool val = itemA <= itemB; });
            Assert.Throws<ArgumentNullException>(delegate { bool val = itemA > itemB; });
            Assert.Throws<ArgumentNullException>(delegate { bool val = itemA >= itemB; });
#pragma warning restore 0219

        }

        [TestCase("1.0", "1.0.0.0")]
        [TestCase("1.23.01", "1.23.1")]
        [TestCase("1.45.6", "1.45.6.0")]
        [TestCase("1.45.6-Alpha", "1.45.6-Alpha")]
        [TestCase("1.6.2-BeTa", "1.6.02-beta")]
        [TestCase("22.3.07     ", "22.3.07")]
        public void SemVerEqualsOperatorWorks(string versionA, string versionB)
        {
            // Arrange
            var itemA = SemanticVersion.ParseNuGet(versionA);
            var itemB = SemanticVersion.ParseNuGet(versionB);
            object objectB = itemB;

            // Act and Assert
            Assert.True(itemA == itemB);
            Assert.True(itemA.Equals(itemB));
            Assert.True(itemA.Equals(objectB));
            Assert.True(itemA <= itemB);
            Assert.True(itemB == itemA);
            Assert.True(itemB >= itemA);
        }

        [Test]
        public void SemVerEqualityComparisonsWorkForNullValues()
        {
            // Arrange
            SemanticVersion itemA = null;
            SemanticVersion itemB = null;

            // Act and Assert
            Assert.True(itemA == itemB);
            Assert.True(itemB == itemA);
            Assert.True(itemA <= itemB);
            Assert.True(itemB <= itemA);
            Assert.True(itemA >= itemB);
            Assert.True(itemB >= itemA);
        }

        [TestCase("1.0")]
        [TestCase("1.0.0")]
        [TestCase("1.0.0.0")]
        [TestCase("1.0-alpha")]
        [TestCase("1.0.0-b")]
        [TestCase("3.0.1.2")]
        [TestCase("2.1.4.3-pre-1")]
        public void ToStringReturnsOriginalValue(string version)
        {
            // Act
            SemanticVersion semVer = SemanticVersion.ParseNuGet(version);

            // Assert
            Assert.AreEqual(version, semVer.ToString());
        }

        [TestCase("2.7")]
        [TestCase("1.3.4.5")]
        [TestCase("1.3-alpha")]
        [TestCase("1.3 .4")]
        [TestCase("2.3.18.2-a")]
        public void TryParseStrictReturnsFalseIfVersionIsNotStrictSemVer(string version)
        {
            // Act 
            SemanticVersion semanticVersion;
            bool result = SemanticVersion.TryParse(version, out semanticVersion);

            // Assert
            Assert.False(result);
            Assert.Null(semanticVersion);
        }
    }
}
