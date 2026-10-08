using Blazor.Quartz.Common;
using Blazor.Quartz.Core.Const;
using Blazor.Quartz.Core.Dapper;
using Blazor.Quartz.Core.Entity;
using Blazor.Quartz.Core.Service.Admin.Dto;
using Blazor.Quartz.Core.Service.Base.Dto;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Blazor.Quartz.Core.Service.Admin
{
    public class AdminService : IAdminService
    {
        /// <summary>
        /// PBKDF2迭代次数
        /// </summary>
        private const int Pbkdf2Iterations = 100000;

        /// <summary>
        /// 盐长度（字节）
        /// </summary>
        private const int SaltSize = 16;

        /// <summary>
        /// 哈希长度（字节）
        /// </summary>
        private const int KeySize = 32;

        /// <summary>
        /// 初始化管理员表（表不存在时自动创建，表中无数据时写入默认管理员）
        /// </summary>
        /// <returns></returns>
        public async Task<BaseResult> EnsureInitializedAsync()
        {
            BaseResult result = new BaseResult();
            try
            {
                //管理员表不存在时自动创建
                await DbContext.ExecuteAsync($@"IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{AdminConstant.TableName}]') AND type = N'U')
BEGIN
    CREATE TABLE [dbo].[{AdminConstant.TableName}](
        [ID] INT IDENTITY(1,1) PRIMARY KEY,
        [USERNAME] NVARCHAR(50) NOT NULL,
        [PASSWORD] NVARCHAR(200) NOT NULL,
        [SALT] NVARCHAR(100) NOT NULL,
        [REAL_NAME] NVARCHAR(50) NULL,
        [IS_ENABLE] INT NOT NULL DEFAULT(1),
        [CREATE_TIME] DATETIME NULL DEFAULT(GETDATE()),
        [LAST_LOGIN_TIME] DATETIME NULL
    )
END");

                //表中无数据时写入默认管理员（账号密码可在配置文件SysConfig节点中调整）
                var count = await DbContext.ExecuteScalarAsync<int>($"SELECT COUNT(1) FROM {AdminConstant.TableName}");
                if (count == 0)
                {
                    var userName = AppConfig.DefaultAdminUser;
                    var password = AppConfig.DefaultAdminPassword;
                    var salt = GenerateSalt();
                    var passwordHash = HashPassword(password, salt);
                    await DbContext.ExecuteAsync($@"INSERT INTO {AdminConstant.TableName}([USERNAME],[PASSWORD],[SALT],[REAL_NAME],[IS_ENABLE])
VALUES(@USERNAME,@PASSWORD,@SALT,@REAL_NAME,1)",
                        new { USERNAME = userName, PASSWORD = passwordHash, SALT = salt, REAL_NAME = "系统管理员" });
                    Log.Information($"已初始化默认管理员账号：{userName}");
                }
                result.Msg = "初始化成功";
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Msg = "初始化管理员表失败";
                Log.Error(ex, ex.Message);
            }
            return result;
        }

        /// <summary>
        /// 管理员登录校验
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<BaseResult<AdminInfo>> Login(LoginDto model)
        {
            BaseResult<AdminInfo> result = new BaseResult<AdminInfo>();
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.USERNAME) || string.IsNullOrWhiteSpace(model.PASSWORD))
                {
                    result.Code = -1;
                    result.Msg = "用户名或密码不能为空";
                    return result;
                }

                var admin = await DbContext.QueryFirstOrDefaultAsync<AdminEntity>(
                    $"SELECT * FROM {AdminConstant.TableName} WHERE USERNAME=@USERNAME", new { USERNAME = model.USERNAME });

                //用户不存在或密码错误统一提示，避免暴露账号是否存在
                if (admin == null || !VerifyPassword(model.PASSWORD, admin.SALT, admin.PASSWORD))
                {
                    result.Code = -1;
                    result.Msg = "用户名或密码错误";
                    return result;
                }

                if (admin.IS_ENABLE != 1)
                {
                    result.Code = -1;
                    result.Msg = "该账号已被禁用";
                    return result;
                }

                //更新最后登录时间
                await DbContext.ExecuteAsync($"UPDATE {AdminConstant.TableName} SET LAST_LOGIN_TIME=GETDATE() WHERE ID=@ID", new { admin.ID });

                result.Msg = "登录成功";
                result.Data = new AdminInfo { ID = admin.ID, USERNAME = admin.USERNAME, REAL_NAME = admin.REAL_NAME };
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Msg = "登录失败";
                Log.Error(ex, ex.Message);
            }
            return result;
        }

        /// <summary>
        /// 生成随机盐
        /// </summary>
        /// <returns></returns>
        private static string GenerateSalt()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(SaltSize));
        }

        /// <summary>
        /// PBKDF2哈希密码
        /// </summary>
        /// <param name="password">明文密码</param>
        /// <param name="salt">盐（Base64）</param>
        /// <returns>哈希值（Base64）</returns>
        private static string HashPassword(string password, string salt)
        {
            return Convert.ToBase64String(Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                Convert.FromBase64String(salt),
                Pbkdf2Iterations,
                HashAlgorithmName.SHA256,
                KeySize));
        }

        /// <summary>
        /// 校验密码（固定时间比较，防时序攻击）
        /// </summary>
        /// <param name="password">明文密码</param>
        /// <param name="salt">盐（Base64）</param>
        /// <param name="expectedHash">库中存储的哈希值（Base64）</param>
        /// <returns></returns>
        private static bool VerifyPassword(string password, string salt, string expectedHash)
        {
            try
            {
                var expectedBytes = Convert.FromBase64String(expectedHash);
                var actualBytes = Convert.FromBase64String(HashPassword(password, salt));
                return CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes);
            }
            catch
            {
                return false;
            }
        }
    }

    public interface IAdminService
    {
        /// <summary>
        /// 初始化管理员表（表不存在时自动创建，表中无数据时写入默认管理员）
        /// </summary>
        /// <returns></returns>
        Task<BaseResult> EnsureInitializedAsync();

        /// <summary>
        /// 管理员登录校验
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<BaseResult<AdminInfo>> Login(LoginDto model);
    }
}
