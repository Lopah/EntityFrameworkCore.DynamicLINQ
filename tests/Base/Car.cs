namespace EntityFrameworkCore.Tests.Base;

public enum Color
{
    Red,
    Blue
}

public class Car
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    public Color Color { get; set; }

    public bool Bought { get; set; }
}
