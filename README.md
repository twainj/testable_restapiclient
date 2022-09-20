# testable_restapiclient

This repo represents a pattern for including a REST client into a C# project.
The example project is an SDK, which wraps that REST client, and defines a more targetted interface for working with a specific REST API.
The API itself is contrived and minimal. The focus is in how the REST client is wrapped to make it testable and isolated from its implementation.

## Testing
The tests in this project are included in the same project. Rather than separate them into a separate project, they coexist, so that tests for a class are immediately discoverable
See the .csproj file for how this is managed. They are conditionally compiled based on the build configuration.
This has it's trade-offs, but is an interesting and useful approach for building the test-first habit.

## Testability
The RestClient adheres to Interfaces for testability. Those interfaces were strongly influenced by the implementation of this particular example, RestSharp.
However, effort was taken to try and make the resulting interface applicable even if the underlying implementation were to be changed.
It would be a good exercise to take this example and replace the implementation, perhaps with HttpClient, to verify that the interface still holds useful.

The RestClient wrapper was developed test-first, so it should have good coverage, though no coverage metrics have been traced.
The intent was not to have 100% code coverage, but to cover the new code for all usage cases.
