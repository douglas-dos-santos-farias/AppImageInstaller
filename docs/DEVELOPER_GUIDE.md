# Developer Guide

## Visão geral

AppImage Installer é um app desktop Linux em .NET/Avalonia para instalar arquivos `AppImage` com integração de launcher (`.desktop`) e ícone personalizado.

## O que o app faz

Ao instalar, o app:
1. Usa `~/.local/share/applications` por padrão ou uma pasta custom escolhida pelo usuário
2. Copia o AppImage para o diretório de instalação selecionado
3. Copia o ícone para o mesmo diretório
4. Gera o `.desktop` nesse mesmo diretório
5. Salva a última pasta selecionada em `~/.config/AppImageInstaller/settings.json`
6. Aplica permissão de execução no AppImage

Também existe validação de formulário e modal de resultado (sucesso/erro) dentro da própria janela.
Também existe seção de campos customizados `Key=Value` no card `Launcher details`.

## Stack

- .NET `10.0`
- Avalonia `12.0.3` (Desktop + Fluent Theme)
- C# com `Nullable` e `ImplicitUsings` habilitados

## Estrutura do projeto

```text
AppImageInstaller/
  Models/
    InstallRequest.cs
    InstallResult.cs
  Services/
    AppImageInstallerService.cs
    DesktopEntryWriter.cs
    AvaloniaFilePickerService.cs
    IAppImageInstallerService.cs
    IDesktopEntryWriter.cs
    IFilePickerService.cs
  ViewModels/
    MainWindowViewModel.cs
    ViewModelBase.cs
    AsyncCommand.cs
    RelayCommand.cs
  App.axaml
  App.axaml.cs
  MainWindow.axaml
  MainWindow.axaml.cs
  Program.cs
```

## Arquitetura (resumo)

- `MainWindowViewModel`:
  - Estado da tela (paths, nome, categoria, tema, modal de resultado)
  - Validação
  - Comandos de UI
- `AppImageInstallerService`:
  - Regra de instalação no Linux
  - Cópia de arquivos e permissões
- `DesktopEntryWriter`:
  - Serialização do conteúdo `.desktop`
  - Escrita de campos customizados no fim do grupo `[Desktop Entry]`
- `AvaloniaFilePickerService`:
  - Abre diálogo nativo para escolher AppImage e ícone

## Pré-requisitos

- Linux desktop
- SDK do .NET 10 instalado

Verificar:

```bash
dotnet --info
```

## Como executar

Na raiz do repositório:

```bash
dotnet restore
dotnet run --project AppImageInstaller/AppImageInstaller.csproj
```

## Como usar (passo a passo)

1. Abra o app
2. (Opcional) alterne o tema no botão com ícone de sol/lua
3. Clique em `Choose AppImage`
4. Clique em `Choose icon`
5. Ajuste `Display name`
6. Escolha a `Category`
7. Clique em `Install and create desktop entry`
8. Veja o modal de resultado com os caminhos finais

## Comportamento de instalação

Destino atual:
- padrão: `~/.local/share/applications`
- custom: definido via botão `Change folder` na UI

Nomes gerados:
- Baseados no `Display name` (slug)
- Exemplo: `My Cool App` -> `my-cool-app.AppImage`, `my-cool-app.png`, `my-cool-app.desktop`

Permissões:
- AppImage: executável para usuário/grupo/outros
- `.desktop`: leitura para grupo/outros e leitura/escrita para usuário

## Exemplo de `.desktop` gerado

```ini
[Desktop Entry]
Version=1.0
Type=Application
Name=My Cool App
Exec=/home/user/.local/share/applications/my-cool-app.AppImage
Icon=/home/user/.local/share/applications/my-cool-app.png
Categories=Utility;
Terminal=false
```

## Campos customizados no `.desktop`

- A UI permite adicionar/remover pares `Key=Value` em uma lista com rolagem.
- Os campos customizados são gravados ao final do arquivo.
- Em caso de conflito com chaves padrão (`Name`, `Exec`, etc.), a última ocorrência prevalece; portanto o custom sobrescreve por ordem de escrita.

Exemplo:

```ini
[Desktop Entry]
Version=1.0
Type=Application
Name=My Cool App
Exec=/home/user/.local/share/applications/my-cool-app.AppImage
Icon=/home/user/.local/share/applications/my-cool-app.png
Categories=Utility;
Terminal=false
Name=My Cool App (Custom)
X-AppImage-Channel=beta
```

## Tema (claro/escuro)

O app usa:
- `RequestedThemeVariant` do Avalonia
- Paleta customizada aplicada programaticamente para manter consistência entre cards, textos, lista de categorias, botões e modal

Se o tema parecer inconsistente, feche e reabra o app para validar se é cache de compositor da sessão gráfica.

## Limitações atuais

- Escopo v1: apenas instalação (sem desinstalar/atualizar/listar apps instalados)
- Suporte focado em Linux
- Sobrescrita de arquivos no destino quando nomes colidem

## Troubleshooting

- `PlatformNotSupportedException`:
  - O serviço de instalação só roda em Linux
- Ícone não aparece no launcher:
  - Confira se o arquivo de ícone foi copiado e se o `.desktop` aponta para caminho válido
- App não aparece no menu:
  - Valide conteúdo do `.desktop` e permissões do AppImage
- Build falha:
  - Rode `dotnet restore` e depois `dotnet build`

## Build local

```bash
dotnet build
```

## Roadmap sugerido

- Desinstalar app instalado
- Detectar conflito de nomes com confirmação antes de sobrescrever
- Categorias como chips em vez de lista
- Histórico de instalações
- Internacionalização (pt-BR / en-US)

## Licença

Ainda não definida neste repositório.
