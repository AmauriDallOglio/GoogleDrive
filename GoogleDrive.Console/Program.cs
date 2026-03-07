using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using GoogleDrive.Console;
using System.Text.Json;

//https://developers.google.com/workspace/drive/api/reference/rest/v3?hl=pt-br
//https://developers.google.com/workspace/drive/api/guides/folder?hl=pt-br#.net
//https://console.cloud.google.com/cloud-resource-manager?project=natural-motif-489312-q4


internal class Program
{
    static ArquivoLog _arquivoLog = new ArquivoLog();

    static void Main(string[] args)
    {
        try
        {
            GoogleDriveConfigDto _GoogleDriveConfiguracaoDto;
            string caminhoConfig = @"C:\Amauri\GitHub\GOOGLEDRIVE_CONFIGURACAO.txt";
            if (File.Exists(caminhoConfig))
            {
                string json = File.ReadAllText(caminhoConfig);
                _GoogleDriveConfiguracaoDto = JsonSerializer.Deserialize<GoogleDriveConfigDto>(json);
                if (_GoogleDriveConfiguracaoDto == null)
                {
                    _arquivoLog.Error("Configuração do Google Drive está vazia.");
                    return;
                }
            }
            else
            {
                _arquivoLog.Error("Arquivo de configuração não encontrado.");
                return;
            }

            string _nomeDaPastaNoDrive = _GoogleDriveConfiguracaoDto.ApplicationName;
            string _googleClientId = _GoogleDriveConfiguracaoDto.GoogleClientId;
            string _googleClientSecret = _GoogleDriveConfiguracaoDto.GoogleClientSecret;
            string caminhoPastaApp = _GoogleDriveConfiguracaoDto.CaminhoPastaApp;

            string caminhoToken = Path.Combine(caminhoPastaApp, "token.json");


            //UserCredential credential = LoginTarefa(caminhoPastaApp, _googleClientId, _googleClientSecret, caminhoToken);
            UserCredential credential = Login(_googleClientId, _googleClientSecret);
            Espera();

            using (DriveService driveService = new DriveService(new BaseClientService.Initializer() { HttpClientInitializer = credential, ApplicationName = _nomeDaPastaNoDrive }))
            {
                TestarComunicacao(driveService);
                Espera();
                string idPastaDestino = ObterOuCriarPasta(driveService, _nomeDaPastaNoDrive);
                FazerUploadDiretoDaMemoria(driveService, idPastaDestino);
                Espera();
                ListarArquivosDaPasta(driveService, idPastaDestino);
                Espera();
            }
        }
        catch (Exception ex)
        {
            _arquivoLog.Error($"[ERRO CRÍTICO]: {ex.Message}");
            EsperaErro();
        }

        _arquivoLog.Sucesso("\nProcesso finalizado!");
        Espera();

    }

    private static void Espera()
    {
        // Espera 2 segundos (2000 milissegundos)
        System.Threading.Thread.Sleep(2000);
    }

    private static void EsperaErro()
    {
        // Espera 2 segundos (2000 milissegundos)
        System.Threading.Thread.Sleep(10000);
    }

    /// <summary>
    /// Realiza o login e gerencia o token de acesso.
    /// </summary>
    private static UserCredential Login(string _googleClientId, string _googleClientSecret)
    {
        _arquivoLog.Padrao("\n-------------------------------------------");
        _arquivoLog.Padrao("--- Login ---");
        _arquivoLog.Padrao("-------------------------------------------\n");

        ClientSecrets secrets = new ClientSecrets()
        {
            ClientId = _googleClientId,
            ClientSecret = _googleClientSecret
        };

        string[] _scopes = new[] { DriveService.Scope.DriveFile };
        UserCredential resultado = GoogleWebAuthorizationBroker.AuthorizeAsync(secrets, _scopes, "user", CancellationToken.None, new FileDataStore("token.json", true)).Result;
        return resultado;
    }

    private static UserCredential LoginTarefa(string caminhoPastaApp, string _googleClientId, string _googleClientSecret, string caminhoToken)
    {
        _arquivoLog.Padrao("\n-------------------------------------------");
        _arquivoLog.Padrao("--- Login ---");
        _arquivoLog.Padrao("-------------------------------------------\n");

        if (!Directory.Exists(caminhoPastaApp))
            Directory.CreateDirectory(caminhoPastaApp);

        ClientSecrets secrets = new ClientSecrets()
        {
            ClientId = _googleClientId,
            ClientSecret = _googleClientSecret
        };

        var resultado = GoogleWebAuthorizationBroker.AuthorizeAsync(
            secrets,
            new[] { DriveService.Scope.DriveFile },
            "user",
            CancellationToken.None,
            new FileDataStore(caminhoToken, true)).Result;

        return resultado;
    }


    /// <summary>
    /// Verifica conexão
    /// </summary>
    static void TestarComunicacao(DriveService service)
    {
        try
        {
            _arquivoLog.Info("Testando conexão com o Google Drive...");

            // Faz uma requisição mínima apenas para validar o token e a rede
            var request = service.About.Get();
            request.Fields = "user(displayName, emailAddress)";
            var info = request.Execute();

            _arquivoLog.Sucesso("Comunicação OK!");
            _arquivoLog.Sucesso($"Conectado como: {info.User.DisplayName} ({info.User.EmailAddress})");
        }
        catch (Exception ex)
        {
            _arquivoLog.Error("Falha na comunicação!");
            _arquivoLog.Error($"Motivo: {ex.Message}");
            EsperaErro();
            // Aqui você pode decidir se encerra o programa ou tenta novamente
            throw;
        }
    }


