using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JanusXD.Shell.Extensions
{
    public static class ArrayExtensions
    {
        public static bool ContainsAny(this string[] source, out int index, params string[] values)
        {
            foreach (var value in values)
            {
                index = Array.FindIndex(source, x => x.Equals(value));
                if (index < 0) continue;

                return true;
            }

            index = -1;
            return false;
        }

        public static bool ContainsAnyInvariant(this string[] source, out int index, params string[] values)
        {
            foreach (var value in values)
            {
                index = Array.FindIndex(source, x => x == value); ;
                if (index < 0) continue;

                return true;
            }

            index = -1;
            return false;
        }

        public static bool HasArgument(this string[] source, out string value, params string[] arguments)
        {
            if (source.ContainsAnyInvariant(out int index, arguments))
                value = source.ElementAtOrDefault(index + 1);
            else
            {
                value = null;
                return false;
            }

            return true;
        }
    }
}
