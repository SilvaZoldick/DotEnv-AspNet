using GSF.Units;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Configuration;
using System.Web.UI.HtmlControls;
using System.Xml;

namespace TesteEnv
{
    public static class DotEnv
    {
        public static void CriarEnvs(string path)
        {
            if (!File.Exists(path))
                return;

            foreach (var line in File.ReadAllLines(path))
            {
                string[] part = line.Split(
                    '=',
                    (char)StringSplitOptions.RemoveEmptyEntries);

                if (part.Length != 2)
                    continue;

                Environment.SetEnvironmentVariable(part[0], part[1]);
            }
        }
        public static string CaminhoEnv(string currentPath = null)
        {
            DirectoryInfo server = new DirectoryInfo(currentPath ?? HttpContext.Current.Server.MapPath(null));

            while (server != null && !server.GetFiles("*.sln").Any())
            {
                server = server.Parent;
            }

            string serverPath = server.Parent.FullName;
            var envPath = Path.Combine(serverPath.ToString(), ".env");

            return envPath;
        }
        public static void EditarCamposVariaveis()
        {          
            var configuration = WebConfigurationManager.OpenWebConfiguration("~");

            var conStringSection = (ConnectionStringsSection)configuration.GetSection("connectionStrings");
            var appSettingsSection = (AppSettingsSection)configuration.GetSection("appSettings");

            conStringSection.ConnectionStrings["MyConnectionString"].ConnectionString = "Data Source=" + DataSource();
            appSettingsSection.Settings["URL"].Value = Environment.GetEnvironmentVariable("IP");

            configuration.Save();
        }
        private static string DataSource()
        {
            string dataSource = $"server={Environment.GetEnvironmentVariable("ConString")};database=myDb;uid=myUser;password=myPass";
            
            return dataSource;
        }
    }
}