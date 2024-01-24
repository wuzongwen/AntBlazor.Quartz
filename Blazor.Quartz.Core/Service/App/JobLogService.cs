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
        public async Task<PageRes<List<JOB_EXECUTION_LOG>>> GetList(QueryLogDto query)
        {
            string sql = @"SELECT [JOB_NAME]
                            ,[JOB_GROUP]
                            ,[EXECUTION_STATUS]
                            ,[REQUEST_URL]
                            ,[REQUEST_TYPE]
                            ,[HEADERS]
                            ,[REQUEST_DATA]
                            ,[RESPONSE_DATA]
                            ,[BEGIN_TIME]
                    FROM {0}";
            string sqlCondition = " WHERE 1=1";
            var dynamicParams = new DynamicParameters();
            if (!string.IsNullOrEmpty(query.group))
            {
                sqlCondition += " AND JOB_GROUP = @JOB_GROUP";
                dynamicParams.Add("JOB_GROUP", query.group);
            }
            if (!string.IsNullOrEmpty(query.name))
            {
                sqlCondition += " AND JOB_NAME = @JOB_NAME";
                dynamicParams.Add("JOB_NAME", query.name);
            }
            if (query.status != null) 
            {
                sqlCondition += " AND EXECUTION_STATUS = @EXECUTION_STATUS";
                dynamicParams.Add("EXECUTION_STATUS", query.status);
            }
            var dataCount = await DbContext.QueryFirstOrDefaultAsync<int>($"SELECT COUNT(*)FROM {QuartzConstant.TablePrefix}JOB_EXECUTION_LOG {sqlCondition}", dynamicParams);
            dynamicParams.Add("StartRow", (query.page_index - 1) * query.page_size + 1);
            dynamicParams.Add("EndRow", query.page_index * query.page_size);
            var dataPageListSql = string.Format(sql, $@"(
                    SELECT [JOB_NAME]
                            ,[JOB_GROUP]
                            ,[EXECUTION_STATUS]
                            ,[REQUEST_URL]
                            ,[REQUEST_TYPE]
                            ,[HEADERS]
                            ,[REQUEST_DATA]
                            ,[RESPONSE_DATA]
                            ,[BEGIN_TIME], ROW_NUMBER() OVER (ORDER BY BEGIN_TIME DESC) AS RowNum
                    FROM {QuartzConstant.TablePrefix}JOB_EXECUTION_LOG {sqlCondition}
                ) AS Temp WHERE RowNum BETWEEN @StartRow AND @EndRow");
            var res = await DbContext.QueryAsync<JOB_EXECUTION_LOG>(dataPageListSql, dynamicParams);
            PageRes<List<JOB_EXECUTION_LOG>> pageRes = new PageRes<List<JOB_EXECUTION_LOG>>();
            pageRes.data = res.ToList();
            pageRes.total = dataCount;
            return pageRes;
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
        Task<PageRes<List<JOB_EXECUTION_LOG>>> GetList(QueryLogDto query);
    }
}
