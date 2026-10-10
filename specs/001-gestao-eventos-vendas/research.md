# Pesquisa técnica — Meia-Entrada

**Data**: 2026-10-08 | **Base**: [spec.md](spec.md), constituição 1.0.0 e stack obrigatória definida pelo responsável.

## Questões resolvidas

Arquitetura e tecnologias foram fixadas pelo usuário: C#, ASP.NET Core MVC, Razor Views, HTML5, CSS3, Bootstrap 5, SQLite e Entity Framework Core com migrations, em uma aplicação monolítica. A pesquisa resolve versões compatíveis, concorrência, representação de dados, validação e testes dentro dessa arquitetura. Nenhuma alteração de requisitos é proposta.

## 1. Plataforma e organização

**Decision**: C# 14, .NET 10 LTS, ASP.NET Core MVC 10 e EF Core 10, target `net10.0`. Uma aplicação web com Controllers, ViewModels, Razor Views, Services e Data. Usar `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.EntityFrameworkCore.Design` e ferramenta local `dotnet-ef` da série 10.0.x. Bootstrap 5.3.x, HTML5 e CSS3 locais em `wwwroot`.

**Rationale**: C# 14 integra .NET 10; EF Core 10 requer .NET 10. MVC com views atende formulários e tabelas sem aplicação cliente separada. DbContext e serviços pequenos separam persistência e regras de negócio sem criar vários projetos de produção.

**Alternatives considered**: SPA, API separada, Identity, microsserviços e repositório genérico não são necessários. Não se avalia troca de linguagem, framework ou banco: a stack é obrigatória.

Na implementação, fixar SDK em `global.json`, patches estáveis corrigidos de EF/provider/Design/ferramenta e dependências de teste; versionar `packages.lock.json` e o manifesto local de ferramentas. Versões exatas serão registradas no momento da restauração; não é uma decisão arquitetural pendente.

