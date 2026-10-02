# Backend

API e camada de persistência da aplicação TodoList.

## Estado

Ainda não inicializado. A solution e os projetos .NET só serão criados por tarefas derivadas de uma especificação aprovada.

## Linha de base proposta

- ASP.NET Core Web API sobre .NET 10.
- Entity Framework Core para persistência.
- PostgreSQL como banco relacional principal.
- OpenAPI como contrato público da API.
- xUnit para testes e testes de integração com infraestrutura isolada.

## Limites do módulo

O backend é responsável pelas regras de negócio, validação autoritativa, autorização, persistência e evolução do contrato da API. Detalhes do banco não devem vazar para o contrato HTTP.

Quando o módulo for criado, este README deve documentar estrutura real, pré-requisitos, comandos de execução, testes, migrações e configuração local — sem armazenar segredos.
