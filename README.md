### Installation

IdentityTokenCache can be installed via the nuget UI (as CL.Identity.Tokens.Cache), or via the nuget package manager console:

```PowerShell
PM> Install-Package CL.Identity.Tokens.Cache -Pre
```

### Configuration

The nuget package should enable "tokenReplayDetection", and add the caches/tokenReplayCache/redisCache configuration. All should have to do is specify the connectionString property on the redisCache configuration. 

```XML
  <system.identityModel>
    <identityConfiguration>
      <tokenReplayDetection enabled="true" />
      <caches>
        <tokenReplayCache type="CampusLabs.Identity.Tokens.Cache.RedisTokenReplayCache, CampusLabs.Identity.Tokens.Cache">
          <redisCache connectionString="<<insert redis connection string>>" />
        </tokenReplayCache>
      </caches>
    </identityConfiguration>
  </system.identityModel>
```

### Error Handling

The RedisTokenReplayCache exposes the NoticeError event to allow you to handle excptions in your application.

```C#
FederatedAuthentication.FederationConfigurationCreated += (createdSender, createdEventArgs) =>
{
    var tokenReplayCache = createdEventArgs.FederationConfiguration.IdentityConfiguration.Caches.TokenReplayCache as RedisTokenReplayCache;

    if (tokenReplayCache != null)
    {
        tokenReplayCache.NoticeError += (errorSender, exception) =>
        {
            //handle the error
        };
    }
};
```
