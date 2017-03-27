namespace SURFnet.Authentication.Service.ModelBinders
{
    using System.Collections.Specialized;
    using System.Web.Mvc;

    using Newtonsoft.Json;

    using SURFnet.Authentication.Core;

    /// <summary>
    /// Binds the post data from the plugin to a <see cref="SecondFactorAuthRequest"/>
    /// </summary>
    /// <seealso cref="System.Web.ModelBinding.IModelBinder" />
    public class SecondFactorAuthRequestModelBinder : IModelBinder
    {
        /// <summary>
        /// Binds the model.
        /// </summary>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="bindingContext">The binding context.</param>
        /// <returns>A <see cref="SecondFactorAuthRequest"/>.</returns>
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var form = controllerContext.HttpContext.Request.Form;
            if (form == null)
            {
                return null;
            }

            var result = SecondFactorAuthRequest.Deserialize(form.Get("request"));
            if (result == null)
            {
                return null;
            }

            result.AuthMethod = form.Get("AuthMethod");
            result.AdfsContext = form.Get("Context");

            return result;
        }
    }
}