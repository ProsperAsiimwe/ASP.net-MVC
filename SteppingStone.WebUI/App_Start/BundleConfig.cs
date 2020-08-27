using System.Web;
using System.Web.Optimization;

namespace SteppingStone.WebUI
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/jquery.number.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*",
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/mvcfoolproof.unobtrusive*"));

            bundles.Add(new ScriptBundle("~/bundles/jquery-ui").Include(
                       "~/Scripts/jquery-ui.js"));

            bundles.Add(new ScriptBundle("~/bundles/sitescripts").Include(
                        "~/Scripts/site/jquery.knob.min.js",
                        "~/Scripts/site/sparkline.js",
                        "~/Scripts/site/script.js",
                        "~/Scripts/admin/admin-modal.js",
                        "~/Scripts/admin/numbers.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/bootstrap-datepicker.js",
                      "~/Scripts/TwitterBootstrapMvcJs.js",
                      "~/Scripts/bootstrap-tabcollapse.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/styles/core").Include(
                      "~/Content/css/core/bootstrap.css",
                      "~/Content/css/core/font-awesome.css",
                      "~/Content/css/core/bootstrap-datepicker.css",
                      "~/Content/css/core/bootstrap-custom.css"));

            bundles.Add(new StyleBundle("~/Content/styles/site").Include(
                      "~/Content/css/site/style.css",
                      "~/Content/css/site/style-custom.css",
                      "~/Content/css/site/content.css"));
        }
    }
}
