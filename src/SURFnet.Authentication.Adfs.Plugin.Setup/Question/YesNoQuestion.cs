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
    using System.Text;

    /// <summary>
    /// Class YesNoQuestion.
    /// </summary>
    public class YesNoQuestion : Question<YesNoAnswer>
    {
        /// <summary>
        /// The description to show as help.
        /// </summary>
        private readonly StringBuilder description;

        /// <summary>
        /// The default answer.
        /// </summary>
        private readonly DefaultAnswer defaultAnswer;

        /// <summary>
        /// Initializes a new instance of the <see cref="YesNoQuestion"/> class.
        /// </summary>
        /// <param name="question">The question.</param>
        /// <param name="defaultAnswer">The default answer.</param>
        public YesNoQuestion(string question, DefaultAnswer defaultAnswer) : base($"{question} (Y/N)[{defaultAnswer.ToString()[0]}]")
        {
            this.defaultAnswer = defaultAnswer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YesNoQuestion"/> class.
        /// </summary>
        /// <param name="question">The question.</param>
        /// <param name="description">The description.</param>
        /// <param name="defaultAnswer">The default answer.</param>
        public YesNoQuestion(string question, StringBuilder description, DefaultAnswer defaultAnswer) : base($"{question} (Y/N/?)[{defaultAnswer.ToString()[0]}]")
        {
            this.description = description;
            this.defaultAnswer = defaultAnswer;
        }

        /// <summary>
        /// Reads the user response.
        /// </summary>
        /// <returns><see cref="T:SURFnet.Authentication.Adfs.Plugin.Setup.Question.SettingsQuestions.IAnswer" />.</returns>
        public override YesNoAnswer ReadUserResponse()
        {
            YesNoAnswer answer;
            do
            {
                answer = new YesNoAnswer(this.defaultAnswer, this.description);
                if (answer.HelpRequested)
                {
                    this.WriteQuestion();
                }
            }
            while (answer.HelpRequested);

            return answer;
        }
    }
}
