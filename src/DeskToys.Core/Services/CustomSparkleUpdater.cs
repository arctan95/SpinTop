using NetSparkleUpdater.Interfaces;

namespace DeskToys.Core.Services;

public class CustomSparkleUpdater : NetSparkleUpdater.SparkleUpdater
{
    public CustomSparkleUpdater(string appcastUrl, ISignatureVerifier signatureVerifier)
            : base(appcastUrl, signatureVerifier, null)
        { }
}