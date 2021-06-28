namespace DbAssertions
{
    /// <summary>
    /// Databasesクラスに対する拡張メソッドクラス
    /// </summary>
    public static class DatabasesExtensions
    {
        /// <summary>
        /// DatabasesAssertionsを作成する
        /// </summary>
        /// <param name="databases"></param>
        /// <returns></returns>
        public static DatabaseAssertions Should(this Database databases) => new(databases);
    }
}