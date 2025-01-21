namespace Usefull.PullPackage
{
    /// <summary>
    /// The source configuration setup interface.
    /// </summary>
    public interface ISourceConfig : IPullerConfig
    {
        /// <summary>
        /// Appends the package mapping.
        /// </summary>
        /// <param name="pattern">The package mapping pattern.</param>
        /// <returns>A source configuration.</returns>
        ISourceConfig WithMapping(string pattern);
    }
}