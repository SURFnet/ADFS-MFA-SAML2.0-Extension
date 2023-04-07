/*
* Copyright 2017 SURFnet bv, The Netherlands
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Common.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Class SurfNetException.
    /// Implements the <see cref="System.Exception" />
    /// </summary>
    /// <seealso cref="System.Exception" />
    [Serializable]
    public abstract class SurfNetException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SurfNetException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected SurfNetException(SerializationInfo info, StreamingContext context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SurfNetException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="isTransient">if set to <c>true</c> [is transient].</param>
        protected SurfNetException(string message, bool isTransient) : base(message)
        {
            this.IsTransient = isTransient;
            this.MessageResourceId = ErrorMessageValues.DefaultErrorMessageResourcerId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SurfNetException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="isTransient">if set to <c>true</c> [is transient].</param>
        /// <param name="resourceId">The resource identifier for the localized user message.</param>
        protected SurfNetException(string message, bool isTransient, string resourceId) : base(message)
        {
            this.IsTransient = isTransient;
            this.MessageResourceId = resourceId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SurfNetException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="isTransient">if set to <c>true</c> [is transient].</param>
        /// <param name="innerException">The inner exception.</param>
        protected SurfNetException(string message, bool isTransient, Exception innerException) : base(message, innerException)
        {
            this.IsTransient = isTransient;
            this.MessageResourceId = ErrorMessageValues.DefaultErrorMessageResourcerId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SurfNetException" /> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="isTransient">if set to <c>true</c> [is transient].</param>
        /// <param name="resourceId">The resource identifier for the localized user message.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (<see langword="Nothing" /> in Visual Basic) if no inner exception is specified.</param>
        protected SurfNetException(string message, bool isTransient, string resourceId, Exception innerException) : base(message, innerException)
        {
            this.IsTransient = isTransient;
            this.MessageResourceId = resourceId;
        }

        /// <summary>
        /// Gets a value indicating whether this exception is transient.
        /// </summary>
        /// <value><c>true</c> if this exception is transient; otherwise, <c>false</c>.</value>
        public bool IsTransient { get; }

        /// <summary>
        /// Gets the message resource identifier to display a localized message to the user (if the LICD is supported, otherwise the message is in LCID 1033).
        /// </summary>
        /// <value>The message resource identifier.</value>
        public string MessageResourceId { get; }
    }
}
