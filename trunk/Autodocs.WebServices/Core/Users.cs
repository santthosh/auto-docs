using System;
using System.ServiceModel;
using Autodocs.WebServices.Contracts.Data;
using Autodocs.WebServices.Contracts.Services;

namespace Autodocs.WebServices.Core
{
    [ServiceBehavior(Namespace = "http://autodocs.notionforge.com", AddressFilterMode = AddressFilterMode.Any)]    
    public class Users : IUsers
    {
        #region Implementation of IUsers

        /// <summary>
        /// Add a user in the system
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public int AddUser(User user)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Update an existing user in the system
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public int UpdateUser(string userId, User user)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get the details of an existing user in the system
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public User GetUser(string userId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Delete the user from system
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool DeleteUser(string userId)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

}