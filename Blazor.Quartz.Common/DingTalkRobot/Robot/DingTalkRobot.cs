using Flurl.Http;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talk.Extensions;

namespace Blazor.Quartz.Common.DingTalkRobot.Robot
{
    /// <summary>
    /// 钉钉自定义机器人
    /// </summary>
    public class DingTalkRobot
    {
        private static string Webhook = AppConfig.DingTalkWebHook;
        private static string Keyword = AppConfig.DingTalkKeyWord;
        private List<string> AtMobiles = null;
        private bool AtAll = false;

        /// <summary>
        ///  调用自定义机器人发Text类型消息
        /// </summary>
        /// <param name="webhook">webHook地址</param>
        /// <param name="content">消息内容</param>
        /// <param name="atMobiles">被@人的手机号</param>
        /// <param name="isAtAll">@所有人时:true,否则为:false</param>
        /// <returns></returns>
        public static void SendTextMessage(string content, List<string> atMobiles, bool isAtAll)
        {
            try
            {
                //@手机号
                var mobiles = "";
                if (!isAtAll)
                {
                    mobiles = GetAtMobiles(atMobiles);
                }
                //消息内容
                object message = new
                {
                    msgtype = "text",
                    text = new
                    {
                        content = (string.Format("【{0}】{1}", Keyword, content) + mobiles).Trim(),
                    },
                    at = new
                    {
                        atMobiles = atMobiles,
                        isAtAll = isAtAll
                    }
                };
                SendMessage(Webhook, message);

            }
            catch (Exception ex)
            {
                Log.Error("调用自定义机器人发Text类型消息失败，" + ex.Message);
            }
        }

        /// <summary>
        /// 调用自定义机器人发Link类型消息
        /// </summary>
        /// <param name="webhook">webHook地址</param>
        /// <param name="title">消息标题</param>
        /// <param name="text">消息内容(注：如果太长只会部分展示)</param>
        /// <param name="picUrl">图片url</param>
        /// <param name="messageUrl">点击消息跳转的url</param>
        /// <returns></returns>
        public void SendLinkMessage(string webhook, string title, string text, string picUrl, string messageUrl)
        {
            try
            {
                object message = new
                {
                    msgtype = "link",
                    link = new
                    {
                        title = string.Format("【{0}】{1}", Keyword, title),
                        text = text,
                        picUrl = picUrl,
                        messageUrl = messageUrl
                    },
                };
                SendMessage(webhook, message);
            }
            catch (Exception ex)
            {
                Log.Error("调用自定义机器人发Link类型消息失败，" + ex.Message);
            }
        }

        /// <summary>
        /// 调用自定义机器人发Markdown类型消息
        /// </summary>
        /// <param name="title">消息标题</param>
        /// <param name="titleType">标题类型等级</param>
        /// <param name="markdownMessages">消息内容</param>
        /// <param name="atMobiles">被@手机号</param>
        /// <param name="isAtAll">@所有人时:true,否则为:false</param>
        /// <returns></returns>
        public void SendMarkdownMessage(string title, TitleType titleType, List<MarkdownMessage> markdownMessages, List<string> atMobiles, bool isAtAll)
        {
            try
            {
                //@手机号
                var mobiles = "";
                if (!isAtAll)
                {
                    mobiles = GetAtMobiles(atMobiles.IsEmpty() ? this.AtMobiles : atMobiles);
                }
                //消息内容头部(标题+被@的人;注：自动换行)
                var textTop = GetContentGrade(titleType, string.Format("【{0}】{1}", Keyword, title)) + "\n >";
                //消息内容
                var text = textTop + RestructureMessage(markdownMessages) + mobiles + "\n >";
                object message = new
                {
                    msgtype = "markdown",
                    markdown = new
                    {
                        title = title,
                        text = text,
                    },
                    at = new
                    {
                        atMobiles = atMobiles.IsEmpty() ? this.AtMobiles : atMobiles,
                        isAtAll = isAtAll,
                    }
                };
                SendMessage(Webhook, message);
            }
            catch (Exception ex)
            {
                Log.Error("调用自定义机器人发Markdown类型消息失败，" + ex.Message);
            }
        }

        /// <summary>
        /// 调用自定义机器人发整体跳转ActionCard类型消息
        /// </summary>
        /// <param name="webhook">webHook地址</param>
        /// <param name="title">消息标题</param>
        /// <param name="markdownMessages">消息内容</param>
        /// <param name="hideAvatar">0-正常发消息者头像,1-隐藏发消息者头像</param>
        /// <param name="btnOrientation">0-按钮竖直排列，1-按钮横向排列</param>
        /// <param name="singleTitle">单个按钮的方案。(设置此项和singleUrl后btns无效。)</param>
        /// <param name="singleUrl">点击singleTitle按钮触发的URL</param>
        /// <returns></returns>
        public void SendActionCardMessage(string webhook, string title, List<MarkdownMessage> markdownMessages, int hideAvatar, int btnOrientation, string singleTitle, string singleUrl)
        {
            try
            {
                object message = new
                {
                    msgtype = "actionCard",
                    actionCard = new
                    {
                        title = string.Format("【{0}】{1}", Keyword, title),
                        text = RestructureMessage(markdownMessages),
                        hideAvatar = hideAvatar,
                        btnOrientation = btnOrientation,
                        singleTitle = singleTitle,
                        singleURL = singleUrl
                    },
                };
                SendMessage(webhook, message);
            }
            catch (Exception ex)
            {
                Log.Error("调用自定义机器人发整体跳转ActionCard类型消息失败，" + ex.Message);
            }
        }

