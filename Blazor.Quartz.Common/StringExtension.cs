﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor.Quartz.Common
{
    public static class StringExtension
    {
        public static string ToBase64(this string str)
        {
            byte[] base64 = System.Text.Encoding.Default.GetBytes(str);
            return Convert.ToBase64String(base64);
        }
    }
}
