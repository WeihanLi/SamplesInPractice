WriteLine(MyFile.Exists(Path.Combine(Directory.GetCurrentDirectory(), "undefined.dll")));

WriteLine("Hello, World!");

InvokeHelper.TryInvoke(() => WriteLine("ImplicitUsingSample"));
