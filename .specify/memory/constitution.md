

# Meia-Entrada Constitution

## Core Principles

### I. Simplicidade

A aplicação DEVE priorizar uma solução funcional, organizada e fácil de entender.
Cada funcionalidade e dependência DEVE atender a uma necessidade do escopo especificado.
Abstrações e infraestrutura adicionais DEVEM ter justificativa concreta no plano técnico;
possíveis necessidades futuras, isoladamente, não justificam sua inclusão. O objetivo inicial
é praticar o desenvolvimento de um sistema CRUD com o GitHub Spec Kit.

### II. Organização do código

O código DEVE separar apresentação, regras de negócio e acesso aos dados, com responsabilidades
identificáveis. Regras de estoque e vendas DEVEM ter uma implementação centralizada, evitando
duplicação entre interface e persistência. Essa separação DEVE facilitar manutenção e testes
sem exigir serviços independentes ou uma arquitetura complexa.

### III. Integridade dos dados

A aplicação DEVE validar campos obrigatórios, tipos e valores antes de persistir alterações.
Todo ingresso DEVE estar vinculado a um evento existente, ter tipo inteira ou meia-entrada,
preço não negativo e quantidade disponível inteira e não negativa. Vendas DEVEM exigir
quantidade inteira positiva e NÃO DEVEM exceder a disponibilidade, inclusive quando houver
requisições concorrentes.

O registro da venda e a redução de disponibilidade DEVEM ocorrer de forma atômica. O cancelamento
DEVE preservar o histórico e restituir a quantidade correspondente uma única vez; repetições
NÃO DEVEM aumentar o estoque novamente. Alterações e exclusões de eventos ou ingressos NÃO DEVEM
produzir referências inválidas nem apagar ou distorcer o histórico de vendas. A especificação
DEVE definir o comportamento de exclusão quando houver registros vinculados, respeitando essas
garantias.

### IV. Segurança básica

Dados recebidos DEVEM ser validados no servidor, mesmo quando a interface também os valida.
Entradas inválidas DEVEM ser rejeitadas com mensagens compreensíveis, sem expor credenciais,
consultas internas ou detalhes sensíveis. O acesso ao banco DEVE usar consultas parametrizadas
ou mecanismo equivalente seguro; entradas do usuário NÃO DEVEM ser concatenadas em comandos SQL.
Credenciais NÃO DEVEM ser incluídas no código versionado. Conteúdo apresentado na interface
DEVE ser tratado de modo a impedir a execução de código fornecido como entrada.

### V. Qualidade

As funcionalidades principais DEVEM ser implementadas e verificadas por critérios de aceitação,
com testes automatizados das regras de negócio e das operações de persistência críticas.
A verificação DEVE cobrir cadastro, consulta, atualização e exclusão de eventos e ingressos,
além de registro, consulta do histórico e cancelamento de vendas.

Os testes DEVEM incluir dados inválidos, estoque insuficiente, concorrência na venda,
cancelamento repetido, consistência após falha e preservação do histórico em alterações e
exclusões. A interface DEVE passar por verificação dos fluxos principais e das mensagens de erro.
Uma funcionalidade somente pode ser declarada concluída após passar nas verificações aplicáveis;
falhas ou verificações não executadas DEVEM ser registradas explicitamente.

### VI. Evolução gradual

O desenvolvimento DEVE começar pelas funcionalidades essenciais e avançar em incrementos
verificáveis. Recursos adicionais DEVEM ser especificados antes de sua implementação e NÃO DEVEM
comprometer os princípios existentes. Escolhas técnicas DEVEM permitir manutenção e evolução
sem antecipar funcionalidades fora do escopo da primeira versão.

## Escopo e restrições da primeira versão

O Meia-Entrada é uma aplicação web simples para gerenciamento de eventos e venda de ingressos.
O escopo inicial compreende:

- Cadastrar, consultar, editar e excluir eventos e ingressos.
- Vincular cada ingresso a um evento, com tipo inteira ou meia-entrada, preço e disponibilidade.
- Registrar vendas, consultar seu histórico e cancelar vendas com consistência de estoque.
- Oferecer interface web simples e intuitiva, com ações identificáveis, validações e retorno
  claro sobre sucesso ou erro das operações.

A primeira versão NÃO DEVE incluir pagamentos reais, autenticação de usuários, geração de
QR Code ou integração com serviços externos. A inclusão desses recursos exige revisão explícita
do escopo e desta restrição antes da implementação.

Linguagens, frameworks, banco de dados e demais tecnologias DEVEM ser definidos durante o
planejamento técnico, com justificativa compatível com a simplicidade e o objetivo de aprendizado.

## Fluxo de desenvolvimento e critérios de qualidade

O trabalho DEVE seguir o fluxo do GitHub Spec Kit: especificação dos comportamentos e critérios
de aceitação, planejamento técnico, decomposição em tarefas e implementação incremental.
Ambiguidades que afetem integridade dos dados DEVEM ser resolvidas na especificação antes da
implementação correspondente.

Cada plano DEVE verificar sua conformidade com esta constituição. Cada conjunto de tarefas DEVE
incluir a validação necessária aos comportamentos implementados. A revisão de cada mudança DEVE
verificar escopo, separação de responsabilidades, validação de entradas, consistência dos dados
e evidências dos testes aplicáveis. Mudanças que violem estas regras DEVEM ser corrigidas ou
precedidas por uma emenda explícita à constituição.

## Governance

Esta constituição rege as especificações, planos, tarefas e decisões de implementação do projeto.
Em caso de conflito entre esses artefatos, eles DEVEM ser ajustados para cumprir a constituição
vigente.

Emendas DEVEM registrar a motivação, as regras afetadas, o impacto nos artefatos e no comportamento
existente, além de eventual necessidade de migração. O responsável pelo projeto DEVE aprovar a
mudança de governança antes de sua adoção. Cada emenda DEVE atualizar a versão e a data da última
alteração, preservando a data original de ratificação.

O versionamento DEVE seguir MAJOR.MINOR.PATCH: MAJOR para remoções ou redefinições incompatíveis
de princípios ou regras; MINOR para novos princípios, seções ou ampliações materiais de orientação;
PATCH para esclarecimentos e correções sem mudança de significado. A versão 1.0.0 corresponde
à adoção inicial dos princípios fornecidos pelo responsável pelo projeto.

A conformidade DEVE ser revista no planejamento e na revisão das mudanças. Desvios NÃO DEVEM ser
aceitos implicitamente: exigem correção ou emenda conforme o procedimento acima.

**Version**: 1.0.0 | **Ratified**: 2026-10-08 | **Last Amended**: 2026-10-08
