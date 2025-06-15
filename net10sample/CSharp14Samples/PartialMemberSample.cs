namespace CSharp14Samples;

// https://github.com/dotnet/csharplang/blob/main/proposals/partial-events-and-constructors.md
// https://github.com/dotnet/csharplang/issues/9058
// https://github.com/dotnet/csharplang/discussions/8064
public partial class PartialMemberSample
{
    public partial PartialMemberSample();
    
    public partial event Action OnUpdated;

    public static void Run()
    {
        var partialMember = new PartialMemberSample();
        Console.WriteLine(partialMember);

        partialMember.OnUpdated += () => Console.WriteLine("Event Handler 1");
        partialMember.OnUpdated += PartialMemberSample_OnUpdated;
        partialMember.Updated();
        partialMember.OnUpdated -= PartialMemberSample_OnUpdated;
        partialMember.Updated();

        static void PartialMemberSample_OnUpdated()
        {
            Console.WriteLine("Event Handler 2");
        }
    }
}

public partial class PartialMemberSample
{
    public partial PartialMemberSample()
    {
        Console.WriteLine("Partial Constructor");
    }

    private event Action _updateHandler = () => Console.WriteLine("Default Event Handler");
    public partial event Action OnUpdated
    {
        add => _updateHandler += value;
        remove => _updateHandler -= value;
    }

    public void Updated() => _updateHandler.Invoke();
}
