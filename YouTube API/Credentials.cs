using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouTube_Video_Uploader__cross_platform_.YouTube_API
{
     public class Credentials
{
    private Credentials()
    {
    }

    public string ApplicationName { get; private set; } = string.Empty;

    public string SecretsPath { get; private set; } = string.Empty;

    public string SecretJSON { get; private set; } = string.Empty;

    public string ChannelID { get; private set; } = "user";

    public string ClientID { get; private set; }

    public string ClientSecret { get; private set; }

    public string AuthorizationDataStorePath { get; private set; }

    public static Credentials FromSecret(
        string secretPath,
        string applicationName,
        string chennelID = "user",
        string authorizationDataStorePath = null)
    {
        return new Credentials()
        {
            ApplicationName = applicationName,
            SecretsPath = secretPath,
            ChannelID = chennelID,
            AuthorizationDataStorePath = authorizationDataStorePath ?? typeof(YoutubeUploader).ToString()
        };
    }

    public static Credentials FromJSON(
        string secretJSON,
        string applicationName,
        string chennelID = "user",
        string authorizationDataStorePath = null)
    {
        return new Credentials()
        {
            ApplicationName = applicationName,
            SecretJSON = secretJSON,
            ChannelID = chennelID,
            AuthorizationDataStorePath = authorizationDataStorePath ?? typeof(YoutubeUploader).ToString()
        };
    }

    public static Credentials FromIdentifiers(
        string clientID,
        string clientSecret,
        string applicationName,
        string chennelID = "user",
        string authorizationDataStorePath = null)
    {
        return new Credentials()
        {
            ApplicationName = applicationName,
            ClientID = clientID,
            ClientSecret = clientSecret,
            ChannelID = chennelID,
            AuthorizationDataStorePath = authorizationDataStorePath ?? typeof(YoutubeUploader).ToString()
        };
    }
}

}
