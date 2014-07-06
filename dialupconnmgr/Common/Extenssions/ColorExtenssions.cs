using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SEAppBuilder.Common.Extenssions
{
    public static class ColorExtenssions
    {
        public static string ToStringEx(this Color color, string format = "#RGB")
        {
            string result = format = format.ToLower();

            result = result.Replace("a", color.A.ToString("X2"));
            result = result.Replace("r", color.R.ToString("X2"));
            result = result.Replace("g", color.G.ToString("X2"));
            result = result.Replace("b", color.B.ToString("X2"));
            
            return result;
        }
    }
}
