# Project Brief: TodoList SDD

**ID:** IDEA-001
**Status:** Accepted
**Criado em:** 2026-10-02

## Problema

Pessoas precisam organizar tarefas pessoais de forma clara e acessível. O projeto também precisa servir como portfólio e laboratório prático de Spec-Driven Development, tornando decisões e mudanças auditáveis.

## Visão

Construir uma aplicação web de tarefas inspirada no Microsoft To Do, sem buscar uma cópia completa. O produto deve evoluir em pequenos incrementos demonstráveis, cada um iniciado por uma especificação.

## Público inicial

Uma pessoa que deseja gerenciar suas próprias tarefas em navegador desktop ou móvel. Personas, autenticação e colaboração ainda precisam ser especificadas antes de entrar no produto.

## Capacidades candidatas, ainda não aprovadas como escopo

- criar, visualizar, editar, concluir e excluir tarefas;
- organizar tarefas em listas;
- definir datas, importância e lembretes;
- buscar e filtrar tarefas;
- manter dados entre sessões;
- usar a aplicação com teclado e tecnologias assistivas.

Esta lista orienta descoberta; ela não substitui specs de funcionalidade.

## Fora do escopo inicial

- paridade completa com Microsoft To Do;
- aplicativos móveis nativos;
- colaboração em tempo real;
- integrações com ecossistemas de terceiros;
- arquitetura distribuída sem necessidade comprovada.

## Restrições e escolhas iniciais

- monorepo público;
- frontend React;
- backend C#/.NET 10;
- banco gratuito e publicável: PostgreSQL;
- processo SDD e documentação em português;
- nenhuma funcionalidade é implementada durante a preparação desta fundação.

## Sinais de sucesso do projeto

- uma pessoa consegue executar e demonstrar incrementos completos;
- cada comportamento entregue é rastreável até requisito, tarefa, teste e evidência;
- uma nova sessão do Codex consegue retomar o trabalho lendo apenas o repositório;
- o projeto pode ser publicado usando serviços compatíveis com a stack escolhida.
