# Protocolo de usabilidade: criação mínima de tarefa

**Spec:** [SPEC-001](./spec.md)  
**Critério:** SC-001  
**Estado da coleta:** Not started

## Objetivo

Verificar se pelo menos 90% das pessoas participantes conseguem criar uma tarefa somente com título e reencontrá-la em até 20 segundos, sem orientação. Com a amostra mínima de 10 pessoas, o critério exige ao menos 9 sucessos.

## Amostra e ambiente

- Recrutar no mínimo 10 pessoas que não tenham participado da implementação.
- Registrar apenas um identificador anônimo, nunca nome, e-mail ou conteúdo pessoal.
- Usar a mesma versão aprovada da aplicação, navegador suportado e banco reiniciado para cada sessão.
- Inserir previamente duas tarefas neutras para que “reencontrar” exija identificar a tarefa criada.
- Manter tamanho de tela, conexão e dispositivo comparáveis; registrar qualquer desvio.

## Preparação de cada sessão

1. Reiniciar os dados para o estado-base documentado.
2. Abrir a tela inicial e aguardar o carregamento completo.
3. Posicionar a pessoa diante da aplicação sem indicar controles.
4. Preparar um cronômetro que mostre centésimos, mas não seja visível à pessoa.
5. Ler exatamente a instrução abaixo; não responder perguntas sobre como usar a interface.

## Instrução padronizada

> “Crie uma tarefa chamada ‘Marcar consulta’ usando somente o título. Em seguida, mostre onde essa tarefa ficou na aplicação. Avise quando terminar.”

Não mencionar botão, campo, tecla, lista ou localização visual. Se a pessoa pedir ajuda operacional, responder: “Faça como considerar mais natural”.

## Cronometragem e resultado

- Iniciar o cronômetro ao terminar de ler a instrução.
- Parar quando a pessoa indicar a tarefa criada e ela estiver visível na lista ativa.
- Marcar **sucesso** somente se houver exatamente uma tarefa com o título solicitado, ela for reencontrada e o tempo for de no máximo 20,00 segundos.
- Marcar **falha** em timeout, desistência, duplicação, título incorreto, ajuda indevida ou tarefa não reencontrada.
- Em caso de falha técnica da aplicação ou ambiente, invalidar a sessão, corrigir o ambiente e recrutar uma sessão substituta; não convertê-la em sucesso ou falha de usabilidade.

## Registro de coleta

| Participante | Tempo (s) | Criou uma única tarefa? | Reencontrou? | Sem orientação? | Resultado | Observação objetiva |
|---|---:|---|---|---|---|---|
| P01 | — | — | — | — | Não coletado | — |
| P02 | — | — | — | — | Não coletado | — |
| P03 | — | — | — | — | Não coletado | — |
| P04 | — | — | — | — | Não coletado | — |
| P05 | — | — | — | — | Não coletado | — |
| P06 | — | — | — | — | Não coletado | — |
| P07 | — | — | — | — | Não coletado | — |
| P08 | — | — | — | — | Não coletado | — |
| P09 | — | — | — | — | Não coletado | — |
| P10 | — | — | — | — | Não coletado | — |

Acrescente linhas se houver mais participantes. Não preencha valores estimados nem resultados retroativos.

## Consolidação para a validação

Registrar quantidade de sessões válidas, sucessos, falhas, taxa de sucesso e distribuição dos tempos. SC-001 passa somente com amostra válida de pelo menos 10 pessoas e taxa mínima de 90%. Enquanto a coleta permanecer `Not started`, a validação integral deve registrar SC-001 como pendência ou exceção real, nunca como aprovado por automação.
