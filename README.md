# AntBlazor.Quartz
可配置钉钉机器人接收错误日志及每日报表等信息。已内置管理员登录功能：未登录时所有页面及接口均不可访问，管理员账号存放于数据库管理员表 SYS_ADMIN（首次启动自动建表并写入默认管理员 admin/123456，可在 appsettings.json 的 SysConfig:DefaultAdminUser/DefaultAdminPassword 中修改；默认密码仅用于初始化，请登录后及时修改库中记录）。密码采用 PBKDF2加盐哈希存储，登录状态通过Cookie保存（默认8小时滑动过期）。
# 更新日志
## 2022-01-12
1.已知问题修复 
## 2021-11-29
1.相关逻辑优化  
2.新增接口请求超时配置
## 2021-11-24
1.修复编辑任务后任务列表重置问题  
2.新增任务可选是否约定返回模型
## 2021-11-23
1.折线图新增失败数
![折线图新增失败数](https://github.com/wuzongwen/picturehost/blob/main/Blazor.Quartz/20211123171627.png)
## 2021-11-22
1.新增约定返回模型{"resCode":0,"resMsg":"成功","resData":null,"isSuccess":true}
# 数据面板
![数据面板](https://github.com/wuzongwen/picturehost/blob/main/Blazor.Quartz/20211108164543.png)
# 应用管理
![应用管理](https://github.com/wuzongwen/picturehost/blob/main/Blazor.Quartz/20211108164350.png)
# 任务管理
![任务管理](https://github.com/wuzongwen/picturehost/blob/main/Blazor.Quartz/20211108164428.png)
# 新增任务
![新增任务](https://github.com/wuzongwen/picturehost/blob/main/Blazor.Quartz/20211108170156.png)
# 日志列表
![日志列表](https://github.com/wuzongwen/picturehost/blob/main/Blazor.Quartz/20211108165511.png)
# 钉钉机器人
![钉钉机器人](https://github.com/wuzongwen/picturehost/blob/main/Blazor.Quartz/20211108170354.png)
