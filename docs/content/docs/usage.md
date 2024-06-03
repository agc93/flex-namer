---
weight: 300
title: "Command-Line Usage"
description: "Get more familiar with the FlexNamer CLI"
icon: "terminal"
date: "2024-03-04T02:17:43+10:00"
lastmod: "2024-03-04T02:17:43+10:00"
draft: false
toc: true
---

{{< alert icon="💡" context="success" text="The FlexNamer CLI supports both Windows and Linux! Just download the right binary for your target platform." />}}

## Getting Help

You can use the `-h`/`--help` option to get help for the available commands and options when using the CLI:

```
USAGE:
    FlexNamer rename [DIR] [OPTIONS]

ARGUMENTS:
    [DIR]    The folder to search for file(s) to rename

OPTIONS:
    -h, --help               Prints help information                                                                                                          
    -f, --filter             Wildcard pattern to match file(s) to, such as "*.txt" or "Export*.*"
    -d, --directory          Uses the current directory name instead of the input file name as the basis for renaming
    -b, --batch              Enables renaming of multiple files at once. Otherwise, renaming will only proceed if exactly one file is matched
    -z, --iso                Instructs naming formats to prefer ISO date format to "friendlier" date formats
        --dry-run            Does not perform any actual renaming, instead only simulating the rename operation
        --non-interactive    Forces the command to proceed without any user input. This will bypass any prompts and skip formats that require user interaction
    -s, --separator          The separator to use between the components of a target name (where applicable)

```