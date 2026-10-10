# Evidências de implementação

Os resultados atuais estão na seção “Finalização — 2026-10-09” ao final deste documento. As seções anteriores são registros históricos; suas contagens e pendências retratam o momento de cada execução.

## Ambiente
# Evidências de validação — Meia-Entrada

Estado atual: consultar a seção **Consolidação — 2026-10-10** ao final. As seções anteriores são registros históricos e não significam que suas pendências continuem integralmente sem evidência.

- Windows x64; SDK .NET 10.0.401; runtime ASP.NET Core 10.0.12.
- Checklist requirements.md: 16/16; preservado sem alterações.
- Não há repositório Git nem configuração de hooks neste diretório.
- Implementação em andamento; nenhuma aprovação final de SC-003.

## Pendências de aceitação humana

SC-001 e SC-002 exigem cinco participantes reais. Registrar para cada participante o tempo de cadastro de evento/dois tipos, registro da venda e localização/cancelamento, conclusão e dificuldades. Não substituir por simulação automatizada.

| Participante | Evento + dois tipos (até 5 min) | Venda (até 1 min) | Localizar/cancelar (até 1 min) | Dificuldades |
|---|---|---|---|---|
| 1 | Pendente | Pendente | Pendente | |
| 2 | Pendente | Pendente | Pendente | |
| 3 | Pendente | Pendente | Pendente | |
| 4 | Pendente | Pendente | Pendente | |
| 5 | Pendente | Pendente | Pendente | |

## Base e US1

Base: 32 testes aprovados (inclui um teste vazio do template, a remover). US1: 6 testes aprovados, CRUD/validação/contratos/antiforgery/XSS/vínculos/histórico. Testes escritos antes do serviço e dos formulários; primeira execução falhou por tipos ausentes. Migration InitialCreate gerada e aplicada ao banco local. Build sem avisos.

Validação em navegador pendente: inventário de Computer Use sem browsers; tentativa de abrir iab retornou 'Browser is not available: iab'. T021 permanece aberta quanto ao navegador. Testes HTTP reais por WebApplicationFactory passam. Prosseguir com os incrementos automatizados independentes; não declarar SC-003 concluído.

## US2

7 testes aprovados de regras e HTTP. Versão obsoleta, unicidade, zero e bloqueios com vendas canceladas verificados. Primeiro teste falhou por ausência dos tipos antes da implementação. Navegador permanece indisponível; T027 pendente apenas da verificação visual.

## US3

20 testes aprovados: registro/conferência/HTTP, cálculo, atomicidade, concorrência e vínculos. SC-004: 20 tentativas, 10 vendas, 10 conflitos de saldo, saldo zero. Duas vendas de quatro sobre saldo cinco: uma aceita, saldo um. Rollback após escrita e antes de commit confirmado. Disputas com exclusão/tipo/preço/saldo/nome preservam integridade. T037 segue pendente de navegador.

## US4

Filtro Historico: 9 testes aprovados, incluindo cenários anteriores relacionados. Ordenação por instante/ID, 26 registros em duas páginas, cópias históricas e operações proibidas verificadas. T042 aguarda navegador.

## US5

13 testes aprovados no filtro de cancelamento/atomicidade/estoque: 20 cancelamentos geram uma única transição e reposição de 3, saldo final 10 e Versao 2; venda/cancelamento simultâneos resultam saldo 8; repetição mantém instante e versão; saldo obsoleto rejeitado mesmo após voltar ao valor inicial. T049 aguarda navegador.

## Complementação de cobertura automatizada — 2026-10-09

Escopo autorizado: somente T015–T017, T022–T023, T028, T031, T036, T038–T039, T043–T044 e T046. Referências: constituição 1.0.0, `spec.md`, `plan.md` e contrato HTTP. Nenhuma funcionalidade nova, migration, dependência, avaliação humana ou validação em navegador foi acrescentada. SQLite real em arquivos temporários, contextos independentes e antiforgery real foram mantidos. Não houve alteração do banco operacional.

### Cobertura comprovada

Os caminhos da tabela são relativos a `tests/MeiaEntrada.Tests/`, além dos testes originais de cada tarefa.

