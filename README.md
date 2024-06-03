# FlexNamer

A lightweight extensible utility for renaming batches of files according to pre-defined repeatable naming formats.

## Introduction

There are already great tools out there for batch renaming files (my personal tool of choice is the PowerRename in PowerToys), but my workflow often involves renaming batches of files from external sources with the same format into a new naming format.

#### Couldn't this just be a PowerShell script?

**Yes**. Yes, it could, but I felt that was hacky and didn't like having a separate script for each data source that I would need to switch between so I built this instead.

#### How does it work?

At a high level, when FlexNamer runs, it will rename a given file (or files) by looking for a matching "format". Formats are simple types, implemented in plugins, that represent a 1:1 mapping of input to output names.

When an existing file is supported by a loaded format, that format will transform the name however you have written it, and the file will be renamed to whatever the format outputs. Simple as that.

Formats are designed to be built in small plugins for the main library, making it trivial to maintain any number of naming patterns for any number of data sources.

## Building

Run `dotnet tool restore` then `dotnet cake` to build FlexNamer.