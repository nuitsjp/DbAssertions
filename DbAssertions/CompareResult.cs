using System.Collections.Generic;
using System.Linq;

namespace DbAssertions
{
    /// <summary>
    /// 比較結果
    /// </summary>
    public class CompareResult
    {
        /// <summary>
        /// 不一致メッセージ
        /// </summary>
        private readonly List<string> _mismatchedMessages = new ();

        /// <summary>
        /// 全ての不一致メッセージを取得する
        /// </summary>
        public IEnumerable<string> MismatchedMessages => _mismatchedMessages;

        /// <summary>
        /// 不一致有無を取得する
        /// </summary>
        public bool HasMismatched => _mismatchedMessages.Any();

        /// <summary>
        /// 不一致メッセージを追加する
        /// </summary>
        /// <param name="message"></param>
        public void AddMismatchedMessage(string message) => _mismatchedMessages.Add(message);
    }
}