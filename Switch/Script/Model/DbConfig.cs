using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Switch.Script.Model
{
    class DbConfig
    {
        public const string Config = "GameConfig";
        public const string Data = "GameData";
        public const string Log = "GameLog";
        public const string PersonalName = "UserId";

        public static string ConfigConnectString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings[Config].ConnectionString;
            }
        }

        public static string DataConnectString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings[Data].ConnectionString;
            }
        }

        public static string LogConnectString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings[Log].ConnectionString;
            }
        }
    }
}
