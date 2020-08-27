using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MagicApps.Infrastructure.Helpers
{
    public class ControllerHelper
    {
        ControllerContext controller;
        
        public ControllerHelper(ControllerContext controllerContext)
        {
            this.controller = controllerContext;
        }

        public List<string> GetModelStateErrors()
        {
            List<string> errors = new List<string>();

            foreach (ModelState modelState in controller.Controller.ViewData.ModelState.Values) {
                foreach (ModelError error in modelState.Errors) {
                    errors.Add(error.ErrorMessage);
                }
            }

            return errors;
        }


        public string FormatModelStataErrors()
        {
            List<string> errors = GetModelStateErrors();
            return "<ul><li>" + string.Join("</li><li>", errors.ToArray()) + "</li></ul>";
        }

        public String RenderRazorViewToString(String viewName, Object model)
        {
            controller.Controller.ViewData.Model = model;

            using (var sw = new StringWriter()) {
                var ViewResult = ViewEngines.Engines.FindPartialView(controller, viewName);
                var ViewContext = new ViewContext(controller, ViewResult.View, controller.Controller.ViewData, controller.Controller.TempData, sw);
                ViewResult.View.Render(ViewContext, sw);
                ViewResult.ViewEngine.ReleaseView(controller, ViewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }
    }
}