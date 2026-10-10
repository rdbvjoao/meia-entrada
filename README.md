# Meia-Entrada

Aplicação web para gerenciar eventos e ingressos, registrar vendas com controle de estoque, consultar o histórico e cancelar vendas com restituição das unidades. Projeto organizado com os artefatos do GitHub Spec Kit.

**Estado:** protótipo funcional estável no ambiente local avaliado; a versão 1.0.0 ainda não está integralmente validada. A última execução automatizada, em 2026-10-10, teve **197 testes aprovados, nenhuma falha ou teste ignorado**, e compilação sem avisos ou erros. Há verificações de interface e avaliação com cinco participantes pendentes. Consulte as [evidências e limitações](specs/001-gestao-eventos-vendas/validation.md).

## Funcionalidades

- Cadastro, consulta, edição e exclusão de eventos, com validação e proteção de vínculos.
- Ingressos inteira e meia-entrada, preços e saldo por evento, com controle de duplicidade e edição desatualizada.
- Conferência e confirmação de vendas, cálculo em centavos e baixa transacional do estoque.
- Histórico com dados originais da venda e situação registrada ou cancelada.
- Cancelamento com restituição única, preservação do histórico e persistência em SQLite.

O escopo não inclui autenticação, pagamentos reais, QR Code ou integrações externas. A execução descrita aqui destina-se ao uso local; publicação em produção não foi validada.

## Tecnologias

C# 14 / .NET 10, ASP.NET Core MVC com Razor, Entity Framework Core e SQLite 10.0.12, Bootstrap 5.3.3 local e xUnit 2.9.3. A solução contém um projeto web e um projeto de testes, sem frontend separado. Os testes incluem regras, contratos HTTP, persistência, transações e concorrência com arquivos SQLite reais.

## Pré-requisitos

- SDK .NET **10.0.401**, exigido por `global.json` com `rollForward=disable`.
- PowerShell e navegador. O ambiente verificado é Windows x64.
- Acesso à restauração de pacotes na preparação e disco local gravável para banco e chaves.

Não é necessário instalar servidor de banco de dados ou Docker. A ferramenta `dotnet-ef` 10.0.12 está fixada no manifesto local.

## Execução local

Abra o PowerShell na raiz desta pasta, onde está `MeiaEntrada.slnx`. Execute com a aplicação parada:

```powershell
dotnet --version
dotnet tool restore
dotnet restore MeiaEntrada.slnx --locked-mode
dotnet build MeiaEntrada.slnx --no-restore

New-Item -ItemType Directory -Path ./var -Force | Out-Null
$dbPath = Join-Path (Get-Location) 'var/meia-entrada.db'
$keyPath = Join-Path (Get-Location) 'var/keys'
$env:ConnectionStrings__DefaultConnection = "Data Source=$dbPath;Foreign Keys=True;Default Timeout=30"
$env:DataProtection__KeyPath = $keyPath
$env:ASPNETCORE_ENVIRONMENT = 'Development'

dotnet ef database update --project src/MeiaEntrada.Web --startup-project src/MeiaEntrada.Web
dotnet run --project src/MeiaEntrada.Web --no-build --no-launch-profile --urls http://localhost:5080
```

Abra `http://localhost:5080`. Encerre com Ctrl+C. Em um novo terminal, redefina as mesmas variáveis antes de executar novamente; preserve o banco e as chaves para manter os dados e a configuração. O schema é preparado pelo comando EF, não por migrations automáticas na inicialização.

Este é um resumo de execução. O [quickstart](specs/001-gestao-eventos-vendas/quickstart.md) contém o procedimento detalhado, migrations, roteiro funcional, cenários de concorrência e protocolo de avaliação humana.

## Testes

Após compilar:

```powershell
dotnet test MeiaEntrada.slnx --no-build --no-restore --logger "trx;LogFileName=meia-entrada.trx" --results-directory TestResults
```

O teste de contenção mantém um escritor ocupado intencionalmente por mais de 30 segundos. Aprovação automatizada não substitui os checkpoints manuais nem a avaliação com cinco participantes.

## Estrutura e documentação

| Caminho | Conteúdo |
|---|---|
| `src/MeiaEntrada.Web/` | Aplicação MVC, serviços, persistência, migrations e recursos locais |
| `tests/MeiaEntrada.Tests/` | Testes unitários, de integração e HTTP |
| [spec.md](specs/001-gestao-eventos-vendas/spec.md) | Histórias, requisitos e critérios de aceite |
| [plan.md](specs/001-gestao-eventos-vendas/plan.md) | Plano técnico e arquitetura |
| [tasks.md](specs/001-gestao-eventos-vendas/tasks.md) | 48 tarefas concluídas e 8 pendentes |
| [validation.md](specs/001-gestao-eventos-vendas/validation.md) | Evidências executadas, relatos manuais e pendências |
| [quickstart.md](specs/001-gestao-eventos-vendas/quickstart.md) | Guia detalhado de execução e validação |

## Entrega local ou pelo GitHub

Inclua fontes, testes, migrations, recursos Bootstrap e licença, documentação Spec Kit, solução, projetos, `global.json`, `.config/dotnet-tools.json`, arquivos `packages.lock.json`, `.editorconfig` e `.gitignore`.

Não inclua bancos locais/WAL/SHM/journal, chaves Data Protection, `var/`, `bin/`, `obj/`, `packages/`, `.dotnet-home/` ou `TestResults/`. O `.gitignore` cobre esses artefatos; ao montar um ZIP, exclua-os explicitamente, pois o ZIP não aplica essas regras automaticamente. Relatórios TRX podem acompanhar a entrega separadamente como evidência, após revisão. Proteja o diretório local de chaves, que usa XML sem criptografia em repouso nesta configuração.

Na revisão de 2026-10-10, esta cópia não tinha `.git`: não foi possível verificar arquivos rastreados, branch ou remoto. Para entrega pelo GitHub, preparar o repositório e revisar os arquivos a adicionar ainda são passos pendentes. Regras de ignore não removem arquivos já rastreados em outra cópia.

Para validar integralmente a versão 1.0.0, completar T021/T027/T037/T042/T049, o restante de T053/T054 e a avaliação real de T055. SC-001/SC-002 permanecem pendentes e SC-003 não está satisfeito.
