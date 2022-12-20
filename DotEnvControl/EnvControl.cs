using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;

namespace DotEnvControl
{
    public static class EnvControl
    {
        private static string dataDir; // Localização dos arquivos da base de dados (modelo ADO.NET)

        private static DirectoryInfo projectDir, solutionDir, serverDir;

        public static void CriarEnvs(string res)
        {
            dataDir = res;

            projectDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            solutionDir = new DirectoryInfo(projectDir.Parent.FullName);
            serverDir = new DirectoryInfo(solutionDir.Parent.FullName);

            if (serverDir.GetDirectories("envs").Any())
            {
                AdicionarEnvs("dev");
                EditarConfigFile("Web", "Debug");
                EditarConfigFile("App", "Debug");

                AdicionarEnvs("prod");
                EditarConfigFile("Web", "Release");
                EditarConfigFile("App", "Release");
            }
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
        private static string CaminhoEnv(string build)
        {
            string envPath = null;

            if (serverDir != null)
            {
                envPath = $"{serverDir.FullName}/envs/{build}.env";
            }

            return envPath;
        }
        private static XmlDocument GetConfigFile(string tipo, string build)
        {
            XmlDocument doc = new XmlDocument();

            string fileDir = null;
            string nameFile = $"{tipo}.{build}.config";

            foreach (DirectoryInfo projectFolder in solutionDir.GetDirectories())
            {
                if (projectFolder.GetFiles(nameFile).Any())
                {
                    if (tipo != "Web" || projectFolder.Name == projectDir.Name)
                    {
                        fileDir = projectFolder.FullName + "/" + nameFile;
                        break;
                    }
                }
            }

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
            try
            {
                XmlDocument configFile = GetConfigFile(tipo, build);

                if (configFile.InnerXml != "")
                {
                    XmlNode configurationNode;

                    configurationNode = configFile.ChildNodes[1];
                    SetConnectionString(configurationNode.ChildNodes[0]);
                    SetAppSetting(configurationNode.ChildNodes[1]);

                    string fileDir = null;
                    string nameFile = $"{tipo}.{build}.config";

                    foreach (DirectoryInfo projectFolder in solutionDir.GetDirectories())
                    {
                        if (projectFolder.GetFiles(nameFile).Any())
                        {
                            if (tipo != "Web" || projectFolder.Name == projectDir.Name)
                            {
                                fileDir = projectFolder.FullName + "/" + nameFile;
                                break;
                            }
                        }
                    }

                    configFile.Save(fileDir);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Erro: " + e.Message);
            }
        }
        private static void SetConnectionString(XmlNode connectionStringList)
        {
            string connection;

            if (connectionStringList != null)
            {
                foreach (XmlNode connectionString in connectionStringList)
                {
                    connection = $"Data Source={Environment.GetEnvironmentVariable("fonteBase")};" +
                                $"Initial Catalog={Environment.GetEnvironmentVariable("DB")};" +
                                $"User ID={Environment.GetEnvironmentVariable("loginBase")};" +
                                $"Password={Environment.GetEnvironmentVariable("senha")}";

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