| Tarefas | Complementos | Evidências verificadas |
|---|---|---|
| T015 | `Integration/MigrationsCoberturaTests.cs`, `Unit/ValidacaoEntradaTests.cs` | Índice único e índices de FK, ordem descendente do índice histórico, FKs RESTRICT, constraints de texto/tipo/situação/inteiros, rejeição de REAL, integridade após rejeição, reaplicação da migration, WAL/FK/FULL em conexões independentes, IDs positivos e limites monetários/inteiros. Só há InitialCreate: atualização desde uma segunda migration não se aplica; nenhuma migration artificial foi criada. |
| T016–T017 | `Integration/EventosCoberturaTests.cs`, `Web/CadastrosContratoWebCoberturaTests.cs`, `Web/IdDaRotaContratoWebCoberturaTests.cs` | Obrigatórios nulos/vazios/whitespace em criação e edição, ausência de mutação, vínculos com vendas registradas/canceladas e histórico íntegro; ordem Data/Horario/Id, 26 registros, páginas inválidas, vazio, campos preservados, campos extras, antiforgery, confirmações e métodos HTTP. |
| T022–T023 | `Integration/IngressosCoberturaTests.cs`, `Web/CadastrosContratoWebCoberturaTests.cs`, `Web/IdDaRotaContratoWebCoberturaTests.cs` | Evento inexistente, entradas inválidas, troca de evento/tipo sem vendas, bloqueio após vendas registradas/canceladas, duplicidade na edição, preço/saldo editáveis, edição antiga após venda/cancelamento/outra edição, overflow de versão, 26 ingressos, filtro/pré-seleção, ordem, correção explícita do conflito com saldo atual 9 e tentativa 20, campos extras e antiforgery. |
| T028 | `Integration/VendasCoberturaTests.cs`, testes de histórico e limites | Seleção inexistente/incompatível, limites de total/versão sem baixa parcial, revalidação de vínculo/tipo/saldo/exclusão entre conferência e confirmação. Casos originais de venda gratuita/exata e ausência de reserva continuam aprovados. |
| T031 | `Web/VendasContratoWebCoberturaTests.cs` e teste existente `ContratoWebTests.CommitIncertoOrientaHistoricoSemRepetir` | Preço 40→50 após conferência de 3 unidades: resposta 409, total 150, novo token e nenhuma venda/baixa; confirmação ausente retorna 400; confirmação explícita grava 3 × 50 = 150 e saldo 7. Campos livres de preço/total ignorados; saldo revalidado; indisponibilidade, antiforgery e 400/404/405/409. Orientação de commit incerto reexecutada sem ampliar T050. |
| T036 | `Integration/VinculosConcorrenciaTests.cs` | Acrescentada disputa por troca de evento. Asserções agora exigem resultados compatíveis com a ordem serial para cada operação, contagem exata de vendas, saldo/versão, vínculo, preço e cópias históricas; não basta ausência de exceção ou lista vazia. |
| T038–T039 | `Integration/HistoricoTests.cs`, `Integration/HistoricoCoberturaTests.cs`, `Web/HistoricoCancelamentosContratoWebCoberturaTests.cs` | Comparação de todos os campos históricos por `Fixtures/EstadoVenda.cs`; 2 × 40 = 80 após mudança de nome/preço e exclusões bloqueadas; registrados/cancelados, ordenação por instante/ID, 26 registros acessíveis, valores BRL, UTC e Brasília com mudança de data; edição/exclusão/reativação proibidas sem mutação. |
| T043–T044 | `Integration/CancelamentosCoberturaTests.cs`, `Integration/EstoqueConcorrenciaTests.cs` | Venda gratuita, repetição sem entidades modificadas, overflow de saldo/versão preservando estado integral; 20 cancelamentos = 1 transição e 19 repetições, saldo 10/versão 2, histórico e instante preservados; concorrência mista saldo 8/versão 3 e situações corretas. |
| T046 | `Web/HistoricoCancelamentosContratoWebCoberturaTests.cs`, `Web/IdDaRotaContratoWebCoberturaTests.cs` | Desistência via GET, inexistência GET/POST, confirmação ausente, antiforgery adulterado, redirect, mensagem “Venda já cancelada”, preservação de todos os campos históricos, saldo, versão e primeiro instante; ID extra não pode cancelar outra venda. |

### Defeito real encontrado e correção limitada

O teste `IngressosIgnoramCamposExtrasNaCriacaoEEdicao` revelou que um campo extra `Id` no formulário substituía o ID da URL no binding do ASP.NET Core MVC. A edição recebia 409 por tentar modificar outro ingresso. Quatro casos adicionais reproduziram o mesmo defeito na edição de evento, exclusão de evento/ingresso e cancelamento de venda: registros diferentes dos indicados na rota eram atingidos.

Antes da correção, os cinco testes de regressão falharam (`TestResults/regressao-id-antes.trx`). A única alteração na aplicação foi adicionar `[FromRoute]` ao parâmetro `id` dos cinco POSTs correspondentes em `EventosController`, `IngressosController` e `VendasController`. Após a correção, os cinco passaram (`TestResults/regressao-id-depois.trx`). Serviços, regras de estoque, schema e arquitetura permaneceram iguais.

### Execuções

Ambiente: Windows x64, SDK .NET 10.0.401, ASP.NET Core/EF Core 10.0.12, xUnit 2.9.3. Compilação executada por `dotnet test --no-restore`, sem diagnósticos de erro ou aviso. Execução fora do sandbox, conforme necessidade de compilador/testhost já identificada no diagnóstico.

- Primeira seleção de complementos: 108 aprovados, zero falhas/ignorados.
- Seleção ampliada antes da correção: 184 aprovados, 1 falha real, zero ignorados.
- Reprodução isolada do defeito de ID: 5 falhas antes; 5 aprovados após a correção.
- Seleção final de todos os testes afetados: **189 aprovados, zero falhas, zero ignorados**, aproximadamente 14 segundos. Relatório: `TestResults/cobertura-afetados.trx`.
- Suíte completa após a correção: **196 aprovados, zero falhas, zero ignorados**, aproximadamente 34 segundos. Relatório: `TestResults/cobertura-suite-completa.trx`. São **117 casos adicionais** em relação aos 79 do diagnóstico, além do reforço das asserções em testes existentes. Contençao/reinício e os demais testes existentes passaram como regressão, sem desenvolvimento de outras tarefas.

