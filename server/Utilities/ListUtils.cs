using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KarmaWeb.Utilities
{
    public static class ListUtils
    {
        /// <summary>
        /// Partition an IEnumerable into groups of the size specified (or smaller)
        /// </summary>
        public static IEnumerable<List<T>> InSetsOf<T>(this IEnumerable<T> source, int max)
        {
            List<T> toReturn = new List<T>(max);
            foreach (var item in source)
            {
                toReturn.Add(item);
                if (toReturn.Count == max)
                {
                    yield return toReturn;
                    toReturn = new List<T>(max);
                }
            }
            if (toReturn.Any())
            {
                yield return toReturn;
            }
        }

        public static IEnumerable<string> ListFromCSV(string csvValues)
        {
            if (!string.IsNullOrEmpty(csvValues))
            {
                return csvValues.Split(',');
            }
            else
                return new List<string>();
        }


    }
}
