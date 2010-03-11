using System.ServiceModel;
using System.ServiceModel.Web;
using Autodocs.ServiceModel.Web;
using Autodocs.WebServices.Contracts.Data;

namespace Autodocs.WebServices.Contracts.Services
{
    /// <summary>
    /// User Management Service
    /// </summary>
    [ServiceContract(Name="UserManagementService")]   
    [WebServiceDoc(Title="Users REST API Documentation",Description = "Web Services API for user management in the foo system.",SupportLink = "mailto:santthosh@notionforge.com")]
    public interface IUsers
    {
        /// <summary>
        /// Add a user in the system
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [OperationContract(Name="AddUser")]
        [WebInvoke(Method = "POST", UriTemplate = "user/add", ResponseFormat = WebMessageFormat.Xml)]
        [WebOperationDoc(Description = "Adds a new user in the system.")]
        int AddUser(User user);

        /// <summary>
        /// Update an existing user in the system
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "PUT",UriTemplate = "user/{userId}/update", ResponseFormat = WebMessageFormat.Json)]
        [WebOperationDoc(Description = "Update an existing user in the system. If UserId is invalid, null or doesn't exist it throws HTTP 404 - Not found.")]
        int UpdateUser(string userId,User user);

        /// <summary>
        /// Get the details of an existing user in the system
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebGet(UriTemplate = "user/{userId}", ResponseFormat = WebMessageFormat.Xml)]
        [WebOperationDoc(Description = "Get the details of an existing user in the system. If UserId is invalid, null or doesn't exist it throws HTTP 404 - Not found.")]
        User GetUser(string userId);

        /// <summary>
        /// Delete the user from system
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method="DELETE", UriTemplate = "user/{userId}/delete",ResponseFormat = WebMessageFormat.Xml)]
        [WebOperationDoc(Description = "DELETE an existing user in the system. If UserId is invalid, null or doesn't exist it returns false, if found and deleted returns true.")]        
        bool DeleteUser(string userId);
    }
}