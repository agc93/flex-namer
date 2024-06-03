---
weight: 100
title: "Introduction"
description: "An introduction to the FlexNamer project"
icon: "circle"
date: "2024-03-04T02:17:43+10:00"
lastmod: "2024-03-04T02:17:43+10:00"
draft: false
toc: true
---

FlexNamer is a small utility designed for automating repeatable file renaming tasks.

There are already great tools out there for batch renaming files (my personal tool of choice is the PowerRename in PowerToys), but my workflow often involves renaming batches of files from external sources with the same format into a new naming format.

Up until now, I just had a text file with a bunch of pre-made regexes I would drop into PowerRename but that felt sub-optimal, and I like over-engineering things so here we are.

{{< alert icon="ℹ" context="info" text="These docs are pretty basic, and the tool as a whole is pretty unpolished. This was just a small utility I threw together for myself that I'm publishing because someone out there might find this tool or the code useful." />}}

#### Couldn't this just be a PowerShell script?

**Yes**. Yes, it could, but I felt that was hacky and didn't like having a separate script for each data source that I would need to switch between so I built this instead.

#### How does it work?

At a high level, when FlexNamer runs, it will rename a given file (or files) by looking for a matching "format". Formats are simple types, implemented in plugins, that represent a 1:1 mapping of input to output names.

When an existing file is supported by a loaded format, that format will transform the name however you have written it, and the file will be renamed to whatever the format outputs. Simple as that.

Formats are designed to be built in small plugins for the main library, making it trivial to maintain any number of naming patterns for any number of data sources.