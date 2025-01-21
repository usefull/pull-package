using NuGet.Common;
using System.Threading.Tasks;

namespace Usefull.PullPackage
{
    /// <summary>
    /// The default puller logger.
    /// </summary>
    /// <remarks>Mutes all log messages.</remarks>
    internal class DummyLogger : ILogger
    {
        /// <summary>
        /// Writes the log message.
        /// </summary>
        /// <param name="level">The log message level.</param>
        /// <param name="data">The log message.</param>
        public void Log(LogLevel level, string data) { }

        /// <summary>
        /// Writes the log message.
        /// </summary>
        /// <param name="message">The log message.</param>
        public void Log(ILogMessage message) { }

        /// <summary>
        /// Writes the log message.
        /// </summary>
        /// <param name="level">The log message level.</param>
        /// <param name="data">The log message.</param>
        /// <returns>A task that represents the asynchronous log operation.</returns>
        public Task LogAsync(LogLevel level, string data) => Task.CompletedTask;

        /// <summary>
        /// Writes the log message.
        /// </summary>
        /// <param name="message">The log message.</param>
        /// <returns>A task that represents the asynchronous log operation.</returns>
        public Task LogAsync(ILogMessage message) => Task.CompletedTask;

        /// <summary>
        /// Writes the debug log message.
        /// </summary>
        /// <param name="data">The log message.</param>
        public void LogDebug(string data) { }

        /// <summary>
        /// Writes the error log message.
        /// </summary>
        /// <param name="data">The log message.</param>
        public void LogError(string data) { }

        /// <summary>
        /// Writes the information log message.
        /// </summary>
        /// <param name="data">The log message.</param>
        public void LogInformation(string data) { }

        /// <summary>
        /// Writes the information summary log message.
        /// </summary>
        /// <param name="data">The log message.</param>
        public void LogInformationSummary(string data) { }

        /// <summary>
        /// Writes the minimal log message.
        /// </summary>
        /// <param name="data">The log message.</param>
        public void LogMinimal(string data) { }

        /// <summary>
        /// Writes the verbose log message.
        /// </summary>
        /// <param name="data">The log message.</param>
        public void LogVerbose(string data) { }

        /// <summary>
        /// Writes the warning log message.
        /// </summary>
        /// <param name="data">The log message.</param>
        public void LogWarning(string data) { }
    }
}