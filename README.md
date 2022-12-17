# DotEnv-AspNet

1- Após adicionar o pacote DotEnvControl pelo nuget, chame o método EnvControl.CriarEnvs() no seu Global.asax;
2- Ponha o nome da pasta do seu diagrama de base de dados, caso use Entity. Se não usar, pode por uma string vazia;
3- Altere o .csproj dos seus projetos para permitir transformção em Build e Rebuild (inclusive para App.Config);
4- Crie uma versão base para cada arquivo de configuração (Web.Base.config e App.Base.config);
5- Edite os arquivos transformadores de configuração de acordo com sua necessidade. Deixe somente as tags de declaração e configuração.
