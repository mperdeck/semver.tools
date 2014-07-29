using System.Globalization;
using System.Text;
using System;

namespace semver.tools
{
    /// <summary>
    /// Represents a version specification, that is, a collection of one or more versions.
    /// </summary>
    public partial class VersionSpec : IVersionSpec
    {
        private const string LessThanOrEqualTo = "\u2264";
        private const string GreaterThanOrEqualTo = "\u2265";

        /// <summary>
        /// Parameterless constructor.
        /// </summary>
        public VersionSpec()
        {
        }

        /// <summary>
        /// Constructs a version spec that contains just one version.
        /// </summary>
        public VersionSpec(SemanticVersion version)
        {
            IsMinInclusive = true;
            IsMaxInclusive = true;
            MinVersion = version;
            MaxVersion = version;
        }

        /// <summary>
        /// Constructs a version spec.
        /// </summary>
        /// <param name="minVersion">Lower bound of the version spec.</param>
        /// <param name="isMinInclusive">
        /// True if minVersion is included in the version spec.
        /// False if only versions higher (but not equal) to minVersion are in the version spec.
        /// </param>
        /// <param name="maxVersion">Upper bound of the version spec</param>
        /// <param name="isMaxInclusive">
        /// True if maxVersion is included in the version spec.
        /// False if only versions lower (but not equal) to maxVersion are in the version spec.
        /// </param>
        public VersionSpec(SemanticVersion minVersion, bool isMinInclusive, SemanticVersion maxVersion, bool isMaxInclusive)
        {
            IsMinInclusive = isMinInclusive;
            IsMaxInclusive = isMaxInclusive;
            MinVersion = minVersion;
            MaxVersion = maxVersion;
        }

        public SemanticVersion MinVersion { get; private set; }
        public bool IsMinInclusive { get; private set; }
        public SemanticVersion MaxVersion { get; private set; }
        public bool IsMaxInclusive { get; private set; }

        /// <summary>
        /// Returns true if the specified version is within the version spec.
        /// </summary>
        public bool Satisfies(SemanticVersion version)
        {
            return ToDelegate<SemanticVersion>(v => v)(version);
        }

        /// <summary>
        /// Returns a string with a nice human readable bit of text describing the version spec.
        /// </summary>
        public string PrettyPrint()
        {
            if (this.MinVersion != null && this.IsMinInclusive && this.MaxVersion == null && !this.IsMaxInclusive)
            {
                return String.Format(CultureInfo.InvariantCulture, "({0} {1})", GreaterThanOrEqualTo, this.MinVersion);
            }

            if (this.MinVersion != null && this.MaxVersion != null && this.MinVersion == this.MaxVersion && this.IsMinInclusive && this.IsMaxInclusive)
            {
                return String.Format(CultureInfo.InvariantCulture, "(= {0})", this.MinVersion);
            }

            var versionBuilder = new StringBuilder();
            if (this.MinVersion != null)
            {
                if (this.IsMinInclusive)
                {
                    versionBuilder.AppendFormat(CultureInfo.InvariantCulture, "({0} ", GreaterThanOrEqualTo);
                }
                else
                {
                    versionBuilder.Append("(> ");
                }
                versionBuilder.Append(this.MinVersion);
            }

            if (this.MaxVersion != null)
            {
                if (versionBuilder.Length == 0)
                {
                    versionBuilder.Append("(");
                }
                else
                {
                    versionBuilder.Append(" && ");
                }

                if (this.IsMaxInclusive)
                {
                    versionBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0} ", LessThanOrEqualTo);
                }
                else
                {
                    versionBuilder.Append("< ");
                }
                versionBuilder.Append(this.MaxVersion);
            }

            if (versionBuilder.Length > 0)
            {
                versionBuilder.Append(")");
            }

            return versionBuilder.ToString();
        }

        private Func<T, bool> ToDelegate<T>(Func<T, SemanticVersion> extractor)
        {
            if (extractor == null)
            {
                throw new ArgumentNullException("extractor");
            }

            return p =>
            {
                SemanticVersion version = extractor(p);
                bool condition = true;
                if (this.MinVersion != null)
                {
                    if (this.IsMinInclusive)
                    {
                        condition = condition && version >= this.MinVersion;
                    }
                    else
                    {
                        condition = condition && version > this.MinVersion;
                    }
                }

                if (this.MaxVersion != null)
                {
                    if (this.IsMaxInclusive)
                    {
                        condition = condition && version <= this.MaxVersion;
                    }
                    else
                    {
                        condition = condition && version < this.MaxVersion;
                    }
                }

                return condition;
            };
        }


    }
}