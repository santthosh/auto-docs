using System;
using System.Runtime.Serialization;

namespace Autodocs.WebServices.Contracts.Data
{
    /// <summary>
    /// Object to hold a Twitter user
    /// </summary>
    [DataContract]
    public class User
    {
        [DataMember]
        public ulong Id;

        [DataMember]
        public string Name;       
    }
}