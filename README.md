# WebTemplates
In the repositories, there are two visual studio sloutions.

**JwtOAuth2Server**
  1. A .net framework 4.6.2 OAuth2 server with JWT format templete. 
  2. The audience, issuer and security key are configurable in settings.
  3. An implementation of IIdentityValidator in IdentityValidator folder is required, which is for identity verification while issuing token.
  
**JwtWebApiIIS**
  1. A .net framework 4.6.2 MVC project for IIS.
  2. Injections ready. 
  3. Web Apis are protected by JWT which is issued by JwtOAuth2Server. The project can be deployed on different IIS from JwtOAuth2Server. The authenticaion is verified by a shared security key.
  4. Swagger help page. The configuration paramters such as Title, Description, Version and Company strings can be set in Startup.cs.
  
  
