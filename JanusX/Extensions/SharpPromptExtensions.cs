using Sharprompt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JanusXD.Shell.Extensions
{
    public static class SharpPromptExtensions
    {
        #region Methods
        public static int SelectOption(this IEnumerable<string> items, string message, int? pageSize = null, object defaultValue = null)
        {
            var options = items.Select((x, i) => (Val: x, Index: i + 1));
            
            var res = Prompt.Select(message, options, pageSize, defaultValue, textSelector: (x) => $"{x.Index}. {x.Val}");
            return res.Index;
        }
        #endregion
    }
}
