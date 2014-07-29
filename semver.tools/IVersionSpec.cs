
namespace semver.tools
{
    /// <summary>
    /// Represents a version specification, that is, a collection of one or more versions.
    /// </summary>
    public interface IVersionSpec
    {
        /// <summary>
        /// Lower bound of the version spec.
        /// </summary>
        SemanticVersion MinVersion { get; }

        /// <summary>
        /// True if minVersion is included in the version spec.
        /// False if only versions higher (but not equal) to minVersion are in the version spec.
        /// </summary>
        bool IsMinInclusive { get; }

        /// <summary>
        /// Upper bound of the version spec.
        /// </summary>
        SemanticVersion MaxVersion { get; }

        /// <summary>
        /// True if maxVersion is included in the version spec.
        /// False if only versions lower (but not equal) to maxVersion are in the version spec.
        /// </summary>
        bool IsMaxInclusive { get; }
    }
}
