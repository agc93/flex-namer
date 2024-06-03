---
weight: 400
title: "Formats"
description: "Building naming formats for FlexNamer"
icon: integration_instructions
lead: ""
date: 2023-01-21T16:13:15+00:00
lastmod: 2023-01-21T16:13:15+00:00
draft: false
images: []
---

## Introduction

FlexNamer is built on a simple plugin architecture that keeps individual naming formats in separate plugins so that FlexNamer itself doesn't need to know about every possible name variation you might want to use.

As well as keeping the application simple and allowing you to build your own naming formats for your exact use case, this also makes it possible to share naming formats if you are renaming files from a commonly used data source.

## Building plugins

Building FlexNamer plugins is pretty simple:

- Create a class library targeting .NET 8
- Add a reference to `FlexNamer.Core`
- Add at least one implementation of the `INamingFormat` interface
- Put your compiled library in a ([correctly named](#plugin-folders)) folder beside the FlexNamer binary

That's it! When you run FlexNamer, it checks the current directory, and the directory containing the executable, for a folder named "Formats" and load any plugin it finds there.

### Plugin Folders

For your plugins to be identified and loaded by FlexNamer, they need to be in a folder named the same as the assembly itself. So if your class library outputs a `MyFileNameFormat.dll`, then it should be in a folder at `Formats/MyFileNameFormat/` like below:

```text
│   FlexNamer.exe
└───Formats
	└───MyFileNameFormat
		│   // trimmed for brevity
		└───MyFileNameFormat.dll
```
