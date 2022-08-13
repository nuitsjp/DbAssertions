using System.Collections.Generic;
using System.Linq;

namespace DbAssertions
{
    /// <summary>
    /// Comparison Results
    /// </summary>
    public class CompareResult
    {
        /// <summary>
        /// Mismatched message
        /// </summary>
        private readonly List<string> _mismatchedMessages = new ();

        /// <summary>
        /// Get all mismatched messages.
        /// </summary>
        public IEnumerable<string> MismatchedMessages => _mismatchedMessages;

        /// <summary>
        /// Get existence of a mismatch.
        /// </summary>
        public bool HasMismatched => _mismatchedMessages.Any();

        /// <summary>
        /// Add a mismatch message.
        /// </summary>
        /// <param name="message"></param>
        public void AddMismatchedMessage(string message) => _mismatchedMessages.Add(message);
    }
}