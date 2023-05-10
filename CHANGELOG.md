# Changelog

## v2.0.0 - 2023.05.10
- update react-router-dom library to version 6

## v1.6.15 - 2022.09.05
- Replace `moment.js` with `date-fns`
- Fast date filters (front) now work in local time

## v1.6.8 - 2022.07.06

- `SkbKontur.Cassandra.DistributedTaskQueue.Monitoring` explicitly sets `track_total_hits=true` when searching in elasticsearch (since v7.0 it defaults to 10000 - https://www.elastic.co/guide/en/elasticsearch/reference/7.17/breaking-changes-7.0.html#track-total-hits-10000-default)

## v1.6.6 - 2022.06.15

- Update `Elasticsearch.NET` to 7.17.2
- Monitoring compatibility with elasticsearch v6.x and v7.x

## v1.5.27 - 2022.06.15

- Update packages:
  - `@skbkontur/react-ui` 3.3.1 => 4.1.0
  - `@skbkontur/react-icons` 5.1.0 => 5.2.4
  - `@skbkontur/react-ui-validations` 1.4.0 => 1.8.3

## v1.5.X - 2021.11.23

- Update Elasticsearch.NET to v6.8.9

## v1.4.X - 2021.08.06

- Reporting pending tasks count by topic and name

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
