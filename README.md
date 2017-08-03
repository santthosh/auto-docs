**Project Description**
Autodocs is an automatic API documentation generator for .NET applications that use Windows Communication Foundation (WCF) to establish REST API's.

#### How to use Autodocs to document your WCF REST API? 

1. Download library and reference Autodocs.ServiceModel.Web.dll in your WCF WebServices project

2. Modify the Web.config's system.ServiceModel section of the WCF WebServices project as illustrated below
a) Add a behavior extension
{code:xml}
<extensions>
  <behaviorExtensions>
    <add name="autodocsBehaviorExtension" type="Autodocs.ServiceModel.Web.AutodocsBehaviorExtension, 
Autodocs.ServiceModel.Web, Version=1.0.0.0, Culture=neutral, PublicKeyToken=ad2dc2e36787b789"/>
  </behaviorExtensions>
</extensions>
{code:xml}
Note: It's very important to have the fully qualified assembly name (since WCF has a [bug](http://connect.microsoft.com/wcf/feedback/details/216431/wcf-fails-to-find-custom-behaviorextensionelement-if-type-attribute-doesnt-match-exactly) on the behavior extension element) 

b) Reference the behavior extension in endpoint behavior
{code:xml}
<endpointBehaviors>
  <behavior name="restBehavior">
    <webHttp/>
    <autodocsBehaviorExtension/>
  </behavior>
</endpointBehaviors> 
{code:xml}
Here is a sample [Web.config](http://autodocs.codeplex.com/SourceControl/changeset/view/47399#618692)

Note: It's very important to maintain the order of behaviors, in the endpointBehaviors element. If you specify autodocs first, you might not get the desired documentation, as webHttp will override autodocs message inspectors

c) Start documenting your Service interface as shown below
{code:c#}
[WebServiceDoc(Title="Users REST API Documentation",
   Description = "Web Services API for user management in the foo system.",
   SupportLink = "mailto:santthosh@notionforge.com")]
{code:c#}
d) Start documenting your Operations as shown below
{code:c#}
[WebOperationDoc(Description = 
   "Update an existing user in the system. If UserId is invalid, null or doesn't exist it throws HTTP 404 - Not found.")]
{code:c#}
e) Deploy or launch your service, the documentation should be automatically availabe in '/docs'

**Service Documentation:** http://servername/{servicename}.svc/docs (http://myserver/users.svc/docs)

**Operation Documentation:** http://servername/{servicename}.svc/{operation}/docs (http://myserver/users.svc/adduser/docs)

**Screen Shots**

a) Sample Service Documentation
![](Home_http://www.notionforge.com/projects/autodocs/Service%20Documentation.JPG)

b) Sample XML Operation Documentation
![](Home_http://www.notionforge.com/projects/autodocs/XML%20Operation.JPG)

c) Sample JSON Operation Documentation
![](Home_http://www.notionforge.com/projects/autodocs/JSON%20Operation.JPG)

Thank you!