using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor.Quartz.Common.DingTalkRobot.Robot
{
    public class RobotKeyConst
    {
        /// <summary>
        /// 错误报警，必须要处理的错误
        /// </summary>
        public const string ErrorRobot = "ErrorRobot";

        /// <summary>
        /// 警告通知，可延后处理或分析的:sql超时，用户操作不合规,api返回错误
        /// </summary>
        public const string WarnRobot = "WarnRobot";

        /// <summary>
        /// 业务通知
        /// </summary>
        public const string BizRobot = "BizRobot";
    }
}
