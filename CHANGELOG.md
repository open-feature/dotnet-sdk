# Changelog

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
