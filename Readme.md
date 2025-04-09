Proof of concept of writing a cross plane composite function in .Net.

# 🧑‍🏫 Useful links

- Functions spec: https://github.com/crossplane/crossplane/blob/main/contributing/specifications/functions.md
- Functions design doc: https://github.com/crossplane/crossplane/blob/main/design/design-doc-composition-functions.md
- How composition functions work - https://docs.crossplane.io/latest/concepts/compositions/#how-composition-functions-work
- Testing compositions -  https://docs.crossplane.io/latest/concepts/compositions/#test-a-composition

# ⚙️ Updating proto
```
dotnet grpc refresh
```

# 🧪 E2E testing

Get Crossplane cli from https://releases.crossplane.io/stable/current/bin/windows_amd64/ - you actualy want `crank` and rename it to `crossplane` as `crossplane` is the kubernetes binary, not CLI.

```
cd tests
crossplane render xr.yaml composition.yaml functions.yaml --verbose --timeout 2s
```