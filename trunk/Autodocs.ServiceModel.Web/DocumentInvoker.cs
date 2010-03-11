﻿using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Syndication;
using System.ServiceModel.Web;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Autodocs.ServiceModel.Web
{
    public class DocumentInvoker : IOperationInvoker
    {
        #region Constants

        private const string CssTemplate = "#content{ FONT-SIZE: 0.7em; PADDING-BOTTOM: 2em; MARGIN-LEFT: 30px}BODY{MARGIN-TOP: 0px; MARGIN-LEFT: 0px; COLOR: #000000; FONT-FAMILY: Verdana; BACKGROUND-COLOR: white}P{MARGIN-TOP: 0px; MARGIN-BOTTOM: 12px; COLOR: #000000; FONT-FAMILY: Verdana}PRE{BORDER-RIGHT: #f0f0e0 1px solid; PADDING-RIGHT: 5px; BORDER-TOP: #f0f0e0 1px solid; MARGIN-TOP: -5px; PADDING-LEFT: 5px; FONT-SIZE: 1.2em; PADDING-BOTTOM: 5px; BORDER-LEFT: #f0f0e0 1px solid; PADDING-TOP: 5px; BORDER-BOTTOM: #f0f0e0 1px solid; FONT-FAMILY: Courier New; BACKGROUND-COLOR: #e5e5cc}.heading1{MARGIN-TOP: 0px; PADDING-LEFT: 15px; FONT-WEIGHT: normal; FONT-SIZE: 26px; MARGIN-BOTTOM: 0px; PADDING-BOTTOM: 3px; MARGIN-LEFT: -30px; WIDTH: 100%; COLOR: #ffffff; PADDING-TOP: 10px; FONT-FAMILY: Tahoma; BACKGROUND-COLOR: #003366}.intro{MARGIN-LEFT: -15px}.heading2{MARGIN-TOP: 0px; PADDING-LEFT: 15px; FONT-WEIGHT: normal; FONT-SIZE: 18px; MARGIN-BOTTOM: 0px; PADDING-BOTTOM: 3px; MARGIN-LEFT: -30px; WIDTH: 100%; COLOR: #ffffff; PADDING-TOP: 10px; FONT-FAMILY: Tahoma; BACKGROUND-COLOR: #23819C}";

        #endregion

        #region Properties

        public ContractDescription ContractDescription { get; set; }

        public Uri BaseUri { get; set; } 

        #endregion

        #region Implementation of IOperationInvoker

        /// <summary>
        /// Returns an <see cref="T:System.Array"/> of parameter objects.
        /// </summary>
        /// <returns>
        /// The parameters that are to be used as arguments to the operation.
        /// </returns>
        public object[] AllocateInputs()
        {
            return new object[] { null };
        }

        /// <summary>
        /// Get Operation Description from name
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        private OperationDescription GetOperationDescription(string operation)
        {
            foreach (OperationDescription operationDescription in ContractDescription.Operations)
            {
                if (String.Compare(operationDescription.Name,operation,true)==0)
                    return operationDescription;
            }
            return null;
        }

        /// <summary>
        /// Returns an object and a set of output objects from an instance and set of input objects.  
        /// </summary>
        /// <returns>
        /// The return value.
        /// </returns>
        /// <param name="instance">The object to be invoked.</param><param name="inputs">The inputs to the method.</param><param name="outputs">The outputs from the method.</param>
        public object Invoke(object instance, object[] inputs, out object[] outputs)
        {
            //There are no outputs from this method
            outputs = null;

            Message result = null;

            try
            {
                //Extract the UriTemplateMatch object set by DocumentOperationSelector
                UriTemplateMatch templateMatch = (UriTemplateMatch)OperationContext.Current.IncomingMessageProperties[DocumentOperationSelector.DocumentationMatchPropertyKey];

                DocumentType documentType = (DocumentType)Enum.Parse(typeof(DocumentType), (string)templateMatch.Data);                

                switch (documentType)
                {
                    case DocumentType.ServiceDocumentation:
                        if (WebOperationContext.Current != null)
                            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";
                        result = GetServiceDocumentation();
                        break;
                    case DocumentType.OperationDocumentation:
                        if (WebOperationContext.Current != null)
                            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";
                        result = GetOperationDocumentationAsMessage(GetOperationDescription(templateMatch.BoundVariables["operation"]));
                        break;
                    case DocumentType.OperationRequestSchema:
                        if (WebOperationContext.Current != null)
                            WebOperationContext.Current.OutgoingResponse.ContentType = "text/xml";
                        result = GetRequestXmlSchemaAsMessage(GetOperationDescription(templateMatch.BoundVariables["operation"]));
                        break;
                    case DocumentType.OperationRequestExample:
                        if (WebOperationContext.Current != null)
                            WebOperationContext.Current.OutgoingResponse.ContentType = "text/xml";
                        result = GetRequestExampleAsMessage(GetOperationDescription(templateMatch.BoundVariables["operation"]));
                        break;
                    case DocumentType.OperationResponeSchema:
                        if (WebOperationContext.Current != null)
                            WebOperationContext.Current.OutgoingResponse.ContentType = "text/xml";
                        result = GetResponseXmlSchemaAsMessage(GetOperationDescription(templateMatch.BoundVariables["operation"]));
                        break;
                    case DocumentType.OperationResponseExample:
                        if (WebOperationContext.Current != null)
                            WebOperationContext.Current.OutgoingResponse.ContentType = "text/xml";
                        result = GetResponseExampleAsMessage(GetOperationDescription(templateMatch.BoundVariables["operation"]));
                        break;
                    default:
                        if (WebOperationContext.Current != null)
                            WebOperationContext.Current.OutgoingResponse.SetStatusAsNotFound();
                        break;
                }
            }
            catch (Exception ex)
            {
                EventLog applicationLog = new EventLog{Source = Assembly.GetExecutingAssembly().FullName};
                applicationLog.WriteEntry(ex.ToString(), EventLogEntryType.Error);

                if (WebOperationContext.Current != null)
                {
                    WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.InternalServerError;
                    WebOperationContext.Current.OutgoingResponse.StatusDescription = ex.Message;
                }
            }

            return result;
        }

        /// <summary>
        /// An asynchronous implementation of the <see cref="M:System.ServiceModel.Dispatcher.IOperationInvoker.Invoke(System.Object,System.Object[],System.Object[]@)"/> method.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.IAsyncResult"/> used to complete the asynchronous call.
        /// </returns>
        /// <param name="instance">The object to be invoked.</param><param name="inputs">The inputs to the method.</param><param name="callback">The asynchronous callback object.</param><param name="state">Associated state data.</param>
        public IAsyncResult InvokeBegin(object instance, object[] inputs, AsyncCallback callback, object state)
        {
            throw new NotSupportedException("Asynchronous documentation invocation is not supported.");
        }

        /// <summary>
        /// The asynchronous end method.
        /// </summary>
        /// <returns>
        /// The return value.
        /// </returns>
        /// <param name="instance">The object invoked.</param><param name="outputs">The outputs from the method.</param><param name="result">The <see cref="T:System.IAsyncResult"/> object.</param>
        public object InvokeEnd(object instance, out object[] outputs, IAsyncResult result)
        {
            throw new NotSupportedException("Asynchronous documentation invocation is not supported.");
        }

        /// <summary>
        /// Gets a value that specifies whether the <see cref="M:System.ServiceModel.Dispatcher.IOperationInvoker.Invoke(System.Object,System.Object[],System.Object[]@)"/> or <see cref="M:System.ServiceModel.Dispatcher.IOperationInvoker.InvokeBegin(System.Object,System.Object[],System.AsyncCallback,System.Object)"/> method is called by the dispatcher.
        /// </summary>
        /// <returns>
        /// true if the dispatcher invokes the synchronous operation; otherwise, false.
        /// </returns>
        public bool IsSynchronous
        {
            get { return true; }
        }

        #endregion

        #region Reflectors and Component constructors
        
        /// <summary>
        /// Check to see if this is a special type of body
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        private static bool IsBodySpecial(Type body)
        {
            return body == null ||
                   body == typeof(void) ||
                   body == typeof(Stream) ||
                   typeof(Atom10FeedFormatter).IsAssignableFrom(body) ||
                   typeof(Atom10ItemFormatter).IsAssignableFrom(body) ||
                   typeof(AtomPub10ServiceDocumentFormatter).IsAssignableFrom(body) ||
                   typeof(AtomPub10CategoriesDocumentFormatter).IsAssignableFrom(body) ||
                   typeof(Rss20FeedFormatter).IsAssignableFrom(body) ||
                   typeof(NameValueCollection).IsAssignableFrom(body) ||
                   typeof(XElement).IsAssignableFrom(body) || 
                   typeof(XmlElement).IsAssignableFrom(body);
        }

        /// <summary>
        /// Check to see if this is a special type of response
        /// </summary>
        /// <param name="body"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        private static string GetSpecialBodyDocumentation(Type body,string direction)
        {
            string message = null;
            if (body == null || body == typeof(void))
            {
                message = String.Format("The {0} body is empty.", direction);
            }
            else if (body == typeof(Stream))
            {
                message = String.Format("The {0} body is a byte stream. See the service documentation for allowed content types.", direction);
            }
            else if (typeof(Atom10FeedFormatter).IsAssignableFrom(body))
            {
                message = String.Format("The {0} body is an Atom 1.0 syndication feed. See http://tools.ietf.org/html/rfc4287 for more details.", direction);
            }
            else if (typeof(Atom10ItemFormatter).IsAssignableFrom(body))
            {
                message = String.Format("The {0} body is an Atom 1.0 syndication entry. See http://tools.ietf.org/html/rfc4287 for more details.", direction);
            }
            else if (typeof(AtomPub10ServiceDocumentFormatter).IsAssignableFrom(body))
            {
                message = String.Format("The {0} body is an Atom Pub service document. See http://www.rfc-editor.org/rfc/rfc5023.txt for more details.", direction);
            }
            else if (typeof(AtomPub10CategoriesDocumentFormatter).IsAssignableFrom(body))
            {
                message = String.Format("The {0} body is an Atom Pub categories document. See http://www.rfc-editor.org/rfc/rfc5023.txt for more details.", direction);
            }
            else if (typeof(Rss20FeedFormatter).IsAssignableFrom(body))
            {
                message = String.Format("The {0} body is an RSS 2.0 syndication feed. See http://validator.w3.org/feed/docs/rss2.html for more details.", direction);
            }
            else if (typeof(NameValueCollection).IsAssignableFrom(body))
            {
                message = String.Format("The {0} body is a HTML Forms data.", direction);
            }
            else if (typeof(XElement).IsAssignableFrom(body) || typeof(XmlElement).IsAssignableFrom(body))
            {
                message = String.Format("The {0} body is arbitrary XML. See the service documentation for conformant XML documents.", direction);
            }
            return message;
        }

        /// <summary>
        /// Build a TextMessage from string
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private static Message GetTextMessage(string message)
        {
            Message result = Message.CreateMessage(MessageVersion.None, null, new TextBodyWriter(message));
            result.Properties[WebBodyFormatMessageProperty.Name] = new WebBodyFormatMessageProperty(WebContentFormat.Raw);
            if (WebOperationContext.Current != null)
                WebOperationContext.Current.OutgoingResponse.ContentType = "text/plain";
            return result;
        }

        /// <summary>
        /// Gets the Special Body Documentation as Message
        /// </summary>
        /// <param name="body"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        private static Message GetSpecialBodyDocumentationAsMessage(Type body, string direction)
        {
            return GetTextMessage(GetSpecialBodyDocumentation(body, direction));
        }

        /// <summary>
        /// Get request object example that is in JSON format
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        private static string GetJsonExample(object instance, Type body)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(body);
            using (MemoryStream stream = new MemoryStream())
            {
                serializer.WriteObject(stream, instance);
                if(stream.CanSeek)
                    stream.Seek(0, SeekOrigin.Begin);

                if (stream.CanRead)
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
                return String.Empty;
            }
        }

        /// <summary>
        /// Get request object example that is in JSON format
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        private static Message GetJsonExampleAsMessage(object instance, Type body)
        {
            Message message = Message.CreateMessage(MessageVersion.None, null, new JsonBodyWriter(instance, body));
            message.Properties[WebBodyFormatMessageProperty.Name] = new WebBodyFormatMessageProperty(WebContentFormat.Json);
            if (WebOperationContext.Current != null)
                WebOperationContext.Current.OutgoingResponse.ContentType = "text/json";
            return message;
        }

        /// <summary>
        /// Get request object example that is in XML format
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="body"></param>
        /// <param name="isXmlSerializerType"></param>
        /// <returns></returns>
        private static string GetXmlExample(object instance, Type body, bool isXmlSerializerType)
        {
            if (isXmlSerializerType)
            {
                XmlSerializer serializer = new XmlSerializer(body);
                using (MemoryStream stream = new MemoryStream())
                {
                    XmlWriterSettings xws = new XmlWriterSettings { Indent = true };
                    using (XmlWriter writer = XmlWriter.Create(stream, xws))
                    {
                        if (writer != null) 
                            serializer.Serialize(writer,instance);
                    }
                    stream.Seek(0, SeekOrigin.Begin);
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            else
            {
                DataContractSerializer serializer = new DataContractSerializer(body);
                using (MemoryStream stream = new MemoryStream())
                {
                    XmlWriterSettings xws = new XmlWriterSettings { Indent = true };
                    using (XmlWriter writer = XmlWriter.Create(stream, xws))
                    {
                        if (writer != null) 
                            serializer.WriteObject(writer, instance);
                    }
                    stream.Seek(0, SeekOrigin.Begin);
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }

        /// <summary>
        /// Get and Xml example as Message
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="body"></param>
        /// <param name="isXmlSerializerType"></param>
        /// <returns></returns>
        private static Message GetXmlExampleAsMessage(object instance, Type body, bool isXmlSerializerType)
        {
            if (isXmlSerializerType)
            {
                XmlSerializer serializer = new XmlSerializer(body);
                using (MemoryStream stream = new MemoryStream())
                {
                    XmlWriterSettings xws = new XmlWriterSettings { Indent = true };
                    using (XmlWriter writer = XmlWriter.Create(stream, xws))
                    {
                        if (writer != null) serializer.Serialize(writer, instance);
                    }
                    stream.Seek(0, SeekOrigin.Begin);
                    using (XmlReader reader = XmlReader.Create(stream))
                    {
                        return Message.CreateMessage(MessageVersion.None, null, XElement.Load(reader, LoadOptions.PreserveWhitespace));
                    }
                }
            }
            else
            {
                DataContractSerializer serializer = new DataContractSerializer(body);
                using (MemoryStream stream = new MemoryStream())
                {
                    XmlWriterSettings xws = new XmlWriterSettings { Indent = true };
                    using (XmlWriter writer = XmlWriter.Create(stream, xws))
                    {
                        if (writer != null) 
                            serializer.WriteObject(writer, instance);
                    }
                    stream.Seek(0, SeekOrigin.Begin);
                    using (XmlReader reader = XmlReader.Create(stream))
                    {
                        return Message.CreateMessage(MessageVersion.None, null, XElement.Load(reader, LoadOptions.PreserveWhitespace));
                    }
                }
            }
        }

        /// <summary>
        /// Get the request example
        /// </summary>
        /// <param name="operationDescription"></param>
        /// <returns></returns>
        public string GetRequestExample(OperationDescription operationDescription)
        {
            bool isXmlSerializerType;
            Type body = OperationDescriptionUtilites.GetRequestBodyType(operationDescription, out isXmlSerializerType);
            
            if (IsBodySpecial(body))            
                return GetSpecialBodyDocumentation(body,"request");            

            try
            {
                object instance = Activator.CreateInstance(body);
                return OperationDescriptionUtilites.GetResponseFormat(operationDescription) == "Json" ? 
                    GetJsonExample(instance, body) : 
                    GetXmlExample(instance, body, isXmlSerializerType);
            }
            catch (Exception e)
            {
                return String.Format("Could not generate example for request. Failed with error: {0}", e.Message);
            }
        }

        /// <summary>
        /// Get the request example
        /// </summary>
        /// <param name="operationDescription"></param>
        /// <returns></returns>
        public Message GetRequestExampleAsMessage(OperationDescription operationDescription)
        {
            bool isXmlSerializerType;
            Type body = OperationDescriptionUtilites.GetRequestBodyType(operationDescription, out isXmlSerializerType);

            if (IsBodySpecial(body))
                return GetSpecialBodyDocumentationAsMessage(body, "request");

            try
            {
                object instance = Activator.CreateInstance(body);
                return OperationDescriptionUtilites.GetResponseFormat(operationDescription) == "Json" ?
                    GetJsonExampleAsMessage(instance, body) :
                    GetXmlExampleAsMessage(instance, body, isXmlSerializerType);
            }
            catch (Exception e)
            {
                return GetTextMessage(String.Format("Could not generate example for request. Failed with error: {0}", e.Message));
            }
        }

        /// <summary>
        /// Get the response example
        /// </summary>
        /// <param name="operationDescription"></param>
        /// <returns></returns>
        public string GetResponseExample(OperationDescription operationDescription)
        {
            bool isXmlSerializerType;
            Type body = OperationDescriptionUtilites.GetResponseBodyType(operationDescription, out isXmlSerializerType);

            if (IsBodySpecial(body))
                return GetSpecialBodyDocumentation(body, "response");

            try
            {
                object instance = Activator.CreateInstance(body);
                return OperationDescriptionUtilites.GetResponseFormat(operationDescription) == "Json" ?
                    GetJsonExample(instance, body) :
                    GetXmlExample(instance, body, isXmlSerializerType);
            }
            catch (Exception e)
            {
                return String.Format("Could not generate example for response. Failed with error: {0}", e.Message);
            }
        }

        /// <summary>
        /// Get the response example
        /// </summary>
        /// <param name="operationDescription"></param>
        /// <returns></returns>
        public Message GetResponseExampleAsMessage(OperationDescription operationDescription)
        {
            bool isXmlSerializerType;
            Type body = OperationDescriptionUtilites.GetResponseBodyType(operationDescription, out isXmlSerializerType);

            if (IsBodySpecial(body))
                return GetSpecialBodyDocumentationAsMessage(body, "response");

            try
            {
                object instance = Activator.CreateInstance(body);
                return OperationDescriptionUtilites.GetResponseFormat(operationDescription) == "Json" ?
                    GetJsonExampleAsMessage(instance, body) :
                    GetXmlExampleAsMessage(instance, body, isXmlSerializerType);
            }
            catch (Exception e)
            {
                return GetTextMessage(String.Format("Could not generate example for response. Failed with error: {0}", e.Message));
            }
        }

        /// <summary>
        /// Get Schema for a given body
        /// </summary>
        /// <param name="body"></param>
        /// <param name="isXmlSerializerType"></param>
        /// <returns></returns>
        private static string GetXmlSchema(Type body, bool isXmlSerializerType)
        {
            System.Collections.IEnumerable schemas;
            if (isXmlSerializerType)
            {
                XmlReflectionImporter importer = new XmlReflectionImporter();
                XmlTypeMapping typeMapping = importer.ImportTypeMapping(body);
                XmlSchemas s = new XmlSchemas();
                XmlSchemaExporter exporter = new XmlSchemaExporter(s);
                exporter.ExportTypeMapping(typeMapping);
                schemas = s.GetSchemas(null);
            }
            else
            {
                XsdDataContractExporter exporter = new XsdDataContractExporter();
                exporter.Export(body);
                schemas = exporter.Schemas.Schemas();
            }
            using (MemoryStream stream = new MemoryStream())
            {
                XmlWriterSettings xws = new XmlWriterSettings { Indent = true };
                using (XmlWriter writer = XmlWriter.Create(stream, xws))
                {
                    if (writer != null)
                    {
                        writer.WriteStartElement("Schemas");
                        foreach (XmlSchema schema in schemas)
                        {
                            if (schema.TargetNamespace != "http://www.w3.org/2001/XMLSchema")
                            {
                                schema.Write(writer);
                            }
                        }
                    }
                }
                stream.Seek(0, SeekOrigin.Begin);
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Get Schema for a given body
        /// </summary>
        /// <param name="body"></param>
        /// <param name="isXmlSerializerType"></param>
        /// <returns></returns>
        private static Message GetXmlSchemaAsMessage(Type body, bool isXmlSerializerType)
        {
            System.Collections.IEnumerable schemas;
            if (isXmlSerializerType)
            {
                XmlReflectionImporter importer = new XmlReflectionImporter();
                XmlTypeMapping typeMapping = importer.ImportTypeMapping(body);
                XmlSchemas s = new XmlSchemas();
                XmlSchemaExporter exporter = new XmlSchemaExporter(s);
                exporter.ExportTypeMapping(typeMapping);
                schemas = s.GetSchemas(null);
            }
            else
            {
                XsdDataContractExporter exporter = new XsdDataContractExporter();
                exporter.Export(body);
                schemas = exporter.Schemas.Schemas();
            }
            using (MemoryStream stream = new MemoryStream())
            {
                XmlWriterSettings xws = new XmlWriterSettings { Indent = true };
                using (XmlWriter writer = XmlWriter.Create(stream, xws))
                {
                    if (writer != null)
                    {
                        writer.WriteStartElement("Schemas");
                        foreach (XmlSchema schema in schemas)
                        {
                            if (schema.TargetNamespace != "http://www.w3.org/2001/XMLSchema")
                            {
                                schema.Write(writer);
                            }
                        }
                    }
                }
                stream.Seek(0, SeekOrigin.Begin);
                using (XmlReader reader = XmlReader.Create(stream))
                {
                    return Message.CreateMessage(MessageVersion.None, null, XElement.Load(reader, LoadOptions.PreserveWhitespace));
                }
            }
        }

        /// <summary>
        /// Get request schema
        /// </summary>
        /// <param name="operationDescription"></param>
        /// <returns></returns>
        public string GetRequestXmlSchema(OperationDescription operationDescription)
        {
            bool isXmlSerializerType;
            Type body = OperationDescriptionUtilites.GetRequestBodyType(operationDescription, out isXmlSerializerType);

            if (IsBodySpecial(body))
                return GetSpecialBodyDocumentation(body,"request");
            
            try
            {
                return GetXmlSchema(body, isXmlSerializerType);
            }
            catch (Exception e)
            {
                return String.Format("Could not generate schema for request. Failed with error: {0}", e.Message);
            }
        }

        /// <summary>
        /// Get request schema
        /// </summary>
        /// <param name="operationDescription"></param>
        /// <returns></returns>
        public Message GetRequestXmlSchemaAsMessage(OperationDescription operationDescription)
        {
            bool isXmlSerializerType;
            Type body = OperationDescriptionUtilites.GetRequestBodyType(operationDescription, out isXmlSerializerType);

            if (IsBodySpecial(body))
                return GetSpecialBodyDocumentationAsMessage(body, "request");

            try
            {
                return GetXmlSchemaAsMessage(body, isXmlSerializerType);
            }
            catch (Exception e)
            {
                return GetTextMessage(String.Format("Could not generate schema for request. Failed with error: {0}", e.Message));
            }
        }

        /// <summary>
        /// Get response schema
        /// </summary>
        /// <param name="operationDescription"></param>
        /// <returns></returns>
        public string GetResponseXmlSchema(OperationDescription operationDescription)
        {
            bool isXmlSerializerType;
            Type body = OperationDescriptionUtilites.GetResponseBodyType(operationDescription, out isXmlSerializerType);

            if (IsBodySpecial(body))
                return GetSpecialBodyDocumentation(body, "response");

            try
            {
                return GetXmlSchema(body, isXmlSerializerType);
            }
            catch (Exception e)
            {
                return String.Format("Could not generate schema for request. Failed with error: {0}", e.Message);
            }
        }

        /// <summary>
        /// Get response schema
        /// </summary>
        /// <param name="operationDescription"></param>
        /// <returns></returns>
        public Message GetResponseXmlSchemaAsMessage(OperationDescription operationDescription)
        {
            bool isXmlSerializerType;
            Type body = OperationDescriptionUtilites.GetResponseBodyType(operationDescription, out isXmlSerializerType);

            if (IsBodySpecial(body))
                return GetSpecialBodyDocumentationAsMessage(body, "response");

            try
            {
                return GetXmlSchemaAsMessage(body, isXmlSerializerType);
            }
            catch (Exception e)
            {
                return GetTextMessage(String.Format("Could not generate schema for request. Failed with error: {0}", e.Message));
            }
        }

        #endregion

        #region Page Builder Methods

        /// <summary>
        /// Format the given code block
        /// </summary>
        /// <param name="codeBlock"></param>
        /// <returns></returns>
        private static void FormatCodeBlock(ref string codeBlock)
        {
            if (String.IsNullOrEmpty(codeBlock))
                return;

            String id = Guid.NewGuid().ToString();
            codeBlock = HttpUtility.HtmlEncode(codeBlock);
            codeBlock = String.Format("<div id=\"{0}\"><pre lang=\"xml\">{1}</pre></div>", id,codeBlock);
        }

        #region CSS and Javascript references

        /// <summary>
        /// Get the Execution Path
        /// </summary>
        private static string ExecutionPath
        {
            get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase).Replace("file:\\", string.Empty); }
        }

        #endregion

        /// <summary>
        /// Get ServiceDocumentation
        /// </summary>
        /// <returns></returns>
        private Message GetServiceDocumentation()
        {
            XmlDocument doc = new XmlDocument();
            
            XmlDeclaration declaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(declaration);            

            WebServiceDocAttribute docAttribute = ContractDescription.Behaviors.Find<WebServiceDocAttribute>();

            string pageTitle = docAttribute == null || String.IsNullOrEmpty(docAttribute.Title)
                                   ? String.Format("{0} REST API Documentation", ContractDescription.Name)
                                   : docAttribute.Title;

            XmlElement root = doc.CreateElement("html");
            XmlElement title = doc.CreateElement("title");
            XmlElement head = doc.CreateElement("head");
            XmlElement style = doc.CreateElement("style");
            XmlAttribute type = doc.CreateAttribute("type","text/css");
            style.Attributes.Append(type);
            style.InnerText = CssTemplate;
            title.InnerXml = pageTitle;                     

            XmlElement body = doc.CreateElement("body");
            StringBuilder bodyXml = new StringBuilder();

            bodyXml.Append("<div id=\"content\">");
            bodyXml.AppendFormat("<p class=\"heading1\">{0}</p><br/>",HttpUtility.HtmlEncode(pageTitle));
            if (docAttribute != null)
            {                
                if (!String.IsNullOrEmpty(docAttribute.Description))
                {
                    bodyXml.AppendFormat("<p class=\"intro\">{0}", HttpUtility.HtmlEncode(docAttribute.Description));
                    if (!String.IsNullOrEmpty(docAttribute.SupportLink))
                        bodyXml.AppendFormat("   <a href=\"{0}\">Report Bug</a>", HttpUtility.HtmlEncode(docAttribute.SupportLink));
                    bodyXml.Append("</p>");
                }
            }
            
            foreach (OperationDescription operationDescription in ContractDescription.Operations)
            {
                bodyXml.AppendFormat("<p class=\"intro\"><PRE>{0} : <a href=\"{1}/docs\">{2}</a>",
                    HttpUtility.HtmlEncode(operationDescription.Name),
                    HttpUtility.HtmlEncode(operationDescription.Name.ToLowerInvariant()),
                    HttpUtility.HtmlEncode(OperationDescriptionUtilites.GetUriTemplate(operationDescription).ToLowerInvariant()));
                bodyXml.AppendFormat("</PRE></p>");
            }
            bodyXml.AppendFormat("</div>");                        
            body.InnerXml = bodyXml.ToString();

            head.AppendChild(style);
            head.AppendChild(title);
            root.AppendChild(head);
            root.AppendChild(body);
            doc.AppendChild(root);

            Message message = Message.CreateMessage(MessageVersion.None,null, new XmlNodeReader(doc));
            return message;
        }

        /// <summary>
        /// Builds the Operation's documentation
        /// </summary>
        /// <param name="operationDescription"></param>
        /// <returns></returns>
        private Message GetOperationDocumentationAsMessage(OperationDescription operationDescription)
        {
            XmlDocument doc = new XmlDocument();

            XmlDeclaration declaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(declaration);

            WebServiceDocAttribute docAttribute = ContractDescription.Behaviors.Find<WebServiceDocAttribute>();

            string pageTitle = docAttribute == null || String.IsNullOrEmpty(docAttribute.Title)
                       ? String.Format("{0} REST API Documentation", ContractDescription.Name)
                       : docAttribute.Title;

            XmlElement root = doc.CreateElement("html");
            XmlElement title = doc.CreateElement("title");
            XmlElement head = doc.CreateElement("head");
            XmlElement style = doc.CreateElement("style");
            XmlAttribute type = doc.CreateAttribute("type", "text/css");
            style.Attributes.Append(type);
            style.InnerText = CssTemplate;
            title.InnerXml = HttpUtility.HtmlEncode(pageTitle);            
                   
            XmlElement body = doc.CreateElement("body");            
            StringBuilder bodyXml = new StringBuilder();

            bodyXml.Append("<div id=\"content\">");
            bodyXml.AppendFormat("<p class=\"heading1\">{0}</p><br/>", pageTitle);

            bodyXml.Append(GetOperationDocumentation(operationDescription));

            bodyXml.AppendLine("</div>");            
            body.InnerXml = bodyXml.ToString();

            head.AppendChild(style);
            root.AppendChild(head);
            root.AppendChild(body);
            doc.AppendChild(root);

            Message message = Message.CreateMessage(MessageVersion.None, null, new XmlNodeReader(doc));
            return message;
        }

        /// <summary>
        /// Get Documentation for a particular operation
        /// </summary>
        /// <param name="operationDescription"></param>
        /// <returns></returns>
        private string GetOperationDocumentation(OperationDescription operationDescription)
        {
            StringBuilder bodyXml = new StringBuilder();

            bodyXml.AppendFormat("<p class=\"heading2\"><strong>{0}</strong></p><br/>", operationDescription.Name);

            WebOperationDocAttribute docAttribute = operationDescription.Behaviors.Find<WebOperationDocAttribute>();
            if (docAttribute != null)
            {                
                if (!String.IsNullOrEmpty(docAttribute.Description))
                {
                    bodyXml.AppendFormat("<p class=\"intro\">{0}", HttpUtility.HtmlEncode(docAttribute.Description));
                    if (!String.IsNullOrEmpty(docAttribute.SupportLink))
                        bodyXml.AppendFormat("  <a href=\"{0}\">Report Bug</a>", HttpUtility.HtmlEncode(docAttribute.SupportLink));
                    else
                    {
                        WebServiceDocAttribute serviceDocAttribute = ContractDescription.Behaviors.Find<WebServiceDocAttribute>();

                        if (!String.IsNullOrEmpty(serviceDocAttribute.SupportLink))
                            bodyXml.AppendFormat("  <a href=\"{0}\">Report Bug</a>", HttpUtility.HtmlEncode(serviceDocAttribute.SupportLink));
                    }
                    bodyXml.Append("</p>");
                }
            }

            bodyXml.AppendFormat("<p class=\"intro\"><strong>Method:</strong> {0}</p>", OperationDescriptionUtilites.GetMethod(operationDescription));

            string uriTemplate = OperationDescriptionUtilites.GetUriTemplate(operationDescription);
            uriTemplate = String.Format("{0}/{1}", BaseUri.AbsoluteUri, uriTemplate);
            uriTemplate = HttpUtility.HtmlEncode(uriTemplate);
            bodyXml.AppendFormat("<p class=\"intro\"><strong>URI Template:</strong> {0}</p>", uriTemplate);            

            string requestFormat = OperationDescriptionUtilites.GetRequestFormat(operationDescription);
            if (!String.IsNullOrEmpty(requestFormat))
                bodyXml.AppendFormat("<p class=\"intro\"><strong>Request Format:</strong> {0}</p>", requestFormat);

            string requestExample = GetRequestExample(operationDescription);
            FormatCodeBlock(ref requestExample);
            if (OperationDescriptionUtilites.GetResponseFormat(operationDescription) != "Json")
            {
                bodyXml.AppendFormat("<p class=\"intro\"><strong>Request Example (<a href=\"./docs/request/schema\">schema</a>):</strong> <br/>");
            }
            else
            {
                bodyXml.AppendFormat("<p class=\"intro\"><strong>Request Example:</strong> <br/>");
            }
            bodyXml.AppendLine(requestExample);
            bodyXml.Append("</p>");

            bodyXml.AppendFormat("<p class=\"intro\"><strong>Response Format:</strong> {0}</p>", OperationDescriptionUtilites.GetResponseFormat(operationDescription));

            string responseExample = GetResponseExample(operationDescription);
            FormatCodeBlock(ref responseExample);
            if (OperationDescriptionUtilites.GetResponseFormat(operationDescription) != "Json")
            {
                bodyXml.AppendFormat("<p class=\"intro\"><strong>Response Example (<a href=\"./docs/response/schema\">schema</a>):</strong><br/>");
            }
            else
            {
                bodyXml.AppendFormat("<p class=\"intro\"><strong>Response Example:</strong> <br/>");    
            }
            bodyXml.AppendLine(responseExample);
            bodyXml.AppendLine("</p>");            

            return bodyXml.ToString();
        }

        #endregion
    }
}