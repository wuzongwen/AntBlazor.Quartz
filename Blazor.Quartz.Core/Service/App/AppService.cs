using Blazor.Quartz.Common;
using Blazor.Quartz.Core.Const;
using Blazor.Quartz.Core.Dapper;
using Blazor.Quartz.Core.Entity;
using Blazor.Quartz.Core.Service.App.Dto;
using Blazor.Quartz.Core.Service.App.Enum;
using Blazor.Quartz.Core.Service.Base.Dto;
using Blazor.Quartz.Core.Service.Timer;
using Dapper;
using Microsoft.Data.SqlClient;
using Quartz;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor.Quartz.Core.Service.App
{
    public class AppService : IAppService
    {
        /// <summary>
        /// 调度器
        /// </summary>
        private SchedulerCenter scheduler;

        public AppService(SchedulerCenter schedulerCenter)
        {
            this.scheduler = schedulerCenter;
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<BaseResult> Add(AppInfo model)
        {
            BaseResult result = new BaseResult();
            try
            {
                if (!DbContext.Exists($"SELECT * FROM {QuartzConstant.TablePrefix}JOB_GROUP WHERE JOB_GROUP_NAME=@JOB_GROUP_NAME", new { JOB_GROUP_NAME = model.JOB_GROUP_NAME }))
                {
                    var res = await DbContext.ExecuteAsync($@"INSERT INTO {QuartzConstant.TablePrefix}JOB_GROUP ([JOB_GROUP_NAME]
                            ,[DESCRIPTION]
                            ,[IS_ENABLE]) VALUES(@JOB_GROUP_NAME,@DESCRIPTION,@IS_ENABLE)", model);
                    if (res > 0)
                    {
                        result.Msg = "添加成功";
                        return result;
                    }
                    result.Code = -1;
                    result.Msg = "添加失败";
                }
                else
                {
                    result.Code = -1;
                    result.Msg = "应用已存在";
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Msg = "添加失败";
                Log.Error(ex, ex.Message);
            }
            return result;
        }

        /// <summary>
        /// 编辑
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<BaseResult> Edit(AppInfo model)
        {
            BaseResult result = new BaseResult();
            try
            {
                //获取该应用下的所有任务
                var jobs = await DbContext.QueryAsync<JOB_DETAILS>($"SELECT * FROM {QuartzConstant.TablePrefix}JOB_DETAILS WHERE JOB_GROUP=@JOB_GROUP_NAME", new { JOB_GROUP_NAME = model.JOB_GROUP_NAME });

                if (model.IS_ENABLE == AppStatusEnum.Enable)
                {
                    //恢复该应用下的任务
                    foreach (var item in jobs.ToList())
                    {
                        var jobKey = new JobKey(item.JOB_NAME, item.JOB_GROUP);
                        await scheduler.ResumeJobAsync(jobKey.Group, jobKey.Name);
                    }
                }
                else
                {
                    //暂停该应用下的任务
                    foreach (var item in jobs.ToList())
                    {
                        var jobKey = new JobKey(item.JOB_NAME, item.JOB_GROUP);
                        await scheduler.StopOrDelScheduleJobAsync(jobKey.Group, jobKey.Name);
                    }
                }

                var res = await DbContext.ExecuteAsync($"UPDATE {QuartzConstant.TablePrefix}JOB_GROUP SET IS_ENABLE=@IS_ENABLE WHERE JOB_GROUP_NAME=@JOB_GROUP_NAME", new { IS_ENABLE = model.IS_ENABLE, JOB_GROUP_NAME = model.JOB_GROUP_NAME });
                if (res > 0)
                {
                    result.Msg = "操作成功";
                    return result;
                }
                result.Code = -1;
                result.Msg = "操作失败";
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Msg = "操作失败";
                Log.Error(ex, ex.Message);
            }
            return result;
        }

        /// <summary>
        /// 删除应用
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<BaseResult> Delete(AppInfo model)
        {
            BaseResult result = new BaseResult();
            try
            {
                //获取该应用下的所有任务
                var jobs = await DbContext.QueryAsync<JOB_DETAILS>($"SELECT JOB_NAME,JOB_GROUP FROM {QuartzConstant.TablePrefix}JOB_DETAILS WHERE JOB_GROUP=@JOB_GROUP_NAME", new { JOB_GROUP_NAME = model.JOB_GROUP_NAME });

                if (jobs.Count() > 0) 
                {
                    result.Code = -1;
                    result.Msg = "请先删除此应用下的任务";
                    return result;
                }
                //删除该应用下的任务
                //foreach (var item in jobs.ToList())
                //{
                //    var jobKey = new JobKey(item.JOB_NAME, item.JOB_GROUP);
                //    await scheduler.StopOrDelScheduleJobAsync(jobKey.Group, jobKey.Name, true);
                //    //await scheduler.PauseJob(jobKey);
                //    //await scheduler.DeleteJob(jobKey);
                //}
                //删除任务执行日志
                //await DbContext.ExecuteAsync($"DELETE FROM {QuartzConstant.TablePrefix}JOB_EXECUTION_LOG WHERE JOB_GROUP=@JOB_GROUP", new { JOB_GROUP = model.JOB_GROUP_NAME });
                var res = await DbContext.ExecuteAsync($"DELETE FROM {QuartzConstant.TablePrefix}JOB_GROUP WHERE JOB_GROUP_NAME=@JOB_GROUP_NAME", new { JOB_GROUP_NAME = model.JOB_GROUP_NAME });
                if (res > 0)
                {
                    result.Msg = "操作成功";
                    return result;
                }
                result.Code = -1;
                result.Msg = "操作失败";
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Msg = "操作失败";
                Log.Error(ex, ex.Message);
            }
            return result;
        }

        /// <summary>
        /// 获取应用列表
        /// </summary>
        /// <returns></returns>
        public async Task<BaseResult<List<AppInfo>>> GetList(QueryModel model)
        {
            BaseResult<List<AppInfo>> result = new BaseResult<List<AppInfo>>();
            try
            {
                var sql = $"SELECT * FROM {QuartzConstant.TablePrefix}JOB_GROUP WHERE 1=1";
                var dynamicParams = new DynamicParameters();
                if (!string.IsNullOrEmpty(model.JOB_GROUP_NAME))
                {
                    sql += " AND JOB_GROUP_NAME LIKE @JOB_GROUP_NAME";
                    dynamicParams.Add("JOB_GROUP_NAME", $"%{model.JOB_GROUP_NAME}%");
                }
                if (model.IS_ENABLE != null)
                {
                    sql += " AND IS_ENABLE=@IS_ENABLE";
                    dynamicParams.Add("IS_ENABLE", model.IS_ENABLE);
                }
                var res = await DbContext.QueryAsync<AppInfo>(sql, dynamicParams);
                result.Msg = "获取成功";
                result.Data = res.ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Msg = "获取失败";
                Log.Error(ex, ex.Message);
            }
            return result;
        }

        /// <summary>
        /// 获取应用信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<BaseResult<AppInfo>> GetModel(AppInfo model) 
        {
            BaseResult<AppInfo> result = new BaseResult<AppInfo>();
            try
            {
                var res = await DbContext.QueryFirstOrDefaultAsync<AppInfo>($"SELECT * FROM {QuartzConstant.TablePrefix}JOB_GROUP WHERE JOB_GROUP_NAME=@JOB_GROUP_NAME", new { JOB_GROUP_NAME = model.JOB_GROUP_NAME });
                if (res != null) 
                {
                    result.Msg = "获取成功";
                    result.Data = res;
                }
                result.Code = -1;
                result.Msg = "获取失败";
            }
            catch (Exception ex) 
            {
                result.Code = -1;
                result.Msg = "获取失败";
                Log.Error(ex, ex.Message);
            }
            return result;
        }
    }

    public interface IAppService
    {
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<BaseResult> Add(AppInfo model);

        /// <summary>
        /// 编辑
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<BaseResult> Edit(AppInfo model);

        /// <summary>
        /// 删除应用
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<BaseResult> Delete(AppInfo model);

        /// <summary>
        /// 获取应用列表
        /// </summary>
        /// <returns></returns>
        Task<BaseResult<List<AppInfo>>> GetList(QueryModel model);

        /// <summary>
        /// 获取应用信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<BaseResult<AppInfo>> GetModel(AppInfo model);
    }
}
