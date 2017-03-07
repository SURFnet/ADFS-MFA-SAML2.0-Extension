// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObjectExtensions.cs" company="Winvision bv">
//   Copyright (c) Winvision bv.  All rights reserved.
// </copyright>
// <summary>
//   Contains extensions for objects.
// </summary>
// --------------------------------------------------------------------------------------------------------------------


namespace SURFnet.Authentication.Core.Extensions
{
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;

    /// <summary>
    /// Contains extensions for objects.
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Converts the object to a byte array.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>SA byte array.</returns>
        public static byte[] ToByteArray(this object obj)
        {
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
    }
}
