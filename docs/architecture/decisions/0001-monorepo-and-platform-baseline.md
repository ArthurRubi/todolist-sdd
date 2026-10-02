# ADR-0001: Adotar monorepo web com React, .NET e PostgreSQL

**Status:** Accepted
**Data:** 2026-10-02
**Decisores:** mantenedor do projeto

## Contexto

O projeto precisa exercitar SDD em uma aplicação full stack simples, ser compreensível como portfólio e permitir publicação futura com baixo custo. O ambiente local já possui .NET 10 e npm 10.8.2.

## Decisão

Manter frontend, backend e documentação no mesmo repositório. Usar React com TypeScript no frontend, ASP.NET Core Web API em .NET 10 no backend e PostgreSQL para persistência. Usar OpenAPI como contrato entre os módulos.

O scaffold e as versões exatas das dependências serão definidos no primeiro plano técnico que necessitar deles.

## Consequências

- Uma única alteração pode manter spec, contrato e ambos os módulos sincronizados.
- PostgreSQL é software livre, amplamente hospedável e evita trocar o modelo de dados ao publicar.
- O monorepo requer gates de CI por módulo para evitar builds desnecessários no futuro.
- TypeScript adiciona uma etapa de compilação, mas reduz ambiguidades no contrato do frontend.
- A aplicação começa como monólito modular; serviços distribuídos exigiriam nova decisão e necessidade comprovada.

## Alternativas consideradas

- **SQLite:** excelente para desenvolvimento local, porém menos representativo de uma publicação multiusuário e sujeito a diferenças ao migrar depois.
- **SQL Server:** integração forte com .NET, mas PostgreSQL simplifica a proposta gratuita e multiplataforma.
- **Repositórios separados:** isolamento maior, com custo desnecessário de sincronização para uma aplicação pequena e um único mantenedor.
