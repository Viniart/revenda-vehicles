# Guia de desenvolvimento

Padrão a seguir em qualquer alteração neste repositório e no serviço irmão
`revenda-identity`. O [README.md](README.md) explica o que o serviço faz e como foi
montado; este arquivo trata de como escrever o código.

## Regras que não se negociam

1. **Regra de dependência.** `Domain` não referencia nada. `Application` referencia só
   `Domain`. `Infrastructure` referencia `Application`. `Api` referencia `Application` e,
   exclusivamente no registro de DI, `Infrastructure`. Nunca inverter isso "só desta vez".
2. **Domínio limpo.** Nada de `DbContext`, atributo de EF Core, `HttpContext`,
   `IConfiguration`, `ILogger` ou `DateTime.Now` dentro de `Domain`. Hora vem de `IClock`.
3. **Regra de negócio mora no domínio.** Controller não decide nada; caso de uso
   orquestra; entidade valida a si mesma. Se um `if` de negócio aparecer no controller,
   ele está no lugar errado.
4. **Entidade sempre válida.** Construtor privado e método fábrica estático que valida.
   Sem setter público, sem construtor sem parâmetros (exceto o exigido pelo EF Core, que
   deve ser `private`).
5. **Nada de vazamento de dado pessoal.** `revenda-vehicles` guarda `BuyerId` e mais
   nada do comprador. Nem nome, nem e-mail, nem CPF, em coluna, log ou resposta.
6. **Toda mudança entra por Pull Request** com CI verde. Sem push direto na `main`.

## Estrutura de pastas

```
src/
  <Produto>.<Contexto>.Domain/
    Entities/            raízes de agregado e entidades
    ValueObjects/        Cpf, Email, Money, LicensePlate
    Enums/
    Exceptions/          DomainException e derivadas
  <Produto>.<Contexto>.Application/
    Ports/
      Input/             uma interface por caso de uso
      Output/            repositórios, IUnitOfWork, IClock, ITokenIssuer
    UseCases/
      <Área>/            RegisterVehicleUseCase.cs, ...
    Dtos/                records de entrada e saída dos casos de uso
  <Produto>.<Contexto>.Infrastructure/
    Persistence/
      Context/           DbContext
      Configurations/    IEntityTypeConfiguration por entidade
      Repositories/
      Migrations/
    Security/ Time/ ...  demais adaptadores de saída
    DependencyInjection.cs
  <Produto>.<Contexto>.Api/
    Controllers/
    Contracts/           requests e responses HTTP, com DataAnnotations
    Middlewares/
    Extensions/          Add* de autenticação, Swagger, health
    Program.cs
tests/
  <Produto>.<Contexto>.UnitTests/
  <Produto>.<Contexto>.IntegrationTests/
```

Um caso de uso por arquivo. O `record` de entrada e o de saída ficam **no mesmo arquivo**
do caso de uso quando são usados só por ele — evita espalhar três arquivos para uma
operação de dez linhas.

## Convenções de código

- C# 12, `nullable` habilitado, `TreatWarningsAsErrors` ligado, `ImplicitUsings` ligado.
- Código, nomes de classe, propriedade, variável, rota, tabela e coluna em **inglês**.
  Documentação, README, mensagens de commit e descrição de PR em **português**.
- Nome de arquivo igual ao nome do tipo. Um tipo público por arquivo.
- `async` em tudo que faz I/O, com sufixo `Async` e `CancellationToken` como último
  parâmetro, propagado até o EF Core.
- `record` para DTOs e objetos de valor imutáveis; `class` para entidades.
- Nada de `var` quando o tipo não é óbvio na linha; `var` livre quando é.
- Sem região (`#region`), sem comentário de cabeçalho de arquivo, sem separador ASCII.
- Sem número mágico: constante nomeada ou configuração.
- Exceção só para caminho excepcional. Regra de negócio violada lança exceção de domínio
  específica (`VehicleNotAvailableException`), nunca `Exception` ou `InvalidOperationException`
  genérica.
- `ProblemDetails` é a única forma de erro que sai da API. O middleware faz a tradução;
  controller não monta erro na mão.
- Log com `ILogger` e template estruturado (`_logger.LogInformation("Venda {SaleId} confirmada", id)`),
  nunca interpolação de string. Não logar senha, token, CPF ou e-mail.

### Comentários

