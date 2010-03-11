Autodocs is an automatic API documentation generator for .NET applications that use Windows Communication 
Foundation (WCF) to establish REST API's.

How to use Autodocs to document your WCF REST API?

1. Download library and reference Autodocs.ServiceModel.Web.dll in your WCF WebServices project

2. Modify the Web.config's system.ServiceModel section of the WCF WebServices project as illustrated below
a) Add a behavior extension
<extensions>
  <behaviorExtensions>
    <add name="autodocsBehaviorExtension" type="Autodocs.ServiceModel.Web.AutodocsBehaviorExtension, 
Autodocs.ServiceModel.Web, Version=1.0.0.0, Culture=neutral, PublicKeyToken=ad2dc2e36787b789"/>
  </behaviorExtensions>
</extensions>

b) Reference the behavior extension in endpoint behavior
<endpointBehaviors>
  <behavior name="restBehavior">
    <webHttp/>
    <autodocsBehaviorExtension/>
  </behavior>
</endpointBehaviors> 

Here is a sample Web.config

c) Start documenting your Service interface as shown below
[WebServiceDoc(Title="Users REST API Documentation",Description = "Web Services API for user management in the foo system.",SupportLink = "mailto:santthosh@notionforge.com")]

d) Start documenting your Operations as shown below
[WebOperationDoc(Description = "Update an existing user in the system. If UserId is invalid, null or doesn't exist it throws HTTP 404 - Not found.")]

e) Deploy or launch your service, the documentation should be automatically availabe in '/docs'

Service Documentation: http://servername/{servicename}.svc/docs (http://myserver/users.svc/docs)

Operation Documentation: http://servername/{servicename}.svc/{operation}/docs (http://myserver/users.svc/adduser/docs) 

Please visit http://code.google.com/p/autodocs for more information and updates