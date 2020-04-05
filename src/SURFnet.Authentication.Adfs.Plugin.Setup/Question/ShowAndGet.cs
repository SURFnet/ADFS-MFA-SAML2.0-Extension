using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Question
{
    public abstract class ShowAndGet<T>
    {
        public T Value { get; protected set; }
        //public T Current { get; private set; }
        public T DefaultValue { get; protected set; }

        public bool IsDefault { get; protected set; }
        public bool IsAbort { get; protected set; }
        public bool WantsDescription { get; protected set; }

        protected readonly bool HasDefault;

        private readonly string Question;

        public ShowAndGet(string question)
        {
            Question = question + ": ";
        }

        public ShowAndGet(string question , T defaultValue)
        {
            DefaultValue = defaultValue;
            Question = question + $" [{defaultValue.ToString()}]: ";
            HasDefault = true;
        }

        /// <summary>
        /// Must display the question and leave the location of the cursor ready for input.
        /// </summary>
        public virtual void Show()
        {
            QuestionIO.WriteQuestion(Question);
        }

        /// <summary>
        /// Must read the input from the Console and do check until the input is OK
        /// and return the value on T Value property.
        /// It can return false, the TVaule is not guaranteed. Check the flagfs:
        ///   - WantsDescription
        ///   - IsAbort
        /// The IsDefault flag is for "ENTER" on questions with a default Value.
        /// </summary>
        /// <returns></returns>
        public abstract bool Ask();
    }
}
