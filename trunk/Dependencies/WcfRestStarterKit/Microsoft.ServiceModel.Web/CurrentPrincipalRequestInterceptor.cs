//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.Web;
using System.IdentityModel.Claims;
using System.ServiceModel.Security;
using System.Collections.ObjectModel;
using System.ServiceModel.Dispatcher;
using System.Security.Principal;
using System.IdentityModel.Policy;

namespace Microsoft.ServiceModel.Web
{
    public class CurrentPrincipalRequestInterceptor : RequestInterceptor
    {
        public CurrentPrincipalRequestInterceptor()
            : base(true)
        {
        }

        public override void ProcessRequest(ref RequestContext requestContext)
        {
            if (requestContext == null || requestContext.RequestMessage == null)
            {
                return;
            }
            Message message = requestContext.RequestMessage;
            if (message.Properties.Security == null 
                || message.Properties.Security.ServiceSecurityContext == null 
                || message.Properties.Security.ServiceSecurityContext.IsAnonymous)
            {
                // check if there is an IPrincipal in HttpContext.Current
                if (HttpContext.Current != null)
                {
                    IPrincipal currentUser = HttpContext.Current.User;
                    if (currentUser != null)
                    {
                        List<IAuthorizationPolicy> policies = new List<IAuthorizationPolicy>();
                        policies.Add(new PrincipalAuthorizationPolicy(currentUser));
                        ServiceSecurityContext securityContext = new ServiceSecurityContext(policies.AsReadOnly());
                        if (message.Properties.Security != null)
                        {
                            message.Properties.Security.ServiceSecurityContext = securityContext;
                        }
                        else
                        {
                            message.Properties.Security = new SecurityMessageProperty() { ServiceSecurityContext = securityContext };
                        }
                    }
                }
            }
        }

        class PrincipalAuthorizationPolicy : IAuthorizationPolicy
        {
            string id = Guid.NewGuid().ToString();
            IPrincipal user;

            public PrincipalAuthorizationPolicy(IPrincipal user)
            {
                this.user = user;
            }


            public ClaimSet Issuer
            {
                get { return ClaimSet.System; }
            }

            public string Id
            {
                get { return this.id; }
            }

            public bool Evaluate(EvaluationContext evaluationContext, ref object state)
            {
                evaluationContext.AddClaimSet(this, new DefaultClaimSet(Claim.CreateNameClaim(user.Identity.Name)));
                evaluationContext.Properties["Identities"] = new List<IIdentity>(new IIdentity[] { user.Identity });
                evaluationContext.Properties["Principal"] = user;
                return true;
            }
        }
    }
}
