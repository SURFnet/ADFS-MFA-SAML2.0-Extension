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
namespace SURFnet.Authentication.Adfs.Plugin.Setup
{
    using System;

    /// <summary>
    /// Class ConsoleWriter.
    /// </summary>
    public static class ConsoleExtensions
    {
        /// <summary>
        /// Writes the header.
        /// </summary>
        /// <param name="text">The text.</param>
        public static void WriteHeader(string text)
        {
            Console.WriteLine();
            var totalLength = 75;
            var padding = totalLength - text.Length;
            var paddLeft = padding / 2 + text.Length;
            Console.WriteLine(text.PadLeft(paddLeft, '-').PadRight(totalLength, '-'));
            Console.WriteLine();
        }

        /// <summary>
        /// Reads the user input as int.
        /// </summary>
        /// <param name="minRange">The minimum range.</param>
        /// <param name="maxRange">The maximum range.</param>
        /// <returns>The user input.</returns>
        public static int ReadUserInputAsInt(int minRange, int maxRange)
        {
            bool isInvalid;
            var value = 0;
            do
            {
                isInvalid = false;
                var input = Console.ReadKey();
                Console.WriteLine();
                if (!char.IsNumber(input.KeyChar) || !int.TryParse(input.KeyChar.ToString(), out value))
                {
                    Console.Write($"Enter a numeric value: ");
                    isInvalid = true;
                }
                else if (value < minRange || value > maxRange)
                {
                    Console.Write($"Enter a value between {minRange} and {maxRange}: ");
                    isInvalid = true;
                }
            }
            while (isInvalid);

            return value;
        }
    }
}