Comando da seleção final (após compilar a correção):

```powershell
dotnet test MeiaEntrada.slnx --no-build --no-restore --filter "FullyQualifiedName~CoberturaTests|FullyQualifiedName~VinculosConcorrenciaTests|FullyQualifiedName~EstoqueConcorrenciaTests|FullyQualifiedName~MigrationsTests|FullyQualifiedName~ValidacaoEntradaTests|FullyQualifiedName~EventosTests|FullyQualifiedName~EventosContratoWebTests|FullyQualifiedName~IngressosTests|FullyQualifiedName~IngressosContratoWebTests|FullyQualifiedName~VendasTests|FullyQualifiedName~VendaCalculoTests|FullyQualifiedName~VendasContratoWebTests|FullyQualifiedName~HistoricoTests|FullyQualifiedName~HistoricoContratoWebTests|FullyQualifiedName~CancelamentosTests|FullyQualifiedName~CancelamentosContratoWebTests|FullyQualifiedName~SqliteWriteTransactionTests|FullyQualifiedName~CommitIncertoOrientaHistoricoSemRepetir" --logger "trx;LogFileName=cobertura-afetados.trx" --results-directory TestResults
```

Comando da suíte completa, executado depois da seleção final:

```powershell
dotnet test MeiaEntrada.slnx --no-build --no-restore --logger "trx;LogFileName=cobertura-suite-completa.trx" --results-directory TestResults
```

### Estado das tarefas e limites desta etapa

As 13 tarefas selecionadas permanecem `[X]`, agora respaldadas pelos complementos acima. Nenhuma outra tarefa foi marcada ou ampliada. T021/T027/T037/T042/T049 continuam abertas quanto aos checkpoints completos; T050–T056 continuam no estado anterior, fora desta execução. Executar seus testes existentes como regressão na suíte completa não equivale a concluir essas tarefas.

Não foram executados navegador, teclado/foco, responsividade nem os cinco participantes de SC-001/SC-002. A revalidação automatizada não aprova SC-003 nem declara a primeira versão concluída. A revisão global de evidências e a atualização geral do quickstart permanecem para a etapa correspondente.

## Finalização — 2026-10-09

**Resultado: validação automatizada aprovada; versão 1.0.0 ainda não integralmente validada.** O responsável informou que os fluxos básicos das cinco histórias funcionaram em testes manuais. Esse relato é aceito como evidência dos fluxos básicos, sem inferir execução de todos os cenários, casos de borda ou medidas de usabilidade. Não há resultados de cinco participantes.

### Execução reproduzível

Windows x64, SDK 10.0.401, ASP.NET Core/EF Core/EF CLI 10.0.12, xUnit 2.9.3, Test SDK 17.14.1, MVC.Testing 10.0.12 e Bootstrap local 5.3.3. Pacotes preservados nos arquivos lock; nenhuma alteração de arquitetura ou funcionalidade de produção nesta etapa.

| Verificação | Resultado comprovado |
|---|---|
| `dotnet restore MeiaEntrada.slnx --locked-mode` | Sucesso |
| `dotnet tool restore` | dotnet-ef 10.0.12 restaurado |
| `dotnet build MeiaEntrada.slnx --no-restore` | Sucesso, 0 avisos, 0 erros (23,45 s na execução final) |
| Suíte completa final | **197 aprovados, 0 falhas, 0 ignorados**, aproximadamente 32 s |
| Quatro filtros do quickstart, em seleção conjunta | **31 aprovados, 0 falhas, 0 ignorados**, aproximadamente 5 s |
| EF migrations list/update/update | InitialCreate aplicada; segunda atualização sem migrations pendentes |
| Inicialização real por dotnet run | Development, localhost:5087; GET /eventos/ = 200, HTML com navegação e Bootstrap local; processo de validação encerrado depois |

Relatórios locais: [suíte completa](../../TestResults/finalizacao-suite-completa.trx) e [filtros](../../TestResults/finalizacao-filtros.trx). TestResults é ignorado; arquivar esses relatórios junto à entrega caso a pasta não acompanhe o projeto. Os números acima também permanecem neste documento.

```powershell
dotnet test MeiaEntrada.slnx --no-build --no-restore --logger "trx;LogFileName=finalizacao-suite-completa.trx" --results-directory TestResults
dotnet test MeiaEntrada.slnx --no-build --no-restore --filter "FullyQualifiedName~EstoqueConcorrenciaTests|FullyQualifiedName~AtomicidadeTests|FullyQualifiedName~ContratoWebTests|FullyQualifiedName~MigrationsTests" --logger "trx;LogFileName=finalizacao-filtros.trx" --results-directory TestResults
```

A compilação restrita falhou sem diagnóstico de código; restore/ferramenta sofreram bloqueio de conexão. As repetições autorizadas fora do sandbox passaram, assim como build/testhost. Não se contabilizaram esses bloqueios como falhas de regra de negócio. Pré-requisitos Spec Kit executados com ExecutionPolicy Bypass limitado ao processo; checklist de requisitos 16/16 preservado. Sem repositório Git ou arquivo de hooks.

