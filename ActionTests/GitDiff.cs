namespace ActionTests;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void TestGitDiff()
    {
        var output = Assistants.GitDif.GitDiff();
        Assert.IsNotNull(output);
    }
}