# Projeto: CapyBooks

CapyBooks é uma plataforma social para leitores, inspirada no IMDb, Goodreads e Letterboxd.

O foco do projeto **não** é gerenciamento de biblioteca ou empréstimo de livros. O objetivo é criar uma plataforma moderna para descoberta de livros, avaliações, listas personalizadas, estatísticas de leitura e interação entre usuários.

Este projeto deve ser desenvolvido pensando como um produto real, priorizando qualidade de código, escalabilidade e boas práticas de arquitetura.

---

# Stack obrigatória

## Backend
- .NET 10
- ASP.NET Core Web API
- C#
- Controllers
- Entity Framework Core
- PostgreSQL
- JWT Authentication
- Refresh Token
- FluentValidation
- AutoMapper
- Swagger

## Frontend
- Angular
- TypeScript
- SCSS
- Angular Material

O frontend deve ser um projeto completamente separado da API. Toda comunicação deve ocorrer através de uma API REST.

---

# Arquitetura

O projeto deve obrigatoriamente utilizar Arquitetura em Camadas (Layered Architecture).

Separação mínima:
- API
- Application
- Domain
- Infrastructure

Responsabilidades:

## API
- Controllers
- Middlewares
- Configurações
- Injeção de Dependência
- Autenticação

## Application
- Casos de uso
- DTOs
- Services
- Interfaces
- Validações

## Domain
- Entidades
- Enums
- Regras de negócio
- Interfaces de Repositório

## Infrastructure
- Entity Framework Core
- Repositórios
- DbContext
- Migrations
- Configurações do banco
- Serviços externos (Open Library, Google Books)

O domínio nunca deve depender das demais camadas. Application não deve conhecer Entity Framework. Infrastructure implementa as interfaces definidas no Domain. A API apenas orquestra as requisições.

---

# Perfis de usuário e permissões

| Perfil | Permissões |
|---|---|
| **Admin** | Cadastrar/editar/remover livros, escrever ou ajustar sinopse, gerenciar gêneros, moderar (remover) reviews e comentários impróprios, gerenciar usuários. |
| **Usuário** | Criar conta, avaliar livro (nota), comentar, criar listas personalizadas, marcar estante (quero ler / lendo / lido), editar/remover as próprias avaliações. |
| **Visitante (sem login)** | Navegar no catálogo, ver páginas de livros, ler reviews e comentários — somente leitura. |

Autorização por role (`Admin`, `User`) via JWT, com Refresh Token para renovação de sessão sem novo login.

---

# Modelo de dados (entidades principais)

- **User** — Id, Name, Email, PasswordHash, Role, CreatedAt
- **Book** — Id, Title, Author, ISBN, Synopsis, CoverUrl, PublishedYear, OpenLibraryId, GoogleBooksId, CreatedByAdminId, CreatedAt, UpdatedAt
- **Genre** — Id, Name (N:N com Book)
- **Review** — Id, BookId, UserId, Rating (1–5), Comment (opcional), CreatedAt, UpdatedAt — um usuário tem uma review por livro
- **Bookshelf** — Id, UserId, BookId, Status (quero-ler / lendo / lido)
- **CustomList** — Id, UserId, Name, Description — listas personalizadas de livros (estilo Letterboxd), com **ListItem** (Id, CustomListId, BookId, Order)
- **ReadingLink** — Id, BookId, SourceName, Url — links externos de "onde ler" (ex: Domínio Público, Project Gutenberg)

---

# Integrações externas de dados de livros

- **Open Library API** — fonte primária de metadados: busca por título/ISBN, capas, assuntos/gêneros. Gratuita e sem necessidade de chave.
- **Google Books API** — fallback para metadados mais completos (sinopse, autor). Gratuita, com cota de uso.

Nenhuma biblioteca de empréstimo (ex: BibliON) é integrada automaticamente — elas não expõem API pública. Quando aplicável, o Admin pode cadastrar manualmente um `ReadingLink` apontando para uma fonte gratuita de leitura.

Fluxo esperado: o Admin busca um livro por título/ISBN → o sistema consulta Open Library (e Google Books se necessário) → o Admin confirma/ajusta os dados antes de salvar. Nenhum dado externo é persistido sem confirmação humana.

---

# Convenções

Sempre seguir:
- SOLID
- Clean Code
- DRY
- KISS
- Dependency Injection
- Repository Pattern
- Unit of Work
- DTOs para entrada e saída
- Async/Await
- Paginação
- Tratamento global de exceções
- Logging
- Versionamento da API

Nunca acessar diretamente o banco a partir dos Controllers. Nunca colocar regras de negócio nos Controllers. Toda regra de negócio deve ficar na camada Application ou Domain. Controllers devem ser extremamente enxutos.

---

# Objetivo da IA

Sempre que gerar código:
- Pensar como um desenvolvedor sênior.
- Sugerir a melhor arquitetura antes de implementar.
- Evitar código duplicado.
- Priorizar escalabilidade.
- Seguir padrões utilizados em sistemas corporativos.
- Explicar quando alguma decisão arquitetural for importante.

---

# Fora de escopo

- Gerenciamento de biblioteca física ou empréstimo de livros.
- Qualquer integração automática com plataformas de empréstimo (ex: BibliON) — não possuem API pública.
- Hospedagem/infra definitiva — ainda não decidida; arquitetura deve ser cloud-agnostic.