O ensaio de instalação usou `var/validacao-final/quickstart.db` e `var/validacao-final/keys`, caminhos absolutos fora de wwwroot, sem tocar no banco de uso manual. A porta 5087 isolou a execução; o guia usa 5080. Testes usam arquivos SQLite temporários reais próprios, Production no WebAppFactory, conexões independentes, WAL/FK/FULL e timeout de 30 s. O aviso de chaves Data Protection sem criptografia em repouso foi emitido no host local; o guia orienta proteger o diretório. Não foi feito ensaio de instalação em máquina limpa nem de publicação.

### T050–T052: segurança e persistência

- **T050 concluída**: ContratoWebTests verifica navegação, antiforgery nos nove POSTs, erros e commit incerto; os testes de contrato das histórias e complementos verificam XSS codificado e overposting, incluindo IDs de rota. Novo teste `FalhaInesperadaProductionNaoExpoeDetalhesNemPersiste` injeta exceção após escrita com SQL, caminho e marcador sensível: HTTP 500 sem esses dados, sem tipo/stack da exceção e sem evento persistido. A injeção existe somente no projeto de testes. `CommitIncertoOrientaHistoricoSemRepetir` passa: orientação para consultar histórico, nenhuma repetição/gravação observada no cenário de falha antes do commit. Isso não simula queda de energia nem todos os resultados possíveis de commit desconhecido.
- **T051 concluída**: `EscritorOcupadoRetorna503SemMutacao` passou em aproximadamente 30,94 s. Escritor A mantém transação imediata enquanto B usa outra conexão; resposta 503 com “Sistema ocupado; tente novamente”, eventos/saldo/versão/vendas preservados. Rollback em finally libera A; novo POST explícito grava com sucesso. Separado dos testes de concorrência de estoque; nenhum timeout conta como insuficiência.
- **T052 concluída**: `ReiniciaHostPreservandoDadosEConferencia` passou com três hosts sucessivos e o mesmo arquivo/chaves. Token emitido no primeiro host é aceito no segundo; venda de 3 × 40 = 120 é cancelada, evento e preço são editados, registros temporários de evento/ingresso são criados/excluídos. No terceiro host, histórico cancelado permanece acessível. Contexto novo confirma todos os campos históricos, primeiro instante de cancelamento, dados atuais do evento, preço 50, saldo 10, versão 4, vínculos e conteúdo das chaves. Reaplicar migration preserva dados; CHECK e FK continuam rejeitando escrita inválida/exclusão vinculada. MigrationsTests e MigrationsCoberturaTests complementam índices, constraints e conexões. Existe apenas `20261008203846_InitialCreate`: atualização desde segunda migration não se aplica; não foi criada migration artificial. Reinício de host não equivale a teste de interrupção abrupta do sistema operacional.

### Cinco checkpoints e cenários de aceitação

Os arquivos citados estão em `tests/MeiaEntrada.Tests/Integration/` e `Web/`, incluindo os complementos `*CoberturaTests.cs` descritos na seção anterior. Todos foram reexecutados na suíte final. Aprovação automatizada não equivale a aprovação visual.

| Checkpoint | Cenários/regras comprovados por testes | Evidência manual e pendência |
|---|---|---|
| T021 / US1, cenários 1–5 | EventosTests, EventosCoberturaTests, EventosContratoWebTests e complementos: CRUD, ID estável, inválidos/campos preservados, desistência, vínculos inclusive cancelados | Fluxo básico relatado; falta roteiro detalhado no navegador, mensagens e correção/desistência |
| T027 / US2, cenários 1–7 | IngressosTests e complementos de integração/HTTP: CRUD, zeros, duplicidade, vínculos, preço/saldo, versão antiga e revisão explícita | Fluxo básico relatado; falta conferir visualmente conflitos, bloqueios e revisão |
| T037 / US3, cenários 1–7 | VendasTests, VendaCalculoTests, contratos/complementos, EstoqueConcorrenciaTests, AtomicidadeTests e VinculosConcorrenciaTests: total, insuficiência, inválidos, concorrência, falhas, reconfirmação e indisponibilidade | Fluxo básico relatado; falta roteiro detalhado de reconfirmação, mensagens e correção |
| T042 / US4, cenários 1–3 | HistoricoTests e complementos/contratos: 2 × 40 = 80 preservados após edição, canceladas, ordenação, paginação, campos e vazio | Fluxo básico relatado; falta roteiro independente visual com esses dados, vazio e paginação |
| T049 / US5, cenários 1–6 | CancelamentosTests e complementos/contratos, AtomicidadeTests e EstoqueConcorrenciaTests: restituição, repetição, concorrência, falha, desistência/inexistência e operação mista | Fluxo básico relatado; falta roteiro detalhado visual de repetição, desistência e mensagens |

Todos os cinco checkboxes permanecem abertos por conterem verificação completa de interface. O inventário atual de Computer Use retornou `apps=[]`, `browsers=[]`; não foi possível executar T054. Não foram verificados foco/teclado, telas menores, execução visual de todos os estados nem comportamento no navegador de texto malicioso (o encoding HTML foi testado por HTTP).

### T056: matriz final de requisitos e critérios

Revisão concluída com pendências explícitas. “Automatizado aprovado” abaixo limita-se aos testes executados; não concede aprovação integral de interface.

