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

namespace SURFnet.Authentication.Adfs.Plugin.Exceptions
{
    using System;

    /// <summary>
    /// Thrown when there are errors returned by the SFO endpoint.
    /// Implements the <see cref="SURFnet.Authentication.Adfs.Plugin.Exceptions.SurfNetException" />
    /// </summary>
    /// <seealso cref="SURFnet.Authentication.Adfs.Plugin.Exceptions.SurfNetException" />
    public class StepUpExceptions : SurfNetException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StepUpExceptions" /> class.
        /// </summary>
        /// <param name="resourceId">The resource identifier for the localized user message.</param>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="isTransient">if set to <c>true</c> [is transient].</param>
        public StepUpExceptions(string resourceId, string message, bool isTransient) : base(message, isTransient, resourceId)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StepUpExceptions" /> class.
        /// </summary>
        /// <param name="resourceId">The resource identifier for the localized user message.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="isTransient">if set to <c>true</c> [is transient].</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (<see langword="Nothing" /> in Visual Basic) if no inner exception is specified.</param>
        public StepUpExceptions(string resourceId, string message, bool isTransient, Exception innerException) : base(message, isTransient, resourceId, innerException)
        {
        }
    }
}
