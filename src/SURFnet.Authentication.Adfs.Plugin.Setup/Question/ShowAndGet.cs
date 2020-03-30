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

        public ShowAndGet(string question /*, T currentValue */)
        {
            Question = question + ": ";
        }

        public ShowAndGet(string question , /* T currentValue, */ T defaultValue)
        {
            DefaultValue = defaultValue;
            Question = question + $" [{defaultValue.ToString()}]: ";
            HasDefault = true;
        }

        public virtual void Show()
        {
            Console.Write(Question);
        }

        public abstract bool Ask();
    }
}
