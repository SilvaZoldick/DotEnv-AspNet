# DotEnv-AspNet

1- Após adicionar o pacote DotEnvControl pelo nuget, chame o método EnvControl.CriarEnvs() no seu Global.asax;

2- Ponha o nome da pasta do seu diagrama de base de dados, caso use Entity. Se não usar, pode por uma string vazia;

3- Altere o .csproj dos seus projetos para permitir transformção em Build e Rebuild (inclusive para App.Config);

4- Crie uma versão base para cada arquivo de configuração (Web.Base.config e App.Base.config);

5- Edite os arquivos transformadores de configuração dos projetos Web adicionando o seguinte:
   
    <Import Project="..\packages\Microsoft.ApplicationInsights.Web.2.13.1\build\Microsoft.ApplicationInsights.Web.targets" Condition="Exists('..\packages\Microsoft.ApplicationInsights.Web.2.13.1\build\Microsoft.ApplicationInsights.Web.targets')" />  
    <Target Name="BeforeBuild">
        <TransformXml Source="Web.Base.config" Transform="Web.$(Configuration).config" Destination="Web.config" />
    </Target>
    <Target Name="BeforeRebuild">
        <TransformXml Source="Web.Base.config" Transform="Web.$(Configuration).config" Destination="Web.config" />
    </Target>

6- Edite os arquivos transformadores de configuração dos projetos library adicionando o seguinte:
    
    <Import Project="$(MSBuildToolsPath)\Microsoft.CSharp.targets" />
    <UsingTask TaskName="TransformXml" AssemblyFile="$(MSBuildExtensionsPath32)\Microsoft\VisualStudio\v$(VisualStudioVersion)\Web\Microsoft.Web.Publishing.Tasks.dll" />
    <Target Name="BeforeBuild" Condition="Exists('App.$(Configuration).config')">
        <TransformXml Source="App.config" Destination="App.config" Transform="App.$(Configuration).config" />
    </Target>
    <Target Name="AfterBuild" Condition="Exists('App.$(Configuration).config')">
        <TransformXml Source="App.config" Destination="$(IntermediateOutputPath)$(TargetFileName).config" Transform="App.$(Configuration).config" />
        <ItemGroup>
        <AppConfigWithTargetPath Remove="App.config" />
        <AppConfigWithTargetPath Include="$(IntermediateOutputPath)$(TargetFileName).config">
            <TargetPath>$(TargetFileName).config</TargetPath>
        </AppConfigWithTargetPath>
        </ItemGroup>
    </Target>
    <Target Name="BeforeRebuild" Condition="Exists('App.$(Configuration).config')">
        <TransformXml Source="App.config" Destination="App.config" Transform="App.$(Configuration).config" />
    </Target>
    <Target Name="AfterRebuild" Condition="Exists('App.$(Configuration).config')">
        <TransformXml Source="App.config" Destination="$(IntermediateOutputPath)$(TargetFileName).config" Transform="App.$(Configuration).config" />
        <ItemGroup>
        <AppConfigWithTargetPath Remove="App.config" />
        <AppConfigWithTargetPath Include="$(IntermediateOutputPath)$(TargetFileName).config">
            <TargetPath>$(TargetFileName).config</TargetPath>
        </AppConfigWithTargetPath>
        </ItemGroup>
    </Target>
