using YouTubeMusicAPI.Authentication;

namespace YouTubeMusicApi.Tests.Authentication;

[TestFixture]
public class AuthenticationDataGeneratorTests
{
    [Test]
    public void Should_generate_visitor_data()
    {
        // Act
        string? visitorData = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            visitorData = await AuthenticationDataGenerator.VisitorDataAsync();
        });

        // Assert
        Assert.That(visitorData, Is.Not.Null.Or.Empty);

        // Output
        TestContext.Out.WriteLine("VisitorData: {0}", visitorData);
    }

    [Test]
    [Ignore("Currently not supported.")]
    public void Should_generate_proof_of_origin_token()
    {
        // Act
        string? proofOfOriginToken = null;

        //Assert.DoesNotThrowAsync(async () =>
        //{
        //    proofOfOriginToken = await IdentityGenerator.ProofOfOriginTokenAsync();
        //});

        // Assert
        Assert.That(proofOfOriginToken, Is.Not.Null.Or.Empty);

        // Output
        TestContext.Out.WriteLine("Proof of Origin Token: {0}", proofOfOriginToken);
    }
}