| Requisito | Evidência / avaliação |
|---|---|
| FR-001–FR-002 | US1 integração/HTTP e parsing aprovados; checkpoint visual T021 aberto |
| FR-003–FR-004 | US2 integração/HTTP, zeros, limites e unicidade aprovados; T027 aberto |
| FR-005–FR-006 | Eventos/Ingressos/Historico e VinculosConcorrencia: bloqueios registrados/cancelados e cópias preservadas aprovados |
| FR-007 | IngressosCobertura e HTTP: versão após venda, cancelamento, edição e retorno ao mesmo saldo aprovados |
| FR-008–FR-009 | Vendas/Calculo/contratos: seleção, total exato, gratuita, preço alterado e confirmação explícita aprovados; T037 aberto |
| FR-010–FR-011 | Vendas, concorrência e atomicidade aprovadas; nenhum saldo negativo ou baixa parcial nos casos executados |
| FR-012–FR-013 | Historico e complementos: campos originais, UTC/Brasília, desempate por ID, 26 registros, canceladas e vazio aprovados; T042 aberto |
| FR-014–FR-015 | Cancelamentos/Atomicidade/EstoqueConcorrencia aprovados; T049 aberto |
| FR-016 | Contratos de histórico: editar/excluir/reativar recusados sem mutação |
| FR-017 | Navegação/mensagens/labels/estados cobertos por HTML e relato básico; **validação completa em navegador pendente** (T054) |
| FR-018 | Parsing, antiforgery, overposting, encoding e erro Production aprovados; limite: XSS verificado no HTML, sem execução de navegador |
| FR-019 | PersistenciaReinicioTests e Migrations: cadastros/saldos/histórico/chaves preservados nos reinícios de host |
| FR-020 | Revisão de Program, controllers, serviços e views: somente eventos/ingressos/vendas/cancelamentos; recursos CSS/Bootstrap locais, sem login, pagamento, QR Code ou integração externa |

Casos de borda revisados: listas vazias (contratos), saldo exato/zero e venda gratuita (Vendas/Cancelamentos), cancelamento após ajuste manual (Cancelamentos), exclusões/vínculos concorrentes (VinculosConcorrencia), edição obsoleta (Ingressos), marcação como texto (EventosContratoWeb), erros/inexistência (contratos), datas inválidas/passadas (ValidacaoEntrada/Eventos). Todos têm evidência automatizada na suíte; apresentação visual completa segue pendente.

Revisão estrutural: controllers recebem ViewModels e chamam serviços/queries, sem SaveChanges; EventoService, IngressoService e VendaService usam `SqliteWriteTransaction.Execute` em todas as escritas de domínio. O auxiliar abre contexto/conexão, BEGIN imediato antes da leitura, associa a transação ao EF, confirma ou desfaz e descarta. Consultas/mapeamentos permanecem em Data; views Razor não usam Html.Raw para entradas; não foi encontrada concatenação de entrada em SQL de produção nem coordenação por semáforo em memória. Ignore cobre banco/auxiliares, var, build e relatórios. Nenhum código de produção foi alterado nesta etapa.

| Critério | Resultado |
|---|---|
| SC-001 | **Pendente**: nenhum registro de cinco participantes e tempos de cadastro |
| SC-002 | **Pendente**: nenhum registro de cinco participantes e tempos de venda/localização/cancelamento |
| SC-003 | **Não satisfeito**: checkpoints visuais e avaliação humana pendentes |
| SC-004 | Aprovado no teste: 20 tentativas, 10 vendas, 10 recusas por estoque, saldo 0 |
| SC-005 | Aprovado no teste: 20 cancelamentos, uma transição, 19 repetições, saldo 10/versão 2, histórico/instante preservados |
| SC-006 | Aprovado nos cenários de injeção executados: venda/cancelamento sem alterações parciais; não representa falhas físicas não simuladas |
| SC-007 | Aprovado nos cenários executados de edição/exclusão/vínculos, com todos os campos históricos e referências preservados |

### Tarefas e condição de conclusão

Concluídas nesta etapa: **T050, T051, T052 e T056** (revisão documentada, sem aprovação global). **48/56 tarefas marcadas; 8 pendentes**: T021, T027, T037, T042, T049, T053, T054 e T055.

T053 está parcialmente comprovada: restore/build/suíte/filtros/migrations/inicialização e atualização do quickstart concluídos; o roteiro completo de navegador não foi executado nesta etapa. T054 depende de navegador disponível e registro detalhado de interface. T055 permanece aberta: realizar avaliação real com cinco operadores, registrar conclusão/dificuldades e tempos individuais na tabela deste documento, sem orientação. Exigir quatro de cinco dentro de cada critério temporal. Para aprovar integralmente 1.0.0, concluir essas verificações, corrigir eventuais falhas e reexecutar os testes afetados antes de atualizar SC-003 e os checkboxes correspondentes.

## Consolidação — 2026-10-10

**Parecer: o Meia-Entrada pode ser considerado um protótipo funcional estável para uso local e demonstração, dentro do escopo e ambiente avaliados. Nenhum problema técnico crítico impeditivo foi identificado nas verificações desta etapa. Isso não aprova integralmente a versão 1.0.0: SC-001/SC-002 continuam pendentes e SC-003 não está satisfeito.**

### Origem e limites das evidências manuais

