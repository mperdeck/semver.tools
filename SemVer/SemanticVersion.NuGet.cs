using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SemVer
{
    public partial class SemanticVersion : IComparable, IComparable<SemanticVersion>, IEquatable<SemanticVersion>
    {
        private static readonly Regex _semanticVersionRegex = new Regex(@"^(?<Version>\d+(\s*\.\s*\d+){0,3})(?<Release>-[a-z][0-9a-z-]*)?$", _flags);

        /// <summary>
        /// Constructs a SemanticVersion out of a Major, Minor and Build version and a Revision.
        /// Use this when dealing with Microsoft style versions rather than true semantic versions.
        /// </summary>
        /// <param name="major"></param>
        /// <param name="minor"></param>
        /// <param name="build"></param>
        /// <param name="revision"></param>
        public SemanticVersion(int major, int minor, int build, int revision)
            : this(new Version(major, minor, build, revision))
        {
        }

        /// <summary>
        /// Parses a string using loose semantic versioning rules that allows a Microsoft or NuGet style version with 
        /// 2-4 version components followed by an optional pre release name.
        /// </summary>
        public static bool TryParseNuGet(string s, out SemanticVersion result)
        {
            return TryParseInternal(s, _semanticVersionRegex, out result);
        }

    }
}
