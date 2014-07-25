using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dialupconnmgr;

namespace Common.Extenssions
{
    public static class StatisticsLogEntryExtenssion
    {
        public static StatisticsLogEntry Sum(this IEnumerable<StatisticsLogEntry> source)
        {
            var ordered = source.OrderBy(entry => entry.FirstFixationTime).ToList();
            var result = ordered.Aggregate((e1, e2) => e1 + e2);
            result.Childs = ordered;
            return result;
        }
    }
}
