using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Text;

namespace Autodocs.ServiceModel.Web
{
    public class OperationDescriptionUtilites
    {
        /// <summary>
        /// Get the WebGet attribute specified in an operation if any
        /// </summary>
        /// <param name="operationDescription"></param>
        /// <returns></returns>
        private static WebGetAttribute GetWebGet(OperationDescription operationDescription)
        {
            return operationDescription.Behaviors.Find<WebGetAttribute>();
        }

        /// <summary>
        /// Get the WebInvoke attribute specified in an operation if any
        /// </summary>
        /// <param name="operationDescription"></param>
        /// <returns></returns>
        private static WebInvokeAttribute GetWebInvoke(OperationDescription operationDescription)
        {
            return operationDescription.Behaviors.Find<WebInvokeAttribute>();
        }

        /// <summary>
        /// Get the XmlSerializerOperationBehavior if that is specified
        /// </summary>
        /// <param name="operationDescription"></param>
        /// <returns></returns>
        private static XmlSerializerOperationBehavior GetXmlSerializerBehavior(OperationDescription operationDescription)
        {
            return operationDescription.Behaviors.Find<XmlSerializerOperationBehavior>();
        }

        /// <summary>
        /// Get the REST METHOD specified by the operation description
        /// </summary>
        /// <param name="operationDescription"></param>
        /// <returns></returns>
        public static string GetMethod(OperationDescription operationDescription)
        {
            if (GetWebGet(operationDescription) != null)
                return "GET";

            WebInvokeAttribute invokeAttribute = GetWebInvoke(operationDescription);
            return !String.IsNullOrEmpty(invokeAttribute.Method) ? invokeAttribute.Method : "POST";
        }

