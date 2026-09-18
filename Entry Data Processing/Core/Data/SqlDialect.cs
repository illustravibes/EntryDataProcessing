using System.Text.RegularExpressions;

namespace Entry_Data_Processing.Core.Data
{
    public static class SqlDialect
    {
        public static string Adapt(string sql, DatabaseProvider provider)
        {
            if (provider != DatabaseProvider.Access)
            {
                return sql;
            }

            var accessSql = Regex.Replace(
                sql,
                @"CONVERT\(([^()]+)\s+USING\s+utf8mb4\)",
                "$1",
                RegexOptions.IgnoreCase);

            accessSql = Regex.Replace(accessSql, @"\bCOALESCE\(([^,()]+),\s*'([^']*)'\)", "Nz($1, '$2')", RegexOptions.IgnoreCase);
            accessSql = Regex.Replace(
                accessSql,
                @"COALESCE\(SUM\(CASE\s+WHEN\s+(.+?)\s+THEN\s+1\s+ELSE\s+0\s+END\),\s*0\)",
                "Nz(Sum(IIf($1, 1, 0)), 0)",
                RegexOptions.IgnoreCase);
            accessSql = Regex.Replace(accessSql, @"`([^`]+)`", "[$1]");
            accessSql = Regex.Replace(accessSql, @"\b(FROM|JOIN)\s+user\b", "$1 [user]", RegexOptions.IgnoreCase);
            accessSql = ReplaceLimitWithTop(accessSql);

            return accessSql;
        }

        private static string ReplaceLimitWithTop(string sql)
        {
            var limitMatch = Regex.Match(sql, @"\bLIMIT\s+(\d+)\s*;?", RegexOptions.IgnoreCase);
            if (!limitMatch.Success)
            {
                return sql;
            }

            var limit = limitMatch.Groups[1].Value;
            var withoutLimit = sql.Remove(limitMatch.Index, limitMatch.Length);
            var selectMatch = Regex.Match(withoutLimit, @"\bSELECT\b", RegexOptions.IgnoreCase);
            if (!selectMatch.Success)
            {
                return withoutLimit;
            }

            return withoutLimit.Insert(selectMatch.Index + selectMatch.Length, $" TOP {limit}");
        }
    }
}
