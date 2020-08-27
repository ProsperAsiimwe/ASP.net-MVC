using System;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using System.Collections.Generic;
using MagicApps.Models;

namespace MagicApps.Infrastructure.Helpers
{
    public static class CustomHtmlHelpers
    {
        public static MvcHtmlString PageLinks(this HtmlHelper html, PagingInfo pagingInfo, Func<int, string> pageUrl)
        {
            var context = html.ViewContext.RequestContext.HttpContext;
            var qs_Collection = context.Request.QueryString;
            List<string> qs = new List<string>();

            // Sometimes querystrings are incomplete, this causes errors
            try {
                if (qs_Collection.Count > 0) {
                    foreach (string nv in qs_Collection) {
                        if (nv.ToLower() != "page") {
                            qs.Add(String.Format("{0}={1}", nv, context.Server.UrlEncode(qs_Collection[nv] ?? "0")));
                        }
                    }
                }
            }
            catch {
                // do nothing
            }

            StringBuilder result = new StringBuilder();

            result.Append(@"<ul class=""pagination"">");

            if (pagingInfo.ShowCounters) {
                result.AppendFormat(@"<li class=""pagination-label""><a>{0} to {1} of {2} {3}</a></li>", pagingInfo.ItemsFrom, pagingInfo.ItemsTo, pagingInfo.TotalItems, pagingInfo.Term);
            }

            for (int i = 1; i <= pagingInfo.TotalPages; i++)
            {
                TagBuilder li_tag = new TagBuilder("li"); // Contruct an li tag
                TagBuilder a_tag = new TagBuilder("a"); // Contruct an anchor tag

                string url = pageUrl(i);
                
                if (qs.Count > 0) {
                    if (url.Contains("?")) {
                        url += "&";
                    }
                    else {
                        url += "?";
                    }
                    url += String.Join("&", qs.ToArray());
                }

                a_tag.MergeAttribute("href", url);
                a_tag.InnerHtml = i.ToString();

                if (i == pagingInfo.CurrentPage)
                    li_tag.AddCssClass("active");

                li_tag.InnerHtml = a_tag.ToString();

                result.Append(li_tag.ToString());
            }

            result.Append("</ul>");

            return MvcHtmlString.Create(result.ToString());
        }

        public static MvcHtmlString EmailLink(this HtmlHelper htmlHelper, string emailAddress, object htmlAttributes = null)
        {
            TagBuilder tag = new TagBuilder("a");

            tag.MergeAttribute("href", String.Format("mailto:{0}", emailAddress));
            tag.MergeAttributes(new RouteValueDictionary(htmlAttributes), true);
            tag.InnerHtml = emailAddress;

            return new MvcHtmlString(tag.ToString(TagRenderMode.Normal));
        }

        public static MvcHtmlString EmailLink(this HtmlHelper htmlHelper, string emailAddress, string linkText, object htmlAttributes = null)
        {
            TagBuilder tag = new TagBuilder("a");

            tag.MergeAttribute("href", String.Format("mailto:{0}", emailAddress));
            tag.MergeAttributes(new RouteValueDictionary(htmlAttributes), true);
            tag.InnerHtml = linkText;

            return new MvcHtmlString(tag.ToString(TagRenderMode.Normal));
        }

        public static MvcHtmlString ParseList(this HtmlHelper html, string[] listItems, string cssClass = null)
        {
            TagBuilder tag = new TagBuilder("ul");

            if (!String.IsNullOrEmpty(cssClass)) {
                tag.AddCssClass(cssClass);
            }

            foreach (string item in listItems) {
                TagBuilder itemTag = new TagBuilder("li");
                itemTag.SetInnerText(item);
                tag.InnerHtml += itemTag.ToString();
            }

            return new MvcHtmlString(tag.ToString());
        }

        public static MvcHtmlString Image(this HtmlHelper htmlHelper, string imgSrc, string alternateText, object imgHtmlAttributes = null)
        {
            TagBuilder imgTag = new TagBuilder("img");
            imgTag.MergeAttribute("src", imgSrc);
            imgTag.MergeAttribute("alt", alternateText);
            imgTag.MergeAttributes(new RouteValueDictionary(imgHtmlAttributes), true);

            return new MvcHtmlString(imgTag.ToString(TagRenderMode.SelfClosing));
        }

        public static MvcHtmlString ImageLink(this HtmlHelper htmlHelper, string imgSrc, string alternateText, string actionName, string controllerName, object routeValues = null, object htmlAttributes = null, object imgHtmlAttributes = null)
        {
            UrlHelper urlHelper = ((Controller)htmlHelper.ViewContext.Controller).Url;
            
            string imgTag = htmlHelper.Image(imgSrc, alternateText, imgHtmlAttributes).ToString();
            string url = urlHelper.Action(actionName, controllerName, routeValues);

            TagBuilder imglink = new TagBuilder("a");
            imglink.MergeAttribute("href", url);
            imglink.InnerHtml = imgTag;
            imglink.MergeAttributes(new RouteValueDictionary(htmlAttributes), true);

            return new MvcHtmlString(imglink.ToString());

        }

        public static MvcHtmlString ImageLink(this HtmlHelper htmlHelper, string imgSrc, string alternateText, string url, object htmlAttributes = null, object imgHtmlAttributes = null)
        {
            UrlHelper urlHelper = ((Controller)htmlHelper.ViewContext.Controller).Url;

            string imgTag = htmlHelper.Image(imgSrc, alternateText, imgHtmlAttributes).ToString();

            TagBuilder imglink = new TagBuilder("a");
            imglink.MergeAttribute("href", url);
            imglink.InnerHtml = imgTag;
            imglink.MergeAttributes(new RouteValueDictionary(htmlAttributes), true);

            return new MvcHtmlString(imglink.ToString());

        }

        public static string AbsoluteUrl(this HtmlHelper htmlHelper, string actionName, string controllerName, object routeValues = null)
        {
            UrlHelper urlHelper = ((Controller)htmlHelper.ViewContext.Controller).Url;
            string url = urlHelper.Action(actionName, controllerName, routeValues);
            System.Web.HttpRequestBase request = urlHelper.RequestContext.HttpContext.Request;
            
            return String.Format("{0}://{1}{2}", request.Url.Scheme, request.Url.Host, url);
        }
    }
}