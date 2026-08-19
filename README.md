# Revenda - Serviço de Veículos e Vendas

API do estoque de veículos e das vendas da plataforma de revenda. Cadastra e edita
veículos, publica as listagens de disponíveis e vendidos, e conduz a compra do início
até a efetivação do pagamento.

Serviço irmão: `revenda-identity`, que cadastra e autentica os compradores. Este serviço
não guarda nenhum dado pessoal — apenas o identificador do comprador que vem no token.

## O que ele faz

| Método | Rota | Acesso | Descrição |
| --- | --- | --- | --- |
| GET | `/vehicles?status=forSale` | público | Veículos à venda, do mais barato para o mais caro |
| GET | `/vehicles?status=sold` | público | Veículos vendidos, do mais barato para o mais caro |
| GET | `/vehicles/{id}` | público | Detalhe do veículo |
| POST | `/vehicles` | administrador | Cadastra veículo |
| PUT | `/vehicles/{id}` | administrador | Edita veículo |
| POST | `/sales` | comprador | Inicia a compra e reserva o veículo |
| GET | `/sales/me` | comprador | Compras do próprio comprador |
| POST | `/payments/webhook` | segredo do gateway | Efetiva ou cancela a venda |
| GET | `/health` | público | Verificação de saúde da API e do banco |

A documentação interativa fica em `/swagger`.

## Como a compra funciona

Comprar e pagar são dois eventos distintos, e é isso que o modelo reflete:

1. `POST /sales` reserva o veículo e cria a venda com status `PendingPayment`. A resposta
   traz um `paymentCode`, que é o identificador usado na conciliação com o gateway.
2. O gateway chama `POST /payments/webhook` com esse código. Se o pagamento foi aprovado,
   a venda vira `Paid` e o veículo vira `Sold`. Se foi recusado, a venda é cancelada e o
   veículo volta para a vitrine.

Os dois passos gravam veículo e venda na mesma transação, então não existe estado
intermediário visível: ou o veículo sai da vitrine com uma venda associada, ou nada muda.

O webhook é idempotente. O gateway pode reenviar a mesma notificação quantas vezes quiser
que o resultado continua o mesmo.

Enquanto está `Reserved`, o veículo não aparece em nenhuma das duas listagens: não está
mais à venda, e ainda não foi vendido.

## Como foi implementado

Arquitetura hexagonal com a regra de dependência da Clean Architecture, em quatro
projetos:

- `Domain`: `Vehicle` e `Sale` como raízes de agregado, com os objetos de valor `Money` e
  `LicensePlate`. As transições de estado (reservar, vender, liberar) e as invariantes
  ficam aqui, sem qualquer dependência de framework.
- `Application`: casos de uso e as portas de entrada e saída.
- `Infrastructure`: EF Core com PostgreSQL, repositórios e geração do código de pagamento.
- `Api`: controllers, autorização por papel, tradução de exceção para `ProblemDetails` e
  Swagger.

Duas decisões que valem o registro:

**Concorrência.** Dois compradores clicando no mesmo veículo ao mesmo tempo é o caso de
corrida natural do domínio. O `xmin` do PostgreSQL entra como token de concorrência
otimista em propriedade sombra: a segunda gravação falha e a API responde `409`, em vez
de as duas reservas se sobrescreverem.

**Preço congelado.** A venda guarda o preço do momento da compra. Se o preço de tabela
mudar depois, o histórico não muda junto.

## Autenticação

O token é emitido pelo `revenda-identity` e validado aqui pela chave pública obtida no
JWKS daquele serviço. Não existe segredo compartilhado entre os dois.

Duas políticas de autorização:

- `AdminOnly` para cadastrar e editar veículos, exigindo o papel `Administrator`;
- `BuyerOnly` para comprar, exigindo o papel `Buyer`.

O webhook não usa token, porque quem chama é o gateway. A autenticação dele é um segredo
combinado, enviado no header `X-Webhook-Secret` e comparado em tempo constante.

## Rodando localmente

Suba primeiro o `revenda-identity`, já que este serviço busca o JWKS dele.

```bash
cp .env.example .env
docker compose up -d --build
```

A API sobe em `http://localhost:8080` e o PostgreSQL em `localhost:5432`. As migrations
são aplicadas na subida.

Sem Docker, com o PostgreSQL já disponível:

```bash
dotnet restore
dotnet tool restore
dotnet run --project src/Revenda.Vehicles.Api
```

## Testando

```bash
dotnet test
```

Os testes unitários cobrem o domínio e os casos de uso e rodam sem dependência externa.
Os de integração sobem um PostgreSQL real via Testcontainers e assinam tokens com uma
chave de teste no lugar do serviço de identidade, então precisam do Docker em execução.

Fluxo manual completo, com o token obtido no `revenda-identity`:

```bash
curl -X POST http://localhost:8080/vehicles \
  -H "Authorization: Bearer <token-do-administrador>" \
  -H "Content-Type: application/json" \
  -d '{"brand":"Volkswagen","model":"Gol","year":2022,"color":"Prata","price":55900.00,"licensePlate":"ABC1D23"}'

curl "http://localhost:8080/vehicles?status=forSale"

curl -X POST http://localhost:8080/sales \
  -H "Authorization: Bearer <token-do-comprador>" \
  -H "Content-Type: application/json" \
  -d '{"vehicleId":"<id-do-veiculo>"}'

curl -X POST http://localhost:8080/payments/webhook \
  -H "X-Webhook-Secret: <segredo>" \
  -H "Content-Type: application/json" \
  -d '{"paymentCode":"<codigo-devolvido>","status":"approved"}'

curl "http://localhost:8080/vehicles?status=sold"
```

## Entrega contínua

`ci.yml` roda a cada Pull Request: restore, build com warnings tratados como erro, testes
unitários e de integração, e publicação do relatório de cobertura como artefato.

`cd.yml` roda no merge para `main`: publica a imagem no GHCR com as tags `latest` e
`sha-<commit>` e atualiza a stack no host configurado nas variáveis `DEPLOY_HOST`,
`DEPLOY_USER` e `DEPLOY_PATH`, com a chave em `DEPLOY_SSH_KEY`.

A branch `main` é protegida: alterações entram apenas por Pull Request com o CI verde.
