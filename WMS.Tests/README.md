# WMS.Tests - Comprehensive Test Suite

## Overview
Comprehensive xUnit test project for the Warehouse Management System (WMS) covering all API projects with unit tests, integration tests, and end-to-end scenarios.

## Test Coverage

### 1. **Products API Tests** (`Products/ProductCommandHandlerTests.cs`)
- ? Product CRUD operations
- ? SKU immutability verification
- ? Product activation/deactivation
- ? Duplicate SKU prevention
- ? Paged query results

### 2. **Inbound API Tests** (`Inbound/InboundCommandHandlerTests.cs`)
- ? Inbound creation
- ? Receive operation with atomic transaction
- ? Inventory creation/update on receive
- ? Location capacity validation
- ? Damaged goods handling (not added to inventory)
- ? Product active status validation
- ? Inventory transaction audit trail creation

### 3. **Outbound API Tests** (`Outbound/OutboundCommandHandlerTests.cs`)
- ? Outbound creation
- ? Pick operation (inventory reservation)
- ? Ship operation (inventory deduction)
- ? Insufficient inventory validation
- ? Payment validation (Prepaid vs COD)
- ? Location capacity release
- ? Inventory transaction audit trail

### 4. **Payment Webhook Tests** (`Payment/PaymentWebhookTests.cs`)
- ? Payment status update
- ? Idempotency (duplicate webhook detection)
- ? GatewayEventId uniqueness
- ? Duplicate webhook logging
- ? Invalid status handling
- ? Unknown payment handling
- ? Multiple events processing

### 5. **Delivery Webhook Tests** (`Delivery/DeliveryWebhookTests.cs`)
- ? Delivery status update
- ? Idempotency (duplicate event detection)
- ? PartnerEventId uniqueness
- ? Status transition validation
- ? Delivery date tracking
- ? Unknown tracking number handling

## Test Infrastructure

### Fixtures (`Fixtures/`)
- **TestDbContextFactory**: In-memory database creation for isolated testing
  - Uses EF Core InMemory provider
  - Each test gets a unique database instance
  - Automatic cleanup

### Helpers (`Helpers/`)
- **TestDataGenerator**: Bogus-powered test data generation
  - Realistic product data
  - Location hierarchies
  - Inbound/Outbound orders
  - Payments and deliveries
  - Inventory records

## Running Tests

### Run All Tests
```bash
dotnet test WMS.Tests/WMS.Tests.csproj
```

### Run Specific Test Class
```bash
dotnet test --filter "FullyQualifiedName~ProductCommandHandlerTests"
dotnet test --filter "FullyQualifiedName~InboundCommandHandlerTests"
dotnet test --filter "FullyQualifiedName~OutboundCommandHandlerTests"
dotnet test --filter "FullyQualifiedName~PaymentWebhookTests"
dotnet test --filter "FullyQualifiedName~DeliveryWebhookTests"
```

### Run with Coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
```

## Test Patterns

### 1. **Arrange-Act-Assert (AAA)**
All tests follow the AAA pattern:
```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedResult()
{
    // Arrange
    var entity = TestDataGenerator.GenerateEntity();
    await _repository.AddAsync(entity);
    
    // Act
    var result = await handler.Handle(command, CancellationToken.None);
    
    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
}
```

### 2. **In-Memory Database Isolation**
Each test gets a fresh database:
```csharp
public class MyTests : IDisposable
{
    private readonly WMSDbContext _context;
    
    public MyTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();
    }
    
    public void Dispose()
    {
        _context?.Dispose();
    }
}
```

### 3. **FluentAssertions**
Readable, expressive assertions:
```csharp
result.Should().NotBeNull();
result.IsSuccess.Should().BeTrue();
result.Data!.Status.Should().Be(ExpectedStatus);
result.Errors.Should().Contain(e => e.Contains("expected text"));
```

## Key Test Scenarios

### ACID Transaction Testing
```csharp
[Fact]
public async Task ReceiveInbound_ShouldUpdateInventoryAtomically()
{
    // Tests that inventory, location capacity, and audit trail
    // are all updated in a single atomic transaction
}
```

### Concurrency Testing
```csharp
[Fact]
public async Task PickOutbound_WithInsufficientInventory_ShouldFail()
{
    // Tests optimistic concurrency control
    // Validates RowVersion prevents lost updates
}
```

### Idempotency Testing
```csharp
[Fact]
public async Task ProcessWebhook_DuplicateEvent_ShouldIgnoreAndLogDuplicate()
{
    // Tests webhook idempotency
    // Ensures duplicate webhooks don't corrupt state
}
```

### Business Rule Testing
```csharp
[Fact]
public async Task ShipOutbound_WithPrepaidPaymentNotConfirmed_ShouldFail()
{
    // Tests payment gating logic
    // Ensures prepaid orders require payment confirmation
}
```

## Dependencies

### Testing Frameworks
- **xUnit**: Test framework
- **FluentAssertions**: Assertion library
- **Moq**: Mocking library
- **Bogus**: Test data generator

### Infrastructure
- **Microsoft.EntityFrameworkCore.InMemory**: In-memory database
- **Microsoft.AspNetCore.Mvc.Testing**: Integration testing
- **MediatR**: CQRS pattern support

## Best Practices

1. ? **Test Isolation**: Each test has its own database instance
2. ? **Test Data**: Use TestDataGenerator for consistent, realistic data
3. ? **Cleanup**: Dispose contexts in test class Dispose method
4. ? **Naming**: `MethodName_Scenario_ExpectedResult`
5. ? **Assertions**: Use FluentAssertions for readability
6. ? **Coverage**: Test happy paths AND error conditions
7. ? **Atomicity**: Verify transaction rollback on failures
8. ? **Idempotency**: Test duplicate operations
9. ? **Concurrency**: Test simultaneous operations
10. ? **Business Rules**: Test all validation logic

## Future Enhancements

- [ ] Performance tests (load testing)
- [ ] Integration tests with real database
- [ ] E2E tests with TestServer
- [ ] Mutation testing
- [ ] Property-based testing with FsCheck
- [ ] Contract testing for API endpoints

## Continuous Integration

Add to CI/CD pipeline:
```yaml
- name: Run Tests
  run: dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"
  
- name: Generate Coverage Report
  run: reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coverage -reporttypes:HtmlInline_AzurePipelines
```

## Contributing

When adding new tests:
1. Place tests in appropriate namespace folder
2. Use TestDataGenerator for test data
3. Follow AAA pattern
4. Use FluentAssertions
5. Test both success and failure scenarios
6. Verify database state changes
7. Clean up resources in Dispose