    /// <summary>
    /// Verifica se a pasta existe no Drive. Se não existir, cria uma nova.
    /// Retorna o ID da pasta.
    /// </summary>
    private static string ObterOuCriarPasta(DriveService service, string nomePasta)
    {
        _arquivoLog.Padrao("\n-------------------------------------------");
        _arquivoLog.Padrao("--- Obtendo a pasta do Drive ---");
        _arquivoLog.Padrao("-------------------------------------------\n");

        // Busca a pasta no Drive
        FilesResource.ListRequest listRequest = service.Files.List();
        listRequest.Q = $"name = '{nomePasta}' and mimeType = 'application/vnd.google-apps.folder' and trashed = false";
        listRequest.Fields = "files(id, name)";

        Google.Apis.Drive.v3.Data.FileList resultado = listRequest.Execute();
        Google.Apis.Drive.v3.Data.File? pastaExistente = resultado.Files.FirstOrDefault();
        if (pastaExistente != null)
        {
            _arquivoLog.Info($"Pasta encontrada: {pastaExistente.Name} (ID da pasta: {pastaExistente.Id})");
            return pastaExistente.Id;
        }

        _arquivoLog.Padrao("\n-------------------------------------------");
        _arquivoLog.Padrao("--- Criando a pasta no Drive ---");
        _arquivoLog.Padrao("-------------------------------------------\n");

        // Se não existir, cria
        Google.Apis.Drive.v3.Data.File folderMetadata = new Google.Apis.Drive.v3.Data.File()
        {
            Name = nomePasta,
            MimeType = "application/vnd.google-apps.folder"
        };

        FilesResource.CreateRequest folderRequest = service.Files.Create(folderMetadata);
        folderRequest.Fields = "id";
        Google.Apis.Drive.v3.Data.File novaPasta = folderRequest.Execute();

        _arquivoLog.Sucesso($"Nova pasta criada: {nomePasta} (ID da pasta nova: {novaPasta.Id})");
        return novaPasta.Id;
    }




    private static void FazerUploadDiretoDaMemoria(DriveService service, string idPastaDestino)
    {
        _arquivoLog.Padrao("\n-------------------------------------------");
        _arquivoLog.Padrao("--- Criando o arquivo na pasta do Drive ---");
        _arquivoLog.Padrao("-------------------------------------------\n");

        string nomeArquivoData = DateTime.Now.ToString("ddMMyyyy_HHmmss");
        string nomeArquivo = $"{nomeArquivoData}_Oath.txt";
        string conteudoArquivo = $"Arquivo Criado em: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
        _arquivoLog.Info($"Arquivo criado: {nomeArquivo} assunto: {conteudoArquivo}");

        _arquivoLog.Padrao("\n-------------------------------------------");
        _arquivoLog.Padrao("--- Upload do arquivo na pasta do Drive ---");
        _arquivoLog.Padrao("-------------------------------------------\n");


        // Metadata do arquivo para o Google Drive
        byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(conteudoArquivo);
        var fileMetadata = new Google.Apis.Drive.v3.Data.File()
        {
            Name = nomeArquivo,
            Parents = new List<string> { idPastaDestino }
        };
        // 4. Usamos MemoryStream em vez de FileStream
        using (var stream = new MemoryStream(byteArray))
        {
            var uploadRequest = service.Files.Create(fileMetadata, stream, "text/plain");
            uploadRequest.Fields = "id";

            var progresso = uploadRequest.Upload();

            if (progresso.Status == UploadStatus.Completed)
            {
                _arquivoLog.Sucesso($"Sucesso! Arquivo enviado diretamente da memória.");
                _arquivoLog.Sucesso($"ID no Drive: {uploadRequest.ResponseBody.Id}");
            }
            else
            {
                _arquivoLog.Error($"Falha no upload: {progresso.Exception?.Message}");
                EsperaErro();
            }
        }
    }

    /// <summary>
    /// Lista todos os arquivos contidos em uma pasta específica do Google Drive.
    /// </summary>
    private static void ListarArquivosDaPasta(DriveService service, string idPastaDestino)
    {
        _arquivoLog.Padrao("\n-------------------------------------------");
        _arquivoLog.Padrao("--- Listando arquivos na pasta do Drive ---");
        _arquivoLog.Padrao("-------------------------------------------\n");

        var listRequest = service.Files.List();

        // Filtro: arquivos que estão dentro da pasta (parents), não estão na lixeira
        listRequest.Q = $"'{idPastaDestino}' in parents and trashed = false";

        // Solicitamos os campos Nome, ID e Tamanho (em bytes)
        listRequest.Fields = "files(id, name, size, createdTime)";

        var resultado = listRequest.Execute();

        if (resultado.Files != null && resultado.Files.Count > 0)
        {

            _arquivoLog.Info($"-> Total de arquivos: {resultado.Files.Count} ");

            foreach (var arquivo in resultado.Files)
            {
                string extensao = Path.GetExtension(arquivo.Name); // Obtém a extensão (ex: .txt, .pdf, .jpg)
                long? tamanhoKB = arquivo.Size / 1024;  // 2. Calcula o tamanho em KB 
                _arquivoLog.Info($"-> Nome: {arquivo.Name} | Extensão: {extensao} | ID: {arquivo.Id} | Tamanho: {tamanhoKB}KB");
            }
        }
        else
        {
            _arquivoLog.Error("A pasta está vazia.");
            EsperaErro();
        }
        Console.WriteLine("-------------------------------------------\n");
    }

    public class GoogleDriveConfigDto
    {
        public string ApplicationName { get; set; } = string.Empty;

        public string GoogleClientId { get; set; } = string.Empty;  

        public string GoogleClientSecret { get; set; } = string.Empty;  

        public string CaminhoPastaApp { get; set; } = string.Empty; 
    }

}






/*
 
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
*/