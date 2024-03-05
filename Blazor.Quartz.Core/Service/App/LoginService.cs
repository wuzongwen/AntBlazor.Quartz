using Blazor.Quartz.Core.Const;
using Blazor.Quartz.Core.Dapper;
using Blazor.Quartz.Core.Service.App.Dto;
using Blazor.Quartz.Core.Service.Base.Dto;
using Blazored.SessionStorage;
using Dapper;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Blazor.Quartz.Core.Service.App
{
    public class LoginService : ILoginService
    {
        private const string SessionKey = "CurrentUser";

        public async Task<ManageUser> GetCurrentuserAsync()
        {
            return await _sessionStorage.GetItemAsync<ManageUser>(SessionKey);
        }

        public bool IsLoggedIn => GetCurrentuserAsync != null;

        private readonly ISessionStorageService _sessionStorage;

        public LoginService(ISessionStorageService sessionStorage)
        {
            _sessionStorage = sessionStorage;
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<BaseResult<bool>> UserLogin(LoginReq req) 
        {
            BaseResult<bool> result = new BaseResult<bool>();
            try
            {
                var dynamicParams = new DynamicParameters();
                dynamicParams.Add("UserName", req.Account);
                dynamicParams.Add("Password", req.Password);
                var User = await DbContext.QueryFirstOrDefaultAsync<ManageUser>($"SELECT UserName,Password,IsEnable,LastLoginTime FROM {QuartzConstant.TablePrefix}USER WHERE IsEnable=1 AND UserName=@UserName AND Password=@Password", dynamicParams);
                if (User == null)
                {
                    result.Code = -1;
                    result.Msg = "登录失败";
                    return result;
                }
                if (User.IsEnable != 1) 
                {
                    result.Code = -1;
                    result.Msg = "用户状态异常";
                    return result;
                }
                #region 用户缓存逻辑
                await _sessionStorage.SetItemAsync(SessionKey, User);
                #endregion
                result.Msg = "登录成功";
                return result;
            }
            catch
            {
                result.Code = -1;
                result.Msg = "登录失败";
                return result;
            }
        }

        /// <summary>
        /// 是否登录
        /// </summary>
        /// <returns></returns>
        public async Task<bool> IsLogin() 
        {
            return await GetCurrentuserAsync() != null ? true : false;
        }
    }
    public interface ILoginService
    {
        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        Task<BaseResult<bool>> UserLogin(LoginReq req);

        /// <summary>
        /// 是否登录
        /// </summary>
        /// <returns></returns>
        Task<bool> IsLogin();
    }
}
