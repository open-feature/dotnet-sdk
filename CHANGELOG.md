# Changelog

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
