using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace semver.tools
{
    /// <summary>
    /// Represents a Semantic Version, as described in http://semver.org/
    /// </summary>
    [Serializable]
    [TypeConverter(typeof(SemanticVersionTypeConverter))]
    public partial class SemanticVersion : IComparable, IComparable<SemanticVersion>, IEquatable<SemanticVersion>
    {
        private const RegexOptions _flags = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture;
        private static readonly Regex _strictSemanticVersionRegex = new Regex(@"^(?<Version>\d+(\.\d+){2})(?<Release>-[a-z][0-9a-z-]*)?$", _flags);

        // The original string represents the original form in which the version is represented to be used when printing.
        private readonly string _originalString;

        /// <summary>
        /// Constructs a SemanticVersion out of a Major, Minor and Patch number.
        /// </summary>
        public SemanticVersion(int major, int minor, int patch)
            : this(new Version(major, minor, patch))
        {
        }

        /// <summary>
        /// Constructs a SemanticVersion out of a Major, Minor and Patch number, and a pre release name.
        /// </summary>
        public SemanticVersion(int major, int minor, int patch, string preReleaseName)
            : this(new Version(major, minor, patch), preReleaseName)
        {
        }

        private SemanticVersion(Version version)
            : this(version, String.Empty)
        {
        }

        private SemanticVersion(Version version, string preReleaseName)
            : this(version, preReleaseName, null)
        {
        }

        private SemanticVersion(Version version, string preReleaseName, string originalString)
        {
            if (version == null)
            {
                throw new ArgumentNullException("version");
            }
            Version = NormalizeVersionValue(version);
            PreReleaseName = preReleaseName ?? String.Empty;
            _originalString = String.IsNullOrEmpty(originalString) ? version.ToString() + (!String.IsNullOrEmpty(preReleaseName) ? '-' + preReleaseName : null) : originalString;
        }

        private SemanticVersion(SemanticVersion semVer)
        {
            _originalString = semVer.ToString();
            Version = semVer.Version;
            PreReleaseName = semVer.PreReleaseName;
        }

        /// <summary>
        /// Gets the normalized version portion.
        /// </summary>
        internal Version Version
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the optional pre release name.
        /// </summary>
        internal string PreReleaseName
        {
            get;
            private set;
        }

        /// <summary>
        /// Parses a string with a semantic version and returns the SemanticVersion.
        /// Exception if the string does not contain a valid semantic version.
        /// </summary>
        public static SemanticVersion Parse(string s)
        {
            if (String.IsNullOrEmpty(s))
            {
                throw new ArgumentNullException("version");
            }

            SemanticVersion semVer;
            if (!TryParse(s, out semVer))
            {
                throw new ArgumentException(String.Format("'{0}' is not a valid version string", s), "version");
            }
            return semVer;
        }

        /// <summary>
        /// Attempts to parse a string with a semantic version.
        /// </summary>
        /// <returns>
        /// The SemanticVersion, or null if the input string does not contain a valid semantic version.
        /// </returns>
        public static SemanticVersion ParseOptionalVersion(string s)
        {
            SemanticVersion semVer;
            TryParse(s, out semVer);
            return semVer;
        }

        /// <summary>
        /// Attempts to parse a string with a semantic version.
        /// </summary>
        /// <param name="s">
        /// String to parse.
        /// </param>
        /// <param name="result">
        /// The SemanticVersion, or null if the input string does not contain a valid semantic version.
        /// </param>
        /// <returns>
        /// True if the input string contains a valid semantic version. False otherwise.
        /// </returns>
        public static bool TryParse(string s, out SemanticVersion result)
        {
            return TryParseInternal(s, _strictSemanticVersionRegex, out result);
        }

        private static bool TryParseInternal(string s, Regex regex, out SemanticVersion result)
        {
            result = null;
            if (String.IsNullOrEmpty(s))
            {
                return false;
            }

            var match = regex.Match(s.Trim());
            Version versionValue;
            if (!match.Success || !Version.TryParse(match.Groups["Version"].Value, out versionValue))
            {
                return false;
            }

            result = new SemanticVersion(NormalizeVersionValue(versionValue), match.Groups["Release"].Value.TrimStart('-'), s.Replace(" ", ""));
            return true;
        }

        /// <summary>
        /// Compares two SemanticVersions. Implements IComparable.CompareTo.
        /// </summary>
        /// <param name="obj">
        /// Another SemanticVersion to compare against "this" SemanticVersion.
        /// Exception if it is not a SemanticVersion.
        /// </param>
        /// <returns>
        /// less than 0: This instance precedes obj in the sort order.
        /// equals 0: Both instances are equal.
        /// greater than 0: The other instance precedes obj in the sort order.
        /// </returns>
        public int CompareTo(object obj)
        {
            if (Object.ReferenceEquals(obj, null))
            {
                return 1;
            }
            SemanticVersion other = obj as SemanticVersion;
            if (other == null)
            {
                throw new ArgumentException("Type to compare must be an instance of SemanticVersion", "obj");
            }
            return CompareTo(other);
        }

        /// <summary>
        /// Compares two SemanticVersions. Implements IComparable&lt;SemanticVersion&gt;.CompareTo.
        /// </summary>
        /// <param name="other">
        /// Another SemanticVersion to compare against "this" SemanticVersion.
        /// </param>
        /// <returns>
        /// less than 0: This instance precedes obj in the sort order.
        /// equals 0: Both instances are equal.
        /// greater than 0: The other instance precedes obj in the sort order.
        /// </returns>
        public int CompareTo(SemanticVersion other)
        {
            if (Object.ReferenceEquals(other, null))
            {
                return 1;
            }

            int result = Version.CompareTo(other.Version);

            if (result != 0)
            {
                return result;
            }

            bool empty = String.IsNullOrEmpty(PreReleaseName);
            bool otherEmpty = String.IsNullOrEmpty(other.PreReleaseName);
            if (empty && otherEmpty)
            {
                return 0;
            }
            else if (empty)
            {
                return 1;
            }
            else if (otherEmpty)
            {
                return -1;
            }
            return StringComparer.OrdinalIgnoreCase.Compare(PreReleaseName, other.PreReleaseName);
        }

        /// <summary>
        /// Evalutes to true if version1 equals version2.
        /// </summary>
        public static bool operator ==(SemanticVersion version1, SemanticVersion version2)
        {
            if (Object.ReferenceEquals(version1, null))
            {
                return Object.ReferenceEquals(version2, null);
            }
            return version1.Equals(version2);
        }

        /// <summary>
        /// Evalutes to true if version1 does not equal version2.
        /// </summary>
        public static bool operator !=(SemanticVersion version1, SemanticVersion version2)
        {
            return !(version1 == version2);
        }

        /// <summary>
        /// Evalutes to true if version1 smaller than version2.
        /// </summary>
        public static bool operator <(SemanticVersion version1, SemanticVersion version2)
        {
            if (version1 == null)
            {
                throw new ArgumentNullException("version1");
            }
            return version1.CompareTo(version2) < 0;
        }

        /// <summary>
        /// Evalutes to true if version1 smaller or equal to version2.
        /// </summary>
        public static bool operator <=(SemanticVersion version1, SemanticVersion version2)
        {
            return (version1 == version2) || (version1 < version2);
        }

        /// <summary>
        /// Evalutes to true if version1 greater than version2.
        /// </summary>
        public static bool operator >(SemanticVersion version1, SemanticVersion version2)
        {
            if (version1 == null)
            {
                throw new ArgumentNullException("version1");
            }
            return version2 < version1;
        }

        /// <summary>
        /// Evalutes to true if version1 greater or equal to version2.
        /// </summary>
        public static bool operator >=(SemanticVersion version1, SemanticVersion version2)
        {
            return (version1 == version2) || (version1 > version2);
        }

        public override string ToString()
        {
            return _originalString;
        }

        /// <summary>
        /// Implements IEquatable&lt;SemanticVersion&gt;.Equals. Returns true if the passed in SemanticVersion is equal value wise to this SemanticVersion.
        /// </summary>
        public bool Equals(SemanticVersion other)
        {
            return !Object.ReferenceEquals(null, other) &&
                   Version.Equals(other.Version) &&
                   PreReleaseName.Equals(other.PreReleaseName, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Overrides Object.Equal. Returns true if the passed in obj is a SemanticVersion and equal (value wise) to this SemanticVersion.
        /// </summary>
        public override bool Equals(object obj)
        {
            SemanticVersion semVer = obj as SemanticVersion;
            return !Object.ReferenceEquals(null, semVer) && Equals(semVer);
        }

        /// <summary>
        /// Overrides Object.GetHashCode. 
        /// </summary>
        public override int GetHashCode()
        {
            int hashCode = Version.GetHashCode();
            if (PreReleaseName != null)
            {
                hashCode = hashCode * 4567 + PreReleaseName.GetHashCode();
            }

            return hashCode;
        }

        private static Version NormalizeVersionValue(Version version)
        {
            return new Version(version.Major,
                               version.Minor,
                               Math.Max(version.Build, 0),
                               Math.Max(version.Revision, 0));
        }

    }
}
