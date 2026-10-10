# Feature Specification: Meia-Entrada — primeira versão

**Feature Branch**: Não criada; não há hook de criação de branch neste projeto.

**Created**: 2026-10-08

**Status**: Validada para planejamento.

**Input**: Sistema web para gerenciar eventos e ingressos, registrar vendas com controle de disponibilidade, consultar histórico e cancelar vendas com restituição única. Interface simples com formulários, tabelas e validação. Sem pagamentos reais, autenticação, QR Code ou integrações externas. Decisões tecnológicas reservadas ao planejamento, respeitando a constituição 1.0.0.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Gerenciar eventos (Priority: P1)

Como operador, quero manter os eventos para organizar a oferta de ingressos.

**Why this priority**: O cadastro de eventos é a base das demais operações e permite consultar a programação.

**Independent Test**: Sem dados iniciais, cadastrar, consultar, editar e excluir um evento sem vínculos.

**Acceptance Scenarios**:

1. **Given** um formulário vazio, **When** nome, descrição, local, data e horário válidos são informados e salvos, **Then** o evento recebe identificação própria, aparece na listagem e pode ser consultado com todos os campos.
2. **Given** evento existente, **When** seus campos são editados validamente, **Then** a consulta apresenta os novos dados, mantendo a identificação.
3. **Given** campo obrigatório vazio ou composto apenas por espaços, data inexistente ou horário inválido, **When** o formulário é enviado, **Then** a operação é rejeitada, os campos problemáticos são identificados e os dados válidos digitados permanecem disponíveis para correção.
4. **Given** evento sem ingressos nem vendas vinculados, **When** sua exclusão é confirmada, **Then** ele deixa de aparecer nas consultas; desistir da confirmação preserva o registro.
5. **Given** evento com ingressos ou vendas vinculados, inclusive canceladas, **When** sua exclusão é solicitada, **Then** ela é bloqueada com explicação dos vínculos, sem excluir registros relacionados.

### User Story 2 - Gerenciar ingressos (Priority: P1)

Como operador, quero definir os tipos, preços e saldos por evento para preparar as vendas.

**Why this priority**: Define a oferta que poderá ser vendida e evita saldos inválidos.

**Independent Test**: Com um evento preparado como dado de teste, cadastrar, consultar, editar e excluir ingressos sem executar o fluxo de vendas.

**Acceptance Scenarios**:

1. **Given** evento existente, **When** um ingresso inteira por R$ 40,00 com 10 unidades é cadastrado, **Then** listagem e consulta mostram evento, tipo, preço e saldo corretos.
2. **Given** ingresso existente, **When** preço e quantidade disponível são editados validamente, **Then** os novos valores são apresentados e usados nas próximas vendas.
3. **Given** evento inexistente, tipo diferente de inteira ou meia-entrada, preço negativo ou com mais de duas casas decimais, quantidade fracionária ou negativa ou campo obrigatório ausente, **When** o cadastro ou edição é enviado, **Then** a operação é rejeitada com indicação do problema e sem alterar dados.
4. **Given** tipo já cadastrado para um evento, **When** outro ingresso com o mesmo par evento/tipo é criado ou editado, **Then** a duplicidade é rejeitada. Preço zero e saldo zero são aceitos.
5. **Given** ingresso sem vendas, **When** sua exclusão é confirmada, **Then** ele é removido; com qualquer venda vinculada, inclusive cancelada, a exclusão é bloqueada e a razão é informada.
6. **Given** ingresso com vendas vinculadas, **When** se tenta mudar evento ou tipo, **Then** a alteração é bloqueada; preço e saldo continuam editáveis. Sem vendas, evento e tipo podem mudar respeitando existência e unicidade.
7. **Given** edição de saldo aberta e venda, cancelamento ou outra edição concluída depois da abertura, **When** o saldo antigo é enviado, **Then** a edição é rejeitada, o saldo atual é mostrado e uma nova confirmação é exigida para editar, sem sobrescrever a movimentação concluída.

### User Story 3 - Registrar vendas (Priority: P1)

Como operador, quero selecionar evento, tipo e quantidade para registrar uma venda sem ultrapassar a disponibilidade.

**Why this priority**: É o fluxo central de venda e movimentação do estoque.

**Independent Test**: Com evento e ingresso preparados como dados de teste, registrar vendas válidas e inválidas e conferir venda e saldo resultantes.

**Acceptance Scenarios**:

