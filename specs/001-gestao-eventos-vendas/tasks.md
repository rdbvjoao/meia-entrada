# Tasks: Meia-Entrada — primeira versão

**Input**: `specs/001-gestao-eventos-vendas/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/web.md, quickstart.md e constituição 1.0.0.
**Tests**: Incluídos por exigência explícita da especificação (premissas, SC-003–SC-007) e da constituição. Escrever os testes de cada história antes da implementação correspondente, observar falha pertinente e executá-los novamente no checkpoint.
**Organization**: Cinco histórias, na ordem de prioridade da especificação. A base contém o schema compartilhado para testar vínculos com fixtures sem depender de telas futuras. Não implementar fluxos de negócio futuros na base.

**Revalidação automatizada — 2026-10-09**: T015–T017, T022–T023, T028, T031, T036, T038–T039, T043–T044 e T046 tiveram a cobertura complementada e reexecutada. Os `[X]` dessas 13 tarefas são mantidos com evidências em [validation.md](validation.md#complementação-de-cobertura-automatizada--2026-10-09), incluindo os arquivos adicionais `*CoberturaTests.cs`. Nenhum checkbox de outra tarefa foi alterado: testes HTTP não substituem os checkpoints de navegador, a avaliação humana ou a revisão final. Permanecem 44 tarefas marcadas e 12 abertas.

**Finalização — 2026-10-09**: T050–T052 e T056 concluídas com evidências na seção final de [validation.md](validation.md#finalização--2026-10-09). Total atual: **48 concluídas e 8 pendentes**. T021/T027/T037/T042/T049 mantêm pendências de interface; T053 tem comandos/testes/documentação concluídos, mas roteiro completo de navegador pendente; T054 e T055 abertas. O relato do responsável comprova fluxos básicos, não todos os cenários. T056 registra a revisão e suas pendências, não aprovação integral da versão.

## Formato e caminhos

**Revisão complementar de entrega — 2026-10-10**: T021/T027/T037/T042/T049 revisadas contra os cenários numerados da especificação na matriz de `validation.md`. Novo relato comprova bloqueio de venda acima do estoque com saldo preservado e presença de venda cancelada no histórico; não comprova segunda tentativa efetiva de cancelamento. Os cinco checkpoints permanecem abertos, assim como T053/T054/T055. Total inalterado: 48/56 concluídas. README de apresentação e execução local preparado; nenhuma tarefa encerrada por essa organização documental.

**Consolidação — 2026-10-10**: permanecem **48/56 tarefas concluídas e 8 pendentes**. T053 e T054 foram decompostas abaixo em partes com evidência e partes abertas, sem alterar os 56 IDs nem contabilizar partes como tarefas adicionais. O relato manual detalhado e os checkpoints restantes estão na seção “Consolidação” de `validation.md`. T055 permanece pendente; SC-003 não está satisfeito.

Cada tarefa usa `- [ ] Tnnn [P?] [USn?] descrição com caminho`. `[P]` permite trabalho simultâneo somente dentro dos grupos e pré-requisitos descritos ao final. Caminhos são relativos à raiz. Aplicação em `src/MeiaEntrada.Web/`, testes em `tests/MeiaEntrada.Tests/`. Não criar frontend separado, autenticação, pagamentos, QR Code ou integrações.

## Phase 1: Setup — estrutura compartilhada

**Objetivo**: Preparar a solução MVC e ferramentas fixadas; não executar publicação.

- [X] T001 Verificar disponibilidade do SDK .NET 10, preparar instalação se ausente e fixar versão compatível com C# 14 em `global.json`; registrar versão efetivamente utilizada e bloqueios em `specs/001-gestao-eventos-vendas/quickstart.md`.
- [X] T002 Criar `MeiaEntrada.slnx`, `src/MeiaEntrada.Web/MeiaEntrada.Web.csproj` e `tests/MeiaEntrada.Tests/MeiaEntrada.Tests.csproj` com net10.0, MVC/Razor, EF Core Sqlite/Design 10.0.x, Microsoft.Data.Sqlite compatível, xUnit e Microsoft.AspNetCore.Mvc.Testing; fixar patches e gerar `src/MeiaEntrada.Web/packages.lock.json`, `tests/MeiaEntrada.Tests/packages.lock.json` e `.config/dotnet-tools.json` com dotnet-ef compatível.
- [X] T003 [P] Configurar `.gitignore` para var/, bancos SQLite/WAL/SHM, chaves, bin/, obj/ e TestResults/ e convenções C#/Razor em `.editorconfig`, sem versionar segredos.
- [X] T004 [P] Incorporar Bootstrap 5.3.x local em `src/MeiaEntrada.Web/wwwroot/lib/bootstrap/css/bootstrap.min.css` e `src/MeiaEntrada.Web/wwwroot/lib/bootstrap/js/bootstrap.bundle.min.js`, incluindo licença em `src/MeiaEntrada.Web/wwwroot/lib/bootstrap/LICENSE`; remover dependência de CDN.

## Phase 2: Foundational — pré-requisitos bloqueadores

**Objetivo**: Disponibilizar persistência, validação, transação e infraestrutura HTTP/testes para todas as histórias. Completar esta fase antes dos fluxos das histórias.

- [X] T005 [P] Criar `src/MeiaEntrada.Web/Models/Evento.cs` e `src/MeiaEntrada.Web/Data/Configurations/EventoConfiguration.cs` com Id long gerado, "Identificação estável"; Nome "Obrigatório, remover espaços das extremidades, rejeitar vazio"; Descricao "Obrigatória, texto simples"; Local "Obrigatório, texto simples"; Data DateOnly "Data válida, inclusive passada"; Horario TimeOnly "Horário local válido; formulário com precisão de minuto"; textos TEXT NOT NULL e não vazios, sem unicidade de nome nem limite textual comercial adicional.
- [X] T006 [P] Criar `src/MeiaEntrada.Web/Models/Ingresso.cs`, `src/MeiaEntrada.Web/Models/TipoIngresso.cs` e `src/MeiaEntrada.Web/Data/Configurations/IngressoConfiguration.cs`: Id "Identificação estável"; EventoId "Obrigatória, DeleteBehavior.Restrict"; Tipo "`inteira` ou `meia_entrada`" por conversão explícita; PrecoCentavos "Não negativo; zero permitido"; QuantidadeDisponivel "Inteiro não negativo; saldo atual"; Versao "Inicia em 1; gerida pela aplicação" com IsConcurrencyToken(), índice único (EventoId, Tipo), índice FK e CHECKs de tipo, não negativos e versão positiva; usar long para inteiros e "typeof(coluna) = 'integer'" nas constraints aplicáveis.
- [X] T007 [P] Criar `src/MeiaEntrada.Web/Models/Venda.cs`, `src/MeiaEntrada.Web/Models/SituacaoVenda.cs` e `src/MeiaEntrada.Web/Data/Configurations/VendaConfiguration.cs`: Id "Identificação estável"; IngressoId "Obrigatória, imutável, DeleteBehavior.Restrict"; EventoNome TEXT NOT NULL "Cópia do nome ao registrar"; IngressoTipo "Cópia de `inteira` ou `meia_entrada`"; Quantidade long "Estritamente positiva"; PrecoUnitarioCentavos long "Cópia do preço confirmado, >= 0"; ValorTotalCentavos long "Produto exato de preço unitário e quantidade"; RegistradaEmUtc DateTime "Instante UTC do servidor"; Situacao "`registrada` ou `cancelada`"; CanceladaEmUtc DateTime? TEXT anulável "Preenchida uma única vez ao cancelar"; não duplicar EventoId, normalizar Kind UTC na leitura, adicionar índices FK e (RegistradaEmUtc DESC, Id DESC), CHECKs de inteiros/tipo/situação/total sem overflow e coerência "registrada exige CanceladaEmUtc nulo; cancelada exige valor não nulo".
- [X] T008 [P] Centralizar parsing e cálculos em `src/MeiaEntrada.Web/Services/ValidacaoEntrada.cs` e `src/MeiaEntrada.Web/Services/ValoresMonetarios.cs`: textos sem whitespace vazio, IDs positivos, inteiros long sem frações, preço decimal textual sem milhares com ponto ou vírgula e até duas casas; rejeitar expoentes, NaN, infinito, negativos, sintaxe ambígua e overflow sem arredondar; converter para centavos long, validar preço <= long.MaxValue / quantidade e usar checked em produto, restituição e versão; não impor teto comercial adicional.
- [X] T009 Criar `src/MeiaEntrada.Web/Data/AppDbContext.cs`, `src/MeiaEntrada.Web/Data/AppDbContextFactory.cs`, `src/MeiaEntrada.Web/Data/SqliteConnectionInterceptor.cs` e `src/MeiaEntrada.Web/appsettings.json` com configurações das três entidades, contextos exclusivos por operação, ConnectionStrings:DefaultConnection e DataProtection:KeyPath fora de wwwroot, Foreign Keys=True, cache privado, timeout provider/EF de 30 s, synchronous=FULL por conexão e preparação/verificação WAL; suportar CLI EF sem EnsureCreated ou migrations automáticas no startup.
- [X] T010 Implementar `src/MeiaEntrada.Web/Data/SqliteWriteTransaction.cs`: abrir contexto/conexão próprios, BeginTransaction(deferred: false), UseTransaction na mesma conexão antes de toda leitura de decisão, commit único e rollback integral/descarte em falha; distinguir BUSY/LOCKED com rollback confirmado de commit incerto, sem semáforo em memória nem repetição automática da operação.
- [X] T011 Gerar migration inicial em `src/MeiaEntrada.Web/Data/Migrations/InitialCreate.cs` e snapshot `src/MeiaEntrada.Web/Data/Migrations/AppDbContextModelSnapshot.cs` com FKs, CHECKs e índices de T005–T007; preservar o prefixo temporal gerado pelo EF no nome físico da migration e aplicar apenas em manutenção com aplicação parada.
- [X] T012 [P] Configurar `src/MeiaEntrada.Web/Program.cs`, `src/MeiaEntrada.Web/Controllers/ErrosController.cs` e `src/MeiaEntrada.Web/Views/Shared/Erro.cshtml` com DI, MVC, pt-BR, Data Protection persistido, antiforgery global, erro seguro 400/404/405/409/503/500 e redirect GET / para /eventos/; permitir execução local HTTP do quickstart, não expor detalhes internos em Production e orientar consulta ao histórico se commit for incerto.
- [X] T013 [P] Criar layout e componentes em `src/MeiaEntrada.Web/Views/Shared/_Layout.cshtml`, `src/MeiaEntrada.Web/Views/Shared/_Mensagens.cshtml`, `src/MeiaEntrada.Web/Views/Shared/_Paginacao.cshtml`, `src/MeiaEntrada.Web/Views/_ViewImports.cshtml`, `src/MeiaEntrada.Web/Views/_ViewStart.cshtml` e `src/MeiaEntrada.Web/wwwroot/css/site.css` com navegação Eventos/Ingressos/Registrar venda/Histórico, Bootstrap local, labels, erros textuais, teclado e tabelas responsivas; links de histórias futuras serão exercitados nos respectivos checkpoints.
- [X] T014 [P] Criar `tests/MeiaEntrada.Tests/Fixtures/SqliteFixture.cs`, `tests/MeiaEntrada.Tests/Fixtures/WebAppFactory.cs` e `tests/MeiaEntrada.Tests/Fixtures/AntiforgeryClient.cs` com arquivos temporários reais migrados, fixtures de eventos/ingressos/vendas inclusive canceladas, conexões separadas por worker, barreira antes de BEGIN, deadline e leitura final com contexto novo; HTTP deve obter cookie/token real e permitir desativar redirects, sem desabilitar antiforgery.
- [X] T015 Criar e executar `tests/MeiaEntrada.Tests/Integration/MigrationsTests.cs`, `tests/MeiaEntrada.Tests/Integration/SqliteWriteTransactionTests.cs` e `tests/MeiaEntrada.Tests/Unit/ValidacaoEntradaTests.cs`: banco do zero, update repetido preservando dados, FKs/CHECKs/índices, inteiros versus REAL, total incoerente/overflow, WAL/FK/FULL por conexão, rollback, parsing e limites; verificar atualização desde migration anterior quando existir uma segunda, sem criar migration artificial só para o teste.

**Checkpoint**: Restore em locked mode, build e testes da base passam; schema e política de escrita disponíveis.

## Phase 3: US1 — Gerenciar eventos (P1) — MVP

**Goal**: Cadastrar, consultar, editar e excluir eventos com validação e proteção de vínculos (FR-001–FR-002, FR-005).
**Independent Test**: Partindo de banco vazio, criar, listar, consultar, editar e excluir evento sem vínculos; fixtures de ingresso/venda permitem testar exclusão bloqueada sem implementar outras telas.

- [X] T016 [P] [US1] Escrever `tests/MeiaEntrada.Tests/Integration/EventosTests.cs` cobrindo os cinco cenários da US1: campos obrigatórios/whitespace, datas e horários inválidos, datas passadas, nomes repetidos, ID estável, exclusão sem vínculos e bloqueio com ingressos/vendas inclusive canceladas; confirmar preservação histórica após edição com fixture.
- [X] T017 [P] [US1] Escrever `tests/MeiaEntrada.Tests/Web/EventosContratoWebTests.cs` para todas as rotas /eventos/ do contrato, GET sem mutação, antiforgery, confirmação/desistência, entradas preservadas, 302/400/404/405/409, campos extras e HTML codificado; testar ordenação, 26 registros, página inválida/inexistente e estado vazio.
- [X] T018 [US1] Criar `src/MeiaEntrada.Web/ViewModels/Eventos/EventoFormViewModel.cs`, `src/MeiaEntrada.Web/ViewModels/Eventos/EventoDetalheViewModel.cs` e `src/MeiaEntrada.Web/Services/EventoService.cs` com somente campos permitidos, formatos YYYY-MM-DD/HH:MM, validação compartilhada e CRUD via transação imediata; reler existência/vínculos antes de editar/excluir, sem reescrever vendas.
- [X] T019 [US1] Implementar consultas AsNoTracking em `src/MeiaEntrada.Web/Data/Queries/EventoQueries.cs` e rotas explícitas em `src/MeiaEntrada.Web/Controllers/EventosController.cs`: lista de 25 por página ordenada por Data/Horario/Id crescentes, detalhe, novo, editar e excluir com Confirmar=sim; mapear os resultados HTTP do contrato sem SaveChanges no controller.
- [X] T020 [US1] Criar `src/MeiaEntrada.Web/Views/Eventos/Index.cshtml`, `src/MeiaEntrada.Web/Views/Eventos/Detalhes.cshtml`, `src/MeiaEntrada.Web/Views/Eventos/Novo.cshtml`, `src/MeiaEntrada.Web/Views/Eventos/Editar.cshtml` e `src/MeiaEntrada.Web/Views/Eventos/Excluir.cshtml` com campos completos, resumo de erros, mensagens, estado vazio, paginação, identificação na confirmação e link aos ingressos.
- [ ] T021 [US1] Executar testes T016–T017 e fluxo de navegador da US1, corrigir falhas e registrar evidências/pendências em `specs/001-gestao-eventos-vendas/validation.md`, incluindo ausência de mutação ao desistir e preservação dos campos válidos.

**Checkpoint**: US1 utilizável isoladamente; MVP de cadastro de eventos, ainda sem ciclo de vendas.

## Phase 4: US2 — Gerenciar ingressos (P1)

**Goal**: Manter tipos, preços e saldo atual com unicidade e proteção contra formulários antigos (FR-003–FR-007).
**Independent Test**: Com evento em fixture, executar CRUD de ingressos, zeros, duplicidade e versão obsoleta; vendas registradas/canceladas preparadas comprovam restrições de vínculo.

- [X] T022 [P] [US2] Escrever `tests/MeiaEntrada.Tests/Integration/IngressosTests.cs` para os sete cenários US2: evento inexistente, enums, preço/quantidade inválidos, zero permitido, par único, mudança de evento/tipo com e sem vendas, exclusão vinculada e edição como saldo atual; simular versão alterada por edição/movimentação inclusive saldo que retorna ao mesmo número, sem sobrescrita e sem incremento em falha.
- [X] T023 [P] [US2] Escrever `tests/MeiaEntrada.Tests/Web/IngressosContratoWebTests.cs` para todas as rotas /ingressos/, filtro evento, pré-seleção, paginação/ordem, estados vazios, confirmação, antiforgery, overposting e 400/404/405/409; conflito deve exibir saldo atual e valor tentado separadamente e exigir revisão com versão atual.
- [X] T024 [US2] Criar `src/MeiaEntrada.Web/ViewModels/Ingressos/IngressoFormViewModel.cs` e `src/MeiaEntrada.Web/Services/IngressoService.cs` com parsing explícito dos tipos e valores, Versao exigida na edição, comparação e incremento checked dentro da transação; reler evento/unicidade/vendas, permitir preço e saldo zero e impedir troca de evento/tipo ou exclusão após qualquer venda.
- [X] T025 [US2] Criar `src/MeiaEntrada.Web/Data/Queries/IngressoQueries.cs` e `src/MeiaEntrada.Web/Controllers/IngressosController.cs` com todas as rotas do contrato, consultas AsNoTracking, filtro por evento, ordem EventoId/Tipo/Id, páginas de 25 e mapeamento de conflitos/confirmar/redirect.
- [X] T026 [US2] Criar `src/MeiaEntrada.Web/Views/Ingressos/Index.cshtml`, `src/MeiaEntrada.Web/Views/Ingressos/Detalhes.cshtml`, `src/MeiaEntrada.Web/Views/Ingressos/Novo.cshtml`, `src/MeiaEntrada.Web/Views/Ingressos/Editar.cshtml` e `src/MeiaEntrada.Web/Views/Ingressos/Excluir.cshtml` com seletores, preço BRL, saldo atual, Versao oculta e formulário de revisão de conflito; informar ausência de evento e restrições de vínculo sem impedir alterações permitidas.
- [ ] T027 [US2] Executar testes T022–T023 e fluxo US2 com fixture de evento, corrigir falhas e registrar em `specs/001-gestao-eventos-vendas/validation.md` resultados de CRUD, duplicidade, zero, versão obsoleta, bloqueios inclusive canceladas e preservação histórica.

**Checkpoint**: Oferta configurável sem executar o fluxo de vendas; eventos continuam funcionando.

## Phase 5: US3 — Registrar vendas (P1)

**Goal**: Selecionar, conferir e confirmar venda com baixa atômica e histórico original (FR-008–FR-012).
**Independent Test**: Evento e ingresso em fixture; vender 3 de 10 a R$ 40,00 gera R$ 120,00 e saldo 7; entradas inválidas e falta de estoque não gravam nada.

- [X] T028 [P] [US3] Escrever `tests/MeiaEntrada.Tests/Integration/VendasTests.cs` e `tests/MeiaEntrada.Tests/Unit/VendaCalculoTests.cs` cobrindo sete cenários US3, venda gratuita/exata, seleção incompatível, limites de total/versão, cópias históricas, mudança de preço exigindo reconfirmação e ausência de reserva na conferência.
- [X] T029 [P] [US3] Escrever parte de vendas em `tests/MeiaEntrada.Tests/Integration/EstoqueConcorrenciaTests.cs`: 20 vendas sobre saldo 10 devem resultar exatamente em 10 sucessos/10 recusas por estoque/saldo 0, e duas vendas de 4 sobre saldo 5 em um sucesso/saldo 1; conexões independentes, barreira antes de BEGIN e nenhum timeout contabilizado como insuficiência.
- [X] T030 [P] [US3] Escrever parte de vendas em `tests/MeiaEntrada.Tests/Integration/AtomicidadeTests.cs` usando `tests/MeiaEntrada.Tests/Fixtures/FalhaPersistenciaInterceptor.cs` para falhar entre escritas e antes de commit; conferir ausência de venda/baixa/versão parciais, descarte do contexto e nenhuma rota de injeção de falha na aplicação.
- [X] T031 [P] [US3] Escrever `tests/MeiaEntrada.Tests/Web/VendasContratoWebTests.cs` para GET seleção, POST conferir, POST confirmar e detalhe: tokens adulterados, antiforgery, Confirmar ausente, preço/total extras ignorados, 302/400/404/405/409, preço alterado com novo resumo/token e orientação segura diante de commit incerto.
- [X] T032 [US3] Criar `src/MeiaEntrada.Web/ViewModels/Vendas/VendaSelecaoViewModel.cs`, `src/MeiaEntrada.Web/ViewModels/Vendas/VendaConferenciaViewModel.cs` e `src/MeiaEntrada.Web/Services/VendaConferenciaService.cs` com Data Protection de {EventoId, IngressoId, Tipo, PrecoUnitarioCentavos, Quantidade}, finalidade MeiaEntrada.Venda.Conferencia.v1, parsing/validação do conteúdo e cálculo compartilhado; não manter reserva nem transação entre telas.
- [X] T033 [US3] Implementar registro em `src/MeiaEntrada.Web/Services/VendaService.cs`: desproteger antes da transação, reler existência/vínculo/tipo/preço/saldo depois de BEGIN, exigir reconfirmação de preço e nova seleção para vínculo/tipo alterados; salvar cópias históricas e instante UTC do servidor, reduzir saldo e incrementar Versao em um commit; reenvio válido é nova venda sujeita ao saldo, sem repetição automática.
- [X] T034 [US3] Criar `src/MeiaEntrada.Web/Data/Queries/VendaQueries.cs`, `src/MeiaEntrada.Web/ViewModels/Vendas/VendaDetalheViewModel.cs` e `src/MeiaEntrada.Web/Controllers/VendasController.cs` com GET /vendas/nova/?evento=, POST /vendas/conferir/, POST /vendas/ e GET /vendas/{id}/; detalhe já consulta cópias históricas, converte UTC para America/Sao_Paulo e BRL pt-BR para suportar redirect de sucesso sem depender da US4.
- [X] T035 [US3] Criar `src/MeiaEntrada.Web/Views/Vendas/Nova.cshtml`, `src/MeiaEntrada.Web/Views/Vendas/Conferir.cshtml` e `src/MeiaEntrada.Web/Views/Vendas/Detalhes.cshtml` com seleção via GET sem JavaScript obrigatório, preço/saldo/total, token protegido, confirmação explícita, correção de insuficiência e indisponibilidade para evento sem oferta ou saldo zero.
- [X] T036 [US3] Escrever e executar `tests/MeiaEntrada.Tests/Integration/VinculosConcorrenciaTests.cs` disputando venda com exclusão de evento/ingresso, troca de evento/tipo, edição de nome/preço/saldo; verificar ordem serial válida, ausência de venda órfã, cópias consistentes e rejeição de versão antiga; nenhuma falha pode relaxar SC-007.
- [ ] T037 [US3] Executar T028–T031/T036 e fluxo US3, corrigir falhas e registrar contagens SC-004 e evidências de atomicidade em `specs/001-gestao-eventos-vendas/validation.md` com versão de SDK/pacotes e relatório de testes.

**Checkpoint**: Venda completa com resultado consultável e estoque consistente; histórico paginado será entregue em US4.

## Phase 6: US4 — Consultar histórico (P2)

**Goal**: Consultar todas as vendas e seus dados originais, inclusive canceladas (FR-013, FR-016 e preservação FR-006/FR-012).
**Independent Test**: Fixtures de vendas registradas/canceladas, incluindo datas empatadas e mais de 25 registros; conferir ordem, acesso a todas as páginas e valores históricos após mudanças cadastrais.

- [X] T038 [P] [US4] Escrever `tests/MeiaEntrada.Tests/Integration/HistoricoTests.cs` com histórico vazio, registrado/cancelado, empate de instante, paginação e preservação de nome/tipo/preço/quantidade/total após alterações, usando fixtures sem depender do cancelamento implementado.
- [X] T039 [P] [US4] Escrever `tests/MeiaEntrada.Tests/Web/HistoricoContratoWebTests.cs` para GET /vendas/ e detalhe, BRL/horário de Brasília, página inválida/inexistente 404, vazio 200 e campos completos; tentativas de edição/exclusão/reativação retornam 404/405 sem mutação.
- [X] T040 [US4] Estender `src/MeiaEntrada.Web/Data/Queries/VendaQueries.cs` e `src/MeiaEntrada.Web/Controllers/VendasController.cs` para histórico AsNoTracking ordenado por RegistradaEmUtc/Id decrescentes com 25 itens por página, usando exclusivamente cópias históricas; não adicionar rotas de editar/excluir/reativar vendas.
- [X] T041 [US4] Criar `src/MeiaEntrada.Web/Views/Vendas/Index.cshtml` e completar `src/MeiaEntrada.Web/Views/Vendas/Detalhes.cshtml` com identificação, evento/tipo originais, quantidade, total, instante, situação, preço unitário e instante de cancelamento no detalhe; estado vazio oferece registrar venda e paginação alcança todas as vendas.
- [ ] T042 [US4] Executar testes T038–T039 e roteiro independente da US4, corrigir falhas e registrar em `specs/001-gestao-eventos-vendas/validation.md` a preservação de venda de 2 × R$ 40,00 = R$ 80,00 após edição de nome/preço e acesso a registros cancelados.

**Checkpoint**: Histórico completo independente da execução do fluxo de cancelamento.

## Phase 7: US5 — Cancelar vendas (P2)

**Goal**: Cancelar com restituição atômica única, mantendo o histórico (FR-014–FR-016).
**Independent Test**: Fixture com venda de 3 e saldo 7; cancelar produz saldo 10, repetir preserva saldo/versão/instante; desistir não altera nada.

- [X] T043 [P] [US5] Escrever `tests/MeiaEntrada.Tests/Integration/CancelamentosTests.cs` para seis cenários US5, inexistência, repetição sem escrita, dados históricos imutáveis, restituição após ajuste manual (20 + 3 = 23), venda gratuita e overflow de saldo/versão sem mutação.
- [X] T044 [P] [US5] Estender `tests/MeiaEntrada.Tests/Integration/EstoqueConcorrenciaTests.cs` com 20 cancelamentos da mesma venda (uma transição, restituição única e instante estável), venda de 2 simultânea a cancelamento de 3 sobre saldo 7 (saldo final 8) e edição antiga após venda/cancelamento; não usar coordenação em memória para garantir integridade.
- [X] T045 [P] [US5] Estender `tests/MeiaEntrada.Tests/Integration/AtomicidadeTests.cs` para falhas entre atualização de situação/estoque e antes de commit no cancelamento; conferir situação, saldo, Versao, instante e histórico totalmente anteriores ou totalmente concluídos.
- [X] T046 [P] [US5] Escrever `tests/MeiaEntrada.Tests/Web/CancelamentosContratoWebTests.cs` para GET/POST /vendas/{id}/cancelar/, antiforgery, confirmação ausente, desistência, inexistência, redirect e repetição com mensagem “Venda já cancelada”, sem alterar primeiro instante nem saldo.
- [X] T047 [US5] Implementar cancelamento em `src/MeiaEntrada.Web/Services/VendaService.cs`: reler venda/ingresso/situação na transação imediata, retornar sem escrita se cancelada; caso registrada, somar quantidade ao saldo vigente e incrementar Versao com checked, alterar somente Situacao/CanceladaEmUtc e confirmar tudo atomicamente.
- [X] T048 [US5] Adicionar GET/POST de cancelamento em `src/MeiaEntrada.Web/Controllers/VendasController.cs` e `src/MeiaEntrada.Web/ViewModels/Vendas/VendaCancelamentoViewModel.cs`, criar `src/MeiaEntrada.Web/Views/Vendas/Cancelar.cshtml` e integrar ações em `src/MeiaEntrada.Web/Views/Vendas/Index.cshtml` e `src/MeiaEntrada.Web/Views/Vendas/Detalhes.cshtml`; identificar venda, exigir Confirmar=sim e oferecer Voltar sem mutação.
- [ ] T049 [US5] Executar T043–T046 e roteiro US5, corrigir falhas e registrar em `specs/001-gestao-eventos-vendas/validation.md` contagens SC-005, concorrência mista, SC-006 e preservação de instante/versão/histórico na repetição.

**Checkpoint**: Ciclo completo de venda e cancelamento; todas as histórias verificadas isoladamente.

## Phase 8: Polish & Cross-Cutting Concerns

**Objetivo**: Demonstrar segurança, persistência, navegação e critérios de aceitação completos sem ampliar o escopo.

- [X] T050 [P] Criar e executar `tests/MeiaEntrada.Tests/Web/ContratoWebTests.cs` consolidando navegação e status HTTP comuns, XSS como texto, overposting, antiforgery em todos os POSTs e erros Production sem SQL/caminhos/stack traces; provocar commit incerto por interceptação de teste e conferir orientação para consultar histórico sem repetição automática.
- [X] T051 [P] Criar e executar `tests/MeiaEntrada.Tests/Integration/ContencaoTests.cs` com conexão A retendo escrita por mais de 30 s e B independente; verificar timeout real, rollback/ausência de mutação e resposta 503 “Sistema ocupado; tente novamente”, liberando A em finally sem bloquear sua thread; manter esse cenário separado dos testes SC-004/SC-005.
- [X] T052 [P] Criar e executar `tests/MeiaEntrada.Tests/Integration/PersistenciaReinicioTests.cs` reabrindo aplicação/banco após CRUD, vendas e cancelamentos e conferindo cadastros/saldos/histórico/chaves persistidos; complementar `tests/MeiaEntrada.Tests/Integration/MigrationsTests.cs` com atualização desde versão anterior se houver e preservação de constraints e dados.
- [ ] T053 Executar restore locked, build, suíte completa e roteiro do quickstart, corrigir falhas e atualizar comandos reais/evidências em `specs/001-gestao-eventos-vendas/quickstart.md` e `specs/001-gestao-eventos-vendas/validation.md`; verificar filtros EstoqueConcorrenciaTests, AtomicidadeTests, ContratoWebTests e MigrationsTests, sem testes críticos ignorados.
- [ ] T054 Verificar em navegador todas as histórias, telas vazias, mensagens, correção de entrada, confirmações/desistência, teclado/foco e telas menores; corrigir `src/MeiaEntrada.Web/Views/Shared/_Layout.cshtml`, views afetadas e `src/MeiaEntrada.Web/wwwroot/css/site.css` conforme achados, registrando evidências e pendências em `specs/001-gestao-eventos-vendas/validation.md`.

Partes de T053 (a tarefa integral permanece aberta):

- [X] Restore locked, ferramenta EF, filtros, migrations e inicialização: evidências de 2026-10-09 preservadas; não constituem nova execução desses comandos em 2026-10-10.
- [X] Compilação e suíte completa reexecutadas em 2026-10-10; comandos/resultados registrados em `validation.md` e resumo em `quickstart.md`.
- [X] Consolidar os nove testes manuais relatados pelo responsável, distinguindo seu alcance do roteiro completo.
- [ ] Completar os passos e casos do quickstart ainda sem evidência manual específica, conforme matriz de checkpoints em `validation.md`.

Partes de T054 (a tarefa integral permanece aberta):

- [X] Registrar como evidência manual relatada: cadastro/edição/exclusão de eventos, acesso e gerenciamento básico de ingressos, venda/baixa, consulta ao histórico, cancelamento/restituição, reinício/persistência, obrigatórios de eventos e exclusão bloqueada por ingressos vinculados.
- [X] Registrar a verificação manual relatada de responsividade em visualização de celular, limitada à visualização usada pelo responsável.
- [ ] Validar todos os estados vazios, mensagens, correção com preservação de entrada, confirmações/desistência e casos específicos das cinco histórias ainda não comprovados visualmente.
- [ ] Verificar teclado/foco e completar a cobertura de telas menores com registro de telas, navegador e dimensões; o relato não comprova todas as telas/resoluções.

- [ ] T055 Realizar avaliação com cinco participantes sem orientação e registrar tempos individuais, conclusão e dificuldades em `specs/001-gestao-eventos-vendas/validation.md`: SC-001 exige quatro de cinco em até cinco minutos para evento/dois tipos; SC-002 exige quatro de cinco em até um minuto para vender e um minuto adicional para localizar/cancelar; se participantes não estiverem disponíveis, registrar pendência sem declarar aprovação.
- [X] T056 Revisar FR-001–FR-020, cenários/casos de borda e SC-001–SC-007 em matriz de evidências em `specs/001-gestao-eventos-vendas/validation.md`, incluindo separação MVC/serviços/dados, todas as escritas pelo protocolo único, recursos locais e ausência de funcionalidades fora do escopo; falha ou verificação pendente impede declarar a versão concluída (SC-003).

## Dependencies & Execution Order

```text
Setup (T001–T004)
  -> Foundational (T005–T015)
     -> US1 (T016–T021) -> US2 (T022–T027) -> US3 (T028–T037)
                                             -> US4 (T038–T042)
                                             -> US5 (T043–T049)
     -> todas as histórias -> Polish (T050–T056)
