using Xunit;

namespace UniversityScheduler.Services.Tests
{
    public class ServicesSmokeTests
    {
        [Fact] public void Service_Test1() => Assert.True(true);
        [Fact] public void Service_Test2() => Assert.Equal(3, 1 + 2);
        [Fact] public void Service_Test3() => Assert.False(false);
        [Fact] public void Service_Test4() => Assert.NotNull(new object());
        [Fact] public void Service_Test5() => Assert.Contains("x", "xyz");
        [Fact] public void Service_Test6() => Assert.StartsWith("S", "Service");
        [Fact] public void Service_Test7() => Assert.EndsWith("ice", "Service");
        [Fact] public void Service_Test8() => Assert.InRange(0, -1, 10);
        [Fact] public void Service_Test9() => Assert.DoesNotContain("q", "abc");
        [Fact] public void Service_Test10() => Assert.Equal("world", string.Concat("wor", "ld"));
    }
}
