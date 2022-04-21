using System;

namespace EntityFrameworkCore.Tests.Base;

public class ApplicationFake : IDisposable
{
    public readonly ApplicationDbContext DbContext;

    public ApplicationFake()
    {
        DbContext = new();

        DbContext.Database.EnsureDeleted();

        DbContext.SaveChanges();

        DbContext.Database.EnsureCreated();

        DbContext.SaveChanges();

        // Seed Data

        DbContext.Cars.AddRange(
            new Car
            {
                Bought = true,
                Color = Color.Blue,
                Name = "Test1"
            }, new Car
            {
                Bought = false,
                Color = Color.Red,
                Name = "Test2"
            }, new Car
            {
                Bought = true,
                Color = Color.Red,
                Name = "Test3"
            }, new Car
            {
                Bought = false,
                Color = Color.Blue,
                Name = "Test4"
            }, new Car
            {
                Bought = true,
                Color = Color.Blue,
                Name = "Test5"
            });

        DbContext.SaveChanges();
    }

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose()
    {
        DbContext.Dispose();
        GC.SuppressFinalize(this);
    }
}
