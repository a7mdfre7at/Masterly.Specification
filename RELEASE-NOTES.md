# What's New

## Added

### Advanced Logic Operators
- **XOR Specification** (`Xor()`) - Returns true when exactly one specification is satisfied
- **Implies Specification** (`Implies()`) - Material implication (A → B), returns false only when A is true and B is false
- **Iff Specification** (`Iff()`) - Biconditional/equivalence, returns true when both have same truth value
- **NAND Specification** (`Nand()`) - Returns true unless both specifications are true
- **NOR Specification** (`Nor()`) - Returns true only when both specifications are false

### N-ary Composition
- **`Specifications.All()`** - All specifications must be satisfied (N-ary AND)
- **`Specifications.AnyOf()`** - At least one specification must be satisfied (N-ary OR)
- **`Specifications.Exactly(n)`** - Exactly N specifications must be satisfied
- **`Specifications.AtLeast(n)`** - At least N specifications must be satisfied
- **`Specifications.AtMost(n)`** - At most N specifications must be satisfied
- **`Specifications.NoneOf()`** - None of the specifications should be satisfied

### Fluent Builder
- **`Spec.For<T>()`** - Start building a specification with fluent API
- **`Spec.Where<T>()`** - Create a simple specification from expression
- **`Spec.Any<T>()`** - Factory method for always-true specification
- **`Spec.None<T>()`** - Factory method for always-false specification
- **`SpecificationBuilder<T>`** - Fluent builder with `Where()`, `And()`, `Or()`, `AndNot()`, `Not()`, `Xor()`, `Group()`, `OrGroup()`, and `Build()` methods

### Property-Based Specifications
- **`Property<T>.For()`** - Create type-safe specifications for entity properties
- **Comparison operators**: `EqualTo()`, `NotEqualTo()`, `GreaterThan()`, `LessThan()`, `GreaterThanOrEqual()`, `LessThanOrEqual()`
- **Range operators**: `InRange()`, `In()`, `NotIn()`
- **Null checks**: `IsNull()`, `IsNotNull()`
- **String operations**: `StartsWith()`, `EndsWith()`, `Contains()`, `IsNullOrEmpty()`, `HasContent()`, `HasLengthBetween()`
- **Custom predicates**: `Matches()`

### Temporal Specifications
- **`Temporal<T>.For()`** - Create specifications for DateTime properties
- **Date comparisons**: `Before()`, `After()`, `Between()`
- **Calendar filtering**: `InYear()`, `InMonth()`, `OnDayOfWeek()`, `OnWeekend()`, `OnWeekday()`
- **Time filtering**: `TimeBetween()`
- **Nullable support**: `HasValue()`, `IsNull()`

### Pipeline & Conditional
- **`OnlyWhen()`** - Apply specification only when condition is true
- **`SkipWhen()`** - Skip specification when condition is true
- **`Chain()`** - Chain multiple specifications together
- **`AsOptional()`** - Handle nullable specifications gracefully
- **`ConditionalSpecification<T>`** - Apply different specs based on entity predicate
- **`PipelineExtensions.When().Otherwise()`** - Fluent conditional builder
- **`OtherwisePass()`** - Pass all when condition not met
- **`OtherwiseFail()`** - Fail all when condition not met
- **`OrElse()`** - Short-circuit OR evaluation
- **`AndThen()`** - Short-circuit AND evaluation

### Diagnostics & Explanation
- **`Explain()`** - Generate human-readable specification description
- **`Evaluate()`** - Get detailed evaluation results with pass/fail information
- **`GetDetailedResult()`** - Get formatted output with all condition results
- **`EvaluationResult`** - Result class with `IsSatisfied`, `Summary`, `GetPassedConditions()`, `GetFailureReasons()`

### Performance & Caching
- **`Cached()`** - Cache compiled expression for repeated evaluations
- **`Compile()`** - Get compiled delegate for maximum performance
- **`Memoized()`** - Cache evaluation results per entity instance
- **`CachedSpecification<T>`** - Specification wrapper with `CompiledPredicate` property

## Changed

- Extended `SpecificationExtensions` with advanced logic operators
- Improved expression tree handling for all composite specifications

## Infrastructure

- Added comprehensive unit tests for all new features (101 tests)
- Updated test project to use xUnit.v3 and Shouldly
- Updated to .NET 9.0 for test project
- Added GlobalUsings.cs for test namespace imports