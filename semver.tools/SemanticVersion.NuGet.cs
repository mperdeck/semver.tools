using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace semver.tools
{
    public partial class SemanticVersion : IComparable, IComparable<SemanticVersion>, IEquatable<SemanticVersion>
    {
        private static readonly Regex _semanticVersionRegex = new Regex(@"^(?<Version>\d+(\s*\.\s*\d+){0,3})(?<Release>-[a-z][0-9a-z-]*)?$", _flags);

        /// <summary>
        /// Constructs a SemanticVersion out of a Major, Minor and Build version and a Revision.
        /// Use this when dealing with Microsoft style versions rather than true semantic versions.
        /// </summary>
        public SemanticVersion(int major, int minor, int build, int revision)
            : this(new Version(major, minor, build, revision))
        {
        }

        /// <summary>
        /// Constructs a SemanticVersion out of a Major, Minor and Build version and a Revision and a pre release name.
        /// Use this when dealing with Microsoft style versions rather than true semantic versions.
        /// </summary>
        public SemanticVersion(int major, int minor, int build, int revision, string preReleaseName)
            : this(new Version(major, minor, build, revision), preReleaseName)
        {
        }

        /// <summary>
        /// Tries to parse a string using loose semantic versioning rules that allows a Microsoft or NuGet style version with 
        /// 2-4 version components followed by an optional pre release name.
        /// </summary>
        public static bool TryParseNuGet(string s, out SemanticVersion result)
        {
            return TryParseInternal(s, _semanticVersionRegex, out result);
        }

        /// <summary>
        /// Parses a string using loose semantic versioning rules that allows a Microsoft or NuGet style version with 
        /// 2-4 version components followed by an optional pre release name.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// Throws an exception if the string does not contain a valid semantic version, or if the input string is null.
        /// </exception>
        public static SemanticVersion ParseNuGet(string s)
        {
            if (String.IsNullOrEmpty(s))
            {
                throw new ArgumentNullException("version");
            }

            SemanticVersion semVer;
            if (!TryParseNuGet(s, out semVer))
            {
                throw new ArgumentException(String.Format("'{0}' is not a valid version string", s), "version");
            }
            return semVer;
        }

    }
}
