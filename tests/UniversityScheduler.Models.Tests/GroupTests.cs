using Xunit;
using UniversityScheduler.Models;

namespace UniversityScheduler.Models.Tests;

public class GroupTests
{
    [Fact]
    public void Group_Constructor_InitializesProperties()
    {
        var group = new Group { Id = 1, Name = "P-101" };
        
        Assert.Equal(1, group.Id);
        Assert.Equal("P-101", group.Name);
    }

    [Fact]
    public void Group_Name_ChangesNotifyPropertyChanged()
    {
        var group = new Group { Id = 1, Name = "P-101" };
        bool notified = false;
        group.PropertyChanged += (s, e) => notified = true;
        
        group.Name = "P-102";
        
        Assert.True(notified);
        Assert.Equal("P-102", group.Name);
    }

    [Fact]
    public void Group_EmptyName_AllowsEmpty()
    {
        var group = new Group { Id = 1, Name = "" };
        
        Assert.Equal("", group.Name);
    }

    [Fact]
    public void Group_NullName_AllowsNull()
    {
        var group = new Group { Id = 1, Name = null };
        
        Assert.Null(group.Name);
    }

    [Fact]
    public void Group_SameId_AreEqualForEquality()
    {
        var group1 = new Group { Id = 1, Name = "P-101" };
        var group2 = new Group { Id = 1, Name = "P-102" };
        
        Assert.Equal(group1.Id, group2.Id);
    }
}