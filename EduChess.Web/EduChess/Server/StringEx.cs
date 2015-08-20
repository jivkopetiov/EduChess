using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace EduChess
{
    public static class StringEx
    {
        public static string CollapseWhitespace(this string self)
        {
            return Regex.Replace(self, @"\s+", " ");
        }
    }
}