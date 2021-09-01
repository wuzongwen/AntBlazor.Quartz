﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Blazor.Quartz.Common.DingTalkRobot.Robot
{
    /// <summary>
    /// 网络工具类
    /// </summary>
    public sealed class WebHelper
    {
        /// <summary>
        /// 浏览器ContentType
        /// </summary>
        public string ContentType { set; get; } = "application/x-www-form-urlencoded";
        /// <summary>
        /// 浏览器Accept
        /// </summary>
        public string Accept { set; get; } = "";
        /// <summary>
        /// 浏览器UserAgent
        /// </summary>
        public string UserAgent { set; get; } = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; .NET CLR 2.0.50727; .NET CLR 3.0.04506.648; .NET CLR 3.5.21022)";
        /// <summary>
        /// 请求与响应的超时时间默认100秒(单位毫秒)
        /// </summary>
        public int Timeout { set; get; } = 100000;
        /// <summary>
        /// Header
        /// </summary>
        public WebHeaderCollection Header { set; get; }

        /// <summary>
        /// 执行HTTP POST请求
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="parameters">参数</param>
        /// <param name="rspEncoding">响应编码</param>
        /// <param name="dateTimeFormat">参数时间格式</param>
        /// <returns></returns>
        public void DoPost<T>(string url, object parameters, Encoding rspEncoding = null, string dateTimeFormat = "yyyy-MM-dd HH:mm:ss")
        {
            HttpWebRequest httpWebRequest = null;
            HttpWebResponse httpWebResponse = null;
            if (rspEncoding == null)
            {
                rspEncoding = Encoding.GetEncoding("UTF-8");
            }
            try
            {
                httpWebRequest = GetWebRequest(url, "POST");
                var @params = JsonConvert.SerializeObject(parameters);
                @params = Regex.Replace(@params, @"\\/Date\((\d+)\)\\/", match =>
                {
                    var dt = new DateTime(1970, 1, 1);
                    dt = dt.AddMilliseconds(long.Parse(match.Groups[1].Value));
                    dt = dt.ToLocalTime();
                    return dt.ToString(dateTimeFormat);
                });
                var reqStreamWriter = new StreamWriter(httpWebRequest.GetRequestStream());
                reqStreamWriter.Write(@params);
                reqStreamWriter.Close();

                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                var responseStream = httpWebResponse.GetResponseStream();
                if (responseStream != null)
                {
                    var streamReader = new StreamReader(responseStream, rspEncoding);
                    var html = streamReader.ReadToEnd();
                    streamReader.Close();
                    responseStream.Close();
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                httpWebRequest?.Abort();
                httpWebResponse?.Close();
            }
        }

        /// <summary>
        /// 执行HTTP POST请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public void DoPost(string url, IDictionary<string, string> parameters)
        {
            DoPost(url, parameters, null, null, null);
        }


        /// <summary>
        /// 执行HTTP POST请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="parameters">参数</param>
        /// <param name="cookieContainer">CookieContainer</param>
        /// <param name="proxy">代理请求信息</param>
        /// <param name="rspEncoding">响应编码</param>
        public void DoPost(string url, IDictionary<string, string> parameters, CookieContainer cookieContainer, WebProxy proxy = null, Encoding rspEncoding = null)
        {
            HttpWebRequest httpWebRequest = null;
            HttpWebResponse httpWebResponse = null;
            if (rspEncoding == null)
            {
                rspEncoding = Encoding.GetEncoding("UTF-8");
            }
            try
            {
                httpWebRequest = GetWebRequest(url, "POST");
                //cookie
                if (cookieContainer != null)
                {
                    httpWebRequest.CookieContainer = cookieContainer;
                }
                //代理请求
                if (proxy != null)
                {
                    httpWebRequest.Proxy = proxy;
                }
                //参数不为空
                if (parameters != null)
                {
                    var byteRequest = Encoding.Default.GetBytes(BuildQuery(parameters));
                    httpWebRequest.ContentLength = byteRequest.Length;
                    var stream = httpWebRequest.GetRequestStream();
                    stream.Write(byteRequest, 0, byteRequest.Length);
                    stream.Close();
                }
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                var responseStream = httpWebResponse.GetResponseStream();
                if (responseStream != null)
                {
                    var streamReader = new StreamReader(responseStream, rspEncoding);
                    var html = streamReader.ReadToEnd();
                    streamReader.Close();
                    responseStream.Close();
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                httpWebRequest?.Abort();
                httpWebResponse?.Close();
            }
        }

        /// <summary>
        /// 执行HTTP GET请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public void DoGet(string url, IDictionary<string, string> parameters)
        {
            DoGet(url, parameters, null, null, null);
        }

        /// <summary>
        /// 执行HTTP GET请求
        /// </summary>
        /// <param name="url">地址</param>
        /// <param name="parameters">参数</param>
        /// <param name="cookieContainer">CookieContainer</param>
        /// <param name="proxy">代理请求信息</param>
        /// <param name="rspEncoding">响应编码</param>
        public void DoGet(string url, IDictionary<string, string> parameters, CookieContainer cookieContainer, WebProxy proxy = null, Encoding rspEncoding = null)
        {
            HttpWebRequest httpWebRequest = null;
            HttpWebResponse httpWebResponse = null;
            if (rspEncoding == null)
            {
                rspEncoding = Encoding.GetEncoding("UTF-8");
            }
            try
            {
                //参数不为空
                if (parameters != null)
                {
                    if (!string.IsNullOrEmpty(url))
                    {
                        var urlArray = url.Split('?');
                        url = $"{urlArray[0]}?{BuildQuery(parameters)}";
                    }
                }
                httpWebRequest = GetWebRequest(url, "GET");
                //cookie
                if (cookieContainer != null)
                {
                    httpWebRequest.CookieContainer = cookieContainer;
                }
                //代理请求
                if (proxy != null)
                {
                    httpWebRequest.Proxy = proxy;
                }
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                var responseStream = httpWebResponse.GetResponseStream();
                if (responseStream != null)
                {
                    var streamReader = new StreamReader(responseStream, rspEncoding);
                    var html = streamReader.ReadToEnd();
                    streamReader.Close();
                    responseStream.Close();
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                httpWebRequest?.Abort();
                httpWebResponse?.Close();
            }
        }

        /// <summary>
        /// 获取WebRequest
        /// </summary>
        /// <param name="url">url</param>
        /// <param name="method">HttpMethod</param>
        /// <returns></returns>
        private HttpWebRequest GetWebRequest(string url, string method)
        {
            HttpWebRequest httpWebRequest;
            if (url.Contains("https"))
            {
                ServicePointManager.ServerCertificateValidationCallback = CheckValidationResult;
                httpWebRequest = (HttpWebRequest)WebRequest.CreateDefault(new Uri(url));
            }
            else
            {
                httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            }
            httpWebRequest.ServicePoint.Expect100Continue = false;
            httpWebRequest.Method = method;
            httpWebRequest.KeepAlive = true;
            httpWebRequest.ContentType = ContentType;
            httpWebRequest.Referer = url;
            httpWebRequest.Accept = Accept;
            httpWebRequest.UserAgent = UserAgent;
            httpWebRequest.Timeout = Timeout;
            if (Header != null)
            {
                httpWebRequest.Headers = Header;
            }
            return httpWebRequest;
        }

        /// <summary>
        /// CheckValidationResult
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="certificate">certificate</param>
        /// <param name="chain">chain</param>
        /// <param name="errors">errors</param>
        /// <returns></returns>
        private bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {   //直接确认，否则打不开
            return true;
        }

        /// <summary>
        /// 组装普通文本请求参数。
        /// </summary>
        /// <param name="parameters">Key-Value形式请求参数字典</param>
        /// <returns>URL编码后的请求数据</returns>
        private static string BuildQuery(IDictionary<string, string> parameters)
        {
            var postData = new StringBuilder();
            var hasParam = false;
            var dem = parameters.GetEnumerator();
            while (dem.MoveNext())
            {
                string name = dem.Current.Key;
                string value = dem.Current.Value;
                // 忽略参数名或参数值为空的参数
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
                {
                    if (hasParam)
                    {
                        postData.Append("&");
                    }

                    postData.Append(name);
                    postData.Append("=");
                    postData.Append(HttpUtility.UrlEncode(value, Encoding.UTF8));
                    hasParam = true;
                }
            }
            return postData.ToString();
        }
    }
}
