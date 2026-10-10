# Implementation Plan: Meia-Entrada — primeira versão

**Branch**: `001-gestao-eventos-vendas` (identificador retornado pelo Spec Kit; não há branch Git criada neste diretório) | **Date**: 2026-10-08 | **Spec**: [spec.md](spec.md)

**Input**: `specs/001-gestao-eventos-vendas/spec.md` e stack obrigatória definida pelo responsável.

## Summary

Implementar eventos, ingressos, vendas, histórico e cancelamento em uma aplicação web monolítica C# / ASP.NET Core MVC. A interface usa Razor Views, HTML5, CSS3 e Bootstrap 5. EF Core com migrations persiste em SQLite local. Serviços centralizam validações e transações; Controllers e ViewModels tratam entrada e navegação.

A integridade do estoque usa uma transação imediata por operação de escrita, iniciada antes de ler o estado que orienta alterações. O único escritor do SQLite serializa vendas, cancelamentos, edições e exclusões. Versão explícita detecta formulários antigos; cópias históricas preservam vendas; valores monetários são centavos inteiros. O detalhamento está em [research.md](research.md) e [data-model.md](data-model.md).

## Technical Context

**Language/Version**: C# 14, .NET 10 LTS, target net10.0. SDK e patches serão fixados durante a implementação.

**Primary Dependencies**: ASP.NET Core MVC 10, Razor Views, Entity Framework Core 10, Microsoft.EntityFrameworkCore.Sqlite e Design 10.0.x, Microsoft.Data.Sqlite compatível, ferramenta local dotnet-ef 10.0.x; Bootstrap 5.3.x, HTML5 e CSS3. Recursos de interface servidos localmente.

**Storage**: Um arquivo SQLite em disco local, fora de wwwroot, com WAL, Foreign Keys=True e synchronous=FULL. Migrations EF Core versionadas, aplicadas com aplicação parada. Sem servidor de banco separado.

**Testing**: xUnit para regras; SQLite real em arquivo temporário para integração/concorrência/migrations; Microsoft.AspNetCore.Mvc.Testing e WebApplicationFactory para contrato HTTP; roteiro de navegador e avaliação com cinco participantes para SC-001/SC-002.

**Target Platform**: Desenvolvimento e validação local em Windows com SDK .NET 10 e navegador; aplicação ASP.NET Core hospedável em processo único com acesso ao disco local. Publicação não integra esta etapa. SDK dotnet não foi localizado no PATH durante o planejamento; deverá ser instalado para executar o quickstart.

**Project Type**: Aplicação web monolítica MVC, um projeto executável e um projeto de testes, sem frontend ou API separados.

**Performance Goals**: Atender os tempos de tarefa SC-001/SC-002 e os resultados exatos de 20 operações concorrentes SC-004/SC-005. Não há requisito adicional de throughput. Paginação de 25 itens mantém consultas de interface limitadas.

**Constraints**: Um escritor por arquivo SQLite; transações curtas, timeout de aquisição/comandos de 30 segundos, sem repetição automática de operação após commit incerto. Sem pagamentos reais, autenticação, QR Code ou integrações externas. Arquivo local não compartilhado por rede; dinheiro nunca usa ponto flutuante.

**Scale/Scope**: Três entidades, cinco histórias de usuário, 20 requisitos funcionais e sete critérios de sucesso. Uso inicial de pequeno porte, com várias requisições concorrentes sobre o mesmo banco. A garantia é de correção do estoque, não de escrita paralela.

## Constitution Check

Verificação anterior à pesquisa: aprovada com a stack obrigatória, desde que o desenho cubra transações e concorrência do SQLite. Verificação posterior ao desenho: aprovada, sem exceções ou emendas. Estas aprovações avaliam o plano; a implementação ainda precisará demonstrar os resultados.

| Princípio / regra | Evidência no desenho | Antes / depois |
|---|---|---|
| I. Simplicidade | Um monólito MVC, SQLite local, Bootstrap e mecanismos nativos do framework; sem camadas genéricas ou serviços extras | Aprovado / aprovado |
| II. Organização do código | Controllers/Views/ViewModels, Services e Data com responsabilidades distintas; uma política transacional para todas as escritas | Aprovado / aprovado |
| III. Integridade dos dados | BEGIN IMMEDIATE antes das leituras de decisão, constraints/FKs, versão, cópias históricas e cancelamento idempotente | Aprovado / aprovado |
| IV. Segurança básica | Validação servidor, ViewModels permitidos, EF parametrizado, antiforgery, encoding Razor e configuração sem segredos versionados | Aprovado / aprovado |
| V. Qualidade | Testes de regras, banco real, concorrência, falhas e contratos, mais validação manual de interface e usabilidade | Aprovado / aprovado |
| VI. Evolução gradual | Base → eventos → ingressos → vendas → histórico/cancelamento; cada incremento verificável | Aprovado / aprovado |
| Escopo e governança | Requisitos e constituição preservados; nenhuma funcionalidade excluída/adicionada fora do escopo; tecnologias definidas somente no plano | Aprovado / aprovado |

## Project Structure

### Documentation (this feature)

```text
specs/001-gestao-eventos-vendas/
  spec.md                       # Preservada
  checklists/requirements.md    # Preservado
  plan.md
  research.md
  data-model.md
  contracts/web.md
  quickstart.md
```

`tasks.md` será produzido por `$speckit-tasks`, não nesta etapa.

