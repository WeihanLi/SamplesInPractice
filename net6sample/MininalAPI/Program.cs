using System;
using Microsoft.AspNetCore.Builder;

var app = WebApplication.Create(args);
app.Map("/", (Func<string>)(() => "Hello World"));
app.Run();
