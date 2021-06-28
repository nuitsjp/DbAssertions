using System;

namespace DbAssertions
{
    /// <summary>
    /// アサーションエラーを表す例外
    /// </summary>
    public class DbAssertionsException : Exception
    {
        /// <summary>
        /// インスタンスを生成する
        /// </summary>
        /// <param name="message"></param>
        public DbAssertionsException(string message) : base(message){}
    }
}