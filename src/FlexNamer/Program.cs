﻿using FlexNamer;
using Shale;
using Spectre.Console.Cli;

var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
services.AddPlugins(c => c.AlwaysLoad<INamingFormat>());
var app = new CommandApp(new Spectre.Console.Cli.Extensions.DependencyInjection.DependencyInjectionRegistrar(services));
app.Configure(c => {
	c.SetApplicationName("FlexNamer");
	c.AddCommand<RenameCommand>("rename");
});
var result = app.Run(args);
return result;