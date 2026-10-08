using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Blazor.Quartz.Web.Services
{
    /// <summary>
    /// 图形验证码（内存存储，适用于单实例Blazor Server；SVG绘制零外部依赖）
    /// </summary>
    public class CaptchaService : ICaptchaService
    {
        /// <summary>
        /// 验证码字符集（去掉易混淆的0/O/1/l/I）
        /// </summary>
        private const string CodeChars = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ";

        /// <summary>
        /// 有效期
        /// </summary>
        private static readonly TimeSpan Expiry = TimeSpan.FromMinutes(5);

        private readonly ConcurrentDictionary<string, CaptchaEntry> store = new();

        public CaptchaModel Generate()
        {
            //顺手清理过期项，避免长期运行内存积累
            var now = DateTime.UtcNow;
            foreach (var kv in store)
            {
                if (now - kv.Value.Created > Expiry)
                {
                    store.TryRemove(kv.Key, out _);
                }
            }

            var code = new string(Enumerable.Range(0, 4)
                .Select(_ => CodeChars[RandomNumberGenerator.GetInt32(CodeChars.Length)])
                .ToArray());
            var captchaId = Guid.NewGuid().ToString("N");
            store[captchaId] = new CaptchaEntry(code, now);

            return new CaptchaModel { CaptchaId = captchaId, Svg = BuildSvg(code) };
        }

        /// <summary>
        /// 校验（一次性：无论对错都立即消费，防止重放枚举；大小写不敏感）
        /// </summary>
        /// <param name="captchaId">验证码ID</param>
        /// <param name="input">用户输入</param>
        /// <returns></returns>
        public bool Validate(string captchaId, string input)
        {
            if (string.IsNullOrEmpty(captchaId) || string.IsNullOrEmpty(input))
            {
                return false;
            }
            if (!store.TryRemove(captchaId, out var entry))
            {
                return false;
            }
            if (DateTime.UtcNow - entry.Created > Expiry)
            {
                return false;
            }
            return string.Equals(entry.Code, input.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 绘制验证码SVG：字符随机旋转/偏移/变色，加干扰线与噪点
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private static string BuildSvg(string code)
        {
            const int w = 110, h = 40;
            var rnd = Random.Shared; //绘制层面的随机性，无需加密随机

            var sb = new StringBuilder(512);
            sb.Append($"<svg xmlns='http://www.w3.org/2000/svg' width='{w}' height='{h}' viewBox='0 0 {w} {h}'>");
            sb.Append($"<rect width='{w}' height='{h}' fill='#f2f4f7'/>");

            //干扰弧线
            for (int i = 0; i < 3; i++)
            {
                sb.Append($"<path d='M {rnd.Next(w)} {rnd.Next(h)} Q {rnd.Next(w)} {rnd.Next(h)} {rnd.Next(w)} {rnd.Next(h)}' fill='none' stroke='hsl({rnd.Next(360)},40%,70%)' stroke-width='1'/>");
            }

            //验证码字符
            for (int i = 0; i < code.Length; i++)
            {
                var x = 16 + i * 26;
                var y = 25 + rnd.Next(-4, 5);
                var rotate = rnd.Next(-18, 19);
                var color = $"hsl({rnd.Next(360)},65%,{rnd.Next(28, 42)}%)";
                sb.Append($"<text x='{x}' y='{y}' font-family='Arial, sans-serif' font-size='22' font-weight='bold' fill='{color}' text-anchor='middle' transform='rotate({rotate} {x} {y})'>{code[i]}</text>");
            }

            //噪点
            for (int i = 0; i < 14; i++)
            {
                sb.Append($"<circle cx='{rnd.Next(w)}' cy='{rnd.Next(h)}' r='{rnd.Next(1, 3)}' fill='hsl({rnd.Next(360)},20%,{rnd.Next(40, 75)}%)' opacity='0.5'/>");
            }

            sb.Append("</svg>");
            return sb.ToString();
        }

        private sealed record CaptchaEntry(string Code, DateTime Created);
    }

    public interface ICaptchaService
    {
        /// <summary>
        /// 生成一对新的验证码（ID + SVG图形）
        /// </summary>
        /// <returns></returns>
        CaptchaModel Generate();

        /// <summary>
        /// 校验验证码（一次性消费）
        /// </summary>
        /// <param name="captchaId"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        bool Validate(string captchaId, string input);
    }

    /// <summary>
    /// 验证码生成结果
    /// </summary>
    public class CaptchaModel
    {
        /// <summary>
        /// 验证码ID（提交时回传用于校验）
        /// </summary>
        public string CaptchaId { get; set; }

        /// <summary>
        /// 验证码图形（SVG）
        /// </summary>
        public string Svg { get; set; }
    }
}
