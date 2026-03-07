# GoogleDrive

A aplicação consiste em uma API desenvolvida em .NET responsável por integrar sistemas com o Google Drive, permitindo o gerenciamento automatizado de arquivos e documentos armazenados na nuvem.

O objetivo principal é centralizar o acesso ao Google Drive por meio de uma camada de serviços, possibilitando operações como upload, download, listagem, atualização e exclusão de arquivos, além da organização automática em pastas e compartilhamento controlado.

Essa integração permite que aplicações internas ou externas utilizem o Google Drive como repositório de documentos, garantindo segurança, rastreabilidade e controle de acesso.


No Google, tudo começa com um "Projeto". É como se fosse um contêiner onde você define quem pode acessar o quê e onde ficam as chaves de segurança.

Para que o seu código C# funcione, o Google exige uma "configuração" entre o computador e os servidores dele. Esse processo é feito no Google Cloud Console. (https://console.cloud.google.com)

## Passo 1: Criar o Projeto no Google Cloud

- Acesse o Google Cloud Console.
- No topo da página, ao lado do logotipo "Google Cloud", clique no menu suspenso de projetos e selecione "Novo Projeto".
- <img width="1011" height="348" alt="image" src="https://github.com/user-attachments/assets/5ff8206e-4789-4795-94cc-ea01412796a2" />
- <img width="1011" height="373" alt="image" src="https://github.com/user-attachments/assets/fb65de25-96ab-4ebd-b5b3-bf4012082911" />

- Dê um nome ao projeto e clique em Criar.
- <img width="973" height="444" alt="image" src="https://github.com/user-attachments/assets/6569f7c0-2809-4060-9215-8248820ee270" />
- Certifique-se de que o novo projeto está selecionado no topo da página após a criação.
- <img width="1011" height="359" alt="image" src="https://github.com/user-attachments/assets/193cf72e-d430-4fa5-b1b2-7d64187d1d31" />


## Passo 2: Ativar a API do Google Drive

- •	No menu lateral esquerdo (as três linhas), vá em APIs e Serviços > Biblioteca.
- <img width="1011" height="555" alt="image" src="https://github.com/user-attachments/assets/20d95575-c17b-4035-8852-15c9d6ca544a" />
- Na barra de pesquisa, digite "Google Drive API".
- <img width="1011" height="453" alt="image" src="https://github.com/user-attachments/assets/ee457583-bb40-427d-912a-4c64a21db66a" />
- <img width="1011" height="403" alt="image" src="https://github.com/user-attachments/assets/f9f53648-de82-46df-bafc-526063e45aae" />
- Clique no resultado encontrado e depois no botão azul Ativar.
- <img width="1011" height="361" alt="image" src="https://github.com/user-attachments/assets/868b2378-71a4-44e9-a033-7ed402f9f21e" />
- <img width="1011" height="319" alt="image" src="https://github.com/user-attachments/assets/bf2b2b13-9fbc-42a6-9287-f24447866d53" />


## Passo 3: Configurar a Tela de Consentimento (OAuth Consent Screen)

Como seu app vai pedir acesso aos arquivos do usuário, você precisa dizer ao Google como essa tela deve aparecer.
- Vá em APIs e Serviços > Tela de permissão OAuth.
- <img width="1011" height="500" alt="image" src="https://github.com/user-attachments/assets/68ab5883-1982-4187-97a8-e6ae42405be7" />
- Preencha: o	Nome do app, e-mail
- <img width="1086" height="623" alt="image" src="https://github.com/user-attachments/assets/647c489f-9638-46d9-a1cd-6f673ecfef7f" />
- Escolha o User Type External (Externo) e clique em Criar.
- <img width="1011" height="538" alt="image" src="https://github.com/user-attachments/assets/23673360-2514-4315-8009-0a2fb569a878" />
- Dados de contato do desenvolvedor: Seu e-mail novamente.
- <img width="1011" height="538" alt="image" src="https://github.com/user-attachments/assets/412409f9-434b-4c00-9070-118f05698df6" />
- Clique em Salvar e Continuar até o final.
- <img width="1011" height="564" alt="image" src="https://github.com/user-attachments/assets/cd651fa1-09af-407b-8344-0d13cb251369" />
- <img width="1011" height="444" alt="image" src="https://github.com/user-attachments/assets/b8c0778d-2b26-485b-a1bc-41551bdddc9c" />

## Passo 4: Definir Usuários de Teste (Crucial para o Erro 403)
- No menu lateral, vá em APIs e serviços > Tela de permissão OAuth (OAuth Consent Screen).
- <img width="1011" height="345" alt="image" src="https://github.com/user-attachments/assets/424ebf37-d523-44aa-bcde-66c9d2f4ba0c" />
- Clique em Criar um cliente OAuth;
- <img width="1011" height="383" alt="image" src="https://github.com/user-attachments/assets/d9b12228-2181-4218-ab3a-e5b0736f050f" />
- Criar o Id do cliente do OAuth
- App para computador: Em Tipo de aplicativo, selecione App para computador.
- <img width="936" height="395" alt="image" src="https://github.com/user-attachments/assets/a4ff94c7-2862-4105-a4a9-329b17fc315c" />
- Clique em Salvar.
- <img width="936" height="719" alt="image" src="https://github.com/user-attachments/assets/3169050c-b0a6-4155-b475-1263b4fd1b63" />
- <img width="936" height="241" alt="image" src="https://github.com/user-attachments/assets/24345364-8cc3-49e7-8a1f-c9dc789abd54" />

## Passo 5: Criar as Credenciais (ClientID e ClientSecret)
- Passo para tratar o erro na execução:
- <img width="1011" height="378" alt="image" src="https://github.com/user-attachments/assets/04233130-7cf2-4369-96ba-311c7763ac29" />
- <img width="1011" height="527" alt="image" src="https://github.com/user-attachments/assets/7ea2f148-11e0-4f5e-9094-ceecbe0b167d" />
- No menu lateral, vá em APIs e Serviços > Tela de permissão OAuth > Público-alvo
- Role a página até encontrar a seção Usuários de teste (Test users).
- Clique no botão + ADD USERS.
- Digite o seu e-mail;
- Clique em Save (Salvar).

