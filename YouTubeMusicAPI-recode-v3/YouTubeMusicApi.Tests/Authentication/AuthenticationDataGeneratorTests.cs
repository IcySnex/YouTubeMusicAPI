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
    public void Should_generate_rollout_token()
    {
        // Act
        string? rolloutToken = null;

        Assert.DoesNotThrowAsync(async () =>
        {
            rolloutToken = await AuthenticationDataGenerator.RolloutTokenAsync();
        });

        // Assert
        Assert.That(rolloutToken, Is.Not.Null.Or.Empty);

        // Output
        TestContext.Out.WriteLine("Rollout Token: {0}", rolloutToken);
    }

    [Test]
    [Ignore("Currently not supported.")]
    public void Should_generate_proof_of_origin_token()
    {
        // Act
        string? proofOfOriginToken = null;

        //Assert.DoesNotThrowAsync(async () =>
        //{
        //    proofOfOriginToken = await AuthenticationDataGenerator.ProofOfOriginTokenAsync();
        //});

        // Assert
        Assert.That(proofOfOriginToken, Is.Not.Null.Or.Empty);

        // Output
        TestContext.Out.WriteLine("Proof of Origin Token: {0}", proofOfOriginToken);
    }
}