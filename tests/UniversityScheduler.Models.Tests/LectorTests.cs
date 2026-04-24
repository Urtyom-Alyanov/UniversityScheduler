using Xunit;
using UniversityScheduler.Models;

namespace UniversityScheduler.Models.Tests;

public class LectorTests
{
    [Fact]
    public void Lector_Constructor_InitializesProperties()
    {
        var lector = new Lector { Id = 1, Name = "Ivanov I.I." };
        
        Assert.Equal(1, lector.Id);
        Assert.Equal("Ivanov I.I.", lector.Name);
    }

    [Fact]
    public void Lector_Name_ChangesNotifyPropertyChanged()
    {
        var lector = new Lector { Id = 1, Name = "Ivanov" };
        bool notified = false;
        lector.PropertyChanged += (s, e) => notified = true;
        
        lector.Name = "Petrov";
        
        Assert.True(notified);
        Assert.Equal("Petrov", lector.Name);
    }

    [Fact]
    public void Lector_EmptyName_AllowsEmpty()
    {
        var lector = new Lector { Id = 1, Name = "" };
        
        Assert.Equal("", lector.Name);
    }

    [Fact]
    public void Lector_NullName_AllowsNull()
    {
        var lector = new Lector { Id = 1, Name = null };
        
        Assert.Null(lector.Name);
    }

    [Fact]
    public void Lector_SameId_AreEqualForEquality()
    {
        var lector1 = new Lector { Id = 1, Name = "Ivanov" };
        var lector2 = new Lector { Id = 1, Name = "Petrov" };
        
        Assert.Equal(lector1.Id, lector2.Id);
    }
}