Fonte: relato do responsável fornecido nesta sessão, registrado em 2026-10-10. A data exata de execução, navegador/versão, dimensões de tela, dados utilizados e capturas não foram informados. Não houve nova execução de navegador pelo agente nesta etapa. Os resultados abaixo são aceitos como evidência manual relatada de funcionamento, sem atribuir valores, tempos, cenários adicionais ou cobertura exaustiva.

| Teste manual relatado | Evidência aceita | Limitação |
|---|---|---|
| Cadastro, edição e exclusão de eventos | Fluxo básico da US1 realizado | Não comprova todos os campos, ID estável, desistência ou casos de borda |
| Acesso e gerenciamento básico de ingressos | Uso básico da US2 realizado | Não discrimina CRUD completo, tipos, duplicidade, zeros, vínculos ou conflitos |
| Registro de venda e atualização do estoque | Fluxo básico da US3 com baixa observado | Não informa quantidade/preço/saldo exatos, insuficiência ou reconfirmação |
| Consulta ao histórico de vendas | Consulta básica da US4 realizada | Não comprova todos os campos, ordenação, paginação ou cópias após edição |
| Cancelamento e restituição do estoque | Fluxo básico da US5 com reposição observado | Não comprova repetição, instante estável, desistência ou concorrência |
| Persistência após reiniciar a aplicação | Dados persistiram no reinício relatado | Não discrimina todos os registros/campos, nem representa queda abrupta ou recuperação de backup |
| Campos obrigatórios no cadastro de eventos | Rejeição de ausência de campos verificada | Não comprova whitespace, data/horário inválidos, edição ou preservação dos demais campos |
| Bloqueio da exclusão de eventos com ingressos | Proteção desse vínculo verificada | Não comprova todas as combinações com vendas canceladas nem exclusão de ingressos |
| Responsividade em visualização de celular | Verificação manual nessa visualização realizada | Telas/dimensões não discriminadas; não comprova todas as resoluções, dispositivos, teclado ou foco |
| Tentativa de venda acima do estoque (relato complementar) | Operação bloqueada e quantidade disponível mantida | Não foram informados valores exatos, texto da mensagem ou consulta independente para conferir ausência de nova venda; não comprova venda com saldo exato nem concorrência |
| Consulta de venda anteriormente cancelada (relato complementar) | Venda permaneceu no histórico com status Cancelada | Não comprova uma segunda tentativa efetiva de cancelamento, saldo/instante inalterados após repetição, nem todos os campos históricos |

O relato de celular complementa a pendência histórica de telas menores; não deve continuar descrito como ausência total de evidência de responsividade. As limitações de disponibilidade de navegador registradas em 2026-10-09 são históricas, não uma nova verificação de disponibilidade nesta sessão.

### Checkpoints ainda abertos

A suíte automatizada cobre regras e contratos descritos nas matrizes anteriores; as pendências abaixo são de validação manual específica, revistas após os dois relatos complementares. Os números de cenários remetem à respectiva história em spec.md. Nenhum dos cinco checkpoints completos foi fechado somente pelos relatos. Concorrência e injeção de falhas já têm evidência automatizada: a ausência de reprodução manual desses testes técnicos não significa ausência de cobertura nem exige injeção de falha na aplicação de uso.

| Tarefa / história | Validação manual restante |
|---|---|
| T021 / US1 | **1–2:** conferir todos os campos em lista/detalhe após cadastro/edição e ID estável; CRUD básico já relatado. **3:** whitespace, data/horário inválidos, identificação dos campos problemáticos e preservação dos válidos; obrigatórios no cadastro já relatados. **4:** desistência sem mutação e conferência da remoção nas consultas; exclusão básica já relatada. **5:** mensagem explicativa e preservação dos relacionados, incluindo vínculos com vendas registradas/canceladas; bloqueio por ingresso já relatado. Complementos do checkpoint: vazio e paginação |
| T027 / US2 | **1:** discriminar cadastro/lista/detalhe com tipo, preço e saldo (exemplo inteira 40/10 e oferta dos dois tipos). **2:** edição de preço/saldo e uso dos valores nas próximas vendas. **3:** evento inexistente, tipo inválido, preço negativo/com mais de duas casas, saldo fracionário/negativo, obrigatórios, mensagem e ausência de mutação. **4:** duplicidade na criação/edição e preço/saldo zero. **5:** exclusão sem vendas, desistência e bloqueio com vendas inclusive canceladas. **6:** mudança de evento/tipo com/sem vendas e manutenção das edições permitidas. **7:** edição antiga após venda/cancelamento/outra edição, saldo atual exibido e nova confirmação sem sobrescrita, inclusive saldo que retorna ao anterior. O relato de gerenciamento básico não discrimina esses cenários |
| T037 / US3 | **1:** confirmar resumo, mensagem e valores do exemplo 3 × 40 = 120 / saldo 7; venda/baixa básicas já relatadas. **2:** bloqueio por quantidade acima do estoque e saldo preservado agora relatados; faltam conferência independente da ausência de nova venda, mensagem de insuficiência e venda de saldo exato até zero (valores 2/3 da especificação não informados). **3:** quantidade vazia/zero/negativa/fracionária e seleção inexistente/incompatível sem alteração. **6:** preço alterado após conferência, novo total/reconfirmação e revalidação do saldo. **7:** evento sem oferta ou saldo zero, mensagem e impossibilidade de confirmar. Complementos: venda gratuita e preservação/correção de entrada. **4–5:** concorrência e atomicidade comprovadas automaticamente, sem novo ensaio manual |
| T042 / US4 | **1:** consulta e presença de venda com status Cancelada agora relatadas; faltam todos os campos (ID, evento, tipo, quantidade, total, data/horário), ordenação, situação registrada e acesso a todas as páginas. **2:** editar nome/preço e conferir cópias originais (2 × 40 = 80), incluindo preço unitário. **3:** histórico vazio com mensagem e acesso a registrar venda. Complementos: formatação BRL/horário e mais de 25 registros. Não exigir novamente a mera presença do status Cancelada como se não houvesse evidência |
| T049 / US5 | **1:** cancelamento/restituição básicos e permanência no histórico com status Cancelada relatados; faltam valores exatos (3 unidades, saldo 7→10), primeiro instante e todos os dados originais preservados. **2:** permanece sem comprovação manual de segunda tentativa efetiva: registrar o reenvio, mensagem, saldo antes/depois e preservação da data do primeiro cancelamento. Consultar a venda cancelada não executa essa operação. **5:** desistência e venda inexistente sem alterações, com resultado informado. Complemento: restituição após ajuste manual de saldo (20 + 3 = 23). **3–4 e 6:** cancelamentos concorrentes, atomicidade e venda/cancelamento simultâneos comprovados automaticamente, sem novo ensaio manual |