Fontes: [C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14), [EF Core 10](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-10.0/whatsnew), [suporte .NET](https://dotnet.microsoft.com/en-us/platform/support/policy), [Bootstrap 5.3](https://getbootstrap.com/docs/5.3/getting-started/introduction/).

## 2. SQLite: serializar escritas antes das leituras de decisão

**Decision**: Arquivo SQLite em disco local, com WAL e chaves estrangeiras habilitadas. Cada operação de escrita abre uma conexão/contexto exclusivo, inicia `SqliteConnection.BeginTransaction(deferred: false)` e associa essa transação à mesma conexão do EF por `Database.UseTransaction`. Somente depois lê o estado atual, valida, grava e confirma. Vale para eventos, ingressos, vendas, cancelamentos, exclusões e mudanças de vínculo.

**Rationale**: A transação imediata adquire o direito de escrita antes da leitura que decide o saldo. O SQLite admite um escritor por arquivo; as operações ficam serializadas, inclusive entre conexões/processos que respeitam o protocolo. WAL permite leitores enquanto o escritor trabalha, mas não escritores paralelos. O mecanismo cobre disputas de saldo, exclusão e cópias históricas sem bloqueio por linha.

**Alternatives considered**: Transação adiada pode falhar ao tentar promover uma leitura a escrita. `SaveChanges` isolado não engloba necessariamente a leitura/validação anterior. Semáforo apenas em memória não protege outro processo. O desenho não depende de `SELECT FOR UPDATE` nem de token gerado pelo banco.

Manter transações curtas, sem renderização, rede ou espera do operador. Configuração: `Foreign Keys=True`, `Default Timeout=30`, cache padrão privado, sem `Cache=Shared`; timeout de comandos EF também 30 segundos. Habilitar e conferir WAL na preparação do banco; configurar `synchronous=FULL` em cada conexão. Arquivo e auxiliares WAL/SHM ficam fora de `wwwroot`, no mesmo disco local. Não usar compartilhamento de rede ou múltiplos servidores para o arquivo desta versão.

Fontes: [transações Microsoft.Data.Sqlite](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/transactions), [BeginTransaction](https://learn.microsoft.com/en-us/dotnet/api/microsoft.data.sqlite.sqliteconnection.begintransaction?view=msdata-sqlite-10.0.0), [transação compartilhada com EF](https://learn.microsoft.com/en-us/ef/core/saving/transactions), [isolamento SQLite](https://sqlite.org/isolation.html), [WAL](https://sqlite.org/wal.html), [connection strings](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/connection-strings).

## 3. Contenção, falhas e formulários desatualizados

**Decision**: Usar a espera/repetição limitada do próprio Microsoft.Data.Sqlite para BUSY/LOCKED, até o timeout configurado. Não adicionar repetição automática de toda a operação. Se a transação não começou ou o rollback está confirmado, retornar 503 com mensagem de ocupação e permitir nova tentativa explícita. Commit com resultado incerto gera mensagem para consultar o histórico antes de repetir venda.

**Rationale**: Impede repetição invisível de venda após resultado desconhecido. Falhas não podem deixar uma baixa isolada ou uma restituição isolada. `DbContext` não é compartilhado entre workers; qualquer tentativa futura usa contexto novo, sem entidades rastreadas de uma tentativa anterior.

**Alternatives considered**: Repetição infinita, repetir depois de qualquer erro e devolver insuficiência de estoque quando há apenas banco ocupado seriam incorretos. Tempo de espera esgotado não é conflito de estoque.

Ingresso terá `Versao` long, configurada como `IsConcurrencyToken()`, incrementada pela aplicação em toda edição, venda ou cancelamento efetivo. O serviço compara a versão do formulário com a atual dentro da transação imediata; divergência retorna 409 e requer revisão. O cancelamento verifica novamente a situação: somente a primeira transição registrada→cancelada restitui estoque; repetições não mudam versão nem horário.

Fontes: [erros e timeouts](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/database-errors), [tokens de concorrência EF](https://learn.microsoft.com/en-us/ef/core/saving/concurrency).

## 4. Dinheiro, datas e histórico

**Decision**: Persistir preço, preço histórico e total em centavos `long`/SQLite INTEGER. Entrada validada com `decimal`, sem arredondar casas excedentes; multiplicação, soma de estoque e incremento de versão em contexto `checked`. Datas de evento usam `DateOnly` e `TimeOnly`. Instantes de venda/cancelamento usam `DateTime` UTC, normalizado também na leitura, e apresentação em America/Sao_Paulo.

**Rationale**: Evita limitações de consultas e precisão monetária do provider. Não usar float/double para dinheiro. Formulários preservam a apresentação em reais; centavos são detalhe interno. Venda copia nome do evento, tipo e preço, preservando-os após mudanças cadastrais. Seu evento é alcançado pelo ingresso, cujo vínculo se torna imutável após qualquer venda.

**Alternatives considered**: Valores monetários binários, total aceito do cliente e histórico calculado a partir do preço atual violariam os requisitos. `DateTimeOffset` persistido não é necessário e tem limitações de comparação/ordenação nesse provider.

Usar PK/FK, `DeleteBehavior.Restrict`, unicidade evento/tipo e CHECKs. Validações de domínio permanecem nos serviços; constraints protegem persistência. Não adicionar trilha genérica de alterações nem exclusão lógica.

Fonte: [limitações do provider SQLite](https://learn.microsoft.com/en-us/ef/core/providers/sqlite/limitations). Centavos e cópias históricas são decisões de projeto, não imposições do framework.

## 5. Interface e segurança

**Decision**: Controllers finos com ViewModels de campos permitidos, ModelState e mensagens em português; serviços revalidam regras de negócio. Razor Views com HTML5, CSS3 e Bootstrap 5 local. Filtro global `AutoValidateAntiforgeryToken`, POST para mutações e escape padrão do Razor. Conferência de venda protegida por ASP.NET Core Data Protection com finalidade específica, sem confiar em preço/total livre do cliente.

**Rationale**: Reutiliza o framework para validação, proteção de formulários e apresentação. EF parametriza consultas; SQL necessário ao gerenciamento da conexão contém apenas configuração fixa. Dados do operador não passam por `Html.Raw`. Configuração sensível usa ambiente/user-secrets, sem segredos versionados. Data Protection persiste chaves fora de `wwwroot` e do versionamento.

**Alternatives considered**: Validar apenas no navegador ou vincular entidades diretamente aos formulários permite contornar regras. Autenticação não será introduzida. O uso de antiforgery e proteção de dados não exige login.

Fontes: [validação MVC](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-10.0), [antiforgery](https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery?view=aspnetcore-10.0), [XSS/Razor](https://learn.microsoft.com/en-us/aspnet/core/security/cross-site-scripting?view=aspnetcore-10.0), [Data Protection](https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/using-data-protection?view=aspnetcore-10.0).

## 6. Migrations e verificação

**Decision**: Schema criado e evoluído exclusivamente por migrations EF Core versionadas. Aplicar `dotnet ef database update` com aplicação parada, sem migrations automáticas concorrentes na inicialização e sem `EnsureCreated`. Testar criação do zero e atualização de banco com dados. Testes xUnit, integração MVC por WebApplicationFactory e persistência em arquivos SQLite temporários reais.

**Rationale**: SQLite pode exigir reconstrução de tabelas em alterações; o caminho de migrations precisa preservar dados e constraints. Testes concorrentes usam o mesmo arquivo por cenário, conexões/contextos separados e barreira antes de iniciar a transação. O mecanismo não pode ser validado com EF InMemory ou uma única conexão compartilhada.

**Alternatives considered**: Scripts de migrations `--idempotent` não são suportados pelo provider. Só testes com mocks não demonstram rollback, FKs nem contenção. Não é necessário introduzir um servidor de banco ou serviço auxiliar.

Fontes: [migrations SQLite](https://learn.microsoft.com/en-us/ef/core/providers/sqlite/limitations), [CLI EF](https://learn.microsoft.com/en-us/ef/core/cli/dotnet), [integração ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-10.0).

## Resultado

Todas as decisões da fase de pesquisa estão resolvidas dentro da stack obrigatória. A garantia depende do protocolo de escrita único, constraints e testes reais; WAL sozinho não garante as regras de estoque. Não há mudança na especificação ou constituição. A execução e os testes da aplicação ficam para a implementação.
