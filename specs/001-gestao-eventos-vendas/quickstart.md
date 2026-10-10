# Quickstart de validação — Meia-Entrada

**Estado (2026-10-10)**: protótipo funcional estável no ambiente local avaliado; versão 1.0.0 ainda não integralmente validada. Compilação e suíte completa reexecutadas: 197 aprovados, nenhuma falha ou teste ignorado. Restore, ferramenta EF e migration inicial/reaplicação têm evidências de 2026-10-09. O roteiro de navegador abaixo é um procedimento de validação, não uma declaração de aprovação; consulte validation.md para evidências e pendências.

Referências: [plan.md](plan.md), [data-model.md](data-model.md), [contracts/web.md](contracts/web.md) e [spec.md](spec.md).

## Pré-requisitos

- Instalar o SDK .NET **10.0.401** (Windows x64 neste ambiente), PowerShell e navegador. O global.json exige essa versão exata (rollForward=disable); somente o runtime não basta. Confirmar com dotnet --version.
- O projeto já contém `MeiaEntrada.slnx`, `src/MeiaEntrada.Web`, `tests/MeiaEntrada.Tests`, migrations e manifesto local contendo dotnet-ef 10.0.12.
- Disco local gravável para SQLite e chaves Data Protection. Não é necessário instalar servidor de banco, CLI sqlite3 ou Docker.
- Acesso à restauração de pacotes na preparação. Bootstrap 5 está incluído localmente; o funcionamento da interface não dependerá de CDN.

## Preparar o ambiente

Executar na raiz do projeto em PowerShell (ajuste o caminho ao local da sua cópia):

```powershell
Set-Location C:/Users/joaov/meia-entrada
dotnet --info
dotnet tool restore
dotnet restore MeiaEntrada.slnx --locked-mode
dotnet build MeiaEntrada.slnx --no-restore
New-Item -ItemType Directory -Path ./var -Force | Out-Null
$dbPath = Join-Path (Get-Location) 'var/meia-entrada.db'
$keyPath = Join-Path (Get-Location) 'var/keys'
$env:ConnectionStrings__DefaultConnection = "Data Source=$dbPath;Foreign Keys=True;Default Timeout=30"
$env:DataProtection__KeyPath = $keyPath
$env:ASPNETCORE_ENVIRONMENT = 'Development'
```

Esperado: SDK 10 reconhecido, versões fixadas restauradas e build sem erros. O parâmetro `--locked-mode` pressupõe packages.lock.json versionados na implementação. A configuração usa ConnectionStrings:DefaultConnection e DataProtection:KeyPath; caminhos absolutos garantem o mesmo arquivo para ferramenta e aplicação. Não persistir banco, WAL/SHM ou chaves no Git.

## Aplicar migrations

Com a aplicação parada:

```powershell
dotnet ef migrations list --project src/MeiaEntrada.Web --startup-project src/MeiaEntrada.Web
dotnet ef database update --project src/MeiaEntrada.Web --startup-project src/MeiaEntrada.Web
```

Esperado: migrations aplicadas e banco criado no caminho configurado. Na inicialização e nos testes, verificar journal_mode=wal, foreign_keys=1 e synchronous=FULL (valor 2), configurados conforme o modelo. Uma segunda execução de database update não deve reaplicar migrations ou perder registros. Não usar EnsureCreated nem gerar script `--idempotent`.

Existe somente a migration 20261008203846_InitialCreate; atualização desde uma segunda versão ainda não se aplica. O teste de migrations cria um arquivo novo a partir de zero e reaplica update verificando preservação de dados. Quando houver uma segunda migration, também parte da versão anterior com eventos, ingressos e vendas e verifica a atualização, FKs, índices, CHECKs, saldos e histórico. Essas verificações usam arquivos próprios, não o banco manual.

## Executar e navegar

```powershell
dotnet run --project src/MeiaEntrada.Web --no-build --no-launch-profile --urls http://localhost:5080
```