1. **Given** 10 ingressos de R$ 40,00 disponíveis, **When** o operador seleciona evento, tipo e quantidade 3, confere total de R$ 120,00 e confirma, **Then** uma venda registrada de 3 unidades é criada, o saldo passa a 7 e uma mensagem confirma a operação.
2. **Given** saldo 2, **When** se tenta vender 3 unidades, **Then** nenhuma venda é criada, o saldo permanece 2 e a insuficiência é informada. Vender exatamente 2 unidades é permitido e deixa saldo zero.
3. **Given** quantidade vazia, zero, negativa ou fracionária ou evento/ingresso inexistente ou incompatível, **When** a venda é enviada, **Then** ela é rejeitada sem alterar o saldo.
4. **Given** saldo 5 e duas solicitações simultâneas de 4 unidades, **When** são processadas, **Then** somente uma é registrada, a outra recebe mensagem de insuficiência e o saldo final é 1.
5. **Given** falha durante a conclusão da venda, **When** a operação não é concluída, **Then** não existe venda parcial nem redução isolada de saldo; quando concluída, ambos os resultados existem juntos.
6. **Given** preço alterado após a apresentação do total, **When** o operador tenta confirmar, **Then** a venda não é registrada com valor diferente do conferido, o novo total é mostrado e exige nova confirmação. O saldo é novamente verificado na confirmação.
7. **Given** evento sem ingressos ou tipo com saldo zero, **When** o operador prepara uma venda, **Then** a interface informa a indisponibilidade e impede a confirmação sem saldo suficiente.

### User Story 4 - Consultar histórico (Priority: P2)

Como operador, quero consultar vendas e situações para acompanhar os registros, inclusive após mudanças nos cadastros.

**Why this priority**: Permite conferência das operações e acesso ao cancelamento.

**Independent Test**: Com vendas registradas e canceladas preparadas como dados de teste, conferir campos, ordem e preservação dos dados.

**Acceptance Scenarios**:

1. **Given** vendas existentes, **When** o histórico é aberto, **Then** todas podem ser consultadas, da mais recente para a mais antiga, com identificação, evento, tipo, quantidade, valor total, data e horário e situação registrada ou cancelada.
2. **Given** venda de 2 unidades a R$ 40,00, **When** o nome do evento e o preço do ingresso são alterados, **Then** o histórico mantém nome do evento na venda, tipo, quantidade, preço unitário original e total de R$ 80,00.
3. **Given** nenhuma venda, **When** o histórico é aberto, **Then** uma mensagem informa que não há vendas e oferece acesso ao registro de venda.

### User Story 5 - Cancelar vendas (Priority: P2)

Como operador, quero cancelar uma venda registrada, mantendo o histórico e devolvendo as unidades uma única vez.

**Why this priority**: Completa o ciclo de venda com reversão controlada.

**Independent Test**: Com venda registrada de 3 unidades e saldo 7 como dados de teste, cancelar e repetir a solicitação, verificando histórico e saldo.

**Acceptance Scenarios**:

1. **Given** venda registrada de 3 unidades e saldo 7, **When** o cancelamento é confirmado, **Then** a situação muda para cancelada, data e horário do cancelamento são registrados, o saldo passa a 10 e os dados originais da venda permanecem consultáveis.
2. **Given** venda já cancelada, **When** o cancelamento é repetido, **Then** a aplicação informa essa situação e não modifica saldo nem data do primeiro cancelamento.
3. **Given** duas solicitações simultâneas de cancelamento da mesma venda, **When** são concluídas, **Then** a quantidade é restituída somente uma vez.
4. **Given** falha durante o cancelamento, **When** a operação não é concluída, **Then** situação e saldo permanecem anteriores; se concluída, ambos são atualizados juntos.
5. **Given** solicitação de cancelamento, **When** o operador desiste da confirmação ou a venda não existe, **Then** nenhum dado é alterado e o resultado é informado.
6. **Given** saldo 7, venda de 3 unidades a cancelar e nova venda simultânea de 2 unidades, **When** ambas são concluídas, **Then** o saldo final é 8, com uma venda cancelada e a nova venda registrada.

### Edge Cases

