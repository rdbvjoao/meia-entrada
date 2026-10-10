# Contrato HTTP e interface web

**Base**: [spec.md](../spec.md), [data-model.md](../data-model.md) e [plan.md](../plan.md).

Aplicação ASP.NET Core MVC monolítica: Controllers recebem ViewModels, invocam serviços e retornam Razor Views. HTML5, CSS3 e Bootstrap 5 com recursos locais em `wwwroot`. Interface pt-BR, sem login, API JSON pública ou integrações externas.

## Regras comuns

IDs de rota são inteiros positivos. GET não altera o domínio. Todo POST exige token antiforgery emitido pelo formulário Razor e cookie correspondente; aplicar `AutoValidateAntiforgeryToken` globalmente. Formulários usam `application/x-www-form-urlencoded`, ViewModels com campos permitidos e validação de ModelState, complementada pelas regras transacionais nos serviços. Nada do formulário é anexado diretamente como entidade para gravação.

## Rotas e resultados

| Método e rota | Entrada / conteúdo | Resultado de sucesso |
|---|---|---|
| GET `/` | Sem parâmetros | 302 para `/eventos/` |
| GET `/eventos/` | `page` opcional | 200 tabela de eventos |
| GET `/eventos/novo/` | Sem parâmetros | 200 formulário |
| POST `/eventos/novo/` | Nome, Descricao, Local, Data, Horario | 302 para detalhe do novo evento |
| GET `/eventos/{id}/` | ID | 200 campos completos e link aos ingressos |
| GET/POST `/eventos/{id}/editar/` | GET carrega; POST campos do cadastro | 200 formulário / 302 para detalhe |
| GET/POST `/eventos/{id}/excluir/` | GET confirmação; POST `Confirmar=sim` | 200 confirmação / 302 para lista |
| GET `/ingressos/` | `evento` e `page` opcionais | 200 tabela, filtrada por evento quando indicado |
| GET `/ingressos/novo/` | `evento` opcional para pré-seleção | 200 formulário |
| POST `/ingressos/novo/` | EventoId, Tipo, Preco, QuantidadeDisponivel | 302 para detalhe do ingresso |
| GET `/ingressos/{id}/` | ID | 200 evento, tipo, preço e saldo |
| GET/POST `/ingressos/{id}/editar/` | POST campos do cadastro e Versao | 200 formulário / 302 para detalhe |
| GET/POST `/ingressos/{id}/excluir/` | GET confirmação; POST `Confirmar=sim` | 200 confirmação / 302 para lista |
| GET `/vendas/nova/` | `evento` opcional | 200 seleção de evento, ingresso e quantidade |
| POST `/vendas/conferir/` | EventoId, IngressoId, Quantidade | 200 resumo; não grava venda nem reserva saldo |
| POST `/vendas/` | Conferencia protegida e `Confirmar=sim` | 302 para detalhe da nova venda |
| GET `/vendas/` | `page` opcional | 200 histórico, inclusive canceladas |
| GET `/vendas/{id}/` | ID | 200 dados históricos completos e situação |
| GET/POST `/vendas/{id}/cancelar/` | GET confirmação; POST `Confirmar=sim` | 200 confirmação / 302 para detalhe com mensagem |

Seleção de evento usa GET `/vendas/nova/?evento={id}` e oferece só ingressos desse evento; funciona sem JavaScript obrigatório. Exibir preço, disponibilidade e quantidade. Desabilitar confirmação sem oferta disponível e revalidar no servidor. Confirmações de exclusão/cancelamento identificam o registro e oferecem Voltar sem mutação.

## Formatos e validação

- Nome, Descricao e Local: texto obrigatório, sem aceitar somente whitespace. Remover espaços das extremidades. Descrição não é HTML.
- Data: `YYYY-MM-DD`; Horario: `HH:MM`. Exibir datas em pt-BR e aceitar datas passadas.
- Tipo: valores textuais `inteira` e `meia_entrada`, convertidos explicitamente para o enum do domínio. Rótulos Inteira e Meia-entrada.
- Preco: decimal textual sem separador de milhar, aceitando ponto ou vírgula decimal e no máximo duas casas. Rejeitar expoentes, NaN, infinito, negativos, precisão excedente e sintaxe ambígua. Converter via decimal para centavos long, sem arredondar.
- Quantidades, IDs e Versao: inteiros; quantidade de venda > 0, disponível >= 0, versão >= 1. Rejeitar frações e valores além da representação do modelo com mensagem de campo.
- Campos não previstos não são enviados à persistência. Situação, horários, total e cópias históricas de venda são definidos pelo servidor.

## Conferência e confirmação de venda