Abrir `http://localhost:5080`. Esperado: redirecionamento para Eventos, navegação para Ingressos, Registrar venda e Histórico, sem login. Formulários e tabelas usam Razor e Bootstrap local. O perfil local HTTP acima deve funcionar sem redirecionamento HTTPS obrigatório; isso não define publicação em produção.

### Roteiro funcional básico

1. Cadastrar evento “Sessão de teste”, descrição “Validação inicial”, local “Sala A”, data 2026-12-01 e horário 19:00. Conferir lista/detalhe e editar o local para “Sala B”. Datas passadas continuam válidas caso o roteiro seja executado depois dessa data.
2. Cadastrar inteira a R$ 40,00 com saldo 10 e meia-entrada a R$ 20,00 com saldo 5. Consultar os dois ingressos. Duplicar um tipo no mesmo evento deve ser rejeitado, sem criar novo registro.
3. Selecionar inteira, quantidade 3 e conferir R$ 120,00. Confirmar: uma venda registrada, saldo 7, preço histórico R$ 40,00 e total R$ 120,00.
4. Tentar vender 8 unidades: recusar e manter saldo 7. Tentar quantidade 0, negativa ou fracionária e preço com três casas: mostrar erros sem alterações. Conferir preservação dos dados válidos do formulário.
5. Alterar nome do evento e preço atual da inteira para R$ 50,00. A primeira venda deve manter nome original e total R$ 120,00. Nova venda deve usar o preço atual (teste separadamente, para não alterar o saldo esperado no passo 6).
6. Cancelar a primeira venda: situação cancelada e saldo 10, com instante de cancelamento. Reenviar o cancelamento: saldo e instante permanecem iguais.
7. Tentar excluir o ingresso ou evento vinculado: rejeitar, inclusive após cancelamento. Criar outro evento sem vínculos e confirmar sua exclusão; testar também desistir da confirmação.
8. Recarregar e reiniciar a aplicação usando a mesma configuração: cadastros, saldos e histórico persistem. Verificar todos os campos do histórico e navegar por páginas quando houver mais de 25 vendas.

### Edição e conferência desatualizadas

- Abrir edição de ingresso em uma aba. Em outra, vender uma unidade. Enviar a edição antiga deve retornar conflito com saldo atual e não sobrescrever a venda; revisar e confirmar o novo formulário permite o ajuste.
- Repetir com cancelamento, outra edição e sequência venda+cancelamento que retorna ao saldo inicial: a versão antiga continua inválida.
- Conferir uma venda; em outra aba alterar o preço; confirmar a conferência antiga. Nenhuma venda é criada até o operador conferir o novo total e confirmar novamente.
- Após ajuste manual de saldo para 20, cancelar uma venda registrada de 3 unidades: saldo final 23. Usar venda e dados próprios deste cenário.

## Executar testes automatizados

```powershell
dotnet test MeiaEntrada.slnx --no-build --no-restore --logger "trx;LogFileName=meia-entrada.trx" --results-directory ./TestResults
```

Esperado: todos os testes aprovados e nenhum ignorado. As classes já existem; executar os filtros abaixo para verificações focadas:

```powershell
dotnet test tests/MeiaEntrada.Tests --no-build --filter "FullyQualifiedName~EstoqueConcorrenciaTests"
dotnet test tests/MeiaEntrada.Tests --no-build --filter "FullyQualifiedName~AtomicidadeTests"
dotnet test tests/MeiaEntrada.Tests --no-build --filter "FullyQualifiedName~ContratoWebTests"
dotnet test tests/MeiaEntrada.Tests --no-build --filter "FullyQualifiedName~MigrationsTests"
```

Os testes abrangem validação e cálculos (unitários), persistência/transações/migrations (integração) e rotas/formulários (WebApplicationFactory). Testes HTTP obtêm cookie e token antiforgery via GET; não desabilitar a proteção para facilitar as verificações.