        /// <summary>
        /// Returns true if message direction is output
        /// </summary>
        /// <param name="operationDescription"></param>
        /// <returns></returns>
        private static bool IsResponseStream(OperationDescription operationDescription)
        {
            foreach (MessageDescription message in operationDescription.Messages)
            {
                if (message.Direction == MessageDirection.Output && (message.Body.ReturnValue != null && message.Body.Parts.Count == 0))
                {
                    return (message.Body.ReturnValue != null && message.Body.ReturnValue.Type == typeof(Stream));
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if message direction is input
        /// </summary>
        /// <param name="od"></param>
        /// <returns></returns>
        private static bool IsRequestStream(OperationDescription od)
        {
            foreach (MessageDescription message in od.Messages)
            {
                if (message.Direction == MessageDirection.Input && message.Body.Parts.Count == 1)
                {
                    return (message.Body.Parts[0].Type == typeof(Stream));
                }
            }
            return false;
        }

        /// <summary>
        /// Get the Request format if this is WebInvoke
        /// </summary>
        /// <param name="operationDescription"></param>
        /// <returns></returns>
        public static string GetRequestFormat(OperationDescription operationDescription)
        {
            if (GetWebGet(operationDescription) != null)
                return null;

            WebInvokeAttribute invokeAttribute = GetWebInvoke(operationDescription);
            return invokeAttribute != null && IsRequestStream(operationDescription) ? "Binary" : "Xml or Json";
        }

        /// <summary>
        /// Get the Response format 
        /// </summary>
        /// <param name="operationDescription"></param>
        /// <returns></returns>
        public static string GetResponseFormat(OperationDescription operationDescription)
        {
            WebGetAttribute webGet = GetWebGet(operationDescription);
            WebInvokeAttribute webInvoke = GetWebInvoke(operationDescription);

            if (IsResponseStream(operationDescription))
                return "Binary";

            if (webGet != null && webGet.IsResponseFormatSetExplicitly)
                return webGet.ResponseFormat.ToString();

            if (webInvoke != null && webInvoke.IsResponseFormatSetExplicitly)
                return webInvoke.ResponseFormat.ToString();

            return (new WebHttpBehavior()).DefaultOutgoingResponseFormat.ToString();
        }

        /// <summary>
        /// Verify if this message is Untyped
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private static bool IsUntypedMessage(MessageDescription message)
        {
            if (message == null)
            {
                return false;
            }
            return (message.Body.ReturnValue != null && message.Body.Parts.Count == 0 && message.Body.ReturnValue.Type == typeof(Message)) ||
                (message.Body.ReturnValue == null && message.Body.Parts.Count == 1 && message.Body.Parts[0].Type == typeof(Message));
        }

        /// <summary>
        /// Get UriTemplate for operation
        /// </summary>
        /// <param name="operationDescription"></param>
        /// <returns></returns>
        public static string GetUriTemplate(OperationDescription operationDescription)
        {
            WebGetAttribute webGet = GetWebGet(operationDescription);
            WebInvokeAttribute webInvoke = GetWebInvoke(operationDescription);

            if (webGet != null)
            {
                if (webGet.UriTemplate != null)
                {
                    return webGet.UriTemplate;
                }
                StringBuilder stringBuilder = new StringBuilder(operationDescription.Name);
                if (!IsUntypedMessage(operationDescription.Messages[0]))
                {
                    stringBuilder.Append("?");
                    foreach (MessagePartDescription mpd in operationDescription.Messages[0].Body.Parts)
                    {
                        string parameterName = mpd.Name;
                        stringBuilder.Append(parameterName);
                        stringBuilder.Append("={");
                        stringBuilder.Append(parameterName);
                        stringBuilder.Append("}&");
                    }
                    stringBuilder.Remove(stringBuilder.Length - 1, 1);
                }
                return stringBuilder.ToString();
            }
            return webInvoke.UriTemplate ?? operationDescription.Name;
        }

        /// <summary>
        /// Get request Body Type if any
        /// </summary>
        /// <param name="operationDescription"></param>
        /// <param name="isXmlSerializerType"></param>
        /// <returns></returns>
        public static Type GetRequestBodyType(OperationDescription operationDescription, out bool isXmlSerializerType)
        {
            isXmlSerializerType = (GetXmlSerializerBehavior(operationDescription) != null);

            //There is no request body if this WebGet
            if (GetWebGet(operationDescription) != null)
                return null;

            WebInvokeAttribute invoke = GetWebInvoke(operationDescription);
            List<string> uriParameters = new List<string>();
            if (invoke.UriTemplate != null)
            {
                UriTemplate template = new UriTemplate(invoke.UriTemplate);

                foreach (string pathVariable in template.PathSegmentVariableNames)
                    uriParameters.Add(pathVariable);

                foreach (string queryVariable in template.QueryValueVariableNames)
                    uriParameters.Add(queryVariable);
            }

            if (operationDescription.Messages[0].MessageType != null)
                return null;

            List<Type> bodyParts = new List<Type>();
            foreach (MessagePartDescription messagePart in operationDescription.Messages[0].Body.Parts)
            {
                bool isUriPart = false;
                foreach (string var in uriParameters)
                {
                    if (String.Equals(var, messagePart.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        isUriPart = true;
                        break;
                    }
                }
                if (isUriPart)
                    continue;

                bodyParts.Add(messagePart.Type);
            }
            if ((bodyParts.Count == 0) || (bodyParts.Count > 1))
                return null;

            return bodyParts[0];
        }

        /// <summary>
        /// Get response Body Type if any
        /// </summary>
        /// <param name="operationDescription"></param>
        /// <param name="isXmlSerializerType"></param>
        /// <returns></returns>
        public static Type GetResponseBodyType(OperationDescription operationDescription, out bool isXmlSerializerType)
        {
            isXmlSerializerType = (GetXmlSerializerBehavior(operationDescription) != null);
            return operationDescription.Messages[1].MessageType != null ? null
                       : (operationDescription.Messages[1].Body.Parts.Count > 0 ? null
                              : (operationDescription.Messages[1].Body.ReturnValue.Type));
        }

    }
}
