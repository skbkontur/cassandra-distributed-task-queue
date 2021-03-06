# Changelog

## v1.3.3 - 2021.06.01
- Add support for dark theme using react-ui ThemeContext
- Update supported peerDependencies versions range

## v1.2.7 - 2021.03.26
- Fix bug with TaskDataJsonSerializer: add TimeGuid and Timestamp converters

## v1.2.4 - 2021.03.14
- Update dependencies.
- Run tests against net5.0 tfm.

## v1.2.2 - 2021.03.05
- Add cancelation token to the `RtqPeriodicJobRunner`.

## v1.1.11 - 2021.02.25
- Update `@skbkontur/react-ui` package
- Remove deprecated react lifecycle methods

## v1.0.8 - 2020.11.23
- More customisation for Graphite profiler prefixes.

## v1.0.3 - 2020.09.24
- Publish `@skbkontur/cassandra-distributed-task-queue-ui` package.
- More reasonable Elasticsearch indexer default settings.

## v0.1.4 - 2020.09.01
- Extract this module from [internal git repository](https://git.skbkontur.ru/edi/edi/tree/f34434a2a859ad584c141329a94f0bee61eb005f/RemoteTaskQueue).
- Use [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning) to automate generation of assembly and nuget package versions.
- Use [SourceLink](https://github.com/dotnet/sourcelink) to help ReSharper decompiler show actual code.
