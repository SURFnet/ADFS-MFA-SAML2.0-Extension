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

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Question
{
    using System;

    using SURFnet.Authentication.Adfs.Plugin.Setup.Question.SettingsQuestions;

    /// <summary>
    /// Class Question.
    /// </summary>
    /// <typeparam name="T">The answer type.</typeparam>
    public abstract class OldQuestion<T>
    {
        /// <summary>
        /// The question.
        /// </summary>
        private readonly string question;

        /// <summary>
        /// Initializes a new instance of the <see cref="OldQuestion{T}"/> class.
        /// </summary>
        /// <param name="question">The question.</param>
        protected OldQuestion(string question)
        {
            this.question = question;
            this.WriteQuestion();
        }

        /// <summary>
        /// Reads the user response.
        /// </summary>
        /// <returns><see cref="IAnswer"/>.</returns>
        public abstract T ReadUserResponse();

        /// <summary>
        /// Writes the question.
        /// </summary>
        protected void WriteQuestion()
        {
            Console.Write($"{this.question}: ");
        }
    }
}
