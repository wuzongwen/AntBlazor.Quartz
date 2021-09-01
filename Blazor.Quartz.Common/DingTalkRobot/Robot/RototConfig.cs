using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor.Quartz.Common.DingTalkRobot.Robot
{
    public class RototConfig
    {
        /// <summary>
        /// 机器人地址
        /// </summary>
        public string Webhook { get; set; }

        /// <summary>
        /// 关键词
        /// </summary>
        public string Keyword { get; set; }

        /// <summary>
        /// @人手机号
        /// </summary>
        public List<string> AtMobiles { get; set; }

        /// <summary>
        /// 是否@所有人
        /// </summary>
        public bool AtAll { get; set; } = false;

    }
}
