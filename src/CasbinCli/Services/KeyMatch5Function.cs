using System;
using System.Text.RegularExpressions;
using NetCasbin.Abstractions;

namespace CasbinCli.Services  
{  
    /// <summary>
    /// KeyMatch5: Ignores query parameters; supports {var} (single segment, ≥1, no /) and '.*' (cross-segment, ≥0, with /)
    /// </summary>
    public class KeyMatch5Function : AbstractFunction
    {
        public KeyMatch5Function() : base("keyMatch5")
        {
        }

        protected override Delegate GetFunc()
        {
            Func<string, string, bool> call = KeyMatch5;
            return call;
        }

        private static bool KeyMatch5(string key1, string key2)
        {
            // 1) Remove query parameters
            var k1 = (key1 ?? string.Empty).Split('?')[0];
            var k2 = (key2 ?? string.Empty).Split('?')[0];

            // 2) Replace {name} with placeholder first to avoid being destroyed by escaping
            const string VAR_TOKEN = "__VAR_SEG__";
            var pre = Regex.Replace(k2, @"\{[^}]+\}", VAR_TOKEN);

            // 3) Escape remaining characters as a whole (ensure literal safety)
            var esc = Regex.Escape(pre);

            // 4) Restore placeholder to "single segment without /, 1+ characters"
            esc = esc.Replace(VAR_TOKEN, @"[^/]+");

            // 5) Strictly restore literal '\.\*' to wildcard '.*' (can cross segments, can be zero length)
            esc = esc.Replace(@"\.\*", @".*");

            // 6) Full match anchoring
            var pattern = "^" + esc + "$";

            return Regex.IsMatch(k1, pattern);
        }
    }  
}
