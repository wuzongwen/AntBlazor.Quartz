using Blazor.Quartz.Core.Const;
using Blazor.Quartz.Core.Dapper;
using Blazor.Quartz.Core.Service.App.Dto;
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
            var res = await DbContext.QueryAsync<JOB_EXECUTION_LOG>($@"SELECT [JOB_NAME]
                            ,[JOB_GROUP]
                            ,[EXECUTION_STATUS]
                            ,[REQUEST_URL]
                            ,[REQUEST_TYPE]
                            ,[HEADERS]
                            ,[REQUEST_DATA]
                            ,[RESPONSE_DATA]
                            ,[BEGIN_TIME] FROM { QuartzConstant.TablePrefix}JOB_EXECUTION_LOG WHERE BEGIN_TIME >= @START_TIME AND BEGIN_TIME<= @END_TIME ORDER BY BEGIN_TIME DESC", new { START_TIME = query.start_time, END_TIME = query.end_time });
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
