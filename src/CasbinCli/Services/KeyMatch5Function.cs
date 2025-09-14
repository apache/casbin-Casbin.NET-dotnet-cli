using System;
using System.Text.RegularExpressions;
using NetCasbin.Abstractions;

namespace CasbinCli.Services  
{  
    /// <summary>
    /// KeyMatch5: 忽略查询参数；支持 {var}（单段、≥1、不含 /）与 '.*'（跨段、≥0、含 /）
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
            // 1) 去掉查询参数
            var k1 = (key1 ?? string.Empty).Split('?')[0];
            var k2 = (key2 ?? string.Empty).Split('?')[0];

            // 2) 先把 {name} 替换为占位符，避免被转义破坏
            const string VAR_TOKEN = "__VAR_SEG__";
            var pre = Regex.Replace(k2, @"\{[^}]+\}", VAR_TOKEN);

            // 3) 对剩余字符整体转义（保证字面量安全）
            var esc = Regex.Escape(pre);

            // 4) 还原占位符为“单段不含 / 的 1+ 字符”
            esc = esc.Replace(VAR_TOKEN, @"[^/]+");

            // 5) 严格只把字面 '\.\*' 还原为通配 '.*'（可跨段、可为零长）
            esc = esc.Replace(@"\.\*", @".*");

            // 6) 全匹配锚定
            var pattern = "^" + esc + "$";

            return Regex.IsMatch(k1, pattern);
        }
    }  
}
