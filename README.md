# Syringe
Data/Dependency Injection

This has been thoroughly tested in multiple production applications.

## Static Usage

### Subscribe, Set, and Clear
```csharp
Syringe<Route>.Once(route => {
  // Only fires once! Immediately, if Route was already Set, or the next time Route is Set.
});

Syringe<Route>.Subscribe(route => {
  // Always called when Route is Set! Immediately, if Route was already Set.
});

// Set a new value.
Syringe<Route>.Set(new Route("/"));

// Clear the subscriptions and the last value set for Route.
Syringe<Route>.Reset();
```

### Subscribe/Unsubscribe Pattern
```csharp
void OnEnable() {
  Syringe<Config>.Subscribe(onConfig);
}

void OnDisable() {
  Syringe<Config>.Unsubscribe(onConfig);
}

void onConfig(Config config) {
  // While GameObject is active, react to Config changes.
}
```

### The Rest...
```csharp
// Update takes a callback function, which takes in the current value, and returns the new value to Set.
Syringe<Config>.Update((config, isSet) => {
  return config;
});

// Fire calls subscribers with the previously Set value.
Syringe<int>.Set(42);
Syringe<int>.Subscribe(answerToTheUltimateQuestionOfLifeTheUniverseAndEverything => {
  Debug.Log("I already know the ultimate answer.");
});
Syringe<int>.Fire();

// Useful methods for state management and clean up.
Syringe<int>.HasValue(); // Returns true if Syringe<int>.Set() was called, else false.
Syringe<int>.Remove(); // Sets value to the default for the type, effectively removing the value.
Syringe<int>.Reset(); // Removes all subscriptions for the type, and calls Remove() to reset the value, too.

// I highly discourage the use of the following method, it is provided for utility only and should not be used otherwise:
Syringe<int>.Get(); // Returns value if Syringe<int>.Set() was called, else the default value for the type.
```

## Runtime Usage

The `Syringe` type (without static `<>` template) is available to use as a serializable version of the aforementioned Static Usage. It is negligably slower than its static counterpart.

```csharp
// Allows you to select ANY class type in the inspector. I do not recommend this usage.
[SerializeField] Syringe anyType;

// Select the IButton interface, or anything that inherits IButton.
[SerializeField] Syringe myButtonType = typeof(IButton);

void Start() {
  myButtonType.Subscribe(button => {
    // Runtime equivalent of: Syringe<IButton>.Subscribe()
  });
}
```

## Motivation
I hated all the alternative options at the time. They were bulky, bloated, buggy and had needlessly complex API's.