## Matriz de integridade e concorrência

Cada cenário usa seu próprio arquivo SQLite temporário com migrations e a mesma configuração da aplicação. Dentro do cenário, cada worker abre conexão e DbContext próprios para o mesmo arquivo. Sincronizar o início antes de BEGIN IMMEDIATE, nunca aguardar todos os workers depois de adquirirem o direito de escrita. Capturar exceções, impor prazo de término e ler o resultado com contexto novo. Não usar EF InMemory, SQLite em memória ou uma conexão compartilhada para provar concorrência de arquivo.

| Cenário | Preparação e execução | Resultado obrigatório |
|---|---|---|
| SC-004 | Saldo 10; 20 solicitações simultâneas de 1 unidade | Exatamente 10 vendas, 10 recusas por estoque e saldo 0; nenhum timeout aceito como recusa por estoque |
| Disputa de saldo | Saldo 5; duas vendas simultâneas de 4 | Uma aceita, outra recusada; saldo 1 |
| SC-005 | Venda de 3 unidades; 20 cancelamentos simultâneos | Uma transição, reposição total 3 e um instante de cancelamento estável |
| Venda/cancelamento | Saldo 7; cancelar venda de 3 e registrar venda de 2 simultaneamente | Saldo 8, uma venda cancelada e a nova registrada |
| Edição antiga | Abrir versão, concluir movimentação/edição e enviar versão anterior | Conflito, nenhuma perda de movimentação; cobrir saldo que volta ao número original |
| Preço concorrente | Alterar preço após conferir venda | Preço conferido válido ou reconfirmação obrigatória; nunca total inesperado |
| Exclusão/vínculo concorrente | Venda versus exclusão de evento/ingresso ou mudança de evento/tipo | Ordem serial válida; venda nunca sem origem; vínculo imutável após venda |
| SC-006 | Injetar falha após primeira escrita e antes de commit em venda e cancelamento | Nenhuma alteração parcial; situação, saldo, versão e histórico coerentes |
| SC-007 | Editar evento/preço e tentar excluir registros vendidos, inclusive cancelados | 100% das vendas preservadas com dados originais |
| Banco ocupado | Conexão A retém escrita por mais de 30 s; operação B usa outra conexão | B retorna 503 após espera limitada, sem mutação; A é liberada com segurança no final |
| Constraints e limites | Inserir valores inválidos por persistência controlada de teste | Rejeitar FKs inexistentes, tipo duplicado, negativos, frações, total incoerente e overflow |
| Migração e reinício | Atualizar schema com dados e reabrir banco | Dados/constraints preservados, sem nova aplicação indevida de migrations |

Nos testes normais com 20 workers, transações não contêm atrasos artificiais. Falha por timeout exige investigar a implementação/ambiente, não relaxar a contagem dos critérios. O teste de contenção prolongada é separado e intencional; ele pode durar mais de 30 segundos. O protocolo deve funcionar sem semáforo em memória; incluir verificação com dois hosts/processos usando o mesmo arquivo local se a implementação introduzir qualquer coordenação no processo.

Para falhas entre escritas, usar interceptação controlada de persistência no ambiente de teste; não acrescentar rotas de falha à aplicação. Uma resposta com commit desconhecido não pode desencadear repetição automática de venda. O teste HTTP dessa situação verifica a orientação para consultar o histórico.

## Interface, segurança e usabilidade

Verificar estados vazios, links, labels, foco por teclado, confirmação/desistência, tabelas em tela menor e mensagens sem depender apenas de cor. Inserir texto contendo marcação/script e conferir que é exibido como texto. Enviar POST sem antiforgery, valores inválidos fora do formulário e campos extras: nenhuma alteração indevida. Erros fora de Development não devem revelar stack trace, SQL, caminho do banco ou dados sensíveis. Os testes HTTP devem exercitar também o ambiente Production para as páginas de erro.

