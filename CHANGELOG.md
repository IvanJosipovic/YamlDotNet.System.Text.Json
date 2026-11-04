# Changelog

## Unreleased

### Breaking changes

- `AddSystemTextJson()` now ignores public fields unless they have `[JsonInclude]`, following System.Text.Json field inclusion behavior. Models that relied on YamlDotNet's default public-field handling must add `[JsonInclude]` to fields they want to serialize or deserialize.
