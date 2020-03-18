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

    /// <summary>
    /// Thrown when a sustainsys configuration setting is missing or invalid.
    /// Implements the <see cref="SurfNetException" />
    /// </summary>
    /// <seealso cref="SurfNetException" />
    public class InvalidConfigurationException : SurfNetException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidConfigurationException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public InvalidConfigurationException(string message) : base(message, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidConfigurationException" /> class.
        /// </summary>
        /// <param name="resourceId">The resource identifier for the localized user message.</param>
        /// <param name="message">The message that describes the error.</param>
        public InvalidConfigurationException(string resourceId, string message) : base(message, false, resourceId)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidConfigurationException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (<see langword="Nothing" /> in Visual Basic) if no inner exception is specified.</param>
        public InvalidConfigurationException(string message, Exception innerException) : base(message, false, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidConfigurationException" /> class.
        /// </summary>
        /// <param name="resourceId">The resource identifier for the localized user message.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (<see langword="Nothing" /> in Visual Basic) if no inner exception is specified.</param>
        public InvalidConfigurationException(string resourceId, string message, Exception innerException) : base(message, false, resourceId, innerException)
        {
        }
    }
}