        /// <summary>
        /// 调用自定义机器人发独立跳转ActionCard类型消息
        /// </summary>
        /// <param name="webhook">webHook地址</param>
        /// <param name="title">消息标题</param>
        /// <param name="markdownMessages">消息内容</param>
        /// <param name="hideAvatar">0-正常发消息者头像,1-隐藏发消息者头像</param>
        /// <param name="btnOrientation">0-按钮竖直排列，1-按钮横向排列</param>
        /// <param name="btns">按钮集合</param>
        /// <returns></returns>
        public void SendSingleActionCardMessage(string webhook, string title, List<MarkdownMessage> markdownMessages, int hideAvatar, int btnOrientation, List<Btn> btns)
        {
            try
            {
                var btnObjs = new List<object>();
                if (btns != null && btns.Count > 0)
                {
                    foreach (var item in btns)
                    {
                        object obj = new { title = item.Title, actionURL = item.ActionUrl };
                        btnObjs.Add(obj);
                    }
                }
                object message = new
                {
                    actionCard = new
                    {
                        title = string.Format("【{0}】{1}", Keyword, title),
                        text = RestructureMessage(markdownMessages),
                        hideAvatar = hideAvatar,
                        btnOrientation = btnOrientation,
                        btns = btnObjs
                    },
                    msgtype = "actionCard",
                };
                SendMessage(webhook, message);
            }
            catch (Exception ex)
            {
                Log.Error("调用自定义机器人发独立跳转ActionCard类型消息失败，" + ex.Message);
            }
        }

        /// <summary>
        /// 调用自定义机器人发FeedCard类型消息
        /// </summary>
        /// <param name="webhook">webHook地址</param>
        /// <param name="links">图片按钮集合</param>
        /// <returns></returns>
        public void SendFeedCardMessage(string webhook, List<Link> links)
        {
            try
            {
                var linkObjs = new List<object>();
                if (links != null && links.Count > 0)
                {
                    foreach (var item in links)
                    {
                        object obj = new { title = item.Title, messageURL = item.MessageUrl, picURL = item.PicUrl };
                        linkObjs.Add(obj);
                    }
                }
                object message = new
                {
                    feedCard = new
                    {
                        links = linkObjs,
                    },
                    msgtype = "feedCard"
                };
                SendMessage(webhook, message);
            }
            catch (Exception ex)
            {
                Log.Error("调用自定义机器人发FeedCard类型消息失败，" + ex.Message);
            }
        }

        /// <summary>
        /// 调用自定义机器人发消息
        /// </summary>
        /// <param name="webhook">webHook地址</param>
        /// <param name="message">消息信息</param>
        /// <returns></returns>

        private static void SendMessage(string webhook, object message)
        {
            try
            {
                Task.Run(async () =>
                {
                    await webhook
                         .WithHeader("Content-Type", "application/json")
                         .WithTimeout(20)
                         .PostJsonAsync(message);
                });
            }
            catch (Exception ex)
            {
                Log.Error("调用自定义机器人发消息失败，" + ex.Message);
            }
        }

        /// <summary>
        /// 重构消息内容
        /// </summary>
        /// <param name="markdownMessages">消息内容</param>
        /// <returns></returns>
        private string RestructureMessage(List<MarkdownMessage> markdownMessages)
        {
            var text = "";
            foreach (var item in markdownMessages.OrderBy(x => x.Index))
            {
                switch (item.MarkdownType)
                {
                    case MarkdownType.文本:
                        {
                            switch (item.Text.ContentType)
                            {
                                case ContentType.默认:
                                    item.Text.Content = item.Text.Content;
                                    break;
                                case ContentType.加粗:
                                    item.Text.Content = "**" + item.Text.Content + "**";
                                    break;
                                case ContentType.斜体:
                                    item.Text.Content = "*" + item.Text.Content + "*";
                                    break;
                                default:
                                    item.Text.Content = item.Text.Content;
                                    break;
                            }
                            //文本等级赋值
                            item.Text.Content = GetContentGrade(item.Text.ContentGrade, item.Text.Content);
                            //判断是否换行
                            text += (item.IsLineFeed) ? item.Text.Content + "\n\n >" : item.Text.Content;
                            break;
                        }
                    case MarkdownType.图片:
                        text += (item.IsLineFeed) ? "![screenshot](" + item.Text.ImgUrl + ")\n >" : "![screenshot](" + item.Text.ImgUrl + ")";
                        break;
                    case MarkdownType.链接:
                        text += (item.IsLineFeed) ? "[" + item.Text.Content + "](" + item.Text.Url + ")\n >" : "[" + item.Text.Content + "](" + item.Text.Url + ")";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return text;
        }

        /// <summary>
        /// 获取被@人的手机号
        /// </summary>
        /// <param name="atMobiles">被@人的手机号</param>
        /// <returns></returns>
        private static string GetAtMobiles(List<string> atMobiles)
        {
            var mobiles = "";
            if (atMobiles == null || atMobiles.Count <= 0) return mobiles;
            foreach (var item in atMobiles)
            {
                mobiles += "@" + item;
            }
            return mobiles;
        }

        /// <summary>
        /// 获取等级文本
        /// </summary>
        /// <param name="titleType">文本类型</param>
        /// <param name="title">文本</param>
        /// <returns></returns>
        private string GetContentGrade(TitleType titleType, string title)
        {
            switch (titleType)
            {
                case TitleType.默认:
                    break;
                case TitleType.一级:
                    title = "# " + title;
                    break;
                case TitleType.二级:
                    title = "## " + title;
                    break;
                case TitleType.三级:
                    title = "### " + title;
                    break;
                case TitleType.四级:
                    title = "#### " + title;
                    break;
                case TitleType.五级:
                    title = "##### " + title;
                    break;
                case TitleType.六级:
                    title = "###### " + title;
                    break;
                default:
                    break;
            }
            return title;
        }
    }
}
