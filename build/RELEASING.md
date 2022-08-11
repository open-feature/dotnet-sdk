# Release process

Only for release managers

1. Decide on the version name to be released. e.g. 0.1.0, 0.1.1 etc
2. Tag the commit with the version number

```shell
git tag -a 0.1.0 -m "0.1.0"
git push origin 0.1.0
```

3. Build and pack the code

```shell
dotnet build --configuration Release --no-restore -p:Deterministic=true
dotnet pack OpenFeature.SDK.proj --configuration Release --no-build
```

4. Push up the package to nuget

```shell
dotnet nuget push OpenFeature.{VERSION}.nupkg --api-key {API_KEY} --source https://api.nuget.org/v3/index.json
```