```

Ordem de entrega recomendada: US1 → US2 → US3 → US4 → US5. US2 usa eventos como dados, mas pode ser testada por fixture sem UI da US1. US3 usa eventos/ingressos preparados sem depender de seus formulários. US4/US5 usam vendas preparadas; US5 não depende da regra de consulta paginada da US4, mas a integração final de suas ações depende das views de US4. US4/US5 compartilham controller, queries/views e serviço já criado na US3: serializar essas edições, não presumir paralelismo irrestrito.

Na base: T001 → T002 → T003/T004; T005–T008 podem avançar simultaneamente após Setup; T009 depende de T005–T007; T010 depende de T009; T011 depende de T009; T012–T014 começam após T010/T011 e usam arquivos separados; T015 exige a base anterior completa. Nas histórias, escrever testes primeiro, implementar validação/serviços, depois consultas/controllers e views, executar verificação final. Modelos compartilhados são criados em T005–T007; sua associação é Evento→US1, Ingresso→US2 e Venda→US3/US4/US5.

T036 depende dos serviços de US1–US3. T044/T045 dependem das extensões de testes já criadas em US3; T048 depende de T041 para integração com a listagem. Checkpoints que atualizam validation.md devem ser sequenciais. T050–T052 podem ocorrer em paralelo após todas as histórias; T053–T056 são sequenciais e requerem os resultados anteriores. Testes novos podem falhar inicialmente por implementação ausente; falhas de ambiente devem ser registradas separadamente, nunca como evidência de regra atendida.

## Parallel Examples por história

| Grupo | Tarefas que podem avançar juntas | Pré-requisito |
|---|---|---|
| Setup | T003 e T004 | T002 concluída |
| Base/modelos | T005, T006, T007 e T008 | Setup concluído |
| Base/integração | T012, T013 e T014 | T010/T011 concluídas |
| US1 | T016 (integração) e T017 (contrato HTTP) | Base concluída |
| US2 | T022 (integração) e T023 (contrato HTTP) | Base concluída; fixture de evento |
| US3 | T028, T029, T030 e T031 (arquivos de testes distintos) | Base concluída; fixtures evento/ingresso |
| US4 | T038 (consulta/histórico) e T039 (contrato HTTP) | Base concluída; fixtures de vendas |
| US5 | T043, T044, T045 e T046 (arquivos distintos) | Testes/infraestrutura US3 concluídos |
| Transversal | T050, T051 e T052 | Histórias concluídas |

Os exemplos indicam oportunidades de execução futura; não exigem agentes paralelos. `[P]` não remove a dependência da fase anterior nem autoriza editar o mesmo arquivo simultaneamente.

## Implementation Strategy

1. Entregar Setup + Base + US1 como MVP de gerenciamento de eventos; validar T021 antes de avançar. Os modelos de ingresso/venda na base servem às constraints e fixtures, sem antecipar suas telas.
2. Acrescentar US2 para oferta e US3 para um incremento operacional de vendas. Se o objetivo da demonstração for vender ingressos, seu recorte mínimo inclui US1–US3.
3. Acrescentar US4 e US5, validando cada história com fixtures independentes e repetindo os testes afetados pela integração.
4. Executar a fase transversal e a avaliação humana. Registrar resultados reais; tarefas não executadas continuam desmarcadas e impedem conclusão integral.

## Rastreabilidade e conferência da lista

| Cobertura | Tarefas |
|---|---|
| FR-001–FR-002 | T005, T008, T016–T021 |
| FR-003–FR-007 | T006, T022–T027, T036, T038–T042 |
| FR-005 (eventos) | T016–T021 |
| FR-008–FR-012 | T007–T010, T028–T037 |
| FR-013, FR-016 | T038–T049 |
| FR-014–FR-015 | T043–T049 |
| FR-017–FR-020 | T003–T004, T012–T015, contratos/views por história, T050–T056 |
| SC-001–SC-002 | T054–T055 |
| SC-003 | Checkpoints por história, T053–T056 |
| SC-004 | T029, T037 |
| SC-005 | T044, T049 |
| SC-006 | T030, T045, T049–T051 |
| SC-007 | T016, T022, T036, T038–T042, T052 |

Total: **56 tarefas** — Setup 4; Base 11; US1 6; US2 6; US3 10; US4 5; US5 7; Transversal 7. Todas começam desmarcadas, possuem ID sequencial e caminho; somente tarefas de histórias possuem rótulo US. A geração desta lista não significa que a implementação ou seus testes tenham sido executados.








