#:property ManagePackageVersionsCentrally=false
#:package WeihanLi.Common@1.0.84

using WeihanLi.Common.Helpers;

ConsoleHelper.WriteLineWithColor("Hello, C# 14!", ConsoleColor.DarkGreen);

// Nullable conditional expressions
var person = GetPerson();
if (person is not null)
{
    person.Description = "No description available.";
}

person?.Description = "This is a description";


static Person? GetPerson()
{
   if (Random.Shared.Next(5) > 3)
   {
       return new Person("Alice");
   }

   return null;
}

file record Person(string Name)
{
    public string? Description { get; set; }
}
