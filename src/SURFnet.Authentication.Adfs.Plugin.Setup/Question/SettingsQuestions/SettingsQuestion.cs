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

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Question.SettingsQuestions
{
    using System;
    using System.Text;

    /// <summary>
    /// Class SettingsQuestion.
    /// </summary>
    /// <typeparam name="T">The answer type.</typeparam>
    public class SettingsQuestion<T> : YesNoQuestion where T : IAnswer, new()
    {
        /// <summary>
        /// The setting name.
        /// </summary>
        private readonly string settingName;

        /// <summary>
        /// if set to <c>true</c> an answer is required
        /// </summary>
        private readonly bool required;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsQuestion{T}"/> class.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="required">if set to <c>true</c> an answer is required.</param>
        /// <param name="currentValue">The current setting value.</param>
        /// <param name="description">The description.</param>
        public SettingsQuestion(string settingName, bool required, string currentValue, string[] description)
            : base($"{settingName}: {currentValue}. Change this setting?", description, YesNo.No)
        {
            this.settingName = settingName;
            this.required = required;
        }

        /// <summary>
        /// Reads the user response.
        /// </summary>
        /// <returns>The user response.</returns>
        public new string ReadUserResponse()
        {
            if (!VersionDetector.IsCleanInstall())
            {
                var answer = base.ReadUserResponse();
                if (answer.IsDefaultAnswer)
                {
                    return null;
                }
            }

            Console.WriteLine();
            var question = new T
            {
                DisplayName = this.settingName,
                IsMandatory = this.required
            };

            return question.ReadUserReponse();
        }
    }
}
