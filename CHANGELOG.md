# Changelog

## [2.11.2](https://github.com/open-feature/dotnet-sdk/compare/v2.11.1...v2.11.2) (2026-02-26)


### 🔧 Refactoring

* improve code quality ([#706](https://github.com/open-feature/dotnet-sdk/issues/706)) ([29fd9c1](https://github.com/open-feature/dotnet-sdk/commit/29fd9c167a41cee1099aff9618d40b24ef9bdce6))

## [2.11.1](https://github.com/open-feature/dotnet-sdk/compare/v2.11.0...v2.11.1) (2025-12-30)


### 🐛 Bug Fixes

* Fix dimension mismatch in active evaluation metric ([#679](https://github.com/open-feature/dotnet-sdk/issues/679)) ([0624332](https://github.com/open-feature/dotnet-sdk/commit/06243323e960464305e540b04bec069730f3cb79))

## [2.11.0](https://github.com/open-feature/dotnet-sdk/compare/v2.10.0...v2.11.0) (2025-12-18)


### ✨ New Features

* Upgrade to dotnet version 10 ([#658](https://github.com/open-feature/dotnet-sdk/issues/658)) ([c7be7e0](https://github.com/open-feature/dotnet-sdk/commit/c7be7e0fbff694ec9ef794548f6e7a478412b68b))

## [2.10.0](https://github.com/open-feature/dotnet-sdk/compare/v2.9.0...v2.10.0) (2025-12-01)


### 🐛 Bug Fixes

* Address issue with FeatureClient not being resolved when no Provider added ([#607](https://github.com/open-feature/dotnet-sdk/issues/607)) ([a8d12ef](https://github.com/open-feature/dotnet-sdk/commit/a8d12ef12d75aaa770551b3052cd8725b65b5fd8))
* Address issues when evaluating the context in the InMemoryProvider ([#615](https://github.com/open-feature/dotnet-sdk/issues/615)) ([94fcdc1](https://github.com/open-feature/dotnet-sdk/commit/94fcdc142c61f41619af222778d6d84264f2831c))
* Ensure AddPolicyName without adding a Provider does not get stuck in infinite loop ([#606](https://github.com/open-feature/dotnet-sdk/issues/606)) ([4b965dd](https://github.com/open-feature/dotnet-sdk/commit/4b965dddcaeef761e01f8fcbd28941ae3f3074c9))
* Ensure EvaluationContext is reliably added to the injected FeatureClient ([#605](https://github.com/open-feature/dotnet-sdk/issues/605)) ([c987b58](https://github.com/open-feature/dotnet-sdk/commit/c987b58b66c8186486fd06aebdc4042052f30beb))


### ✨ New Features

* Add DI for multi provider ([#621](https://github.com/open-feature/dotnet-sdk/issues/621)) ([ee862f0](https://github.com/open-feature/dotnet-sdk/commit/ee862f09cb2c58f43f84957fa95e8b25e8e36f72))
* Add disabled flag support to InMemoryProvider ([#632](https://github.com/open-feature/dotnet-sdk/issues/632)) ([df1765c](https://github.com/open-feature/dotnet-sdk/commit/df1765c7abc4e9e5f76954ddb361b3fd5bf0ddf7))
* Add optional CancellationToken parameter to SetProviderAsync ([#638](https://github.com/open-feature/dotnet-sdk/issues/638)) ([a1f7ff6](https://github.com/open-feature/dotnet-sdk/commit/a1f7ff6434842ff051e32af5c787e1bf40a5cb66))
* Add SourceLink configuration for .NET SDK 8+ to enhance debugging experience ([1b40391](https://github.com/open-feature/dotnet-sdk/commit/1b40391034b0762aa755a05374a908eb97cdf444))
* Add SourceLink configuration for .NET to enhance debugging experience ([#614](https://github.com/open-feature/dotnet-sdk/issues/614)) ([1b40391](https://github.com/open-feature/dotnet-sdk/commit/1b40391034b0762aa755a05374a908eb97cdf444))
* Add tracking to multi-provider ([#612](https://github.com/open-feature/dotnet-sdk/issues/612)) ([186b357](https://github.com/open-feature/dotnet-sdk/commit/186b3574702258fb33716162094888b9f7560c7c))


### 🔧 Refactoring

* Clean up project files by removing TargetFrameworks and formatting ([#611](https://github.com/open-feature/dotnet-sdk/issues/611)) ([dfbc3ee](https://github.com/open-feature/dotnet-sdk/commit/dfbc3eef1f7468dc363c71fef1eb1f42e1bb8a88))
* Pass cancellation tokens to Provider Initialization functions ([#640](https://github.com/open-feature/dotnet-sdk/issues/640)) ([8b472d8](https://github.com/open-feature/dotnet-sdk/commit/8b472d8ccd1367ba82a2ab39ad7a77b1a6609ce0))
* Remove deprecated Dependency Injection code ([#626](https://github.com/open-feature/dotnet-sdk/issues/626)) ([a36a906](https://github.com/open-feature/dotnet-sdk/commit/a36a9067102a70f80e7837ce18d287430c7452fc))

## [2.9.0](https://github.com/open-feature/dotnet-sdk/compare/v2.8.1...v2.9.0) (2025-10-16)


### 🐛 Bug Fixes

* update provider status to Fatal during disposal ([#580](https://github.com/open-feature/dotnet-sdk/issues/580)) ([76bd94b](https://github.com/open-feature/dotnet-sdk/commit/76bd94b03ea19ad3c432a52dd644317e362b99ec))


### ✨ New Features

* Add events to the multi provider ([#568](https://github.com/open-feature/dotnet-sdk/issues/568)) ([9d8ab03](https://github.com/open-feature/dotnet-sdk/commit/9d8ab037df1749d098f5e1e210f71cf9d1e7adff))
* Add multi-provider support ([#488](https://github.com/open-feature/dotnet-sdk/issues/488)) ([7237053](https://github.com/open-feature/dotnet-sdk/commit/7237053561d9c36194197169734522f0b978f6e5))
* Deprecate AddHostedFeatureLifecycle method ([#531](https://github.com/open-feature/dotnet-sdk/issues/531)) ([fdf2297](https://github.com/open-feature/dotnet-sdk/commit/fdf229737118639d323e74cceac490d44c4c24dd))
* Implement hooks in multi provider ([#594](https://github.com/open-feature/dotnet-sdk/issues/594)) ([95ae7f0](https://github.com/open-feature/dotnet-sdk/commit/95ae7f03249e351c20ccd6152d88400a7e1ef764))
* Support retrieving numeric metadata as either integers or decimals  ([#490](https://github.com/open-feature/dotnet-sdk/issues/490)) ([12de5f1](https://github.com/open-feature/dotnet-sdk/commit/12de5f10421bac749fdd45c748e7b970f3f69a39))


### 🚀 Performance

* Add NativeAOT Support ([#554](https://github.com/open-feature/dotnet-sdk/issues/554)) ([acd0486](https://github.com/open-feature/dotnet-sdk/commit/acd0486563f7b67a782ee169315922fb5d0f343e))

## [2.8.1](https://github.com/open-feature/dotnet-sdk/compare/v2.8.0...v2.8.1) (2025-07-31)


### 🐛 Bug Fixes

* expose ValueJsonConverter for generator support and add JsonSourceGenerator test cases ([#537](https://github.com/open-feature/dotnet-sdk/issues/537)) ([e03aeba](https://github.com/open-feature/dotnet-sdk/commit/e03aeba0f515f668afaba0a3c6f0ea01b44d6ee4))

## [2.8.0](https://github.com/open-feature/dotnet-sdk/compare/v2.7.0...v2.8.0) (2025-07-30)


### 🐛 Bug Fixes

* update DI lifecycle to use container instead of static instance ([#534](https://github.com/open-feature/dotnet-sdk/issues/534)) ([1a3846d](https://github.com/open-feature/dotnet-sdk/commit/1a3846d7575e75b5d7d05ec2a7db0b0f82c7b274))


### ✨ New Features

* Add Hook Dependency Injection extension method with Hook instance ([#513](https://github.com/open-feature/dotnet-sdk/issues/513)) ([12396b7](https://github.com/open-feature/dotnet-sdk/commit/12396b7872a2db6533b33267cf9c299248c41472))
* Add TraceEnricherHookOptions Custom Attributes ([#526](https://github.com/open-feature/dotnet-sdk/issues/526)) ([5a91005](https://github.com/open-feature/dotnet-sdk/commit/5a91005c888c8966145eae7745cc40b2b066f343))
* Add Track method to IFeatureClient ([#519](https://github.com/open-feature/dotnet-sdk/issues/519)) ([2e70072](https://github.com/open-feature/dotnet-sdk/commit/2e7007277e19a0fbc4c4c3944d24eea1608712e6))
* Support JSON Serialize for Value ([#529](https://github.com/open-feature/dotnet-sdk/issues/529)) ([6e521d2](https://github.com/open-feature/dotnet-sdk/commit/6e521d25c3dd53c45f2fd30f5319cae5cd2ff46d))
* Add Metric Hook Custom Attributes ([#512](https://github.com/open-feature/dotnet-sdk/issues/512)) ([8c05d1d](https://github.com/open-feature/dotnet-sdk/commit/8c05d1d7363db89b8379e1a4e46e455f210888e2))


### 🧹 Chore

* Add comparison to Value ([#523](https://github.com/open-feature/dotnet-sdk/issues/523)) ([883f4f3](https://github.com/open-feature/dotnet-sdk/commit/883f4f3c8b553dc01b5accdbae2782ca7805e8ed))
* **deps:** update github/codeql-action digest to 181d5ee ([#520](https://github.com/open-feature/dotnet-sdk/issues/520)) ([40bec0d](https://github.com/open-feature/dotnet-sdk/commit/40bec0d51b6fa782a8b6d90a3d84463f9fb73c1b))
* **deps:** update github/codeql-action digest to 4e828ff ([#532](https://github.com/open-feature/dotnet-sdk/issues/532)) ([20d1f37](https://github.com/open-feature/dotnet-sdk/commit/20d1f37a4f8991419bb14dae7eec9a08c2b32bc6))
* **deps:** update github/codeql-action digest to d6bbdef ([#527](https://github.com/open-feature/dotnet-sdk/issues/527)) ([03d3b9e](https://github.com/open-feature/dotnet-sdk/commit/03d3b9e5d6ff1706faffc25afeba80a0e2bb37ec))
* **deps:** update spec digest to 224b26e ([#521](https://github.com/open-feature/dotnet-sdk/issues/521)) ([fbc2645](https://github.com/open-feature/dotnet-sdk/commit/fbc2645efd649c0c37bd1a1cf473fbd98d920948))
* **deps:** update spec digest to baec39b ([#528](https://github.com/open-feature/dotnet-sdk/issues/528)) ([a0ae014](https://github.com/open-feature/dotnet-sdk/commit/a0ae014d3194fcf6e5e5e4a17a2f92b1df3dc7c7))
* remove redundant rule (now in parent) ([929fa74](https://github.com/open-feature/dotnet-sdk/commit/929fa7497197214d385eeaa40aba008932d00896))


### 📚 Documentation

* fix anchor link in readme ([#525](https://github.com/open-feature/dotnet-sdk/issues/525)) ([18705c7](https://github.com/open-feature/dotnet-sdk/commit/18705c7338a0c89f163f808c81e513a029c95239))
* remove curly brace from readme ([8c92524](https://github.com/open-feature/dotnet-sdk/commit/8c92524edbf4579d4ad62c699b338b9811a783fd))


### 🔄 Refactoring

* Simplify Provider Repository ([#515](https://github.com/open-feature/dotnet-sdk/issues/515)) ([2547a57](https://github.com/open-feature/dotnet-sdk/commit/2547a574e0d0328f909b7e69f3775d07492de3dd))

## [2.7.0](https://github.com/open-feature/dotnet-sdk/compare/v2.6.0...v2.7.0) (2025-07-03)


### 🐛 Bug Fixes

* Add generic to evaluation event builder ([#500](https://github.com/open-feature/dotnet-sdk/issues/500)) ([68af649](https://github.com/open-feature/dotnet-sdk/commit/68af6493b09d29be5d4cdda9e6f792ee8667bf4f))
* ArgumentNullException when creating a client with optional name ([#508](https://github.com/open-feature/dotnet-sdk/issues/508)) ([9151dcd](https://github.com/open-feature/dotnet-sdk/commit/9151dcdf2cecde9b4b01f06c73d149e0ad3bb539))


### ✨ New Features

* Move OTEL hooks to the SDK ([#338](https://github.com/open-feature/dotnet-sdk/issues/338)) ([77f6e1b](https://github.com/open-feature/dotnet-sdk/commit/77f6e1bbb76973e078c1999ad0784c9edc9def96))


### 🧹 Chore

* **deps:** update actions/attest-build-provenance action to v2.4.0 ([#495](https://github.com/open-feature/dotnet-sdk/issues/495)) ([349c073](https://github.com/open-feature/dotnet-sdk/commit/349c07301d0ff97c759417344eef74a00b06edbc))
* **deps:** update actions/attest-sbom action to v2.4.0 ([#496](https://github.com/open-feature/dotnet-sdk/issues/496)) ([f7ca416](https://github.com/open-feature/dotnet-sdk/commit/f7ca4163e0ce549a015a7a27cb184fb76a199a04))
* **deps:** update dependency benchmarkdotnet to 0.15.0 ([#481](https://github.com/open-feature/dotnet-sdk/issues/481)) ([714425d](https://github.com/open-feature/dotnet-sdk/commit/714425d405a33231e85b1e62019fc678b2e883ef))
* **deps:** update dependency benchmarkdotnet to 0.15.2 ([#494](https://github.com/open-feature/dotnet-sdk/issues/494)) ([cab3807](https://github.com/open-feature/dotnet-sdk/commit/cab380727fe95b941384ae71f022626cdf23db53))
* **deps:** update dependency microsoft.net.test.sdk to 17.14.0 ([#482](https://github.com/open-feature/dotnet-sdk/issues/482)) ([520d383](https://github.com/open-feature/dotnet-sdk/commit/520d38305c6949c88b057f28e5dfe3305257e437))
* **deps:** update dependency microsoft.net.test.sdk to 17.14.1 ([#485](https://github.com/open-feature/dotnet-sdk/issues/485)) ([78bfdbf](https://github.com/open-feature/dotnet-sdk/commit/78bfdbf0850e2d5eb80cfbae3bfac8208f6c45b1))
* **deps:** update dependency opentelemetry.instrumentation.aspnetcore to 1.12.0 ([#505](https://github.com/open-feature/dotnet-sdk/issues/505)) ([241d880](https://github.com/open-feature/dotnet-sdk/commit/241d88024ff13ddd57f4e9c5719add95b5864043))
* **deps:** update dependency reqnroll.xunit to 2.4.1 ([#483](https://github.com/open-feature/dotnet-sdk/issues/483)) ([99f7584](https://github.com/open-feature/dotnet-sdk/commit/99f7584c91882ba59412e2306167172470cd4677))
* **deps:** update dependency system.valuetuple to 4.6.1 ([#503](https://github.com/open-feature/dotnet-sdk/issues/503)) ([39f884d](https://github.com/open-feature/dotnet-sdk/commit/39f884df420f1a9346852159948c288e728672b8))
* **deps:** update github/codeql-action digest to 39edc49 ([#504](https://github.com/open-feature/dotnet-sdk/issues/504)) ([08ff43c](https://github.com/open-feature/dotnet-sdk/commit/08ff43ce3426c8bb9f24446bdf62e56b10534c1f))
* **deps:** update github/codeql-action digest to ce28f5b ([#492](https://github.com/open-feature/dotnet-sdk/issues/492)) ([cce224f](https://github.com/open-feature/dotnet-sdk/commit/cce224fcf81aede5a626936a26546fe710fbcc30))
* **deps:** update github/codeql-action digest to fca7ace ([#486](https://github.com/open-feature/dotnet-sdk/issues/486)) ([e18ad50](https://github.com/open-feature/dotnet-sdk/commit/e18ad50e3298cb0dd19143678c3ef0fdcb4484d9))
* **deps:** update opentelemetry-dotnet monorepo to 1.12.0 ([#506](https://github.com/open-feature/dotnet-sdk/issues/506)) ([69dc186](https://github.com/open-feature/dotnet-sdk/commit/69dc18611399ab5e573268c35d414a028c77f0ff))
* **deps:** update spec digest to 1965aae ([#499](https://github.com/open-feature/dotnet-sdk/issues/499)) ([2e3dffd](https://github.com/open-feature/dotnet-sdk/commit/2e3dffd0ebbba4a9d95763e2ce9f3e2ac051a317))
* **deps:** update spec digest to 42340bb ([#493](https://github.com/open-feature/dotnet-sdk/issues/493)) ([909c51d](https://github.com/open-feature/dotnet-sdk/commit/909c51d4e25917d6a9a5ae9bb04cfe48665186ba))
* **deps:** update spec digest to c37ac17 ([#502](https://github.com/open-feature/dotnet-sdk/issues/502)) ([38f63fc](https://github.com/open-feature/dotnet-sdk/commit/38f63fceb5516cd474fd0e867aa25eae252cf2c1))
* **deps:** update spec digest to f014806 ([#479](https://github.com/open-feature/dotnet-sdk/issues/479)) ([dbe8b08](https://github.com/open-feature/dotnet-sdk/commit/dbe8b082c28739a1b81b74b29ed28fbccc94f7bc))
* fix sample build warning ([#498](https://github.com/open-feature/dotnet-sdk/issues/498)) ([08a00e1](https://github.com/open-feature/dotnet-sdk/commit/08a00e1d35834635ca296fe8a13507001ad25c57))


### 📚 Documentation

* add XML comment on FeatureClient ([#507](https://github.com/open-feature/dotnet-sdk/issues/507)) ([f923cea](https://github.com/open-feature/dotnet-sdk/commit/f923cea14eb552098edb987950ad4bc82bbadab1))
* updated contributing link on the README ([8435bf7](https://github.com/open-feature/dotnet-sdk/commit/8435bf7d8131307e627e59453008124ac4c71906))

## [2.6.0](https://github.com/open-feature/dotnet-sdk/compare/v2.5.0...v2.6.0) (2025-05-23)


### ✨ New Features

* add AddHandler extension method to Dependency Injection package ([#462](https://github.com/open-feature/dotnet-sdk/issues/462)) ([ff414b8](https://github.com/open-feature/dotnet-sdk/commit/ff414b8a860108ca3cb372dc4a69b942bf4cd005))
* Add Extension Method for adding global Hook via DependencyInjection ([#459](https://github.com/open-feature/dotnet-sdk/issues/459)) ([9b04485](https://github.com/open-feature/dotnet-sdk/commit/9b04485173978d600a4e3fd24df111347070dc70))
* Add OTEL compatible telemetry object builder ([#397](https://github.com/open-feature/dotnet-sdk/issues/397)) ([6c44db9](https://github.com/open-feature/dotnet-sdk/commit/6c44db9237748735a22a162a88f61b2de7f3e9bd))


### 🧹 Chore

* Cleanup .props file ([#476](https://github.com/open-feature/dotnet-sdk/issues/476)) ([6d7a535](https://github.com/open-feature/dotnet-sdk/commit/6d7a5359bfcb8d12d648a75cf469e93909b999ed))
* **deps:** update actions/attest-build-provenance action to v2.3.0 ([#464](https://github.com/open-feature/dotnet-sdk/issues/464)) ([0a5ab0c](https://github.com/open-feature/dotnet-sdk/commit/0a5ab0c3c71a1a615b0ee8627dd4ff5db39cac9b))
* **deps:** update codecov/codecov-action action to v5.4.3 ([#475](https://github.com/open-feature/dotnet-sdk/issues/475)) ([fbcf3a4](https://github.com/open-feature/dotnet-sdk/commit/fbcf3a4b2478fae49f1566b828c9d0a8cffddd46))
* **deps:** update github/codeql-action digest to 60168ef ([#463](https://github.com/open-feature/dotnet-sdk/issues/463)) ([ea76351](https://github.com/open-feature/dotnet-sdk/commit/ea76351a095b7eeb777941aaf7ac42e4d925c366))
* **deps:** update github/codeql-action digest to ff0a06e ([#473](https://github.com/open-feature/dotnet-sdk/issues/473)) ([af1b20f](https://github.com/open-feature/dotnet-sdk/commit/af1b20f0822497b46b11ce8c21c47c8d39a5fbe9))
* **deps:** update spec digest to edf0deb ([#474](https://github.com/open-feature/dotnet-sdk/issues/474)) ([fc3bdfe](https://github.com/open-feature/dotnet-sdk/commit/fc3bdfef63147dbd10ace398127eb633dd34c773))


### 📚 Documentation

* Add AspNetCore sample app ([#477](https://github.com/open-feature/dotnet-sdk/issues/477)) ([9742a0d](https://github.com/open-feature/dotnet-sdk/commit/9742a0d4210d2dd6e315b4e5988261aae4352c2f))

## [2.5.0](https://github.com/open-feature/dotnet-sdk/compare/v2.4.0...v2.5.0) (2025-04-25)


### ✨ New Features

* Add support for hook data. ([#387](https://github.com/open-feature/dotnet-sdk/issues/387)) ([4563512](https://github.com/open-feature/dotnet-sdk/commit/456351216ce9113d84b56d0bce1dad39430a26cd))


### 🧹 Chore

* add NuGet auditing ([#454](https://github.com/open-feature/dotnet-sdk/issues/454)) ([42ab536](https://github.com/open-feature/dotnet-sdk/commit/42ab5368d3d8f874f175ab9ad3077f177a592398))
* Change file scoped namespaces and cleanup job ([#453](https://github.com/open-feature/dotnet-sdk/issues/453)) ([1e74a04](https://github.com/open-feature/dotnet-sdk/commit/1e74a04f2b76c128a09c95dfd0b06803f2ef77bf))
* **deps:** update codecov/codecov-action action to v5.4.2 ([#432](https://github.com/open-feature/dotnet-sdk/issues/432)) ([c692ec2](https://github.com/open-feature/dotnet-sdk/commit/c692ec2a26eb4007ff428e54eaa67ea22fd20728))
* **deps:** update github/codeql-action digest to 28deaed ([#446](https://github.com/open-feature/dotnet-sdk/issues/446)) ([dfecd0c](https://github.com/open-feature/dotnet-sdk/commit/dfecd0c6a4467e5c1afe481e785e3e0f179beb25))
* **deps:** update spec digest to 18cde17 ([#395](https://github.com/open-feature/dotnet-sdk/issues/395)) ([5608dfb](https://github.com/open-feature/dotnet-sdk/commit/5608dfbd441b99531add8e89ad842ea9d613f707))
* **deps:** update spec digest to 2ba05d8 ([#452](https://github.com/open-feature/dotnet-sdk/issues/452)) ([eb688c4](https://github.com/open-feature/dotnet-sdk/commit/eb688c412983511c7ec0744df95e4a113f610c5f))
* **deps:** update spec digest to 36944c6 ([#450](https://github.com/open-feature/dotnet-sdk/issues/450)) ([e162169](https://github.com/open-feature/dotnet-sdk/commit/e162169af0b5518f12527a8601d6dfcdf379b4f7))
* **deps:** update spec digest to d27e000 ([#455](https://github.com/open-feature/dotnet-sdk/issues/455)) ([e0ec8ca](https://github.com/open-feature/dotnet-sdk/commit/e0ec8ca28303b7df71699063b02b6967cdc37bcd))
* packages read in release please ([1acc00f](https://github.com/open-feature/dotnet-sdk/commit/1acc00fa7a6a38152d97fd7efc9f7e8befb1c3ed))
* update release permissions ([d0bf40b](https://github.com/open-feature/dotnet-sdk/commit/d0bf40b9b40adc57a2a008a9497098b3cd1a05a7))
* **workflows:** Add permissions for contents and pull-requests ([#439](https://github.com/open-feature/dotnet-sdk/issues/439)) ([568722a](https://github.com/open-feature/dotnet-sdk/commit/568722a4ab1f863d8509dc4a172ac9c29f267825))


### 📚 Documentation

* update documentation on SetProviderAsync ([#449](https://github.com/open-feature/dotnet-sdk/issues/449)) ([858b286](https://github.com/open-feature/dotnet-sdk/commit/858b286dba2313239141c20ec6770504d340fbe0))
* Update README with spec version ([#437](https://github.com/open-feature/dotnet-sdk/issues/437)) ([7318b81](https://github.com/open-feature/dotnet-sdk/commit/7318b8126df9f0ddd5651fdd9fe32da2e4819290)), closes [#204](https://github.com/open-feature/dotnet-sdk/issues/204)


### 🔄 Refactoring

* InMemoryProvider throwing when types mismatched ([#442](https://github.com/open-feature/dotnet-sdk/issues/442)) ([8ecf50d](https://github.com/open-feature/dotnet-sdk/commit/8ecf50db2cab3a266de5c6c5216714570cfc6a52))

## [2.4.0](https://github.com/open-feature/dotnet-sdk/compare/v2.3.2...v2.4.0) (2025-04-14)


### 🐛 Bug Fixes

* Refactor error handling and improve documentation ([#417](https://github.com/open-feature/dotnet-sdk/issues/417)) ([b0b168f](https://github.com/open-feature/dotnet-sdk/commit/b0b168ffc051e3a6c55f66ea6af4208e7d64419d))


### ✨ New Features

* update FeatureLifecycleStateOptions.StopState default to Stopped ([#414](https://github.com/open-feature/dotnet-sdk/issues/414)) ([6c23f21](https://github.com/open-feature/dotnet-sdk/commit/6c23f21d56ef6cc6adce7f798ee302924c227e1f))


### 🧹 Chore

* **deps:** update github/codeql-action digest to 45775bd ([#419](https://github.com/open-feature/dotnet-sdk/issues/419)) ([2bed467](https://github.com/open-feature/dotnet-sdk/commit/2bed467317ab0afa6d3e3718e89a5bb05453d649))
* restrict publish to environment ([#431](https://github.com/open-feature/dotnet-sdk/issues/431)) ([0c222cb](https://github.com/open-feature/dotnet-sdk/commit/0c222cb5e90203e8f4740207d3dd82ec12179594))


### 📚 Documentation

* Update contributing guidelines ([#413](https://github.com/open-feature/dotnet-sdk/issues/413)) ([84ea288](https://github.com/open-feature/dotnet-sdk/commit/84ea288a3bc6e5ec8a797312f36e44c28d03c95c))


### 🔄 Refactoring

* simplify the InternalsVisibleTo usage ([#408](https://github.com/open-feature/dotnet-sdk/issues/408)) ([4043d3d](https://github.com/open-feature/dotnet-sdk/commit/4043d3d7610b398e6be035a0e1bf28e7c81ebf18))

## [2.3.2](https://github.com/open-feature/dotnet-sdk/compare/v2.3.1...v2.3.2) (2025-03-24)


### 🐛 Bug Fixes

* Address issue with newline characters when running Logging Hook Unit Tests on linux ([#374](https://github.com/open-feature/dotnet-sdk/issues/374)) ([a98334e](https://github.com/open-feature/dotnet-sdk/commit/a98334edfc0a6a14ff60e362bd7aa198b70ff255))
* Remove virtual GetEventChannel from FeatureProvider ([#401](https://github.com/open-feature/dotnet-sdk/issues/401)) ([00a4e4a](https://github.com/open-feature/dotnet-sdk/commit/00a4e4ab2ccb8984cd3ca57bad6d25e688b1cf8c))
* Update project name in solution file ([#380](https://github.com/open-feature/dotnet-sdk/issues/380)) ([1f13258](https://github.com/open-feature/dotnet-sdk/commit/1f13258737fa051289d51cf5a064e03b0dc936c8))


### 🧹 Chore

* Correct LoggingHookTest timestamp handling. ([#386](https://github.com/open-feature/dotnet-sdk/issues/386)) ([c69a6e5](https://github.com/open-feature/dotnet-sdk/commit/c69a6e5d71a6d652017a0d46c8390554a1dec59e))
* **deps:** update actions/setup-dotnet digest to 67a3573 ([#402](https://github.com/open-feature/dotnet-sdk/issues/402)) ([2e2c489](https://github.com/open-feature/dotnet-sdk/commit/2e2c4898479b3544d663c08ddd2dc011ca482b43))
* **deps:** update actions/upload-artifact action to v4.6.1 ([#385](https://github.com/open-feature/dotnet-sdk/issues/385)) ([accf571](https://github.com/open-feature/dotnet-sdk/commit/accf57181b34c600cb775a93b173f644d8c445d1))
* **deps:** update actions/upload-artifact action to v4.6.2 ([#406](https://github.com/open-feature/dotnet-sdk/issues/406)) ([16c92b7](https://github.com/open-feature/dotnet-sdk/commit/16c92b7814f49aceab6e6d46a8835c2bdc0f3363))
* **deps:** update codecov/codecov-action action to v5.4.0 ([#392](https://github.com/open-feature/dotnet-sdk/issues/392)) ([06e4e3a](https://github.com/open-feature/dotnet-sdk/commit/06e4e3a7ee11aff5c53eeba2259a840956bc4d5d))
* **deps:** update dependency dotnet-sdk to v9.0.202 ([#405](https://github.com/open-feature/dotnet-sdk/issues/405)) ([a4beaae](https://github.com/open-feature/dotnet-sdk/commit/a4beaaea375b3184578d259cd5ca481d23055a54))
* **deps:** update dependency microsoft.net.test.sdk to 17.13.0 ([#375](https://github.com/open-feature/dotnet-sdk/issues/375)) ([7a735f8](https://github.com/open-feature/dotnet-sdk/commit/7a735f8d8b82b79b205f71716e5cf300a7fff276))
* **deps:** update dependency reqnroll.xunit to 2.3.0 ([#378](https://github.com/open-feature/dotnet-sdk/issues/378)) ([96ba568](https://github.com/open-feature/dotnet-sdk/commit/96ba5686c2ba31996603f464fe7e5df9efa01a92))
* **deps:** update dependency reqnroll.xunit to 2.4.0 ([#396](https://github.com/open-feature/dotnet-sdk/issues/396)) ([b30350b](https://github.com/open-feature/dotnet-sdk/commit/b30350bd49f4a8709b69a3eb2db1152d5a4b7f6c))
* **deps:** update dependency system.valuetuple to 4.6.0 ([#403](https://github.com/open-feature/dotnet-sdk/issues/403)) ([75468d2](https://github.com/open-feature/dotnet-sdk/commit/75468d28ba4d8200c7199fe89d6d1a63f3bdd674))
* **deps:** update dotnet monorepo ([#379](https://github.com/open-feature/dotnet-sdk/issues/379)) ([53ced91](https://github.com/open-feature/dotnet-sdk/commit/53ced9118ffcb8cda5142dc2f80465416922030b))
* **deps:** update dotnet monorepo to 9.0.2 ([#377](https://github.com/open-feature/dotnet-sdk/issues/377)) ([3bdc79b](https://github.com/open-feature/dotnet-sdk/commit/3bdc79bbaa8d73c4747916d307c431990397cdde))
* **deps:** update github/codeql-action digest to 1b549b9 ([#407](https://github.com/open-feature/dotnet-sdk/issues/407)) ([ae9fc79](https://github.com/open-feature/dotnet-sdk/commit/ae9fc79bcb9847efcb62673f5aa59df403cece78))
* **deps:** update github/codeql-action digest to 5f8171a ([#404](https://github.com/open-feature/dotnet-sdk/issues/404)) ([73a5040](https://github.com/open-feature/dotnet-sdk/commit/73a504022d8ba4cbe508a4f0b76f9b73f58c17a6))
* **deps:** update github/codeql-action digest to 6bb031a ([#398](https://github.com/open-feature/dotnet-sdk/issues/398)) ([9b6feab](https://github.com/open-feature/dotnet-sdk/commit/9b6feab50085ee7dfcca190fe42f583c072ae50d))
* **deps:** update github/codeql-action digest to 9e8d078 ([#371](https://github.com/open-feature/dotnet-sdk/issues/371)) ([e74e8e7](https://github.com/open-feature/dotnet-sdk/commit/e74e8e7a58d90e46bbcd5d7e9433545412e07bbd))
* **deps:** update github/codeql-action digest to b56ba49 ([#384](https://github.com/open-feature/dotnet-sdk/issues/384)) ([cc2990f](https://github.com/open-feature/dotnet-sdk/commit/cc2990ff8e7bf5148ab1cd867d9bfabfc0b7af8a))
* **deps:** update spec digest to 0cd553d ([#389](https://github.com/open-feature/dotnet-sdk/issues/389)) ([85075ac](https://github.com/open-feature/dotnet-sdk/commit/85075ac7f46783dd1bcfdbbe6bd10d81eb9adb8a))
* **deps:** update spec digest to 54952f3 ([#373](https://github.com/open-feature/dotnet-sdk/issues/373)) ([1e8b230](https://github.com/open-feature/dotnet-sdk/commit/1e8b2307369710ea0b5ae0e8a8f1f1293ea066dc))
* **deps:** update spec digest to a69f748 ([#382](https://github.com/open-feature/dotnet-sdk/issues/382)) ([4977542](https://github.com/open-feature/dotnet-sdk/commit/4977542515bff302c7a88f3fa301bb129d7ea8cf))
* remove FluentAssertions ([#361](https://github.com/open-feature/dotnet-sdk/issues/361)) ([4ecfd24](https://github.com/open-feature/dotnet-sdk/commit/4ecfd249181cf8fe372810a1fc3369347c6302fc))
* Replace SpecFlow with Reqnroll for testing framework ([#368](https://github.com/open-feature/dotnet-sdk/issues/368)) ([ed6ee2c](https://github.com/open-feature/dotnet-sdk/commit/ed6ee2c502b16e49c91c6363ae6b3f54401a85cb)), closes [#354](https://github.com/open-feature/dotnet-sdk/issues/354)
* update release please repo, specify action permissions ([#369](https://github.com/open-feature/dotnet-sdk/issues/369)) ([63846ad](https://github.com/open-feature/dotnet-sdk/commit/63846ad1033399e9c84ad5946367c5eef2663b5b))


### 🔄 Refactoring

* Improve EventExecutor ([#393](https://github.com/open-feature/dotnet-sdk/issues/393)) ([46274a2](https://github.com/open-feature/dotnet-sdk/commit/46274a21d74b5cfffd4cfbc30e5e49e2dc1f256c))

## [2.3.1](https://github.com/open-feature/dotnet-sdk/compare/v2.3.0...v2.3.1) (2025-02-04)


### 🐛 Bug Fixes

* Fix SBOM release pipeline ([#367](https://github.com/open-feature/dotnet-sdk/issues/367)) ([dad6282](https://github.com/open-feature/dotnet-sdk/commit/dad62826404e1d2e679ef35560a1dd858c95ffdc))


### 🧹 Chore

* **deps:** pin dependencies ([#365](https://github.com/open-feature/dotnet-sdk/issues/365)) ([3160cd2](https://github.com/open-feature/dotnet-sdk/commit/3160cd2262739ba0a2981a9dc04fc0d278799546))
* **deps:** update actions/upload-artifact action to v4.6.0 ([#341](https://github.com/open-feature/dotnet-sdk/issues/341)) ([cb7105b](https://github.com/open-feature/dotnet-sdk/commit/cb7105b21c4e0fc1674365f9fd6a4b26e95f45c3))
* **deps:** update dependency autofixture to 5.0.0-preview0012 ([#351](https://github.com/open-feature/dotnet-sdk/issues/351)) ([9b0b319](https://github.com/open-feature/dotnet-sdk/commit/9b0b3195fa206f11c1acc7336c4e4f6252b8b2ad))
* **deps:** update dependency coverlet.collector to 6.0.4 ([#347](https://github.com/open-feature/dotnet-sdk/issues/347)) ([e59034d](https://github.com/open-feature/dotnet-sdk/commit/e59034dd56038351f79a7e226adae268d172ccbb))
* **deps:** update dependency coverlet.msbuild to 6.0.4 ([#348](https://github.com/open-feature/dotnet-sdk/issues/348)) ([5ebe4f6](https://github.com/open-feature/dotnet-sdk/commit/5ebe4f685ec0bf4d7d0acbf3790c908e04c5efd7))
* **deps:** update dependency xunit to 2.9.3 ([#340](https://github.com/open-feature/dotnet-sdk/issues/340)) ([fb8e5aa](https://github.com/open-feature/dotnet-sdk/commit/fb8e5aa9d3a020de3ea57948130951d0d282f465))
* **deps:** update dotnet monorepo ([#343](https://github.com/open-feature/dotnet-sdk/issues/343)) ([32dab9b](https://github.com/open-feature/dotnet-sdk/commit/32dab9ba0904a6b27fc53e21402ae95d5594ad01))
* **deps:** update spec digest to 8d6eeb3 ([#366](https://github.com/open-feature/dotnet-sdk/issues/366)) ([0cb58db](https://github.com/open-feature/dotnet-sdk/commit/0cb58db59573f9f8266fc417083c91e86e499772))
* update renovate config to extend the shared config ([#364](https://github.com/open-feature/dotnet-sdk/issues/364)) ([e3965db](https://github.com/open-feature/dotnet-sdk/commit/e3965dbc31561c9a09342f8808f1175974e14317))

## [2.3.0](https://github.com/open-feature/dotnet-sdk/compare/v2.2.0...v2.3.0) (2025-01-31)


#### Hook Changes

The signature of the `finally` hook stage has been changed. The signature now includes the `evaluation details`, as per the [OpenFeature specification](https://openfeature.dev/specification/sections/hooks#requirement-438). Note that since hooks are still `experimental,` this does not constitute a change requiring a new major version. To migrate, update any hook that implements the `finally` stage to accept `evaluation details` as the second argument.

* Add evaluation details to finally hook stage ([#335](https://github.com/open-feature/dotnet-sdk/issues/335)) ([2ef9955](https://github.com/open-feature/dotnet-sdk/commit/2ef995529d377826d467fa486f18af20bfeeba60))

#### .NET 6

Removed support for .NET 6.

* add dotnet 9 support, rm dotnet 6 ([#317](https://github.com/open-feature/dotnet-sdk/issues/317)) ([2774b0d](https://github.com/open-feature/dotnet-sdk/commit/2774b0d3c09f2f206834ca3fe2526e3eb3ca8087))

### 🐛 Bug Fixes

* Adding Async Lifetime method to fix flaky unit tests ([#333](https://github.com/open-feature/dotnet-sdk/issues/333)) ([e14ab39](https://github.com/open-feature/dotnet-sdk/commit/e14ab39180d38544132e9fe92244b7b37255d2cf))
* Fix issue with DI documentation ([#350](https://github.com/open-feature/dotnet-sdk/issues/350)) ([728ae47](https://github.com/open-feature/dotnet-sdk/commit/728ae471625ab1ff5f166b60a5830afbaf9ad276))


### ✨ New Features

* add dotnet 9 support, rm dotnet 6 ([#317](https://github.com/open-feature/dotnet-sdk/issues/317)) ([2774b0d](https://github.com/open-feature/dotnet-sdk/commit/2774b0d3c09f2f206834ca3fe2526e3eb3ca8087))
* Add evaluation details to finally hook stage ([#335](https://github.com/open-feature/dotnet-sdk/issues/335)) ([2ef9955](https://github.com/open-feature/dotnet-sdk/commit/2ef995529d377826d467fa486f18af20bfeeba60))
* Implement Default Logging Hook ([#308](https://github.com/open-feature/dotnet-sdk/issues/308)) ([7013e95](https://github.com/open-feature/dotnet-sdk/commit/7013e9503f6721bd5f241c6c4d082a4a4e9eceed))
* Implement transaction context ([#312](https://github.com/open-feature/dotnet-sdk/issues/312)) ([1b5a0a9](https://github.com/open-feature/dotnet-sdk/commit/1b5a0a9823e4f68e9356536ad5aa8418d8ca815f))


### 🧹 Chore

* **deps:** update actions/upload-artifact action to v4.5.0 ([#332](https://github.com/open-feature/dotnet-sdk/issues/332)) ([fd68cb0](https://github.com/open-feature/dotnet-sdk/commit/fd68cb0bed0228607cc2369ef6822dd518c5fbec))
* **deps:** update codecov/codecov-action action to v5 ([#316](https://github.com/open-feature/dotnet-sdk/issues/316)) ([6c4cd02](https://github.com/open-feature/dotnet-sdk/commit/6c4cd0273f85bc0be0b07753d47bf13a613bbf82))
* **deps:** update codecov/codecov-action action to v5.1.2 ([#334](https://github.com/open-feature/dotnet-sdk/issues/334)) ([b9ebddf](https://github.com/open-feature/dotnet-sdk/commit/b9ebddfccb094f45a50e8196e43c087b4e97ffa4))
* **deps:** update codecov/codecov-action action to v5.3.1 ([#355](https://github.com/open-feature/dotnet-sdk/issues/355)) ([1e8ebc4](https://github.com/open-feature/dotnet-sdk/commit/1e8ebc447f5f0d76cfb6e03d034d663ae0c32830))
* **deps:** update dependency coverlet.collector to 6.0.3 ([#336](https://github.com/open-feature/dotnet-sdk/issues/336)) ([8527b03](https://github.com/open-feature/dotnet-sdk/commit/8527b03fb020a9604463da80f305978baa85f172))
* **deps:** update dependency coverlet.msbuild to 6.0.3 ([#337](https://github.com/open-feature/dotnet-sdk/issues/337)) ([26fd235](https://github.com/open-feature/dotnet-sdk/commit/26fd2356c1835271dee2f7b8b03b2c83e9cb2eea))
* **deps:** update dependency dotnet-sdk to v9.0.101 ([#339](https://github.com/open-feature/dotnet-sdk/issues/339)) ([dd26ad6](https://github.com/open-feature/dotnet-sdk/commit/dd26ad6d35e134ab40a290e644d5f8bdc8e56c66))
* **deps:** update dependency fluentassertions to 7.1.0 ([#346](https://github.com/open-feature/dotnet-sdk/issues/346)) ([dd1c8e4](https://github.com/open-feature/dotnet-sdk/commit/dd1c8e4f78bf17b5fdb36a070a517a5fff0546d2))
* **deps:** update dependency microsoft.net.test.sdk to 17.12.0 ([#322](https://github.com/open-feature/dotnet-sdk/issues/322)) ([6f5b049](https://github.com/open-feature/dotnet-sdk/commit/6f5b04997aee44c2023e75471932e9f5ff27b0be))


### 📚 Documentation

* disable space in link text lint rule ([#329](https://github.com/open-feature/dotnet-sdk/issues/329)) ([583b2a9](https://github.com/open-feature/dotnet-sdk/commit/583b2a9beab18ba70f8789b903d61a4c685560f0))

## [2.2.0](https://github.com/open-feature/dotnet-sdk/compare/v2.1.0...v2.2.0) (2024-12-12)


### ✨ New Features

* Feature Provider Enhancements- [#321](https://github.com/open-feature/dotnet-sdk/issues/321) ([#324](https://github.com/open-feature/dotnet-sdk/issues/324)) ([70f847b](https://github.com/open-feature/dotnet-sdk/commit/70f847b2979e9b2b69f4e560799e2bc9fe87d5e8))
* Implement Tracking in .NET [#309](https://github.com/open-feature/dotnet-sdk/issues/309) ([#327](https://github.com/open-feature/dotnet-sdk/issues/327)) ([cbf4f25](https://github.com/open-feature/dotnet-sdk/commit/cbf4f25a4365eac15e37987d2d7163cb1aefacfe))
* Support Returning Error Resolutions from Providers ([#323](https://github.com/open-feature/dotnet-sdk/issues/323)) ([bf9de4e](https://github.com/open-feature/dotnet-sdk/commit/bf9de4e177a4963340278854a25dd355f95dfc51))


### 🧹 Chore

* **deps:** update dependency fluentassertions to v7 ([#325](https://github.com/open-feature/dotnet-sdk/issues/325)) ([35cd77b](https://github.com/open-feature/dotnet-sdk/commit/35cd77b59dc938301e7e22ddefd9b39ef8e21a4b))

## [2.1.0](https://github.com/open-feature/dotnet-sdk/compare/v2.0.0...v2.1.0) (2024-11-18)


### 🐛 Bug Fixes

* Fix action syntax in workflow configuration ([#315](https://github.com/open-feature/dotnet-sdk/issues/315)) ([ccf0250](https://github.com/open-feature/dotnet-sdk/commit/ccf02506ecd924738b6ae03dedf25c8e2df6d1fb))
* Fix unit test clean context  ([#313](https://github.com/open-feature/dotnet-sdk/issues/313)) ([3038142](https://github.com/open-feature/dotnet-sdk/commit/30381423333c54e1df98d7721dd72697fc5406dc))


### ✨ New Features

* Add Dependency Injection and Hosting support for OpenFeature ([#310](https://github.com/open-feature/dotnet-sdk/issues/310)) ([1aaa0ec](https://github.com/open-feature/dotnet-sdk/commit/1aaa0ec0e75d5048554752db30193694f0999a4a))


### 🧹 Chore

* **deps:** update actions/upload-artifact action to v4.4.3 ([#292](https://github.com/open-feature/dotnet-sdk/issues/292)) ([9b693f7](https://github.com/open-feature/dotnet-sdk/commit/9b693f737f111ed878749f725dd4c831206b308a))
* **deps:** update codecov/codecov-action action to v4.6.0 ([#306](https://github.com/open-feature/dotnet-sdk/issues/306)) ([4b92528](https://github.com/open-feature/dotnet-sdk/commit/4b92528bd56541ca3701bd4cf80467cdda80f046))
* **deps:** update dependency dotnet-sdk to v8.0.401 ([#296](https://github.com/open-feature/dotnet-sdk/issues/296)) ([0bae29d](https://github.com/open-feature/dotnet-sdk/commit/0bae29d4771c4901e0c511b8d3587e6501e67ecd))
* **deps:** update dependency fluentassertions to 6.12.2 ([#302](https://github.com/open-feature/dotnet-sdk/issues/302)) ([bc7e187](https://github.com/open-feature/dotnet-sdk/commit/bc7e187b7586a04e0feb9ef28291ce14c9ac35c5))
* **deps:** update dependency microsoft.net.test.sdk to 17.11.0 ([#297](https://github.com/open-feature/dotnet-sdk/issues/297)) ([5593e19](https://github.com/open-feature/dotnet-sdk/commit/5593e19ca990196f754cd0be69391abb8f0dbcd5))
* **deps:** update dependency microsoft.net.test.sdk to 17.11.1 ([#301](https://github.com/open-feature/dotnet-sdk/issues/301)) ([5b979d2](https://github.com/open-feature/dotnet-sdk/commit/5b979d290d96020ffe7f3e5729550d6f988b2af2))
* **deps:** update dependency nsubstitute to 5.3.0 ([#311](https://github.com/open-feature/dotnet-sdk/issues/311)) ([87f9cfa](https://github.com/open-feature/dotnet-sdk/commit/87f9cfa9b5ace84546690fea95f33bf06fd1947b))
* **deps:** update dependency xunit to 2.9.2 ([#303](https://github.com/open-feature/dotnet-sdk/issues/303)) ([2273948](https://github.com/open-feature/dotnet-sdk/commit/22739486ee107562c72d02a46190c651e59a753c))
* **deps:** update dotnet monorepo ([#305](https://github.com/open-feature/dotnet-sdk/issues/305)) ([3955b16](https://github.com/open-feature/dotnet-sdk/commit/3955b1604d5dad9b67e01974d96d53d5cacb9aad))
* **deps:** update dotnet monorepo to 8.0.2 ([#319](https://github.com/open-feature/dotnet-sdk/issues/319)) ([94681f3](https://github.com/open-feature/dotnet-sdk/commit/94681f37821cc44388f0cd8898924cbfbcda0cd3))
* update release please config ([#304](https://github.com/open-feature/dotnet-sdk/issues/304)) ([c471c06](https://github.com/open-feature/dotnet-sdk/commit/c471c062cf70d78b67f597f468c62dbfbf0674d2))

## [2.0.0](https://github.com/open-feature/dotnet-sdk/compare/v1.5.0...v2.0.0) (2024-08-21)

Today we're announcing the release of the OpenFeature SDK for .NET, v2.0! This release contains several ergonomic improvements to the SDK, which .NET developers will appreciate. It also includes some performance optimizations brought to you by the latest .NET primitives.

For details and migration tips, check out: https://openfeature.dev/blog/dotnet-sdk-v2 

### ⚠ BREAKING CHANGES

* domain instead of client name ([#294](https://github.com/open-feature/dotnet-sdk/issues/294))
* internally maintain provider status ([#276](https://github.com/open-feature/dotnet-sdk/issues/276))
* add CancellationTokens, ValueTasks hooks ([#268](https://github.com/open-feature/dotnet-sdk/issues/268))
* Use same type for flag metadata and event metadata ([#241](https://github.com/open-feature/dotnet-sdk/issues/241))
* Enable nullable reference types ([#253](https://github.com/open-feature/dotnet-sdk/issues/253))

### 🐛 Bug Fixes

* Add missing error message when an error occurred ([#256](https://github.com/open-feature/dotnet-sdk/issues/256)) ([949d53c](https://github.com/open-feature/dotnet-sdk/commit/949d53cada68bee8e80d113357fa6df8d425d3c1))
* Should map metadata when converting from ResolutionDetails to FlagEvaluationDetails ([#282](https://github.com/open-feature/dotnet-sdk/issues/282)) ([2f8bd21](https://github.com/open-feature/dotnet-sdk/commit/2f8bd2179ec35f79cbbab77206de78dd9b0f58d6))


### ✨ New Features

* add CancellationTokens, ValueTasks hooks ([#268](https://github.com/open-feature/dotnet-sdk/issues/268)) ([33154d2](https://github.com/open-feature/dotnet-sdk/commit/33154d2ed6b0b27f4a86a5fbad440a784a89c881))
* back targetingKey with internal map ([#287](https://github.com/open-feature/dotnet-sdk/issues/287)) ([ccc2f7f](https://github.com/open-feature/dotnet-sdk/commit/ccc2f7fbd4e4f67eb03c2e6a07140ca31225da2c))
* domain instead of client name ([#294](https://github.com/open-feature/dotnet-sdk/issues/294)) ([4c0592e](https://github.com/open-feature/dotnet-sdk/commit/4c0592e6baf86d831fc7b39762c960ca0dd843a9))
* Drop net7 TFM ([#284](https://github.com/open-feature/dotnet-sdk/issues/284)) ([2dbe1f4](https://github.com/open-feature/dotnet-sdk/commit/2dbe1f4c95aeae501c8b5154b1ccefafa7df2632))
* internally maintain provider status ([#276](https://github.com/open-feature/dotnet-sdk/issues/276)) ([63faa84](https://github.com/open-feature/dotnet-sdk/commit/63faa8440cd650b0bd6c3ec009ad9bd78bc31f32))
* Use same type for flag metadata and event metadata ([#241](https://github.com/open-feature/dotnet-sdk/issues/241)) ([ac7d7de](https://github.com/open-feature/dotnet-sdk/commit/ac7d7debf50cef08668bcd9457d3f830b8718806))


### 🧹 Chore

* cleanup code ([#277](https://github.com/open-feature/dotnet-sdk/issues/277)) ([44cf586](https://github.com/open-feature/dotnet-sdk/commit/44cf586f96607716fb8b4464d81edfd6074f7376))
* **deps:** Project file cleanup and remove unnecessary dependencies ([#251](https://github.com/open-feature/dotnet-sdk/issues/251)) ([79def47](https://github.com/open-feature/dotnet-sdk/commit/79def47106b19b316b691fa195f7160ddcfb9a41))
* **deps:** update actions/upload-artifact action to v4.3.3 ([#263](https://github.com/open-feature/dotnet-sdk/issues/263)) ([7718649](https://github.com/open-feature/dotnet-sdk/commit/77186495cd3d567b0aabd418f23a65567656b54d))
* **deps:** update actions/upload-artifact action to v4.3.4 ([#278](https://github.com/open-feature/dotnet-sdk/issues/278)) ([15189f1](https://github.com/open-feature/dotnet-sdk/commit/15189f1c6f7eb0931036e022eed68f58a1110b5b))
* **deps:** update actions/upload-artifact action to v4.3.5 ([#291](https://github.com/open-feature/dotnet-sdk/issues/291)) ([00e99d6](https://github.com/open-feature/dotnet-sdk/commit/00e99d6c2208b304748d00a931f460d6d6aab4de))
* **deps:** update codecov/codecov-action action to v4 ([#227](https://github.com/open-feature/dotnet-sdk/issues/227)) ([11a0333](https://github.com/open-feature/dotnet-sdk/commit/11a03332726f07dd0327d222e6bd6e1843db460c))
* **deps:** update codecov/codecov-action action to v4.3.1 ([#267](https://github.com/open-feature/dotnet-sdk/issues/267)) ([ff9df59](https://github.com/open-feature/dotnet-sdk/commit/ff9df593400f92c016eee1a45bd7097da008d4dc))
* **deps:** update codecov/codecov-action action to v4.5.0 ([#272](https://github.com/open-feature/dotnet-sdk/issues/272)) ([281295d](https://github.com/open-feature/dotnet-sdk/commit/281295d2999e4d36c5a2078cbfdfe5e59f4652b2))
* **deps:** update dependency benchmarkdotnet to v0.14.0 ([#293](https://github.com/open-feature/dotnet-sdk/issues/293)) ([aec222f](https://github.com/open-feature/dotnet-sdk/commit/aec222fe1b1a5b52f8349ceb98c12b636eb155eb))
* **deps:** update dependency coverlet.collector to v6.0.2 ([#247](https://github.com/open-feature/dotnet-sdk/issues/247)) ([ab34c16](https://github.com/open-feature/dotnet-sdk/commit/ab34c16b513ddbd0a53e925baaccd088163fbcc8))
* **deps:** update dependency coverlet.msbuild to v6.0.2 ([#239](https://github.com/open-feature/dotnet-sdk/issues/239)) ([e654222](https://github.com/open-feature/dotnet-sdk/commit/e6542222827cc25cd5a1acc5af47ce55149c0623))
* **deps:** update dependency dotnet-sdk to v8.0.204 ([#261](https://github.com/open-feature/dotnet-sdk/issues/261)) ([8f82645](https://github.com/open-feature/dotnet-sdk/commit/8f8264520814a42b7ed2af8f70340e7673259b6f))
* **deps:** update dependency dotnet-sdk to v8.0.301 ([#271](https://github.com/open-feature/dotnet-sdk/issues/271)) ([acd0385](https://github.com/open-feature/dotnet-sdk/commit/acd0385641e114a16d0ee56e3a143baa7d3c0535))
* **deps:** update dependency dotnet-sdk to v8.0.303 ([#275](https://github.com/open-feature/dotnet-sdk/issues/275)) ([871dcac](https://github.com/open-feature/dotnet-sdk/commit/871dcacc94fa2abb10434616c469cad6f674f07a))
* **deps:** update dependency dotnet-sdk to v8.0.400 ([#295](https://github.com/open-feature/dotnet-sdk/issues/295)) ([bb4f352](https://github.com/open-feature/dotnet-sdk/commit/bb4f3526c2c2c2ca48ae61e883d6962847ebc5a6))
* **deps:** update dependency githubactionstestlogger to v2.4.1 ([#274](https://github.com/open-feature/dotnet-sdk/issues/274)) ([46c2b15](https://github.com/open-feature/dotnet-sdk/commit/46c2b153c848bd3a500b828ddb89bd3b07753bf1))
* **deps:** update dependency microsoft.net.test.sdk to v17.10.0 ([#273](https://github.com/open-feature/dotnet-sdk/issues/273)) ([581ff81](https://github.com/open-feature/dotnet-sdk/commit/581ff81c7b1840c34840229bf20444c528c64cc6))
* **deps:** update dotnet monorepo ([#218](https://github.com/open-feature/dotnet-sdk/issues/218)) ([bc8301d](https://github.com/open-feature/dotnet-sdk/commit/bc8301d1c54e0b48ede3235877d969f28d61fb29))
* **deps:** update xunit-dotnet monorepo ([#262](https://github.com/open-feature/dotnet-sdk/issues/262)) ([43f14cc](https://github.com/open-feature/dotnet-sdk/commit/43f14cca072372ecacec89a949c85f763c1ee7b4))
* **deps:** update xunit-dotnet monorepo ([#279](https://github.com/open-feature/dotnet-sdk/issues/279)) ([fb1cc66](https://github.com/open-feature/dotnet-sdk/commit/fb1cc66440dd6bdbbef1ac1f85bf3228b80073af))
* **deps:** update xunit-dotnet monorepo to v2.8.1 ([#266](https://github.com/open-feature/dotnet-sdk/issues/266)) ([a7b6d85](https://github.com/open-feature/dotnet-sdk/commit/a7b6d8561716763f324325a8803b913c4d69c044))
* Enable nullable reference types ([#253](https://github.com/open-feature/dotnet-sdk/issues/253)) ([5a5312c](https://github.com/open-feature/dotnet-sdk/commit/5a5312cc082ccd880b65165135e05b4f3b035df7))
* in-memory UpdateFlags to UpdateFlagsAsync ([#298](https://github.com/open-feature/dotnet-sdk/issues/298)) ([390205a](https://github.com/open-feature/dotnet-sdk/commit/390205a41d29d786b5f41b0d91f34ec237276cb4))
* prompt 2.0 ([9b9c3fd](https://github.com/open-feature/dotnet-sdk/commit/9b9c3fd09c27b191104d7ceaa726b6edd71fcd06))
* Support for determining spec support for the repo ([#270](https://github.com/open-feature/dotnet-sdk/issues/270)) ([67a1a0a](https://github.com/open-feature/dotnet-sdk/commit/67a1a0aea95ee943976990b1d1782e4061300b50))

## [1.5.0](https://github.com/open-feature/dotnet-sdk/compare/v1.4.1...v1.5.0) (2024-03-12)


### 🐛 Bug Fixes

* Add targeting key ([#231](https://github.com/open-feature/dotnet-sdk/issues/231)) ([d792b32](https://github.com/open-feature/dotnet-sdk/commit/d792b32c567b3c4ecded3fb8aab7ad9832048dcc))
* Fix NU1009 reference assembly warning ([#222](https://github.com/open-feature/dotnet-sdk/issues/222)) ([7eebcdd](https://github.com/open-feature/dotnet-sdk/commit/7eebcdda123f9a432a8462d918b7454a26d3e389))
* invalid editorconfig ([#244](https://github.com/open-feature/dotnet-sdk/issues/244)) ([3c00757](https://github.com/open-feature/dotnet-sdk/commit/3c0075738c07e0bb2bc9875be9037f7ccbf90ac5))


### ✨ New Features

* Flag metadata ([#223](https://github.com/open-feature/dotnet-sdk/issues/223)) ([fd0a541](https://github.com/open-feature/dotnet-sdk/commit/fd0a54110866f3245152b28b64dedd286a752f64))
* implement in-memory provider ([#232](https://github.com/open-feature/dotnet-sdk/issues/232)) ([1082094](https://github.com/open-feature/dotnet-sdk/commit/10820947f3d1ad0f710bccf5990b7c993956ff51))


### 🧹 Chore

* bump spec version badge ([#246](https://github.com/open-feature/dotnet-sdk/issues/246)) ([ebf5552](https://github.com/open-feature/dotnet-sdk/commit/ebf55522146dad0432792bdc8cdf8772aae7d627))
* cleanup unused usings 🧹  ([#240](https://github.com/open-feature/dotnet-sdk/issues/240)) ([cdc1bee](https://github.com/open-feature/dotnet-sdk/commit/cdc1beeb00b50d47658b5fa9f053385afa227a94))
* **deps:** update actions/upload-artifact action to v4.3.0 ([#203](https://github.com/open-feature/dotnet-sdk/issues/203)) ([0a7e98d](https://github.com/open-feature/dotnet-sdk/commit/0a7e98daf7d5f66f5aa8d97146e8444aa2685a33))
* **deps:** update actions/upload-artifact action to v4.3.1 ([#233](https://github.com/open-feature/dotnet-sdk/issues/233)) ([cfaf1c8](https://github.com/open-feature/dotnet-sdk/commit/cfaf1c8350a1d6754e2cfadc5daaddf2a40524e9))
* **deps:** update codecov/codecov-action action to v3.1.5 ([#209](https://github.com/open-feature/dotnet-sdk/issues/209)) ([a509b1f](https://github.com/open-feature/dotnet-sdk/commit/a509b1fb1d360ea0ac25e515ef5c7827996d4b4e))
* **deps:** update codecov/codecov-action action to v3.1.6 ([#226](https://github.com/open-feature/dotnet-sdk/issues/226)) ([a577a80](https://github.com/open-feature/dotnet-sdk/commit/a577a80fc9b93fa5ddced6452da1e74f3bf9afc7))
* **deps:** update dependency coverlet.collector to v6.0.1 ([#238](https://github.com/open-feature/dotnet-sdk/issues/238)) ([f2cb67b](https://github.com/open-feature/dotnet-sdk/commit/f2cb67bf40b96981f76da31242c591aeb1a2d2f5))
* **deps:** update dependency fluentassertions to v6.12.0 ([#215](https://github.com/open-feature/dotnet-sdk/issues/215)) ([2c237df](https://github.com/open-feature/dotnet-sdk/commit/2c237df6e0ad278ddd8a51add202b797bf81374e))
* **deps:** update dependency microsoft.net.test.sdk to v17.8.0 ([#216](https://github.com/open-feature/dotnet-sdk/issues/216)) ([4cb3ae0](https://github.com/open-feature/dotnet-sdk/commit/4cb3ae09375ad5f172b2e0673c9c30678939e9fd))
* **deps:** update dependency nsubstitute to v5.1.0 ([#217](https://github.com/open-feature/dotnet-sdk/issues/217)) ([3be76cd](https://github.com/open-feature/dotnet-sdk/commit/3be76cd562bbe942070e3c532edf40694e098440))
* **deps:** update dependency openfeature.contrib.providers.flagd to v0.1.8 ([#211](https://github.com/open-feature/dotnet-sdk/issues/211)) ([c1aece3](https://github.com/open-feature/dotnet-sdk/commit/c1aece35c34e40ec911622e89882527d6815d267))
* **deps:** update xunit-dotnet monorepo ([#236](https://github.com/open-feature/dotnet-sdk/issues/236)) ([fa25ece](https://github.com/open-feature/dotnet-sdk/commit/fa25ece0444c04e2c0a12fca21064920bc09159a))
* Enable Central Package Management (CPM) ([#178](https://github.com/open-feature/dotnet-sdk/issues/178)) ([249a0a8](https://github.com/open-feature/dotnet-sdk/commit/249a0a8b35d0205117153e8f32948d65b7754b44))
* Enforce coding styles on build ([#242](https://github.com/open-feature/dotnet-sdk/issues/242)) ([64699c8](https://github.com/open-feature/dotnet-sdk/commit/64699c8c0b5598b71fa94041797bc98d3afc8863))
* More sln cleanup ([#206](https://github.com/open-feature/dotnet-sdk/issues/206)) ([bac3d94](https://github.com/open-feature/dotnet-sdk/commit/bac3d9483817a330044c8a13a4b3e1ffa296e009))
* SourceLink is built-in for .NET SDK 8.0.100+ ([#198](https://github.com/open-feature/dotnet-sdk/issues/198)) ([45e2c86](https://github.com/open-feature/dotnet-sdk/commit/45e2c862fd96092c3d20ddc5dfba46febfe802c8))
* Sync ci.yml with contrib repo ([#196](https://github.com/open-feature/dotnet-sdk/issues/196)) ([130654b](https://github.com/open-feature/dotnet-sdk/commit/130654b9ae97a20c6d8964a9c0c0e0188209db55))
* Sync release.yml with ci.yml following [#173](https://github.com/open-feature/dotnet-sdk/issues/173) ([#195](https://github.com/open-feature/dotnet-sdk/issues/195)) ([eba8848](https://github.com/open-feature/dotnet-sdk/commit/eba8848cb61f28b64f4a021f1534d300fcddf4eb))


### 📚 Documentation

* fix hook ecosystem link ([#229](https://github.com/open-feature/dotnet-sdk/issues/229)) ([cc6c404](https://github.com/open-feature/dotnet-sdk/commit/cc6c404504d9db1c234cf5642ee0c5595868774f))
* update the feature table key ([f8724cd](https://github.com/open-feature/dotnet-sdk/commit/f8724cd625a1f9edb33cd208aac70db3766593f1))

## [1.4.1](https://github.com/open-feature/dotnet-sdk/compare/v1.4.0...v1.4.1) (2024-01-23)


### 📚 Documentation

* add release please tag twice ([b34fe78](https://github.com/open-feature/dotnet-sdk/commit/b34fe78636dfb6b2c334a68c44ae57b72b04acbc))
* add release please version range ([#201](https://github.com/open-feature/dotnet-sdk/issues/201)) ([aa35e25](https://github.com/open-feature/dotnet-sdk/commit/aa35e253b58755b4e0b75a4b272e5a368cb5566c))
* fix release please tag ([8b1c9d2](https://github.com/open-feature/dotnet-sdk/commit/8b1c9d2cc26c086dcdb2434ba8646ffc07c3d063))
* update readme to be pure markdown ([#199](https://github.com/open-feature/dotnet-sdk/issues/199)) ([33db9d9](https://github.com/open-feature/dotnet-sdk/commit/33db9d925478f8249e9fea7465998c8ec8da686b))
* update release please tags ([8dcb824](https://github.com/open-feature/dotnet-sdk/commit/8dcb824e2b39e14e3b7345cd0d89f7660ca798cd))

## [1.4.0](https://github.com/open-feature/dotnet-sdk/compare/v1.3.1...v1.4.0) (2024-01-23)


### 🐛 Bug Fixes

* Fix ArgumentOutOfRangeException for empty hooks ([#187](https://github.com/open-feature/dotnet-sdk/issues/187)) ([950775b](https://github.com/open-feature/dotnet-sdk/commit/950775b65093e22ce209c947bf9da71b17ad7387))
* More robust shutdown/cleanup/reset ([#188](https://github.com/open-feature/dotnet-sdk/issues/188)) ([a790f78](https://github.com/open-feature/dotnet-sdk/commit/a790f78c32b8500ce27d04d906c98d1de2afd2b4))
* Remove upper-bound version constraint from SCI ([#171](https://github.com/open-feature/dotnet-sdk/issues/171)) ([8f8b661](https://github.com/open-feature/dotnet-sdk/commit/8f8b661f1cac6a4f1c51eb513999372d30a4f726))


### ✨ New Features

* Add dx to catch ConfigureAwait(false) ([#152](https://github.com/open-feature/dotnet-sdk/issues/152)) ([9c42d4a](https://github.com/open-feature/dotnet-sdk/commit/9c42d4afa9139094e0316bbe1306ae4856b7d013))
* add support for eventing ([#166](https://github.com/open-feature/dotnet-sdk/issues/166)) ([f5fc1dd](https://github.com/open-feature/dotnet-sdk/commit/f5fc1ddadc11f712ae0893cde815e7a1c6fe2c1b))
* Add support for provider shutdown and status. ([#158](https://github.com/open-feature/dotnet-sdk/issues/158)) ([24c3441](https://github.com/open-feature/dotnet-sdk/commit/24c344163423973b54a06b73648ba45b944589ee))


### 🧹 Chore

* Add GitHub Actions logger for CI ([#174](https://github.com/open-feature/dotnet-sdk/issues/174)) ([c1a189a](https://github.com/open-feature/dotnet-sdk/commit/c1a189a5cff7106d37f0a45dd5824f18e7ec0cd6))
* add placeholder eventing and shutdown sections ([#156](https://github.com/open-feature/dotnet-sdk/issues/156)) ([5dfea29](https://github.com/open-feature/dotnet-sdk/commit/5dfea29bb3d01f6c8640de321c4fde52f283a1c0))
* Add support for GitHub Packages ([#173](https://github.com/open-feature/dotnet-sdk/issues/173)) ([26cd5cd](https://github.com/open-feature/dotnet-sdk/commit/26cd5cdd613577c53ae79b889d1cf2d89262236f))
* Adding sealed keyword to classes ([#191](https://github.com/open-feature/dotnet-sdk/issues/191)) ([1a14f6c](https://github.com/open-feature/dotnet-sdk/commit/1a14f6cd6c8988756a2cf2da1137a739e8d960f8))
* **deps:** update actions/checkout action to v4 ([#144](https://github.com/open-feature/dotnet-sdk/issues/144)) ([90d9d02](https://github.com/open-feature/dotnet-sdk/commit/90d9d021b227fba626bb99454cb7c0f7fef2d8d8))
* **deps:** update actions/setup-dotnet action to v4 ([#162](https://github.com/open-feature/dotnet-sdk/issues/162)) ([0b0bb10](https://github.com/open-feature/dotnet-sdk/commit/0b0bb10419f836d9cc276fe8ac3c71c9214420ef))
* **deps:** update dependency dotnet-sdk to v7.0.404 ([#148](https://github.com/open-feature/dotnet-sdk/issues/148)) ([e8ca1da](https://github.com/open-feature/dotnet-sdk/commit/e8ca1da9ed63df9685ec49a9569e0ec99ba0b3b9))
* **deps:** update github/codeql-action action to v3 ([#163](https://github.com/open-feature/dotnet-sdk/issues/163)) ([c85e93e](https://github.com/open-feature/dotnet-sdk/commit/c85e93e9c9a97083660f9062c38dcbf6d64a3ad6))
* fix alt text for NuGet on the readme ([2cbdba8](https://github.com/open-feature/dotnet-sdk/commit/2cbdba80d836f8b7850e8dc5f1f1790ef2ed1aca))
* Fix FieldCanBeMadeReadOnly ([#183](https://github.com/open-feature/dotnet-sdk/issues/183)) ([18a092a](https://github.com/open-feature/dotnet-sdk/commit/18a092afcab1b06c25f3b825a6130d22226790fc))
* Fix props to support more than one project ([#177](https://github.com/open-feature/dotnet-sdk/issues/177)) ([f47cf07](https://github.com/open-feature/dotnet-sdk/commit/f47cf07420cdcb6bc74b0455898b7b17a144daf3))
* minor formatting cleanup ([#168](https://github.com/open-feature/dotnet-sdk/issues/168)) ([d0c25af](https://github.com/open-feature/dotnet-sdk/commit/d0c25af7df5176d10088c148eac35b0034536e04))
* Reduce dependency on MEL -&gt; MELA ([#176](https://github.com/open-feature/dotnet-sdk/issues/176)) ([a6062fe](https://github.com/open-feature/dotnet-sdk/commit/a6062fe2b9f0d83490c7ce900e837863521f5f55))
* remove duplicate eventing section in readme ([1efe09d](https://github.com/open-feature/dotnet-sdk/commit/1efe09da3948d5dfd7fd9f1c7a040fc5c2cbe833))
* remove test sleeps, fix flaky test ([#194](https://github.com/open-feature/dotnet-sdk/issues/194)) ([f2b9b03](https://github.com/open-feature/dotnet-sdk/commit/f2b9b03eda5f6d6b4a738f761702cd9d9a105e76))
* revert breaking setProvider ([#190](https://github.com/open-feature/dotnet-sdk/issues/190)) ([2919c2f](https://github.com/open-feature/dotnet-sdk/commit/2919c2f4f2a4629fccd1a50b1885375006445b96))
* update spec release link ([a2f70eb](https://github.com/open-feature/dotnet-sdk/commit/a2f70ebd68357156f9045fc6e94845a53ffd204a))
* updated readme for inclusion in the docs ([6516866](https://github.com/open-feature/dotnet-sdk/commit/6516866ec7601a7adaa4dc6b517c9287dec54fca))


### 📚 Documentation

* Add README.md to the nuget package ([#164](https://github.com/open-feature/dotnet-sdk/issues/164)) ([b6b0ee2](https://github.com/open-feature/dotnet-sdk/commit/b6b0ee2b61a9b0b973b913b53887badfa0c5a3de))
* fixed the contrib url on the readme ([9d8939e](https://github.com/open-feature/dotnet-sdk/commit/9d8939ef57a3be4ee220bd21f36b166887b2c30b))
* remove duplicate a tag from readme ([2687cf0](https://github.com/open-feature/dotnet-sdk/commit/2687cf0663e20aa2dd113569cbf177833639cbbd))
* update README.md ([#155](https://github.com/open-feature/dotnet-sdk/issues/155)) ([b62e21f](https://github.com/open-feature/dotnet-sdk/commit/b62e21f76964e7f6f7456f720814de0997232d71))


### 🔄 Refactoring

* Add TFMs for net{6,7,8}.0 ([#172](https://github.com/open-feature/dotnet-sdk/issues/172)) ([cf2baa8](https://github.com/open-feature/dotnet-sdk/commit/cf2baa8a6b4328f1aa346bbea91160aa2e5f3a8d))

## [1.3.1](https://github.com/open-feature/dotnet-sdk/compare/v1.3.0...v1.3.1) (2023-09-19)


### 🐛 Bug Fixes

* deadlocks in client applications ([#150](https://github.com/open-feature/dotnet-sdk/issues/150)) ([17a7772](https://github.com/open-feature/dotnet-sdk/commit/17a7772c0dad9c68a4a0e0e272fe32ce3bfe0cff))


### 🧹 Chore

* **deps:** update dependency dotnet-sdk to v7.0.306 ([#135](https://github.com/open-feature/dotnet-sdk/issues/135)) ([15473b6](https://github.com/open-feature/dotnet-sdk/commit/15473b6c3ab969ca660b7f3a98e1999373517b42))
* **deps:** update dependency dotnet-sdk to v7.0.400 ([#139](https://github.com/open-feature/dotnet-sdk/issues/139)) ([ecc9707](https://github.com/open-feature/dotnet-sdk/commit/ecc970701ff46815d0116417232f7c6ea670bdef))
* update rp config (emoji) ([f921dc6](https://github.com/open-feature/dotnet-sdk/commit/f921dc699a358070568be93027680d49e0f7cb8e))


### 📚 Documentation

* Update README.md ([#147](https://github.com/open-feature/dotnet-sdk/issues/147)) ([3da02e6](https://github.com/open-feature/dotnet-sdk/commit/3da02e67a6e1e11af72fbd38aa42215b41b4e33b))

## [1.3.0](https://github.com/open-feature/dotnet-sdk/compare/v1.2.0...v1.3.0) (2023-07-14)


### Features

* Support for name client to given provider ([#129](https://github.com/open-feature/dotnet-sdk/issues/129)) ([3f765c6](https://github.com/open-feature/dotnet-sdk/commit/3f765c6fb4ccd651de2d4f46e1fec38cd26610fe))


### Bug Fixes

* max System.Collections.Immutable version ++ ([#137](https://github.com/open-feature/dotnet-sdk/issues/137)) ([55c5e8e](https://github.com/open-feature/dotnet-sdk/commit/55c5e8e5c9e7667afb84d0b7946234e5274d4924))

## [1.2.0](https://github.com/open-feature/dotnet-sdk/compare/v1.1.0...v1.2.0) (2023-02-14)


### Features

* split errors to classes by types ([#115](https://github.com/open-feature/dotnet-sdk/issues/115)) ([5f348f4](https://github.com/open-feature/dotnet-sdk/commit/5f348f46f2d9a5578a0db951bd78508ab74cabc0))

## [1.1.0](https://github.com/open-feature/dotnet-sdk/compare/v1.0.1...v1.1.0) (2023-01-18)


### Features

* add STATIC, CACHED reasons ([#101](https://github.com/open-feature/dotnet-sdk/issues/101)) ([7cc7ab4](https://github.com/open-feature/dotnet-sdk/commit/7cc7ab46fc20a97c9f4398f6d1fe80e43db514e1))
* include net7 in the test suit ([#97](https://github.com/open-feature/dotnet-sdk/issues/97)) ([594d5f2](https://github.com/open-feature/dotnet-sdk/commit/594d5f21f735473bf8585f9f6de67d758b1bf12c))
* Make IFeatureClient interface public. ([#102](https://github.com/open-feature/dotnet-sdk/issues/102)) ([5a09c4f](https://github.com/open-feature/dotnet-sdk/commit/5a09c4f38c15b47b6e1aa62a57ea4f49c08fab77))

## [1.0.1](https://github.com/open-feature/dotnet-sdk/compare/v1.0.0...v1.0.1) (2022-10-28)


### Bug Fixes

* correct version range on logging ([#89](https://github.com/open-feature/dotnet-sdk/issues/89)) ([9443239](https://github.com/open-feature/dotnet-sdk/commit/9443239adeb3144c6f683faf400dddf5ac493628))

## [1.0.0](https://github.com/open-feature/dotnet-sdk/compare/v0.5.0...v1.0.0) (2022-10-21)


### Miscellaneous Chores

* release 1.0.0 ([#85](https://github.com/open-feature/dotnet-sdk/issues/85)) ([79c0d8d](https://github.com/open-feature/dotnet-sdk/commit/79c0d8d0aa07f7aa69023de3437c3774df507e53))

## [0.5.0](https://github.com/open-feature/dotnet-sdk/compare/v0.4.0...v0.5.0) (2022-10-16)


### ⚠ BREAKING CHANGES

* rename OpenFeature class to API and ns to OpenFeature (#82)

### Features

* rename OpenFeature class to API and ns to OpenFeature ([#82](https://github.com/open-feature/dotnet-sdk/issues/82)) ([6090bd9](https://github.com/open-feature/dotnet-sdk/commit/6090bd971817cc6cc8b74487b2850d8e99a2c94d))

## [0.4.0](https://github.com/open-feature/dotnet-sdk/compare/v0.3.0...v0.4.0) (2022-10-12)


### ⚠ BREAKING CHANGES

* Thread safe hooks, provider, and context (#79)
* Implement builders and immutable contexts. (#77)

### Features

* Implement builders and immutable contexts. ([#77](https://github.com/open-feature/dotnet-sdk/issues/77)) ([d980a94](https://github.com/open-feature/dotnet-sdk/commit/d980a94402bdb94cae4c60c1809f1579be7f5449))
* Thread safe hooks, provider, and context ([#79](https://github.com/open-feature/dotnet-sdk/issues/79)) ([609016f](https://github.com/open-feature/dotnet-sdk/commit/609016fc86f8eee8d848a9227b57aaef0d9b85b0))

## [0.3.0](https://github.com/open-feature/dotnet-sdk/compare/v0.2.3...v0.3.0) (2022-09-28)


### ⚠ BREAKING CHANGES

* ErrorType as enum, add ErrorMessage string (#72)

### Features

* ErrorType as enum, add ErrorMessage string ([#72](https://github.com/open-feature/dotnet-sdk/issues/72)) ([e7ab498](https://github.com/open-feature/dotnet-sdk/commit/e7ab49866bd83d7b146059b0c22944a7db6956b4))

## [0.2.3](https://github.com/open-feature/dotnet-sdk/compare/v0.2.2...v0.2.3) (2022-09-22)


### Bug Fixes

* add dir to publish ([#69](https://github.com/open-feature/dotnet-sdk/issues/69)) ([6549dbb](https://github.com/open-feature/dotnet-sdk/commit/6549dbb4f3a525a70cebdc9a63661ce6eaba9266))

## [0.2.2](https://github.com/open-feature/dotnet-sdk/compare/v0.2.1...v0.2.2) (2022-09-22)


### Bug Fixes

* change NUGET_API_KEY -> NUGET_TOKEN ([#67](https://github.com/open-feature/dotnet-sdk/issues/67)) ([87c99b2](https://github.com/open-feature/dotnet-sdk/commit/87c99b2128d50d72b54cb27e2f866f1edb0cd0d3))

## [0.2.1](https://github.com/open-feature/dotnet-sdk/compare/v0.2.0...v0.2.1) (2022-09-22)


### Bug Fixes

* substitute version number into filename when pushing package ([#65](https://github.com/open-feature/dotnet-sdk/issues/65)) ([8c8500c](https://github.com/open-feature/dotnet-sdk/commit/8c8500c71edb84c256b177c40815a34607adb682))

## [0.2.0](https://github.com/open-feature/dotnet-sdk/compare/v0.1.5...v0.2.0) (2022-09-22)


### ⚠ BREAKING CHANGES

* use correct path to extra file (#63)
* Rename namespace from OpenFeature.SDK to OpenFeatureSDK (#62)

### Bug Fixes

* Rename namespace from OpenFeature.SDK to OpenFeatureSDK ([#62](https://github.com/open-feature/dotnet-sdk/issues/62)) ([430ffc0](https://github.com/open-feature/dotnet-sdk/commit/430ffc0a3afc871772286241d39a613c91298da5))
* use correct path to extra file ([#63](https://github.com/open-feature/dotnet-sdk/issues/63)) ([ee39839](https://github.com/open-feature/dotnet-sdk/commit/ee398399d9371517c4b03b55a93619776ecd3a92))