Avaliar cinco participantes representando operadores, sem orientação durante as tarefas:

- SC-001: pelo menos quatro cadastram evento e dois tipos em até cinco minutos.
- SC-002: pelo menos quatro registram uma venda em até um minuto e localizam/cancelam em até um minuto adicional.

Registrar tempos individuais, conclusão e dificuldades. Registrar também versão do SDK, versões dos pacotes, relatório de testes, banco/configuração utilizados e verificações não executadas. SC-003 só está satisfeito quando todos os cenários e casos de borda aplicáveis passam. Resultados pendentes ou falhas impedem declarar a implementação concluída.

## Ambiente verificado na implementação

SDK encontrado: 10.0.401 (Windows x64), fixado em global.json; instalação dispensada. Runtime ASP.NET Core 10.0.12. Evidências e pendências em validation.md.


## Resultado da execução deste guia (2026-10-09)

Restore locked, ferramenta EF, build e suíte passaram: **197 testes aprovados, nenhum com falha ou ignorado**, build sem avisos/erros. Os quatro filtros foram verificados em seleção conjunta: EstoqueConcorrenciaTests (5), AtomicidadeTests (4), ContratoWebTests (9), MigrationsTests (13). O filtro ContratoWebTests é literal e não inclui todos os arquivos com sufixo CoberturaTests; a suíte completa inclui esses complementos.

Migration inicial aplicada e reaplicada sem alterações em `var/validacao-final/quickstart.db`; host iniciado em `http://localhost:5087`, GET /eventos/ retornou 200. Use 5080 normalmente ou outra porta livre. Ctrl+C encerra a aplicação; para reiniciar, mantenha as mesmas variáveis de banco e chaves. Em um novo terminal, execute novamente o bloco que define as variáveis antes de iniciar a aplicação ou usar EF.

O SDK já estava instalado; não houve instalação em máquina limpa. Instale o **SDK 10.0.401** pelo instalador oficial .NET para seu sistema e abra novo terminal antes de executar este guia. O projeto exige essa versão exata. Mantenha o diretório de chaves acessível somente às contas autorizadas do sistema: a configuração local persiste XML sem criptografia em repouso. Não apague as chaves ao reiniciar.

Naquela etapa, o roteiro funcional detalhado, teclado/foco, telas menores e avaliação com cinco participantes permaneciam pendentes. O relato complementar de 2026-10-10 acrescenta evidência manual de visualização em celular, conforme abaixo. Nenhum procedimento pendente deve ser marcado como aprovado apenas porque a suíte automatizada passou.

## Consolidação de evidências (2026-10-10)

Reexecutados `dotnet build MeiaEntrada.slnx --no-restore` (0 avisos/erros) e a suíte completa (197 aprovados, 0 falhas, 0 ignorados):

```powershell
dotnet test MeiaEntrada.slnx --no-build --no-restore --logger "trx;LogFileName=consolidacao-2026-10-10.trx" --results-directory TestResults
```

O responsável relatou cadastro/edição/exclusão de eventos, gerenciamento básico de ingressos, venda/baixa, consulta ao histórico, cancelamento/restituição, persistência após reinício, campos obrigatórios de eventos, bloqueio da exclusão de eventos com ingressos e responsividade em visualização de celular. São evidências manuais relatadas, sem dados suficientes para aprovar todos os passos deste roteiro ou todas as telas/resoluções.

Permanecem os casos detalhados dos cinco checkpoints, teclado/foco, demais verificações de interface e T055 (cinco participantes). T053/T054 estão parcialmente comprovadas; SC-003 não está satisfeito. Nenhum bloqueador técnico crítico foi identificado no escopo verificado. Consulte [validation.md](validation.md#consolidação--2026-10-10) para a matriz atual, limitações, resultado técnico e recomendações. Spec.md e seus critérios de aceite foram preservados.
