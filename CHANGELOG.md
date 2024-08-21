# Changelog

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
