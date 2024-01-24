using Blazor.Quartz.Core.Const;
using Blazor.Quartz.Core.Dapper;
using Blazor.Quartz.Core.Entity;
using Blazor.Quartz.Core.Service.App.Dto;
using Blazor.Quartz.Core.Service.Timer;
using Blazor.Quartz.Core.Service.Timer.Dto;
using Quartz.Impl.Matchers;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor.Quartz.Core.Service.App
{
    public class JobService : IJobService
    {
        /// <summary>
        /// 调度器
        /// </summary>
        private SchedulerCenter scheduler;

        public JobService(SchedulerCenter schedulerCenter)
        {
            this.scheduler = schedulerCenter;
        }

        /// <summary>
        /// 获取运行中的任务
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<JOB_DETAILS>> GetRunningJobList() 
        {
            var allRunningStatus = await DbContext.QueryAsync<JOB_DETAILS>("SELECT[JOB_NAME],[JOB_GROUP]FROM [CZT_JOB].[dbo].[QRTZ_JOB_DETAILS]");
            return allRunningStatus;
        }

        /// <summary>
        /// 获取Job运行状态
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<JobRunningState>> GetJobRunningState()
        {
            var allRunningStatus = await DbContext.QueryAsync<JobRunningState>(@"WITH Hours AS (
                                        SELECT DISTINCT DATEPART(HOUR, DATEADD(HOUR, -number, GETDATE())) AS HourOfDay
                                        FROM master.dbo.spt_values
                                        WHERE type = 'P' AND number BETWEEN 0 AND 12
                                    )
                                    SELECT
                                        h.HourOfDay,
	                                    COUNT(*) AS [Count],
                                        COUNT(CASE WHEN j.EXECUTION_STATUS = 0 THEN 1 END) AS CountStatus0,
                                        COUNT(CASE WHEN j.EXECUTION_STATUS = 1 THEN 1 END) AS CountStatus1
                                    FROM Hours h
                                    LEFT JOIN QRTZ_JOB_EXECUTION_LOG j ON DATEPART(HOUR, j.BEGIN_TIME) = h.HourOfDay
                                    WHERE j.BEGIN_TIME >= DATEADD(HOUR, -12, GETDATE())
                                    GROUP BY
                                        h.HourOfDay
                                    ORDER BY
                                        h.HourOfDay");
            return allRunningStatus;
        }

        /// <summary>
        /// 获取所有Job信息
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<JobBriefInfo>> GetAllJobInfo()
        {
            var allJobInfo = await DbContext.QueryAsync<JobBriefInfo>(@"SELECT
                                    G.JOB_GROUP_NAME AS GroupName,
                                    D.JOB_NAME AS Name,
	                                D.NEXT_FIRE_TIME AS Next_Fire_Time,
	                                D.PREV_FIRE_TIME AS Prev_Fire_Time,
	                                D.TRIGGER_STATE AS Trigger_State,
                                    COUNT(L.JOB_NAME) AS TotalExecutions,
                                    SUM(CASE WHEN L.EXECUTION_STATUS = 0 THEN 1 ELSE 0 END) AS ErrorExecutions
                                FROM
                                    QRTZ_JOB_GROUP G
                                JOIN
                                    QRTZ_TRIGGERS D ON G.JOB_GROUP_NAME = D.JOB_GROUP
                                LEFT JOIN
                                    QRTZ_JOB_EXECUTION_LOG L ON (D.JOB_GROUP = L.JOB_GROUP AND D.JOB_NAME = L.JOB_NAME)
                                WHERE
                                    G.IS_ENABLE = 1
                                GROUP BY
                                    G.JOB_GROUP_NAME, D.JOB_NAME,D.NEXT_FIRE_TIME,D.PREV_FIRE_TIME,D.TRIGGER_STATE
                                ORDER BY
                                    G.JOB_GROUP_NAME, D.JOB_NAME");
            return allJobInfo;
        }
    }

    public interface IJobService 
    {
        /// <summary>
        /// 获取运行中的任务
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<JOB_DETAILS>> GetRunningJobList();

        /// <summary>
        /// 获取Job运行状态
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<JobRunningState>> GetJobRunningState();

        /// <summary>
        /// 获取所有Job信息
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<JobBriefInfo>> GetAllJobInfo();
    }
}
