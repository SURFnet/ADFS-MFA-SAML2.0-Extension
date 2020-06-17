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

namespace SURFnet.Authentication.Adfs.Plugin.Extensions
{
    using System.Collections.Generic;

    using log4net;

    /// <summary>
    /// Class LogExtensions.
    /// </summary>
    public static class LogExtensions
    {
        /// <summary>
        /// Logs all items in the dictionary at INFO level.
        /// </summary>
        /// <typeparam name="T1">The type of the t1.</typeparam>
        /// <typeparam name="T2">The type of the t2.</typeparam>
        /// <param name="log">The log.</param>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="variableName">Name of the variable.</param>
        public static void InfoLogDictionary<T1, T2>(this ILog log, IDictionary<T1, T2> dictionary, string variableName)
        {
            foreach (var d in dictionary)
            {
                log.InfoFormat("{0}: '{1}'='{2}'", variableName, d.Key, d.Value);
            }
        }


        /// <summary>
        /// Logs all items in the dictionary at DEBUG level.
        /// </summary>
        /// <typeparam name="T1">The type of the t1.</typeparam>
        /// <typeparam name="T2">The type of the t2.</typeparam>
        /// <param name="log">The log.</param>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="variableName">Name of the variable.</param>
        public static void DebugLogDictionary<T1, T2>(this ILog log, IDictionary<T1, T2> dictionary, string variableName)
        {
            foreach (var d in dictionary)
            {
                log.DebugFormat("{0}: '{1}'='{2}'", variableName, d.Key, d.Value);
            }
        }
    }
}