Comentar **por que**, nunca **o que**. Se o código precisa de comentário para dizer o que
faz, renomeie em vez de comentar.

```csharp
// ruim
// incrementa o contador
count++;

// bom
// A placa vem do usuário com máscara; o banco tem índice único sobre o valor normalizado.
var normalized = LicensePlate.Normalize(input);
```

`///` (XML doc) só em contratos públicos que aparecem no Swagger e cujo nome não basta.
Não documentar todo método por obrigação.

## Testes

- xUnit + FluentAssertions + NSubstitute. Integração com Testcontainers (PostgreSQL real).
- Nome do teste: `Metodo_DeveFazerAlgo_QuandoCondicao`.
  Exemplo: `Reserve_DeveLancarExcecao_QuandoVeiculoJaEstaVendido`.
- Estrutura Arrange/Act/Assert, sem escrever os rótulos como comentário.
- Um `Assert` conceitual por teste. Não testar EF Core nem o framework.
- Construir dados de teste com builders ou fábricas em `TestData/`, não com literais
  repetidos em vinte testes.
- Todo bug corrigido ganha um teste que falharia antes da correção.
- Antes de abrir PR: `dotnet build` sem warning e `dotnet test` verde.

## Git

- Branch: `feat/cadastro-veiculo`, `fix/reserva-concorrente`, `chore/pipeline-ci`.
- Commits em português, Conventional Commits, imperativo, minúsculo, sem ponto final:
  ```
  feat: adiciona reserva de veiculo na criacao da venda
  test: cobre transicao de venda cancelada
  fix: corrige ordenacao da listagem por preco
  ```
- Commit pequeno e coeso. Não misturar refatoração com funcionalidade.
- Descrição de PR: o que muda, por que, como testar. Sem template gigante.
- **Nunca** incluir trailer de ferramenta (`Co-Authored-By` de assistente,
  `Generated with ...`) em commit, PR ou changelog.

## Estilo do repositório

O projeto é entregue como trabalho autoral e revisado por pessoas. O código e a
documentação devem ler como escritos por um time, o que na prática significa:

- Sem emoji em código, commit, PR, README ou comentário.
- Sem listas de bullets decorativas com negrito em toda linha; texto corrido quando o
  assunto é explicação.
- Sem frases de assistente: "Ótima pergunta", "Vamos mergulhar", "Em resumo, este
  arquivo...", "Nota importante:".
- Sem docstring/XML doc em 100% dos membros. Documentação onde agrega, ausente onde não.
- Sem uniformidade artificial: nem todo arquivo precisa ter o mesmo tamanho, a mesma
  ordem de seções ou o mesmo número de testes.
- README escrito com comandos que realmente funcionam, testados antes de commitar.
- Sem TODO genérico ("TODO: melhorar isso"). Ou resolve, ou abre issue com contexto.

## Fluxo esperado ao receber uma tarefa

1. Localize a etapa correspondente em ARQUITETURA.md §8. Se a tarefa não estiver lá e
   mudar a arquitetura, atualize o documento no mesmo PR.
2. Crie a branch a partir da `main` atualizada.
3. Escreva de dentro para fora: domínio → caso de uso → adaptador de saída → adaptador
   de entrada.
4. Teste unitário junto do domínio e do caso de uso, não depois.
5. Rode build e testes. Corrija warning; não silencie.
6. Abra o PR e espere o CI.

## Antes de propor código novo

- Já existe caso de uso, VO ou extensão que resolve isso? Reutilize.
- A dependência nova é indispensável? A resposta padrão é não. Prefira a BCL.
- Isso é abstração para um problema que existe hoje ou para um que talvez exista?
  Se é o segundo, não crie.
- A mudança cabe em um PR revisável? Se não, quebre.

## Comandos

```bash
dotnet restore
dotnet build --configuration Release
dotnet test
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

dotnet ef migrations add <Nome> --project src/<Produto>.<Contexto>.Infrastructure --startup-project src/<Produto>.<Contexto>.Api
dotnet ef database update --project src/<Produto>.<Contexto>.Infrastructure --startup-project src/<Produto>.<Contexto>.Api

docker compose up -d --build
docker compose logs -f api
docker compose down -v
```

O ambiente local tem SDK .NET 9 e 10 instalados; os projetos fixam
`<TargetFramework>net8.0</TargetFramework>` e o runtime 8 está presente. Não subir o
target framework sem decisão explícita.
