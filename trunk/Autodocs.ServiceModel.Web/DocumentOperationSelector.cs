using System;
using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace Autodocs.ServiceModel.Web
{
    public class DocumentOperationSelector : IDispatchOperationSelector
    {
        #region Constants

        /// <summary>
        /// Key for message property
        /// </summary>
        public const string DocumentationMatchPropertyKey = "DocumentationPageMatch";

        /// <summary>
        /// Constant string to identify this documenting operation
        /// </summary>
        public const string OperationName = "DocumentationOperation";

        /// <summary>
        /// URI template for service level documentation
        /// </summary>
        public const string DocumentAllServiceOperationsTemplate = "docs";

        /// <summary>
        /// URI template for service level documentation
        /// </summary>
        public const string DocumentServiceOperationTemplate = "{operation}/docs";

        /// <summary>
        /// URI template for operation level request schema
        /// </summary>
        public const string DocumentOperationRequestSchemaTemplate = "{operation}/docs/request/schema";

        /// <summary>
        /// URI template for operation level request example
        /// </summary>
        public const string DocumentOperationRequestExampleTemplate = "{operation}/docs/request/example";

        /// <summary>
        /// URI template for operation level response schema
        /// </summary>
        public const string DocumentOperationResponseSchemaTemplate = "{operation}/docs/response/schema";

        /// <summary>
        /// URI template for operation level response example
        /// </summary>
        public const string DocumentOperationResponseExampleTemplate = "{operation}/docs/response/example";

        #endregion

        #region Private variables

        protected UriTemplateTable templateTable;

        protected IDispatchOperationSelector[] selectors;

        #endregion

        #region Constructor

        /// <summary>
        /// Build a UriTemplateTable with the documentation Uri templates
        /// </summary>
        /// <param name="baseUri"></param>
        public DocumentOperationSelector(Uri baseUri,params IDispatchOperationSelector[] selectors)
        {
            List<KeyValuePair<UriTemplate, object>> templateList = new List<KeyValuePair<UriTemplate, object>>
                                                                       {
                                                                           new KeyValuePair<UriTemplate,object>(new UriTemplate(DocumentAllServiceOperationsTemplate),(DocumentType.ServiceDocumentation).ToString()),
                                                                           new KeyValuePair<UriTemplate,object>(new UriTemplate(DocumentServiceOperationTemplate),(DocumentType.OperationDocumentation).ToString()),
                                                                           new KeyValuePair<UriTemplate,object>(new UriTemplate(DocumentOperationRequestSchemaTemplate),(DocumentType.OperationRequestSchema).ToString()),
                                                                           new KeyValuePair<UriTemplate,object>(new UriTemplate(DocumentOperationRequestExampleTemplate),(DocumentType.OperationRequestExample).ToString()),
                                                                           new KeyValuePair<UriTemplate,object>(new UriTemplate(DocumentOperationResponseSchemaTemplate),(DocumentType.OperationResponeSchema).ToString()),
                                                                           new KeyValuePair<UriTemplate,object>(new UriTemplate(DocumentOperationResponseExampleTemplate),(DocumentType.OperationResponseExample).ToString())
                                                                       };

            templateTable = new UriTemplateTable(baseUri, templateList);

            if (selectors != null)
            {
                this.selectors = new IDispatchOperationSelector[selectors.Length];
                for (int i = 0; i < selectors.Length; ++i)
                {
                    this.selectors[i] = selectors[i];
                }
            }
            else
            {
                this.selectors = new IDispatchOperationSelector[] { };
            }
        }       

        #endregion

        #region Implementation of IDispatchOperationSelector

        /// <summary>
        /// Associates a local operation with the incoming method.
        /// </summary>
        /// <returns>
        /// The name of the operation to be associated with the <paramref name="message"/>.
        /// </returns>
        /// <param name="message">The incoming <see cref="T:System.ServiceModel.Channels.Message"/> to be associated with an operation.</param>
        public string SelectOperation(ref Message message)
        {
            //Ensure that the documentation page is invoked via the GET method, else don't send the documentation page
            object propertyValue;
            if (!message.Properties.TryGetValue(HttpRequestMessageProperty.Name, out propertyValue) || 
                ((HttpRequestMessageProperty)propertyValue).Method != "GET")
                return string.Empty;

            //Check for the matches with the help page templates
            UriTemplateMatch match = templateTable.MatchSingle(message.Properties.Via);
            if (match == null)
            {
                for (int i = 0; i < selectors.Length; ++i)
                {
                    string name = selectors[i].SelectOperation(ref message);
                    if (!string.IsNullOrEmpty(name))
                    {
                        return name;
                    }
                }
                return string.Empty;
            }            

            //Attach the matched object to the message properties with a constant key 
            message.Properties[DocumentationMatchPropertyKey] = match;

            //This ensures that our DocumentInvoker is invoked for this documenting operation
            return OperationName;
        }

        #endregion
    }
}