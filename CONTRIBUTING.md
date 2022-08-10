# Contributing to the OpenFeature project

## Development

You can contribute to this project from a Windows, macOS or Linux machine.

On all platforms, the minimum requirements are:

* Git client and command line tools.
* .netstandard 2.0 or higher capable dotnet sdk (.Net Framework 4.6.2 or higher/.Net Core 3 or higher).

### Linux or MacOS

* Jetbrains Rider 2022.2+ or Visual Studio 2022+ for Mac or Visual Studio Code

### Windows

* Jetbrains Rider 2022.2+ or Visual Studio 2022+ or Visual Studio Code
* .NET Framework 4.6.2+

## Pull Request

All contributions to the OpenFeature project are welcome via GitHub pull requests.

To create a new PR, you will need to first fork the GitHub repository and clone upstream.

```bash
git clone https://github.com/open-feature/dotnet-sdk.git openfeature-dotnet-sdk
```

Navigate to the repository folder
```bash
cd openfeature-dotnet-sdk
```

Add your fork as an origin
```bash
git remote add fork https://github.com/YOUR_GITHUB_USERNAME/dotnet-sdk.git
```

Makes sure your development environment is all setup by building and testing
```bash
dotnet build
dotnet test
```

To start working on a new feature or bugfix, create a new branch and start working on it.

```bash
git checkout -b feat/NAME_OF_FEATURE
# Make your changes
git commit
git push fork feat/NAME_OF_FEATURE
```

Open a pull request against the main dotnet-sdk repository.

### How to Receive Comments

* If the PR is not ready for review, please mark it as
  [`draft`](https://github.blog/2019-02-14-introducing-draft-pull-requests/).
* Make sure all required CI checks are clear.
* Submit small, focused PRs addressing a single concern/issue.
* Make sure the PR title reflects the contribution.
* Write a summary that helps understand the change.
* Include usage examples in the summary, where applicable.

### How to Get PRs Merged

A PR is considered to be **ready to merge** when:

* Major feedbacks are resolved.
* It has been open for review for at least one working day. This gives people
  reasonable time to review.
* Trivial change (typo, cosmetic, doc, etc.) doesn't have to wait for one day.
* Urgent fix can take exception as long as it has been actively communicated.

Any Maintainer can merge the PR once it is **ready to merge**. Note, that some
PRs may not be merged immediately if the repo is in the process of a release and
the maintainers decided to defer the PR to the next release train.

If a PR has been stuck (e.g. there are lots of debates and people couldn't agree
on each other), the owner should try to get people aligned by:

* Consolidating the perspectives and putting a summary in the PR. It is
  recommended to add a link into the PR description, which points to a comment
  with a summary in the PR conversation.
* Tagging subdomain experts (by looking at the change history) in the PR asking
  for suggestion.
* Reaching out to more people on the [CNCF OpenFeature Slack channel](https://cloud-native.slack.com/archives/C0344AANLA1).
* Stepping back to see if it makes sense to narrow down the scope of the PR or
  split it up.
* If none of the above worked and the PR has been stuck for more than 2 weeks,
  the owner should bring it to the OpenFeatures [meeting](README.md#contributing).

## Design Choices

As with other OpenFeature SDKs, dotnet-sdk follows the
[openfeature-specification](https://github.com/open-feature/spec).

## Style Guide

This project includes a [`.editorconfig`](./.editorconfig) file which is
supported by all the IDEs/editor mentioned above. It works with the IDE/editor
only and does not affect the actual build of the project.
