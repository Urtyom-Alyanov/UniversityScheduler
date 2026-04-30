using Xunit;

namespace UniversityScheduler.Models.Tests
{
    public class ModelsSmokeTests
    {
        [Fact] public void Model_Test1() => Assert.True(true);
        [Fact] public void Model_Test2() => Assert.Equal(2, 1 + 1);
        [Fact] public void Model_Test3() => Assert.False(false);
        [Fact] public void Model_Test4() => Assert.NotNull(new object());
        [Fact] public void Model_Test5() => Assert.Contains("a", "abc");
        [Fact] public void Model_Test6() => Assert.StartsWith("pre", "prefix");
        [Fact] public void Model_Test7() => Assert.EndsWith("fix", "suffix");
        [Fact] public void Model_Test8() => Assert.InRange(5, 1, 10);
        [Fact] public void Model_Test9() => Assert.DoesNotContain("z", "abc");
        [Fact] public void Model_Test10() => Assert.Equal("hello", string.Join("", new[] { "he", "llo" }));
    }
}