- Listas vazias apresentam mensagem e ação adequada para iniciar o cadastro ou registro.
- Vender exatamente o saldo é permitido; saldo zero impede novas vendas até reposição ou cancelamento.
- Cancelar após ajuste manual soma a quantidade vendida ao saldo vigente, sem restaurar saldo antigo.
- Venda gratuita tem total zero e segue as mesmas regras de estoque e cancelamento.
- Exclusões e alterações simultâneas com vendas preservam vínculos: nenhuma venda pode apontar para evento ou ingresso removido.
- Edições de saldo não podem apagar movimentações ocorridas desde a abertura do formulário.
- Textos com marcação ou comandos aparecem como conteúdo, sem executar ações na interface.
- Falhas e registros inexistentes geram mensagens compreensíveis sem detalhes internos sensíveis.
- Datas e horários inválidos são rejeitados; datas passadas são permitidas conforme premissa desta versão.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Permitir cadastrar, listar, consultar, editar e excluir eventos identificados individualmente, com nome, descrição, local, data e horário obrigatórios. Aceitação: história 1, cenários 1–4.
- **FR-002**: Rejeitar campos textuais obrigatórios vazios ou só com espaços e datas/horários inválidos, preservando dados válidos digitados para correção. Aceitação: história 1, cenário 3.
- **FR-003**: Permitir cadastrar, listar, consultar, editar e excluir ingressos vinculados a evento existente, com tipo inteira ou meia-entrada, preço não negativo de até duas casas decimais e saldo inteiro não negativo; todos os campos são obrigatórios. Aceitação: história 2, cenários 1–3 e 5.
- **FR-004**: Manter no máximo um ingresso de cada tipo por evento e permitir preço e saldo iguais a zero. Aceitação: história 2, cenário 4.
- **FR-005**: Bloquear exclusão de evento com ingressos ou vendas e de ingresso com qualquer venda, mesmo cancelada; informar o impedimento e não excluir registros relacionados. Aceitação: história 1, cenário 5; história 2, cenário 5.
- **FR-006**: Bloquear alteração de evento e tipo de ingresso com vendas vinculadas; demais alterações permitidas não podem modificar os dados históricos das vendas. Aceitação: história 2, cenário 6; história 4, cenário 2.
- **FR-007**: Interpretar edição da quantidade disponível como definição do saldo atual, não da capacidade total do evento, e rejeitar edição baseada em saldo desatualizado por movimentação ou outra edição. Aceitação: história 2, cenários 2 e 7.
- **FR-008**: Permitir selecionar evento, um tipo de ingresso desse evento e quantidade inteira positiva, exibindo preço unitário e total antes da confirmação da venda. Aceitação: história 3, cenários 1 e 3.
- **FR-009**: Calcular total como preço unitário vigente confirmado multiplicado pela quantidade, em reais e centavos, sem descontos ou taxas adicionais; mudança do preço exige apresentação do novo total e nova confirmação. Aceitação: história 3, cenários 1 e 6; caso de venda gratuita.
- **FR-010**: Conferir existência, vínculo e saldo na confirmação, rejeitar venda inválida ou sem estoque suficiente e jamais permitir saldo negativo, inclusive sob solicitações simultâneas. Aceitação: história 3, cenários 2–4 e 7.
- **FR-011**: Concluir venda e redução do saldo como uma única operação: ambos acontecem ou nenhum acontece. Aceitação: história 3, cenário 5.
- **FR-012**: Atribuir identificação, data e horário e situação registrada a cada venda concluída, mantendo evento, tipo, quantidade, preço unitário e total originais para consulta posterior. Aceitação: história 3, cenário 1; história 4, cenários 1–2.
- **FR-013**: Listar histórico em ordem decrescente de data e horário, com acesso a todas as vendas, inclusive canceladas, e campos exigidos na história 4. Em empate, ordenar pela identificação em ordem decrescente. Aceitação: história 4, cenários 1–3.
- **FR-014**: Permitir cancelar apenas vendas registradas, mediante confirmação, preservar seus dados, marcar situação cancelada, registrar data e horário do cancelamento e restituir a quantidade ao ingresso original. Aceitação: história 5, cenários 1 e 5.
- **FR-015**: Efetuar mudança de situação e restituição juntas, uma única vez por venda; repetição, simultaneidade ou falha não podem produzir restituição extra ou atualização parcial. Aceitação: história 5, cenários 2–4 e 6.
- **FR-016**: Não permitir editar, excluir nem reativar vendas. Correções exigem cancelamento e, se necessário, nova venda. Aceitação: histórico oferece consulta e cancelamento, sem ações de edição, exclusão ou reativação; tentativas dessas operações não alteram dados.
- **FR-017**: Oferecer navegação identificável entre eventos, ingressos, registro de venda e histórico, formulários rotulados, tabelas com ações claras e mensagens de sucesso, erro e ausência de dados. Excluir e cancelar exigem confirmação com identificação do registro. Aceitação: executar histórias 1–5 pela interface e conferir mensagens, confirmações e estados vazios.
- **FR-018**: Aplicar validação a toda operação recebida, mesmo quando verificações do formulário forem contornadas; exibir conteúdo informado como texto sem executar comandos e apresentar erros sem detalhes internos sensíveis. Aceitação: enviar valores inválidos fora do formulário, inserir texto com marcação executável e provocar falha; verificar rejeição, ausência de execução e mensagem segura.
- **FR-019**: Preservar cadastros, vendas e saldos após sair, retornar à aplicação e recarregar consultas. Aceitação: concluir operações das histórias 1–5 e conferir os mesmos resultados em nova consulta.
- **FR-020**: Restringir esta versão a eventos, ingressos, vendas e cancelamentos, sem pagamentos reais, autenticação de usuários, QR Code ou integrações externas. Aceitação: os fluxos funcionam sem login, pagamento, leitura de código ou dependência de serviço externo.

