using ContactsApp.Application.DTOs;

namespace ContactsApp.Tests.Unit;

[TestFixture]
public class SetupTests
{
    [Test]
    public void ApiResponse_Success_ShouldHaveCorrectStructure()
    {
        var response = ApiResponse<string>.Ok("test data");

        Assert.That(response.Success, Is.True);
        Assert.That(response.Data, Is.EqualTo("test data"));
        Assert.That(response.Errors, Is.Null);
    }

    [Test]
    public void ApiResponse_Fail_ShouldHaveCorrectStructure()
    {
        var errors = new[] { "champ1: requis", "champ2: invalide" };
        var response = ApiResponse<string>.Fail("Validation echouee", errors);

        Assert.That(response.Success, Is.False);
        Assert.That(response.Data, Is.Null);
        Assert.That(response.Message, Is.EqualTo("Validation echouee"));
        Assert.That(response.Errors, Is.EquivalentTo(errors));
    }
}