A conferência valida existência, vínculo, quantidade e saldo observado; calcula preço e total para exibição. ASP.NET Core Data Protection protege `{EventoId, IngressoId, Tipo, PrecoUnitarioCentavos, Quantidade}` com finalidade `MeiaEntrada.Venda.Conferencia.v1`. O resultado opaco vai em Conferencia. Não há reserva, transação aberta entre telas ou credencial de usuário nesse conteúdo.

Na confirmação, desproteger e validar o conteúdo antes da transação, iniciar a transação imediata e reler vínculo, tipo, preço e saldo. Preço diferente retorna 409 com novo resumo e nova conferência protegida, exigindo clique explícito para reconfirmar; nunca gravar com preço não conferido. Vínculo/tipo diferente exige refazer a seleção. Insuficiência informa saldo atual e permite corrigir quantidade. Não confiar em total ou preço livre recebido do cliente.

Conferência inválida/adulterada retorna 400 e oferece nova seleção. Reenvio de uma conferência válida é outra solicitação de venda sujeita ao saldo: a especificação não exige idempotência de criação. Redirecionar após sucesso evita reenvio ao atualizar a página do resultado. Não repetir uma venda automaticamente após falha de resultado incerto.

## Respostas de erro e concorrência

| Situação | HTTP | Resultado |
|---|---|---|
| Leitura/conferência válida | 200 | View correspondente |
| Mutação concluída | 302 | Location conforme tabela e mensagem na página de destino |
| Campos inválidos ou confirmação ausente | 400 | Formulário com erros e entradas válidas preservadas |
| Antiforgery ausente/inválido | 400 | Erro seguro, nenhuma mutação |
| Conferência inválida | 400 | Orientar nova conferência |
| Registro inexistente | 404 | Página compreensível com navegação |
| Método não previsto | 405 | Nenhuma mutação |
| Saldo insuficiente, preço alterado, versão antiga, duplicidade ou exclusão vinculada | 409 | Explicar conflito e oferecer correção |
| Escritor ocupado até timeout; início não realizado ou rollback confirmado | 503 | Preservar dados, mensagem “Sistema ocupado; tente novamente”, sem repetição automática |
| Falha inesperada ou resultado de commit desconhecido | 500 | Mensagem segura; se resultado incerto, orientar consulta do histórico antes de nova venda |

Erro de banco ocupado não é convertido em saldo insuficiente. Erros internos não revelam caminhos do arquivo, SQL, segredos ou stack traces. Cancelamento já efetuado retorna 302 para detalhe com “Venda já cancelada”, preservando saldo, Versao e CanceladaEmUtc. Conflito de versão mostra saldo atual e valor tentado separadamente e exige revisão antes de novo envio com a versão atual.

## Consultas e apresentação

Listas usam 25 registros por página com navegação anterior/próxima; `page` ausente vale 1, inválido ou inexistente retorna 404. Lista vazia na página 1 retorna 200 com orientação. Eventos ordenam Data/Horario/Id; ingressos EventoId/Tipo/Id; histórico RegistradaEmUtc e Id decrescentes. Todos os registros continuam acessíveis pela paginação.

Histórico mostra identificação, nome original do evento, tipo original, quantidade, total original, data/hora e situação. Detalhe também mostra preço unitário e, se cancelada, instante do cancelamento. Converter centavos para reais; instantes UTC para America/Sao_Paulo. Relações atuais não substituem valores históricos.

Layout Razor comum com Eventos, Ingressos, Registrar venda e Histórico. Formulários têm labels associados, campos obrigatórios, resumo de erros e ações explícitas. Tabelas Bootstrap apresentam cabeçalhos, ações nomeadas e adaptação a telas menores. Mensagens usam texto, sem depender só de cor; preservar foco e navegação por teclado. Razor codifica conteúdo do operador automaticamente; não usar Html.Raw nessas entradas.

Não expor edição, exclusão ou reativação de venda, nem painel administrativo ou rotas de autenticação. Tentativas retornam 404 ou 405 e não alteram dados. Bootstrap, CSS e scripts são locais; nenhuma CDN é necessária para usar a aplicação.

## Validação do contrato

Testes com WebApplicationFactory e SQLite real devem obter formulário/cookie/token antiforgery antes de POST, impedir redirecionamento automático quando verificarem 302 e conferir HTML, Location, mensagens e ausência de gravações em erro. Cobrir 400/404/405/409/503, cancelamento repetido, XSS, confirmação de preço e campos extras. Os cenários de banco ocupado usam conexões distintas, sem travar a thread que precisa liberar o escritor. A navegação real e os tempos de tarefa são verificados conforme [quickstart.md](../quickstart.md).
