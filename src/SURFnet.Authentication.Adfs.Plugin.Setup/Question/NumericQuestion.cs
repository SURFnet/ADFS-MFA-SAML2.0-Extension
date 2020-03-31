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

    /// <summary>
    /// Class NumericQuestion.
    /// </summary>
    public class NumericQuestion : OldQuestion<int>
    {
        /// <summary>
        /// The minimum value.
        /// </summary>
        private readonly int minValue;

        /// <summary>
        /// The maximum value.
        /// </summary>
        private readonly int maxValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="NumericQuestion"/> class.
        /// </summary>
        /// <param name="question">The question.</param>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        public NumericQuestion(string question, int minValue, int maxValue) : base(question)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
        }

        /// <summary>
        /// Reads the user response.
        /// </summary>
        /// <returns><see cref="T:SURFnet.Authentication.Adfs.Plugin.Setup.Question.SettingsQuestions.IAnswer" />.</returns>
        public override int ReadUserResponse()
        {
            var input = this.ReadUserInputAsInt();
            return input;
        }

        /// <summary>
        /// Reads the user input as int.
        /// </summary>
        /// <returns>The user input as int.</returns>
        public int ReadUserInputAsInt()
        {
            bool isInvalid;
            var value = 0;
            do
            {
                isInvalid = false;
                var input = QuestionIO.ReadKey();
                Console.WriteLine();
                if (!char.IsNumber(input.KeyChar) || !int.TryParse(input.KeyChar.ToString(), out value))
                {
                    Console.Write($"Enter a numeric value: ");
                    isInvalid = true;
                }
                else if (value < this.minValue || value > this.maxValue)
                {
                    Console.Write($"Enter a value between {this.minValue} and {this.maxValue}: ");
                    isInvalid = true;
                }
            }
            while (isInvalid);

            return value;
        }
    }
}
