using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using GoogleDrive.Console;
using System.Text.Json;

//https://developers.google.com/workspace/drive/api/reference/rest/v3?hl=pt-br
//https://developers.google.com/workspace/drive/api/guides/folder?hl=pt-br#.net
//https://console.cloud.google.com/cloud-resource-manager?project=natural-motif-489312-q4

ArquivoLog _arquivoLog = new ArquivoLog();
GoogleDriveConfiguracaoDto _GoogleDriveConfiguracaoDto = new GoogleDriveConfiguracaoDto();
string _pasta = string.Empty;
string _ApplicationName = string.Empty;


var caminhoConfig = @"C:\Amauri\GitHub\GOOGLEDRIVE_CONFIGURACAO.txt";
if (File.Exists(caminhoConfig))
{
    var json = await File.ReadAllTextAsync(caminhoConfig);
    _GoogleDriveConfiguracaoDto = JsonSerializer.Deserialize<GoogleDriveConfiguracaoDto>(json);
    if (_GoogleDriveConfiguracaoDto == null)
    {
        _arquivoLog.Error("Configuração do Dropbox está vazia.");
        return;
    }
}
_ApplicationName = _GoogleDriveConfiguracaoDto.ApplicationName;
_pasta = _GoogleDriveConfiguracaoDto.Pasta;
_arquivoLog.Padrao($" - RedirectUri: {_ApplicationName}");
_arquivoLog.Padrao($" - Pasta: {_pasta}");



string[] Scopes = { DriveService.Scope.DriveFile };
UserCredential credential;


try
{
    // 1. Autenticação
    using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
    {
        string credPath = "token.json";
        credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            GoogleClientSecrets.FromStream(stream).Secrets,
            Scopes,
            "user",
            CancellationToken.None,
            new FileDataStore(credPath, true));
    }

    var service = new DriveService(new BaseClientService.Initializer()
    {
        HttpClientInitializer = credential,
        ApplicationName = _ApplicationName,
    });

    // 2. Criar uma Pasta
    var folderMetadata = new Google.Apis.Drive.v3.Data.File()
    {
        Name = _pasta,
        MimeType = "application/vnd.google-apps.folder"
    };
    var request = service.Files.Create(folderMetadata);
    request.Fields = "id";
    var folder = await request.ExecuteAsync();
    Console.WriteLine("Pasta criada. ID: " + folder.Id);

    // 3. Enviar um Arquivo para essa pasta
    var fileMetadata = new Google.Apis.Drive.v3.Data.File()
    {
        Name = "TesteUpload.txt",
        Parents = new List<string> { folder.Id } // Define a pasta pai
    };

    FilesResource.CreateMediaUpload requestUpload;
    using (var stream = new FileStream("arquivo_local.txt", FileMode.Open))
    {
        requestUpload = service.Files.Create(fileMetadata, stream, "text/plain");
        requestUpload.Fields = "id";
        await requestUpload.UploadAsync();
    }

    var file = requestUpload.ResponseBody;
    Console.WriteLine("Arquivo enviado com sucesso! ID: " + file.Id);
}
catch (Exception ex)
{
    _arquivoLog.Error($"!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
    _arquivoLog.Error($" Erro: {ex.Message} / {ex.InnerException?.Message ?? ""}");
    _arquivoLog.Error($"!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! \n");
}



public class GoogleDriveConfiguracaoDto
{
    public string ApplicationName { get; set; } = "";
    public string Pasta { get; set; } = "";

}
