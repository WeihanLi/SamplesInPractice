namespace CSharp10Sample;

public class RecordSealedToStringSample
{
    record Person(string Name, int Age)
    {
        public sealed override string ToString()
        {
            return Name;
        }
    }

    //record Person2(string Name, int Age) : Person(Name, Age)
    //{
    //    //Error	CS0239	'RecordSealedToStringSample.Person2.ToString()': cannot override inherited member 'RecordSealedToStringSample.Person.ToString()' because it is sealed
    //    public override string ToString()
    //    {
    //        return $"{Name}__{Age}";
    //    }
    //}

    //record struct Point(int X, int Y)
    //{
    //    //Error	CS0106	The modifier 'sealed' is not valid for this item
    //    public sealed override string ToString()
    //    {
    //        return $"({X},{Y})";
    //    }
    //}
}
