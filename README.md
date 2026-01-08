# Masterly.Specification

Specification Pattern is used to define **named, reusable, combinable and testable filters** for entities and other business objects.

<img src="https://raw.githubusercontent.com/a7mdfre7at/Masterly.Autofac.DependenciesScanner/master/repo_image.png" width="200" height="180">

[![Nuget](https://img.shields.io/nuget/v/Masterly.Specification?style=flat-square)](https://www.nuget.org/packages/Masterly.Specification) ![Nuget](https://img.shields.io/nuget/dt/Masterly.Specification?style=flat-square) ![GitHub last commit](https://img.shields.io/github/last-commit/a7mdfre7at/Masterly.Specification?style=flat-square) ![GitHub](https://img.shields.io/github/license/a7mdfre7at/Masterly.Specification?style=flat-square) [![Build](https://github.com/a7mdfre7at/Masterly.Specification/actions/workflows/build.yml/badge.svg?branch=master)](https://github.com/a7mdfre7at/Masterly.Specification/actions/workflows/build.yml) [![CodeQL Analysis](https://github.com/a7mdfre7at/Masterly.Specification/actions/workflows/codeql-analysis.yml/badge.svg?branch=master)](https://github.com/a7mdfre7at/Masterly.Specification/actions/workflows/codeql-analysis.yml) [![Publish to NuGet](https://github.com/a7mdfre7at/Masterly.Specification/actions/workflows/publish.yml/badge.svg?branch=master)](https://github.com/a7mdfre7at/Masterly.Specification/actions/workflows/publish.yml)

> This repository is heavily inspired from Asp.net Boilerplate framework

## Give a Star! :star:

If you like or are using this project please give it a star. Thanks!

## Table of Contents

- [Installation](#installation)
- [Defining the Specifications](#defining-the-specifications)
- [Using the Specifications](#using-the-specifications)
- [Composing the Specifications](#composing-the-specifications)
- [Advanced Features](#advanced-features)
  - [Advanced Logic Operators](#advanced-logic-operators)
  - [N-ary Composition](#n-ary-composition)
  - [Fluent Builder](#fluent-builder)
  - [Property-Based Specifications](#property-based-specifications)
  - [Temporal Specifications](#temporal-specifications)
  - [Pipeline & Conditional](#pipeline--conditional)
  - [Diagnostics & Explanation](#diagnostics--explanation)
  - [Performance & Caching](#performance--caching)
- [Discussions](#discussions)

## Installation

Install the [Masterly.Specification NuGet Package](https://www.nuget.org/packages/Masterly.Specification).

### Package Manager Console

```
Install-Package Masterly.Specification
```

### .NET Core CLI

```
dotnet add package Masterly.Specification
```

## Defining the Specifications

Assume that you've a Customer entity as defined below:

```csharp
public class Customer
{
    public string Name { get; set; }
    public byte Age { get; set; }
    public long Balance { get; set; }
    public string Location { get; set; }
}
```

You can create a new Specification class derived from the `Specification<Customer>`.

**Example: A specification to select the customers with 18+ age:**

```csharp
public class Age18PlusCustomerSpecification : Specification<Customer>
{
    public override Expression<Func<Customer, bool>> ToExpression()
    {
        return c => c.Age >= 18;
    }
}
```

You simply define a lambda [Expression](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/lambda-expressions) to define a specification.

> Instead, you can directly implement the `ISpecification<T>` interface, but the `Specification<T>` base class much simplifies it.

## Using the Specifications

There are two common use cases of the specifications.

### IsSatisfiedBy

`IsSatisfiedBy` method can be used to check if a single object satisfies the specification.

**Example: Throw exception if the customer doesn't satisfy the age specification**

```csharp
public class CustomerService
{
    public async Task IssueDrivingLicense(Customer customer)
    {
        if (!new Age18PlusCustomerSpecification().IsSatisfiedBy(customer))
        {
            throw new Exception(
                "This customer doesn't satisfy the Age specification!"
            );
        }

        //TODO...
    }
}
```

### ToExpression & Repositories

`ToExpression()` method can be used to use the specification as Expression. In this way, you can use a specification to **filter entities while querying from the database**.

```csharp
public class CustomerService
{
    private readonly IRepository<Customer> _customerRepository;

    public CustomerManager(IRepository<Customer> customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<List<Customer>> GetCustomersCanIssueDrivingLicense()
    {
        IQueryable<Customer> queryable = await _customerRepository.GetQueryableAsync();
        IQueryable<Customer> query = queryable.Where(
            new Age18PlusCustomerSpecification().ToExpression()
        );

        return await AsyncExecuter.ToListAsync(query);
    }
}
```

### Implicit Conversion to Expression

The `Specification<T>` base class provides an **implicit operator** that automatically converts specifications to `Expression<Func<T, bool>>`. This means you can use specifications directly in LINQ queries without calling `ToExpression()`:

```csharp
IQueryable<Customer> queryable = await _customerRepository.GetQueryableAsync();

// No need to call ToExpression() - implicit conversion handles it
IQueryable<Customer> query = queryable.Where(new Age18PlusCustomerSpecification());
```

This works seamlessly with all LINQ methods:

```csharp
// Count with specification
int count = customers.Count(new PremiumCustomerSpecification());

// First/Single with specification
Customer first = customers.First(new Age18PlusCustomerSpecification());

// Any with specification
bool hasAdults = customers.Any(new Age18PlusCustomerSpecification());

// Combined specifications also implicitly convert
ISpecification<Customer> combined = new Age18PlusCustomerSpecification()
    .And(new PremiumCustomerSpecification());

List<Customer> premiumAdults = customers.Where(combined).ToList();
```

You can also explicitly assign a specification to an expression variable:

```csharp
Expression<Func<Customer, bool>> expression = new Age18PlusCustomerSpecification();

// Use with Entity Framework or other ORMs
List<Customer> results = await dbContext.Customers
    .Where(expression)
    .ToListAsync();
```

## Composing the Specifications

One powerful feature of the specifications is that they are composable with `And`, `Or`, `Not` and `AndNot` extension methods.

Assume that you have another specification as defined below:

```csharp
public class PremiumCustomerSpecification : Specification<Customer>
{
    public override Expression<Func<Customer, bool>> ToExpression()
    {
        return (customer) => (customer.Balance >= 100000);
    }
}
```

You can combine the `PremiumCustomerSpecification` with the `Age18PlusCustomerSpecification` to query the count of premium adult customers as shown below:

```csharp
public class CustomerManager
{
    private readonly IRepository<Customer> _customerRepository;

    public CustomerManager(IRepository<Customer> customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<int> GetAdultPremiumCustomerCountAsync()
    {
        return await _customerRepository.CountAsync(
            new Age18PlusCustomerSpecification()
            .And(new PremiumCustomerSpecification()).ToExpression()
        );
    }
}
```

If you want to make this combination another reusable specification, you can create such a combination specification class deriving from the `AndSpecification`:

```csharp
public class AdultPremiumCustomerSpecification : AndSpecification<Customer>
{
    public AdultPremiumCustomerSpecification()
        : base(new Age18PlusCustomerSpecification(),
               new PremiumCustomerSpecification())
    {
    }
}
```

Now, you can re-write the `GetAdultPremiumCustomerCountAsync` method as shown below:

```csharp
public async Task<int> GetAdultPremiumCustomerCountAsync()
{
    return await _customerRepository.CountAsync(
        new AdultPremiumCustomerSpecification()
    );
}
```

> You see the power of the specifications with these samples. If you change the `PremiumCustomerSpecification` later, say change the balance from `100.000` to `200.000`, all the queries and combined specifications will be effected by the change. This is a good way to reduce code duplication!

---

## Advanced Features

Masterly.Specification provides a rich set of advanced features for complex business logic scenarios.

### Advanced Logic Operators

Beyond the basic `And`, `Or`, `Not`, and `AndNot` operators, the library provides advanced boolean logic operators:

#### XOR (Exclusive Or)

Returns true when exactly one of the specifications is satisfied:

```csharp
ISpecification<Customer> adultSpec = new ExpressionSpecification<Customer>(c => c.Age >= 18);
ISpecification<Customer> premiumSpec = new ExpressionSpecification<Customer>(c => c.Balance >= 100000);

// True when customer is adult XOR premium (but not both)
ISpecification<Customer> xorSpec = adultSpec.Xor(premiumSpec);
```

#### Implies (Material Implication)

Returns true unless the first specification is true and the second is false (A → B):

```csharp
ISpecification<Order> highValueSpec = new ExpressionSpecification<Order>(o => o.Total > 1000);
ISpecification<Order> approvedSpec = new ExpressionSpecification<Order>(o => o.IsApproved);

// If high value, then must be approved (high value orders must be approved)
ISpecification<Order> impliesSpec = highValueSpec.Implies(approvedSpec);
```

| A (highValue) | B (approved) | A → B |
|---------------|--------------|-------|
| true          | true         | true  |
| true          | false        | false |
| false         | true         | true  |
| false         | false        | true  |

#### Iff (If and Only If / Biconditional)

Returns true when both specifications have the same truth value:

```csharp
ISpecification<User> activeSpec = new ExpressionSpecification<User>(u => u.IsActive);
ISpecification<User> verifiedSpec = new ExpressionSpecification<User>(u => u.IsVerified);

// User is active if and only if they are verified
ISpecification<User> iffSpec = activeSpec.Iff(verifiedSpec);
```

#### NAND (Not And)

Returns true unless both specifications are true:

```csharp
ISpecification<Product> expensiveSpec = new ExpressionSpecification<Product>(p => p.Price > 500);
ISpecification<Product> lowStockSpec = new ExpressionSpecification<Product>(p => p.Stock < 10);

// Not both expensive and low stock
ISpecification<Product> nandSpec = expensiveSpec.Nand(lowStockSpec);
```

#### NOR (Not Or)

Returns true only when both specifications are false:

```csharp
ISpecification<Document> expiredSpec = new ExpressionSpecification<Document>(d => d.ExpirationDate < DateTime.Now);
ISpecification<Document> archivedSpec = new ExpressionSpecification<Document>(d => d.IsArchived);

// Neither expired nor archived
ISpecification<Document> norSpec = expiredSpec.Nor(archivedSpec);
```

---

### N-ary Composition

Combine multiple specifications with advanced cardinality requirements using the `Specifications` static class:

#### All

All specifications must be satisfied:

```csharp
ISpecification<Product> expensive = new ExpressionSpecification<Product>(p => p.Price > 200);
ISpecification<Product> electronics = new ExpressionSpecification<Product>(p => p.Category == "Electronics");
ISpecification<Product> inStock = new ExpressionSpecification<Product>(p => p.Stock > 5);

ISpecification<Product> allSpec = Specifications.All(expensive, electronics, inStock);
// Matches products that are expensive AND electronics AND in stock
```

#### AnyOf

At least one specification must be satisfied:

```csharp
ISpecification<Product> veryExpensive = new ExpressionSpecification<Product>(p => p.Price > 500);
ISpecification<Product> furniture = new ExpressionSpecification<Product>(p => p.Category == "Furniture");

ISpecification<Product> anySpec = Specifications.AnyOf(veryExpensive, furniture);
// Matches products that are very expensive OR furniture
```

#### Exactly

Exactly N specifications must be satisfied:

```csharp
ISpecification<Product> expensive = new ExpressionSpecification<Product>(p => p.Price > 200);
ISpecification<Product> electronics = new ExpressionSpecification<Product>(p => p.Category == "Electronics");
ISpecification<Product> lowStock = new ExpressionSpecification<Product>(p => p.Stock < 30);

ISpecification<Product> exactlyTwoSpec = Specifications.Exactly(2, expensive, electronics, lowStock);
// Matches products where exactly 2 of the 3 conditions are true
```

#### AtLeast

At least N specifications must be satisfied:

```csharp
ISpecification<Product> atLeastTwoSpec = Specifications.AtLeast(2, affordable, furniture, highStock);
// Matches products where at least 2 conditions are true
```

#### AtMost

At most N specifications must be satisfied:

```csharp
ISpecification<Product> atMostOneSpec = Specifications.AtMost(1, expensive, electronics, lowStock);
// Matches products where at most 1 condition is true
```

#### NoneOf

None of the specifications should be satisfied:

```csharp
ISpecification<Product> noneSpec = Specifications.NoneOf(expensive, electronics);
// Matches products that are neither expensive nor electronics
```

---

### Fluent Builder

Build specifications inline without creating separate classes using the fluent `Spec` builder:

#### Basic Usage

```csharp
ISpecification<User> spec = Spec.For<User>()
    .Where(u => u.Age >= 18)
    .And(u => u.IsActive)
    .Build();

List<User> adults = users.Where(spec.ToExpression()).ToList();
```

#### Shorthand Syntax

```csharp
ISpecification<User> spec = Spec.Where<User>(u => u.Age >= 18);
```

#### Complex Conditions

```csharp
ISpecification<User> spec = Spec.For<User>()
    .Where(u => u.Age >= 18)
    .And(u => u.IsActive)
    .Or(u => u.Email.Contains("admin"))
    .AndNot(u => u.IsBanned)
    .Build();
```

#### Grouped Conditions

```csharp
ISpecification<User> spec = Spec.For<User>()
    .Where(u => u.Age >= 18)
    .Group(g => g
        .Where(u => u.IsActive)
        .Or(u => u.Email.Contains("test")))
    .Build();
// Equivalent to: Age >= 18 AND (IsActive OR Email contains "test")
```

#### XOR and Advanced Operators

```csharp
ISpecification<User> spec = Spec.For<User>()
    .Where(u => u.Age >= 25)
    .Xor(u => u.IsActive)
    .Build();
```

#### Combining with Existing Specifications

```csharp
ISpecification<User> adultSpec = new Age18PlusCustomerSpecification();

ISpecification<User> spec = Spec.For<User>()
    .Where(u => u.IsActive)
    .And(adultSpec)
    .Build();
```

#### Factory Methods

```csharp
// Always true specification
ISpecification<User> anySpec = Spec.Any<User>();

// Always false specification
ISpecification<User> noneSpec = Spec.None<User>();
```

---

### Property-Based Specifications

Create type-safe specifications for specific properties with rich comparison operations:

#### Numeric Properties

```csharp
// Equal to
ISpecification<Person> spec = Property<Person>.For(p => p.Age).EqualTo(25);

// Not equal to
ISpecification<Person> spec = Property<Person>.For(p => p.Age).NotEqualTo(25);

// Greater than / Less than
ISpecification<Person> spec = Property<Person>.For(p => p.Age).GreaterThan(18);
ISpecification<Person> spec = Property<Person>.For(p => p.Age).LessThan(65);
ISpecification<Person> spec = Property<Person>.For(p => p.Age).GreaterThanOrEqual(21);
ISpecification<Person> spec = Property<Person>.For(p => p.Age).LessThanOrEqual(30);

// Range
ISpecification<Person> spec = Property<Person>.For(p => p.Age).InRange(18, 65);

// In collection
ISpecification<Person> spec = Property<Person>.For(p => p.Age).In(18, 21, 25, 30);
ISpecification<Person> spec = Property<Person>.For(p => p.Age).NotIn(0, 999);
```

#### String Properties

```csharp
// String matching
ISpecification<Person> spec = Property<Person>.For(p => p.Email).StartsWith("admin");
ISpecification<Person> spec = Property<Person>.For(p => p.Email).EndsWith(".com");
ISpecification<Person> spec = Property<Person>.For(p => p.Email).Contains("@example");

// Null/Empty checks
ISpecification<Person> spec = Property<Person>.For(p => p.Name).IsNull();
ISpecification<Person> spec = Property<Person>.For(p => p.Name).IsNotNull();
ISpecification<Person> spec = Property<Person>.For(p => p.Email).IsNullOrEmpty();
ISpecification<Person> spec = Property<Person>.For(p => p.Email).HasContent();

// Length validation
ISpecification<Person> spec = Property<Person>.For(p => p.Email).HasLengthBetween(5, 100);
```

#### Custom Predicates

```csharp
ISpecification<Person> spec = Property<Person>.For(p => p.Age).Matches(age => age % 5 == 0);
```

#### Combining Property Specifications

```csharp
ISpecification<Person> ageSpec = Property<Person>.For(p => p.Age).GreaterThanOrEqual(18);
ISpecification<Person> emailSpec = Property<Person>.For(p => p.Email).Contains("example");

ISpecification<Person> combinedSpec = ageSpec.And(emailSpec);
```

---

### Temporal Specifications

Specialized specifications for DateTime properties:

#### Date Comparisons

```csharp
// Before/After
ISpecification<Event> spec = Temporal<Event>.For(e => e.StartDate).Before(new DateTime(2024, 6, 1));
ISpecification<Event> spec = Temporal<Event>.For(e => e.StartDate).After(DateTime.Now);

// Date range
ISpecification<Event> spec = Temporal<Event>.For(e => e.StartDate)
    .Between(new DateTime(2024, 1, 1), new DateTime(2024, 12, 31));
```

#### Calendar-Based Filtering

```csharp
// Year/Month filtering
ISpecification<Event> spec = Temporal<Event>.For(e => e.StartDate).InYear(2024);
ISpecification<Event> spec = Temporal<Event>.For(e => e.StartDate).InMonth(2024, 6);

// Day of week
ISpecification<Event> spec = Temporal<Event>.For(e => e.StartDate).OnDayOfWeek(DayOfWeek.Saturday);
ISpecification<Event> spec = Temporal<Event>.For(e => e.StartDate).OnWeekend();
ISpecification<Event> spec = Temporal<Event>.For(e => e.StartDate).OnWeekday();
```

#### Time of Day

```csharp
// Business hours (9 AM to 5 PM)
ISpecification<Event> spec = Temporal<Event>.For(e => e.StartDate)
    .TimeBetween(TimeSpan.FromHours(9), TimeSpan.FromHours(17));
```

#### Nullable DateTime Support

```csharp
// Check if nullable DateTime has value
ISpecification<Event> spec = Temporal<Event>.For(e => e.EndDate).HasValue();
ISpecification<Event> spec = Temporal<Event>.For(e => e.EndDate).IsNull();
```

#### Combining Temporal Specifications

```csharp
ISpecification<Event> in2024 = Temporal<Event>.For(e => e.StartDate).InYear(2024);
ISpecification<Event> hasEndDate = Temporal<Event>.For(e => e.EndDate).HasValue();

ISpecification<Event> combinedSpec = in2024.And(hasEndDate);
```

---

### Pipeline & Conditional

Create conditional specification pipelines that apply different rules based on runtime conditions:

#### OnlyWhen

Apply a specification only when a condition is true:

```csharp
ISpecification<Invoice> highValueSpec = new ExpressionSpecification<Invoice>(i => i.Amount > 1000);
bool filterEnabled = true;

ISpecification<Invoice> spec = highValueSpec.OnlyWhen(filterEnabled);
// When filterEnabled is true: applies the high value filter
// When filterEnabled is false: passes all items through
```

#### SkipWhen

Skip a specification when a condition is true:

```csharp
ISpecification<Invoice> approvedSpec = new ExpressionSpecification<Invoice>(i => i.IsApproved);
bool skipApprovalCheck = true;

ISpecification<Invoice> spec = approvedSpec.SkipWhen(skipApprovalCheck);
// When skipApprovalCheck is true: passes all items through
// When skipApprovalCheck is false: applies the approval filter
```

#### Chain

Chain multiple specifications together (AND logic):

```csharp
ISpecification<Invoice> approved = new ExpressionSpecification<Invoice>(i => i.IsApproved);
ISpecification<Invoice> highValue = new ExpressionSpecification<Invoice>(i => i.Amount > 1000);
ISpecification<Invoice> premium = new ExpressionSpecification<Invoice>(i => i.Type == "Premium");

ISpecification<Invoice> spec = approved.Chain(highValue, premium);
// Equivalent to: approved AND highValue AND premium
```

#### AsOptional

Handle nullable specifications gracefully:

```csharp
ISpecification<Invoice>? nullableSpec = GetOptionalFilter();
ISpecification<Invoice> spec = nullableSpec.AsOptional();
// If nullableSpec is null: passes all items through
// If nullableSpec is not null: applies the specification
```

#### Conditional When/Otherwise

Apply different specifications based on a predicate:

```csharp
ISpecification<Invoice> highValueSpec = new ExpressionSpecification<Invoice>(i => i.Amount > 2000);
ISpecification<Invoice> approvedSpec = new ExpressionSpecification<Invoice>(i => i.IsApproved);

// For premium invoices, require high value; otherwise require approved
ISpecification<Invoice> spec = new ConditionalSpecification<Invoice>(
    i => i.Type == "Premium",  // Condition
    highValueSpec,              // Then specification
    approvedSpec                // Else specification
);
```

#### Fluent Conditional Builder

```csharp
ISpecification<Invoice> highValueSpec = new ExpressionSpecification<Invoice>(i => i.Amount > 2000);
ISpecification<Invoice> lowValueSpec = new ExpressionSpecification<Invoice>(i => i.Amount < 200);

// When premium, apply high value; otherwise apply low value
ISpecification<Invoice> spec = PipelineExtensions.When<Invoice>(
    i => i.Type == "Premium",
    highValueSpec)
    .Otherwise(lowValueSpec);

// When premium, apply high value; otherwise pass all
ISpecification<Invoice> spec = PipelineExtensions.When<Invoice>(
    i => i.Type == "Premium",
    highValueSpec)
    .OtherwisePass();

// When premium, apply high value; otherwise fail all
ISpecification<Invoice> spec = PipelineExtensions.When<Invoice>(
    i => i.Type == "Premium",
    highValueSpec)
    .OtherwiseFail();
```

#### Short-Circuit Evaluation

```csharp
ISpecification<Invoice> approvedSpec = new ExpressionSpecification<Invoice>(i => i.IsApproved);
ISpecification<Invoice> premiumSpec = new ExpressionSpecification<Invoice>(i => i.Type == "Premium");

// OrElse: Short-circuit on first match (lazy evaluation)
ISpecification<Invoice> spec = approvedSpec.OrElse(() => premiumSpec);

// AndThen: Short-circuit on first failure (lazy evaluation)
ISpecification<Invoice> spec = approvedSpec.AndThen(() => premiumSpec);
```

---

### Diagnostics & Explanation

Get human-readable explanations and detailed evaluation results:

#### Explain

Generate a human-readable description of a specification:

```csharp
ISpecification<Order> spec = new ExpressionSpecification<Order>(o => o.Total > 100);
string explanation = spec.Explain();
// Output: "o => (o.Total > 100)"

// Complex specifications
ISpecification<Order> highValue = new ExpressionSpecification<Order>(o => o.Total > 100);
ISpecification<Order> priority = new ExpressionSpecification<Order>(o => o.IsPriority);
ISpecification<Order> combinedSpec = highValue.And(priority);

string explanation = combinedSpec.Explain();
// Output: "(o => (o.Total > 100)) AND (o => o.IsPriority)"
```

#### Evaluate

Get detailed evaluation results with pass/fail information:

```csharp
ISpecification<Order> spec = new ExpressionSpecification<Order>(o => o.Total > 100);
Order order = new Order(1, 150m, "Pending", false);

EvaluationResult result = spec.Evaluate(order);

Console.WriteLine(result.IsSatisfied);  // true
Console.WriteLine(result.Summary);       // "PASSED"
```

#### Tracking Multiple Conditions

```csharp
ISpecification<Order> highValue = new ExpressionSpecification<Order>(o => o.Total > 100);
ISpecification<Order> priority = new ExpressionSpecification<Order>(o => o.IsPriority);
ISpecification<Order> combinedSpec = highValue.And(priority);

Order order = new Order(1, 150m, "Pending", false);  // High value but not priority
EvaluationResult result = combinedSpec.Evaluate(order);

Console.WriteLine(result.IsSatisfied);                    // false
IEnumerable<string> passed = result.GetPassedConditions(); // ["o => (o.Total > 100)"]
IEnumerable<string> failed = result.GetFailureReasons();   // ["o => o.IsPriority"]
```

#### Detailed Result Output

```csharp
ISpecification<Order> spec = new ExpressionSpecification<Order>(o => o.Total > 100);
Order order = new Order(1, 150m, "Pending", false);

string detailedResult = spec.GetDetailedResult(order);
// Output:
// Result: PASSED
// Details:
// [PASS] o => (o.Total > 100)
```

---

### Performance & Caching

Optimize specification evaluation with caching and compilation:

#### Cached

Cache the compiled expression for repeated evaluations:

```csharp
ISpecification<Item> original = new ExpressionSpecification<Item>(i => i.Price > 50);
CachedSpecification<Item> cached = original.Cached();

// First call compiles the expression
cached.IsSatisfiedBy(item1);

// Subsequent calls use the cached compiled delegate
cached.IsSatisfiedBy(item2);
cached.IsSatisfiedBy(item3);

// Access the compiled predicate directly
Func<Item, bool> predicate = cached.CompiledPredicate;
```

#### Compile

Get a compiled delegate for maximum performance:

```csharp
ISpecification<Item> spec = new ExpressionSpecification<Item>(i => i.Price > 50);
Func<Item, bool> compiled = spec.Compile();

// Use the compiled delegate directly
bool result = compiled(item);
```

#### Memoized

Cache evaluation results per entity (useful for expensive specifications):

```csharp
ISpecification<Item> spec = new ExpressionSpecification<Item>(i => i.Price > 50).Memoized();

Item item = new Item(1, "Test", 100);

// First call evaluates and caches the result
spec.IsSatisfiedBy(item);  // Evaluates

// Subsequent calls return the cached result
spec.IsSatisfiedBy(item);  // Returns cached result
```

#### Caching with Complex Specifications

```csharp
ISpecification<Item> expensive = new ExpressionSpecification<Item>(i => i.Price > 50);
ISpecification<Item> namedA = new ExpressionSpecification<Item>(i => i.Name.StartsWith("A"));
CachedSpecification<Item> combined = expensive.And(namedA).Cached();

// The combined expression is compiled once and reused
List<Item> results = items.Where(combined.ToExpression()).ToList();
```

---

## Discussions

While the specification pattern is older than C# lambda expressions, it's generally compared to expressions. Some developers may think it's not needed anymore and we can directly pass expressions to a repository or to a domain service as shown below:

```csharp
int count = await _customerRepository.CountAsync(c => c.Balance > 100000 && c.Age >= 18);
```

Since ABP's [Repository](Repositories.md) supports Expressions, this is a completely valid use. You don't have to define or use any specification in your application and you can go with expressions.

So, what's the point of a specification? Why and when should we consider to use them?

### When To Use?

Some benefits of using specifications:

- **Reusable**: Imagine that you need the Premium Customer filter in many places in your code base. If you go with expressions and do not create a specification, what happens if you later change the "Premium Customer" definition? Say you want to change the minimum balance from $100,000 to $250,000 and add another condition to be a customer older than 3 years. If you'd used a specification, you just change a single class. If you repeated (copy/pasted) the same expression everywhere, you need to change all of them.
- **Composable**: You can combine multiple specifications to create new specifications. This is another type of reusability.
- **Named**: `PremiumCustomerSpecification` better explains the intent rather than a complex expression. So, if you have an expression that is meaningful in your business, consider using specifications.
- **Testable**: A specification is a separately (and easily) testable object.

### When To Not Use?

- **Non business expressions**: Do not use specifications for non business-related expressions and operations.
- **Reporting**: If you are just creating a report, do not create specifications, but directly use `IQueryable` & LINQ expressions. You can even use plain SQL, views or another tool for reporting. DDD does not necessarily care about reporting, so the way you query the underlying data store can be important from a performance perspective.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