### Source Code (repository root)

Estrutura planejada, ainda não criada:

```text
MeiaEntrada.slnx
global.json
.config/dotnet-tools.json
src/MeiaEntrada.Web/
  MeiaEntrada.Web.csproj
  Program.cs
  appsettings.json
  Controllers/
    EventosController.cs
    IngressosController.cs
    VendasController.cs
  Models/
    Evento.cs
    Ingresso.cs
    Venda.cs
  ViewModels/
    Eventos/
    Ingressos/
    Vendas/
  Services/
    EventoService.cs
    IngressoService.cs
    VendaService.cs
  Data/
    AppDbContext.cs
    Configurations/
    Queries/
    SqliteWriteTransaction.cs
    Migrations/
  Views/
    Shared/
    Eventos/
    Ingressos/
    Vendas/
  wwwroot/
    css/site.css
    lib/bootstrap/
  packages.lock.json
tests/MeiaEntrada.Tests/
  MeiaEntrada.Tests.csproj
  Unit/
  Integration/
  Web/
  Fixtures/
  packages.lock.json
var/                            # Banco/auxiliares/chaves locais, ignorados no Git
```

**Structure Decision**: Um projeto MVC atende o escopo. Serviços coordenam a unidade de negócio e acessam EF por AppDbContext; Data concentra configurações, consultas e a abertura da transação SQLite. SqliteWriteTransaction é um auxiliar específico para não duplicar a política de escrita, não um framework genérico. DbContextFactory cria contexto exclusivo por operação; consultas de tela são sem rastreamento. Não compartilhar contexto entre requisições ou workers.

## Phase 0 — Resultado da pesquisa

[research.md](research.md) resolve plataforma, dependências, concorrência, timeouts, valores, datas, segurança e testes. Fontes oficiais sustentam as decisões. A stack obrigatória foi adotada integralmente; não restam escolhas técnicas bloqueadoras.

## Phase 1 — Desenho e contratos

- [data-model.md](data-model.md): entidades, tipos C#/SQLite, constraints, índices, transições e fronteiras transacionais.
- [contracts/web.md](contracts/web.md): rotas MVC, ViewModels, formatos, antiforgery, conferência protegida, respostas e interface Razor/Bootstrap.
- [quickstart.md](quickstart.md): pré-requisitos, restauração, migrations, execução, testes e resultados esperados. Os comandos serão executáveis depois que a implementação criar os projetos previstos.

### Decisões críticas de implementação

Todas as mutações iniciam uma transação imediata via Microsoft.Data.Sqlite e a associam ao EF na mesma conexão antes de ler os dados atuais. Um único commit abrange venda/baixa ou cancelamento/restituição. Não confiar apenas na transação automática de SaveChanges, nem em trava em memória. Erros desfazem a unidade completa e descartam o contexto.

Ingresso.Versao é incrementada pela aplicação e configurada como concurrency token. Edição compara o valor recebido dentro da transação. Cancelamento relê situação e, se já cancelada, não altera nada. Exclusões e mudança de evento seguem a mesma política de escrita, protegendo vínculos e nomes históricos.

Valores são long em centavos, obtidos de decimal validado; total e reposição usam checked. A conferência é protegida com Data Protection e preço/vínculo/saldo são revalidados ao confirmar. Arquivos SQLite e chaves de proteção ficam fora de wwwroot e do versionamento. Migrations são executadas em manutenção, não em cada inicialização concorrente.

### Cobertura e ordem dos incrementos

| Incremento / requisitos | Entrega planejada | Verificação |
|---|---|---|
| Base transversal: FR-017–FR-020 | MVC/Razor/Bootstrap, EF+migrations, transação, cultura, erros e antiforgery | Build, migrations, validação, segurança e contrato de navegação |
| Eventos: FR-001–FR-002, parte de FR-005 | Cadastro, consulta, edição e exclusão | História 1; vínculos e campos inválidos |
| Ingressos: FR-003–FR-007 | Tipos, preços, saldo, versão e restrições | História 2; unicidade, versão antiga e mudança de vínculo |
| Vendas: FR-008–FR-012 | Seleção/conferência/confirmação e baixa atômica | História 3; SC-004, falhas e preço alterado |
| Histórico/cancelamento: FR-013–FR-016 | Cópias históricas, consulta e restituição única | Histórias 4–5; SC-005–SC-007 |
| Aceitação transversal | Persistência após reinício e navegação completa | SC-001–SC-003 e todos os casos de borda |

Cada incremento inclui os testes necessários, não os adia para o final. Concorrência usa arquivo real, conexões separadas e barreira antes de BEGIN; validar 20 vendas/20 cancelamentos sem contabilizar timeout como rejeição por estoque. Testes separados provocam contenção prolongada e conferem 503/rollback. Injetar falhas entre escritas para verificar ausência de estados parciais. Testar atualização de migrations com dados e sem perda histórica.

## Complexity Tracking

Nenhuma violação constitucional identificada. Transação explícita, versão e cópias históricas são justificadas diretamente pelos requisitos de integridade. Não é necessária arquitetura adicional.

## Estado da etapa

Fases 0 e 1 concluídas no nível de planejamento. A ausência de `.specify/extensions.yml` dispensa hooks anteriores e posteriores. Código-fonte, migrations executáveis, banco e testes da aplicação não foram criados ou executados nesta etapa. Próxima etapa: `$speckit-tasks`.
