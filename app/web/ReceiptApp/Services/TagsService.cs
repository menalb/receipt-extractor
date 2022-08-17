using System.Collections.Generic;
using System.Linq;

namespace ReceiptApp.Services
{
    public class TagsService
    {
        public static string GetKindIcon(string tag)
        {
            return GetKindIcon(new string[] { tag });
        }

        public static string GetKindIcon(IEnumerable<string> tags)
        {
            if (tags.Contains("bar"))
            {
                return "fas fa-coffee";
            }
            if (tags.Contains("shop"))
            {
                return "fas fa-shopping-cart";
            }
            if (tags.Contains("pharmacy"))
            {
                return "fas fa-prescription-bottle-alt";
            }
            return string.Empty;
        }
    }
}