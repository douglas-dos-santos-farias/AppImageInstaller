# AppImage Installer

Instalador desktop para Linux feito com .NET + Avalonia para transformar um AppImage em app de menu com ícone e `.desktop` automaticamente.

## Destaques

- Fluxo simples: AppImage + ícone + metadados + instalar
- UI moderna em estilo Bento
- Tema claro/escuro com troca em um clique
- Modal de resultado dentro da própria janela

## Quick Start

Pré-requisito: .NET 10 SDK.

```bash
dotnet restore
dotnet run --project AppImageInstaller/AppImageInstaller.csproj
```

## Como funciona

Ao instalar, o app:
1. Copia arquivos para `~/.local/share/applications`
2. Marca o AppImage como executável
3. Gera o arquivo `.desktop` com `Exec`, `Icon`, `Name` e `Categories`

## Captura de escopo atual (v1)

- Instalação de AppImage no contexto do usuário
- Sem desinstalação/listagem ainda
- Foco em Linux desktop

## Documentação completa

Para arquitetura, estrutura de pastas, troubleshooting e roadmap:

- [Developer Guide](/home/douglas/workspace/AppImageInstaller/docs/DEVELOPER_GUIDE.md)

## Licença

Ainda não definida neste repositório.
