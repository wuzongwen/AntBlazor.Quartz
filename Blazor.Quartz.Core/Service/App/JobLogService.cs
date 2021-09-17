using Blazor.Quartz.Core.Const;
using Blazor.Quartz.Core.Dapper;
using Blazor.Quartz.Core.Service.App.Dto;
using Dapper;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor.Quartz.Core.Service.App
{
    public class JobLogService : IJobLogService
    {
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task Add(JOB_EXECUTION_LOG model)
        {
            var res = await DbContext.ExecuteAsync($@"INSERT INTO {QuartzConstant.TablePrefix}JOB_EXECUTION_LOG ([JOB_NAME]
                            ,[JOB_GROUP]
                            ,[EXECUTION_STATUS]
                            ,[REQUEST_URL]
                            ,[REQUEST_TYPE]
                            ,[HEADERS]
                            ,[REQUEST_DATA]
                            ,[RESPONSE_DATA]
                            ,[BEGIN_TIME]
                            ) VALUES(@JOB_NAME,@JOB_GROUP,@EXECUTION_STATUS,@REQUEST_URL,@REQUEST_TYPE,@HEADERS,@REQUEST_DATA,@RESPONSE_DATA
                            ,@BEGIN_TIME,@END_TIME,@EXECUTE_TIME)", model);
        }

        /// <summary>
        /// 获取日志
        /// </summary>
        /// <returns></returns>
        public async Task<List<JOB_EXECUTION_LOG>> GetList(QueryLogDto query)
        {
            var sql = $@"SELECT [JOB_NAME]
                            ,[JOB_GROUP]
                            ,[EXECUTION_STATUS]
                            ,[REQUEST_URL]
                            ,[REQUEST_TYPE]
                            ,[HEADERS]
                            ,[REQUEST_DATA]
                            ,[RESPONSE_DATA]
                            ,[BEGIN_TIME] FROM { QuartzConstant.TablePrefix}JOB_EXECUTION_LOG WHERE 1=1";
            var dynamicParams = new DynamicParameters();
            if (!string.IsNullOrEmpty(query.start_time))
            {
                sql += " AND BEGIN_TIME >= @START_TIME";
                dynamicParams.Add("START_TIME", query.start_time);
            }
            if (!string.IsNullOrEmpty(query.end_time))
            {
                sql += " AND BEGIN_TIME <= @END_TIME";
                dynamicParams.Add("END_TIME", query.end_time);
            }
            if (!string.IsNullOrEmpty(query.group))
            {
                sql += " AND JOB_GROUP = @JOB_GROUP";
                dynamicParams.Add("JOB_GROUP", query.group);
            }
            if (!string.IsNullOrEmpty(query.name))
            {
                sql += " AND JOB_NAME = @JOB_NAME";
                dynamicParams.Add("JOB_NAME", query.name);
            }
            var res = await DbContext.QueryAsync<JOB_EXECUTION_LOG>(sql, dynamicParams);
            return res.ToList();
        }
    }

    public interface IJobLogService
    {
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task Add(JOB_EXECUTION_LOG model);

        /// <summary>
        /// 获取日志
        /// </summary>
        /// <returns></returns>
        Task<List<JOB_EXECUTION_LOG>> GetList(QueryLogDto query);
    }
}
