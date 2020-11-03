using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Controllers;

namespace GlobalLogistics.ActionFilters
{
    public class ArrayInputAttribute : ActionFilterAttribute
    {
        private readonly string[] _parameternames;

        public string Separator { get; set; }


        public ArrayInputAttribute(params string[] parameternames)
        {
            _parameternames = parameternames;
            Separator = ",";
        }



        public void ProcessArrayInput(ActionExecutingContext actionContext, string parametername)
        {
            if (actionContext.ActionArguments.ContainsKey(parametername))
            {
                var parameterdescriptor = actionContext.ActionDescriptor.Parameters.FirstOrDefault(p => p.Name == parametername);

                if (parameterdescriptor != null && parameterdescriptor.ParameterType.IsArray)
                {
                    var type = parameterdescriptor.ParameterType.GetElementType();
                    var parameters = String.Empty;
                    if (actionContext.RouteData.Values.ContainsKey(parametername))
                    {
                        parameters = (string)actionContext.RouteData.Values[parametername];
                    }
                    else
                    {
                        var queryString = actionContext.HttpContext.Request.QueryString;
                        //    if (queryString[parametername] != null)
                        //{
                        //    parameters = queryString[parametername];
                        //}
                    }

                    var values = parameters.Split(new[] { Separator }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(TypeDescriptor.GetConverter(type).ConvertFromString).ToArray();
                    var typedValues = Array.CreateInstance(type, values.Length);
                    values.CopyTo(typedValues, 0);
                    actionContext.ActionArguments[parametername] = typedValues;
                }
            }
        }

        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
           
            foreach (var parameterName in _parameternames)
            {
                ProcessArrayInput(actionContext, parameterName);
            }
        }
    }
}