Transversalmente, T053 aguarda o restante do roteiro detalhado do quickstart. T054 aguarda cobertura de estados/mensagens/correção/confirmações, teclado/foco e detalhamento da cobertura em telas menores. Conteúdo com marcação foi verificado por testes HTTP, sem nova verificação de execução visual no navegador.

### Compilação e suíte completa reexecutadas

Ambiente: Windows x64, SDK confirmado por `dotnet --version`: **10.0.401**. Projetos net10.0; referências fixadas EF Core/MVC.Testing 10.0.12, xUnit 2.9.3 e Test SDK 17.14.1. Nenhum pacote, código de produção, teste ou arquitetura foi alterado.

```powershell
dotnet build MeiaEntrada.slnx --no-restore
dotnet test MeiaEntrada.slnx --no-build --no-restore --logger "trx;LogFileName=consolidacao-2026-10-10.trx" --results-directory TestResults
```

- Build fora do sandbox: **sucesso, 0 avisos e 0 erros**, 3,19 s.
- Suíte completa fora do sandbox: **197 executados, 197 aprovados, 0 falhas, 0 ignorados**, duração informada pelo runner de 38 s; processo com código de saída 0.
- TRX conferido: 0 erros, timeouts, abortados e não executados. Intervalo do TestRun: 08:11:03–08:11:43 (UTC−03:00), em 2026-10-10.
- Relatório: [consolidacao-2026-10-10.trx](../../TestResults/consolidacao-2026-10-10.trx). Arquivar com a entrega, pois TestResults é ignorado.

A primeira compilação restrita retornou código 1, sem diagnóstico de erro de código (0 avisos/0 erros); a repetição autorizada fora do sandbox passou. A tentativa restrita de testes ficou sem saída/resultado e recebeu solicitação de interrupção; não foi contabilizada como aprovação nem falha de regra. A evidência válida é a execução concluída fora do sandbox e seu TRX.

Restore locked, ferramenta EF, filtros isolados, migrations via CLI e inicialização por `dotnet run` não foram repetidos nesta etapa: suas evidências permanecem datadas de 2026-10-09. A suíte atual inclui os testes dessas áreas, sem equivaler à reexecução dos comandos do guia. Os testes utilizam bancos temporários próprios e WebApplicationFactory em Production; não houve operação manual no banco de uso do responsável.

### Verificação técnica e encerramento

Revisados `Program.cs`, `SqliteWriteTransaction.cs`, os três serviços de domínio e as referências dos projetos. As escritas de domínio permanecem dentro de `SqliteWriteTransaction.Execute`, com transação imediata antes das leituras, commit único, rollback e descarte; vendas/cancelamentos usam cálculos checked e preservam cópias históricas. A suíte reexecutou concorrência, atomicidade, contenção, persistência/reinício, migrations e contratos HTTP. Não foi identificado defeito crítico impeditivo nas verificações realizadas; isso não é garantia de ausência de defeitos em cenários não exercitados.

O parecer se limita ao protótipo local com SQLite em disco local e configuração do quickstart. Instalação em máquina limpa, publicação, falhas físicas e validação ampla entre navegadores/dispositivos não foram executadas nesta etapa. Sem repositório Git nesta cópia, não há commit/diff Git a apresentar; foram alterados somente os três documentos de consolidação (tasks, validation e quickstart), além dos artefatos gerados por build/testes.

**Estado das tarefas:** 48/56 tarefas integrais concluídas; 8 pendentes: T021, T027, T037, T042, T049, T053, T054 e T055. Nenhum novo ID integral foi encerrado. As partes comprovadas de T053/T054 estão marcadas separadamente em tasks.md; T050–T052 e T056 mantêm suas evidências anteriores, reforçadas pela regressão atual. T056 concluída significa revisão documentada, não aprovação global.

**Recomendações para encerramento:**