### Key Entities *(include if feature involves data)*

- **Evento**: Identificação, nome, descrição, local, data e horário. Pode ter ingressos dos dois tipos. Não pode ser excluído enquanto houver vínculos.
- **Ingresso**: Identificação, evento, tipo, preço unitário e quantidade atualmente disponível. O par evento/tipo é único. Recebe movimentações de venda e cancelamento e ajustes manuais válidos.
- **Venda**: Identificação, vínculos com evento e ingresso originais, nome do evento e tipo no momento da venda, quantidade, preço unitário aplicado, total, data e horário do registro, situação registrada ou cancelada e, quando cancelada, data e horário do cancelamento. Não é apagada nem tem seus dados originais reescritos.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Em avaliação com cinco participantes representando operadores, pelo menos quatro conseguem cadastrar um evento e seus dois tipos de ingresso em até cinco minutos, sem orientação durante a tarefa.
- **SC-002**: Pelo menos quatro desses cinco participantes conseguem registrar uma venda com dados já disponíveis em até um minuto e localizar e cancelar essa venda em até um minuto adicional, sem orientação durante as tarefas.
- **SC-003**: Todos os cenários de aceitação e casos de borda passam antes de declarar a versão concluída; falhas e verificações pendentes são registradas e impedem essa declaração.
- **SC-004**: Em 20 tentativas simultâneas de venda de uma unidade sobre saldo inicial 10, exatamente 10 vendas são registradas, 10 são rejeitadas por insuficiência e o saldo final é zero.
- **SC-005**: Em 20 solicitações simultâneas de cancelamento da mesma venda, a restituição total equivale exatamente à quantidade daquela venda, há um único cancelamento efetivo e o histórico original permanece íntegro.
- **SC-006**: Em todas as verificações de falha durante venda ou cancelamento, o resultado contém ambas as mudanças previstas ou nenhuma, sem diferenças entre vendas, situações e saldos.
- **SC-007**: Em todas as verificações de edição e exclusão com vínculos, 100% das vendas mantêm seus valores e dados originais, permanecem consultáveis e não perdem referências aos registros de origem.

## Assumptions

- O ator é um operador que gerencia cadastros e registra vendas; não há perfis de acesso nem identificação de compradores nesta versão.
- Cada venda contém um único evento e um único tipo de ingresso. Para outro evento ou tipo, registra-se outra venda.
- Inteira e meia-entrada têm preços definidos independentemente pelo operador; não há cálculo automático de desconto nem comprovação de elegibilidade nesta versão.
- Valores são expressos em reais com até duas casas decimais. Datas e horários são apresentados no horário de Brasília (America/Sao_Paulo).
- Todos os cinco campos do evento são obrigatórios. Nomes de eventos podem se repetir; suas identificações os distinguem.
- Não há restrição temporal solicitada para cadastro, venda ou cancelamento: eventos passados são permitidos e o cancelamento não tem prazo nesta versão.
- O saldo é controlado por ingresso, sem capacidade global adicional por evento. Ajustes manuais definem o saldo disponível; cancelamentos somam a quantidade original ao saldo vigente, mesmo após ajuste.
- A política conservadora de exclusão com vínculos e a preservação do nome original do evento na venda foram adotadas para cumprir a constituição e manter o histórico verificável.
- Confirmar uma nova venda representa uma nova operação; não há reserva prévia de ingressos. A proteção obrigatória contra repetições descrita nesta versão refere-se ao cancelamento.
- Eventos são pré-requisito para ingressos; eventos e ingressos são pré-requisitos para vendas; vendas são pré-requisito para histórico e cancelamento. Dados preparados permitem verificar cada história isoladamente.
- O planejamento deve demonstrar conformidade com os seis princípios e as regras de governança da constituição 1.0.0, incluindo separação de responsabilidades, validação independente da interface, segurança e verificações automatizadas das regras e operações críticas. Esta especificação não seleciona tecnologias nem altera a constituição.
