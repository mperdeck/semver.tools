using System.Globalization;
using System.Text;
using System;
using System.Linq;

namespace semver.tools
{
    public partial class VersionSpec : IVersionSpec
    {
        /// <summary>
        /// Returns a string representing the version spec in NuGet format.
        /// See http://docs.nuget.org/docs/reference/versioning
        /// </summary>
        public string ToStringNuGet()
        {
            if (MinVersion != null && IsMinInclusive && MaxVersion == null && !IsMaxInclusive)
            {
                return MinVersion.ToString();
            }

            if (MinVersion != null && MaxVersion != null && MinVersion == MaxVersion && IsMinInclusive && IsMaxInclusive)
            {
                return "[" + MinVersion + "]";
            }

            var versionBuilder = new StringBuilder();
            versionBuilder.Append(IsMinInclusive ? '[' : '(');
            versionBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}, {1}", MinVersion, MaxVersion);
            versionBuilder.Append(IsMaxInclusive ? ']' : ')');

            return versionBuilder.ToString();
        }

        /// <summary>
        /// Parses a string with a version spec in NuGet format.
        /// See http://docs.nuget.org/docs/reference/versioning
        /// </summary>
        /// <exception cref="ArgumentException">
        /// Throws an exception if the string does not contain a valid version spec in NuGet format.
        /// </exception>
        /// <param name="s">
        /// String to parse.
        /// </param>
        /// <returns>
        /// The version spec.
        /// </returns>
        public static IVersionSpec ParseNuGet(string s)
        {
            IVersionSpec versionInfo;
            if (!TryParseNuGet(s, out versionInfo))
            {
                throw new ArgumentException(
                    String.Format("'{0}' is not a valid version NuGet version spec string.", s));
            }

            return versionInfo;
        }

        /// <summary>
        /// Attempts to parse a string with a version spec in NuGet format.
        /// See http://docs.nuget.org/docs/reference/versioning
        /// </summary>
        /// <param name="s">
        /// String to parse.
        /// </param>
        /// <param name="result">
        /// The version spec, or null if the input string does not contain a valid NuGet version spec.
        /// </param>
        /// <returns>
        /// True if the input string contains a valid version spec. False otherwise.
        /// </returns>
        public static bool TryParseNuGet(string s, out IVersionSpec result)
        {
            if (s == null)
            {
                throw new ArgumentNullException("value");
            }

            var versionSpec = new VersionSpec();
            s = s.Trim();

            // First, try to parse it as a plain version string
            SemanticVersion version;
            if (SemanticVersion.TryParseNuGet(s, out version))
            {
                // A plain version is treated as an inclusive minimum range
                result = new VersionSpec
                {
                    MinVersion = version,
                    IsMinInclusive = true
                };

                return true;
            }

            // It's not a plain version, so it must be using the bracket arithmetic range syntax

            result = null;

            // Fail early if the string is too short to be valid
            if (s.Length < 3)
            {
                return false;
            }

            // The first character must be [ ot (
            if (s.StartsWith("["))
            {
                versionSpec.IsMinInclusive = true;
            }
            else if (s.StartsWith("("))
            {
                versionSpec.IsMinInclusive = false;
            }
            else
            {
                return false;
            }

            // The last character must be ] ot )
            if (s.EndsWith("]"))
            {
                    versionSpec.IsMaxInclusive = true;
            } 
            else if (s.EndsWith(")"))
            {
                    versionSpec.IsMaxInclusive = false;
            }
            else
            {
                    return false;
            }

            // Get rid of the two brackets
            s = s.Substring(1, s.Length - 2);

            // Split by comma, and make sure we don't get more than two pieces
            string[] parts = s.Split(',');
            if (parts.Length > 2)
            {
                return false;
            }
            else if (parts.All(p=>String.IsNullOrEmpty(p)))
            {
                // If all parts are empty, then neither of upper or lower bounds were specified. Version spec is of the format (,]
                return false;
            }

            // If there is only one piece, we use it for both min and max
            string minVersionString = parts[0];
            string maxVersionString = (parts.Length == 2) ? parts[1] : parts[0];

            // Only parse the min version if it's non-empty
            if (!String.IsNullOrWhiteSpace(minVersionString))
            {
                if (!SemanticVersion.TryParseNuGet(minVersionString, out version))
                {
                    return false;
                }
                versionSpec.MinVersion = version;
            }

            // Same deal for max
            if (!String.IsNullOrWhiteSpace(maxVersionString))
            {
                if (!SemanticVersion.TryParseNuGet(maxVersionString, out version))
                {
                    return false;
                }
                versionSpec.MaxVersion = version;
            }

            // Successful parse!
            result = versionSpec;
            return true;
        }
    }
}