1. Entregar como protótipo funcional estável, anexando este registro e o TRX, com as pendências explícitas.
2. Executar o restante dos cinco checkpoints e de T053/T054, registrando cenário, resultado, navegador, dimensões e evidências de interface; corrigir apenas eventuais defeitos encontrados e revalidar as áreas afetadas.
3. Manter T055 aberta até a avaliação real com cinco participantes sem orientação: registrar tempos, conclusão e dificuldades; exigir pelo menos quatro de cinco nos limites de SC-001 e SC-002.
4. Somente declarar a versão 1.0.0 integralmente validada após satisfazer SC-003, sem testes ou cenários aplicáveis pendentes. Preservar spec.md e seus critérios; não reduzir o aceite para ajustar ao resultado atual.

## Preparação da entrega e relatos complementares — 2026-10-10

Foram incorporados à tabela manual e à matriz de checkpoints os dois novos relatos do responsável: tentativa de venda superior ao estoque bloqueada com saldo mantido; consulta de venda anteriormente cancelada ainda presente com status Cancelada. Data exata do ensaio, navegador, valores e capturas não foram fornecidos. Não houve execução manual pelo agente. A segunda tentativa efetiva de cancelamento permanece sem comprovação manual; o teste automatizado de repetição tem evidência anterior e não é substituído pela consulta do histórico.

T021/T027/T037/T042/T049 foram revisadas por número de cenário de spec.md. Todas permanecem abertas pelas lacunas descritas na matriz atual. T055 continua pendente: nenhum novo resultado de cinco participantes foi informado. T053/T054 continuam parciais; total de tarefas integrais permanece 48 concluídas e 8 pendentes.

### README e organização para entrega

Não existia README.md na raiz. Foi criado com apresentação, funcionalidades, stack fixada, pré-requisitos, comandos resumidos de execução local/testes, estrutura, links para os artefatos Spec Kit e distinção entre protótipo funcional e versão integralmente validada. O quickstart permanece como guia detalhado, sem substituição. Os comandos do README foram conferidos contra o quickstart, global.json, manifesto EF e projetos existentes; não constituem novo ensaio de instalação.

### Auditoria local de Git e artefatos

`git status --short` retornou “not a git repository”; a listagem da raiz não contém `.git`. Portanto, não foi possível verificar índice, arquivos rastreados, branch, remotos ou histórico. Não foi inicializado repositório, nem executado commit ou push. A presença de documentação do GitHub Spec Kit não implica a existência de um repositório Git nesta cópia.

Inspeção de nomes/caminhos, sem leitura do conteúdo de bancos ou chaves:

| Artefatos locais encontrados | Regra de exclusão |
|---|---|
| `var/meia-entrada.db` | `var/`, `*.db` |
| `var/validacao-final/quickstart.db`, `.db-wal`, `.db-shm` | `var/`, `*.db`, `*.db-wal`, `*.db-shm` |
| Chaves `var/keys/key-*.xml` e `var/validacao-final/keys/key-*.xml` | `var/`; acrescentado `key-*.xml` para arquivos fora do caminho padrão |
| Relatórios `.trx` em `TestResults/`, incluindo regressões anteriores e consolidação atual | `TestResults/`; acrescentado `*.trx` para relatórios em outros caminhos |
| Cache `packages/`, ambiente `.dotnet-home/` e saídas `bin/`/`obj/` dos projetos | Regras de diretório já existentes |

O `.gitignore` já protegia os artefatos encontrados, logs, temporários, configurações pessoais de IDE e `.env*`. Foram acrescentados `*.db-journal`, `*.trx`, `key-*.xml` e `.vs/`. A verificação foi estática contra os caminhos e padrões: não houve `git check-ignore` em repositório ativo, pois não há `.git`. Nenhum arquivo local foi apagado. Essas regras não removem arquivos já rastreados em outra cópia e não são aplicadas automaticamente ao montar um ZIP.

Para entrega, preservar fontes, testes, migrations, dependências frontend locais com licença, documentos Spec Kit, solução/projetos, global.json, manifesto de ferramentas, arquivos packages.lock.json, .editorconfig e .gitignore. Excluir os artefatos locais acima do pacote de fontes. O TRX pode ser anexado separadamente como evidência revisada. Não há URL de repositório confirmada nesta cópia; o README não inventa um endereço de clone.

### Verificação desta revisão

Alterações restritas a README.md, .gitignore, tasks.md e validation.md; nenhum código, teste, dependência ou configuração de execução foi alterado. Por serem alterações documentais e de exclusão de artefatos, não foi necessário repetir build/suíte. A última execução válida continua sendo a consolidação de 2026-10-10: build sem avisos/erros e 197 testes aprovados, 0 falhas/ignorados, no TRX referenciado acima. Isso é evidência da execução anterior, não uma nova execução nesta revisão.

Para entrega local, a apresentação e as instruções estão preparadas; falta montar o pacote de fontes sem dados/chaves/caches e anexar as evidências escolhidas. Para GitHub, falta preparar o repositório e revisar o conteúdo a versionar antes da publicação em etapa posterior. Para declarar 1.0.0 integralmente validada, completar os cinco checkpoints e T053/T054, realizar T055 e satisfazer SC-003. O parecer de protótipo funcional estável no ambiente avaliado permanece, sem ampliar sua garantia para cenários não executados.
