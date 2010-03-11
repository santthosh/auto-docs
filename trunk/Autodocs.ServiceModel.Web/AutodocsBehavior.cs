using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Autodocs.ServiceModel.Web
{
    public class AutodocsBehavior : IEndpointBehavior
    {
        #region Implementation of IEndpointBehavior

        /// <summary>
        /// Implement to confirm that the endpoint meets some intended criteria.
        /// </summary>
        /// <param name="endpoint">The endpoint to validate.</param>
        public void Validate(ServiceEndpoint endpoint)
        {
        }

        /// <summary>
        /// Implement to pass data at runtime to bindings to support custom behavior.
        /// </summary>
        /// <param name="endpoint">The endpoint to modify.</param><param name="bindingParameters">The objects that binding elements require to support the behavior.</param>
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        /// <summary>
        /// Implements a modification or extension of the service across an endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint that exposes the contract.</param><param name="endpointDispatcher">The endpoint dispatcher to be modified or extended.</param>
        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            DocumentInvoker documentInvoker = new DocumentInvoker { ContractDescription = endpoint.Contract, BaseUri = endpoint.ListenUri };
            endpointDispatcher.DispatchRuntime.OperationSelector = new DocumentOperationSelector(endpoint.ListenUri, endpointDispatcher.DispatchRuntime.OperationSelector);
            
            //Add the documentation page operation
            DispatchOperation documentationPageOperation = new DispatchOperation(endpointDispatcher.DispatchRuntime, DocumentOperationSelector.OperationName, DocumentOperationSelector.DocumentAllServiceOperationsTemplate, null)
                                                               {
                                                                   DeserializeRequest = false,
                                                                   SerializeReply = false,
                                                                   Invoker = documentInvoker,
                                                               };
            endpointDispatcher.DispatchRuntime.Operations.Add(documentationPageOperation);
        }

        /// <summary>
        /// Implements a modification or extension of the client across an endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint that is to be customized.</param><param name="clientRuntime">The client runtime to be customized.</param>
        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        #endregion
    }
}