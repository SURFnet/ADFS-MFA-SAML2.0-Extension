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
    using System.Text;

    /// <summary>
    /// Class YesNoAnswer.
    /// </summary>
    public class YesNoAnswer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="YesNoAnswer"/> class.
        /// </summary>
        /// <param name="defaultAnswer">The default answer.</param>
        /// <param name="description">The description.</param>
        public YesNoAnswer(DefaultAnswer defaultAnswer, StringBuilder description)
        {
            this.ReadAnswer(defaultAnswer, description);
        }

        /// <summary>
        /// Gets a value indicating whether this question is answered with the given default answer.
        /// </summary>
        /// <value><c>true</c> if this this question is answered with the given default answer; otherwise, <c>false</c>.</value>
        public bool IsDefaultAnswer { get; private set; }

        /// <summary>
        /// Gets the user input.
        /// </summary>
        /// <value>The value.</value>
        public char Value { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the user needs more information about the question.
        /// </summary>
        /// <value><c>true</c> if [help requested]; otherwise, <c>false</c>.</value>
        public bool HelpRequested { get; private set; }


        /// <summary>
        /// Reads the answer.
        /// </summary>
        /// <param name="defaultAnswer">The default answer.</param>
        /// <param name="description">The description.</param>
        private void ReadAnswer(DefaultAnswer defaultAnswer, StringBuilder description)
        {
            var defaultAnswerChar = char.ToLowerInvariant(defaultAnswer.ToString()[0]);
            var input = Console.ReadKey();
            var answer = char.ToLowerInvariant(input.KeyChar);


            if (answer != '?' &&
                answer != 'y' &&
                answer != 'n' &&
                input.Key != ConsoleKey.Enter)
            {
                this.ReadAnswer(defaultAnswer, description);
                return;
            }

            if (input.KeyChar.Equals('?'))
            {
                Console.WriteLine();
                Console.WriteLine(description);
                this.HelpRequested = true;
            }

            this.IsDefaultAnswer = input.Key.Equals(ConsoleKey.Enter) || answer.Equals(defaultAnswerChar);
            this.Value = answer;
            Console.WriteLine();
        }
    }
}
