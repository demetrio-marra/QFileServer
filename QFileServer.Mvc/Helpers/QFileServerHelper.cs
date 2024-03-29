﻿using System.Web;

namespace QFileServer.Mvc.Helpers
{
    public class QFileServerHelper
    {
        public static string FormatBytes(long bytes)
        {
            const long KB = 1024,
                        MB = KB * 1024,
                        GB = MB * 1024,
                        TB = GB * 1024L,
                        PB = TB * 1024L,
                        EB = PB * 1024L;
            if (bytes < KB) return bytes.ToString("N0") + " Bytes";
            if (bytes < MB) return decimal.Divide(bytes, KB).ToString("N") + " KB";
            if (bytes < GB) return decimal.Divide(bytes, MB).ToString("N") + " MB";
            if (bytes < TB) return decimal.Divide(bytes, GB).ToString("N") + " GB";
            if (bytes < PB) return decimal.Divide(bytes, TB).ToString("N") + " TB";
            if (bytes < EB) return decimal.Divide(bytes, PB).ToString("N") + " PB";
            return decimal.Divide(bytes, EB).ToString("N") + " EB";
        }

        public static int CalcAvailablePages(long totalItems, int pageSize)
        {
            if (pageSize == 0)
                return 0;
            return totalItems < pageSize ? 1 : Convert.ToInt32(Math.Ceiling((decimal)totalItems / (decimal)pageSize));
        }

        public static string BuildODataQueryString(int pageSize, int pageNumber, string orderByColumn, bool orderByAsc,
            string? filterColumn, string? filterSearchText)
        {
            var topClause = $"$top={pageSize}";
            var skipClause = $"$skip={pageSize * (pageNumber - 1)}";
            var orderByClause = $"$orderby={HttpUtility.UrlEncode(orderByColumn)} {(orderByAsc ? "asc" : "desc")}";
            var countClause = "$count=true";

            string? ret = string.Join("&", topClause, skipClause, orderByClause, countClause);

            if (!string.IsNullOrWhiteSpace(filterSearchText)
                && !string.IsNullOrWhiteSpace(filterColumn))
                ret = string.Join("&", ret,
                    $"$filter=contains({HttpUtility.UrlEncode(filterColumn)}, '{HttpUtility.UrlEncode(filterSearchText)}')");

            return ret;
        }
    }
}
