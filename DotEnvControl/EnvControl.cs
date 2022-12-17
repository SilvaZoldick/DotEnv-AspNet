using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;

namespace DotEnvControl
{
    public static class DotEnv
    {
        private static string dataDir;
        private static string httpContextPath;
        public static void CriarEnvs(string res)
        {
            dataDir = res;
            httpContextPath = HttpContext.Current.Server.MapPath(null);

            AdicionarEnvs("dev");
            EditarConfigFile("Web", "Debug");
            EditarConfigFile("App", "Debug");

            AdicionarEnvs("prod");
            EditarConfigFile("Web", "Release");
            EditarConfigFile("App", "Release");
        }
        private static void AdicionarEnvs(string build)
        {
            string path = CaminhoEnv(build);
            if (!File.Exists(path))
            {
                return;
            }

            foreach (var line in File.ReadAllLines(path))
            {
                string[] part = line.Split(
                    '=',
                    (char)StringSplitOptions.RemoveEmptyEntries
                    );

                if (part.Length != 2)
                    continue;

                Environment.SetEnvironmentVariable(part[0], part[1]);
            }
        }
        private static string CaminhoEnv(string build, string currentPath = null)
        {
            DirectoryInfo server = new DirectoryInfo(currentPath ?? httpContextPath);

            while (server != null && !server.GetDirectories("envs").Any())
            {
                server = server.Parent;
            }

            string serverPath = server.FullName;
            var envPath = Path.Combine(serverPath.ToString(), $"envs/{build}.env");

            return envPath;
        }
        private static XmlDocument GetConfigFile(string tipo, string build)
        {
            XmlDocument doc = new XmlDocument();
            string fileDir = httpContextPath + "/" +
                            tipo == "Web" ? $"Web.{build}.config" : $"App.{build}.config";

            if (!File.Exists(fileDir))
            {
                return new XmlDocument();
            }
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            XmlReader reader = XmlReader.Create(fileDir, settings);

            doc.Load(reader);
            reader.Close();
            return doc;
        }
        private static void EditarConfigFile(string tipo, string build)
        {
            XmlDocument configFile = GetConfigFile(tipo, build);

            if (configFile.InnerXml != "")
            {
                XmlNode configurationNode;

                configurationNode = configFile.ChildNodes[1];
                SetConnectionString(configurationNode.ChildNodes[0]);
                SetAppSetting(configurationNode.ChildNodes[1]);

                string name = configFile.Name;
                string nameFile = tipo == "Web" ? $"Web.{build}.config" : $"App.{build}.config";

                configFile.Save(httpContextPath + "/" + nameFile);
            }
        }
        private static void SetConnectionString(XmlNode connectionStringList)
        {
            string connection = $"Data Source={Environment.GetEnvironmentVariable("IP")};" +
                                $"Initial Catalog={Environment.GetEnvironmentVariable("Catalog")};" +
                                $"User ID={Environment.GetEnvironmentVariable("User")};" +
                                $"Password={Environment.GetEnvironmentVariable("Senha")}";

            if (connectionStringList != null)
            {
                foreach (XmlNode connectionString in connectionStringList)
                {
                    if (connectionString.Attributes[0].Value.Contains("Entities"))
                    {
                        connection = $"metadata=res://*/{dataDir}.csdl|res://*/{dataDir}.ssdl|res://*/{dataDir}.msl;" +
                                     "provider=System.Data.SqlClient;" +
                                     "provider connection string=\";" +
                                     $"{connection};" +
                                     "multipleactiveresultsets=True;application name=EntityFramework\";";
                    }

                    connectionString.Attributes[1].Value = connection;
                }
            }
        }
        private static void SetAppSetting(XmlNode appSettingsList)
        {
            if (appSettingsList != null)
            {
                foreach (XmlNode appSetting in appSettingsList)
                {
                    appSetting.Attributes[1].Value = Environment.GetEnvironmentVariable(appSetting.Attributes[0].Value);
                }
            }
        }
    }
}
