using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoeSoft
{
    internal class GlobalConfig
    {
        public static string ProxyAddress { get; set; } = string.Empty;

        public static bool IsProxyEnabled { get; set; } = false;
        public static string Player { get; set; } = string.Empty;
    }
}
