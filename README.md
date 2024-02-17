# Payment order processor technical task

A payment processing service for a fictituous company FunBooksAndVids.

The aim of the service is to provide some basic REST API's around payment processing for purchase orders and apply several business rules.

## Solution Structure

The solution has a few services.

- [OrderingService](./src/Ordering/OrderingService/): Service that exposes a REST API over the PurchaseOrder domain models, and includes Purchase Order Processing workflows
- [Aspire AppHost](./src/AppHost/): Uses .NET aspire to orchestrate the other projects (for ease of development)

Each service has a mini onion-architecture within a single project

- `Web`: Contains controllers and other http concerns
- `Application`: Contains application features & use-cases
- `Domain`: Contains domain models
- `Infrastructure`: Contains infra level concerns (e.g. Database queries & migrations)
