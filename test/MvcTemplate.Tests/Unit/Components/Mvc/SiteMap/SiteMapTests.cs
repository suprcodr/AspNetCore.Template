using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using MvcTemplate.Components.Extensions;
using MvcTemplate.Components.Security;
using MvcTemplate.Resources;
using MvcTemplate.Tests;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MvcTemplate.Components.Mvc.Tests
{
    public class SiteMapTests
    {
        private IDictionary<String, Object?> route;
        private IAuthorization authorization;
        private ViewContext context;
        private SiteMap siteMap;

        public SiteMapTests()
        {
            authorization = Substitute.For<IAuthorization>();
            siteMap = new SiteMap(CreateSiteMap(), authorization);
            context = HtmlHelperFactory.CreateHtmlHelper().ViewContext;
            IUrlHelper url = context.HttpContext.RequestServices.GetRequiredService<IUrlHelperFactory>().GetUrlHelper(context);

            url.Action(Arg.Any<UrlActionContext>()).Returns("/test");
            route = context.RouteData.Values;
        }

        [Fact]
        public void For_NoAuthorization_ReturnsAllNodes()
        {
            authorization.IsGrantedFor(Arg.Any<Int64?>(), Arg.Any<String>()).Returns(true);

            SiteMapNode[] actual = siteMap.For(context).ToArray();

            Assert.Single(actual);

            Assert.Null(actual[0].Action);
            Assert.Equal("#", actual[0].Url);
            Assert.Null(actual[0].Controller);
            Assert.Equal("Administration", actual[0].Area);
            Assert.Equal("fa fa-cogs", actual[0].IconClass);
            Assert.Equal(Resource.ForSiteMap("Administration//"), actual[0].Title);

            actual = actual[0].Children.ToArray();

            Assert.Equal(2, actual.Length);

            Assert.Empty(actual[0].Children);

            Assert.Equal("/test", actual[0].Url);
            Assert.Equal("Index", actual[0].Action);
            Assert.Equal("Accounts", actual[0].Controller);
            Assert.Equal("Administration", actual[0].Area);
            Assert.Equal("fa fa-user", actual[0].IconClass);
            Assert.Equal(Resource.ForSiteMap("Administration/Accounts/Index"), actual[0].Title);

            Assert.Null(actual[1].Action);
            Assert.Equal("#", actual[1].Url);
            Assert.Equal("Roles", actual[1].Controller);
            Assert.Equal("Administration", actual[1].Area);
            Assert.Equal("fa fa-users", actual[1].IconClass);
            Assert.Equal(Resource.ForSiteMap("Administration/Roles/"), actual[1].Title);

            actual = actual[1].Children.ToArray();

            Assert.Single(actual);
            Assert.Empty(actual[0].Children);

            Assert.Equal("/test", actual[0].Url);
            Assert.Equal("Create", actual[0].Action);
            Assert.Equal("Roles", actual[0].Controller);
            Assert.Equal("Administration", actual[0].Area);
            Assert.Equal("far fa-file", actual[0].IconClass);
            Assert.Equal(Resource.ForSiteMap("Administration/Roles/Create"), actual[0].Title);
        }

        [Fact]
        public void For_ReturnsAuthorizedNodes()
        {
            authorization.IsGrantedFor(context.HttpContext.User.Id(), "Administration/Accounts/Index").Returns(true);

            SiteMapNode[] actual = siteMap.For(context).ToArray();

            Assert.Single(actual);

            Assert.Null(actual[0].Action);
            Assert.Equal("#", actual[0].Url);
            Assert.Null(actual[0].Controller);
            Assert.Equal("Administration", actual[0].Area);
            Assert.Equal("fa fa-cogs", actual[0].IconClass);
            Assert.Equal(Resource.ForSiteMap("Administration//"), actual[0].Title);

            actual = actual[0].Children.ToArray();

            Assert.Single(actual);

            Assert.Empty(actual[0].Children);

            Assert.Equal("/test", actual[0].Url);
            Assert.Equal("Index", actual[0].Action);
            Assert.Equal("Accounts", actual[0].Controller);
            Assert.Equal("Administration", actual[0].Area);
            Assert.Equal("fa fa-user", actual[0].IconClass);
            Assert.Equal(Resource.ForSiteMap("Administration/Accounts/Index"), actual[0].Title);
        }

        [Fact]
        public void For_SetsActiveMenu()
        {
            route["action"] = "Create";
            route["controller"] = "Roles";
            route["area"] = "Administration";

            authorization.IsGrantedFor(Arg.Any<Int64?>(), Arg.Any<String>()).Returns(true);

            SiteMapNode[] actual = siteMap.For(context).ToArray();

            Assert.Single(actual);
            Assert.True(actual[0].IsActive);

            actual = actual[0].Children.ToArray();

            Assert.Equal(2, actual.Length);
            Assert.True(actual[1].IsActive);
            Assert.False(actual[0].IsActive);
            Assert.Empty(actual[0].Children);

            actual = actual[1].Children.ToArray();

            Assert.Empty(actual[0].Children);
            Assert.True(actual[0].IsActive);
            Assert.Single(actual);
        }

        [Fact]
        public void For_RemovesEmptyNodes()
        {
            authorization.IsGrantedFor(Arg.Any<Int64?>(), Arg.Any<String>()).Returns(true);
            authorization.IsGrantedFor(context.HttpContext.User.Id(), "Administration/Roles/Create").Returns(false);

            SiteMapNode[] actual = siteMap.For(context).ToArray();

            Assert.Single(actual);

            Assert.Null(actual[0].Action);
            Assert.Equal("#", actual[0].Url);
            Assert.Null(actual[0].Controller);
            Assert.Equal("Administration", actual[0].Area);
            Assert.Equal("fa fa-cogs", actual[0].IconClass);
            Assert.Equal(Resource.ForSiteMap("Administration//"), actual[0].Title);

            actual = actual[0].Children.ToArray();

            Assert.Single(actual);

            Assert.Empty(actual[0].Children);

            Assert.Equal("/test", actual[0].Url);
            Assert.Equal("Index", actual[0].Action);
            Assert.Equal("Accounts", actual[0].Controller);
            Assert.Equal("Administration", actual[0].Area);
            Assert.Equal("fa fa-user", actual[0].IconClass);
            Assert.Equal(Resource.ForSiteMap("Administration/Accounts/Index"), actual[0].Title);
        }

        [Fact]
        public void BreadcrumbFor_IsCaseInsensitive()
        {
            route["controller"] = "profile";
            route["action"] = "edit";
            route["area"] = null;

            SiteMapNode[] actual = siteMap.BreadcrumbFor(context).ToArray();

            Assert.Equal(3, actual.Length);

            Assert.Equal(Resource.ForSiteMap("/Home/Index"), actual[0].Title);
            Assert.Equal("fa fa-home", actual[0].IconClass);
            Assert.Equal("Home", actual[0].Controller);
            Assert.Equal("Index", actual[0].Action);
            Assert.Equal("/test", actual[0].Url);
            Assert.Null(actual[0].Area);

            Assert.Equal(Resource.ForSiteMap("/Profile/"), actual[1].Title);
            Assert.Equal("fa fa-user", actual[1].IconClass);
            Assert.Equal("Profile", actual[1].Controller);
            Assert.Equal("#", actual[1].Url);
            Assert.Null(actual[1].Action);
            Assert.Null(actual[1].Area);

            Assert.Equal(Resource.ForSiteMap("/Profile/Edit"), actual[2].Title);
            Assert.Equal("fa fa-pencil-alt", actual[2].IconClass);
            Assert.Equal("Profile", actual[2].Controller);
            Assert.Equal("Edit", actual[2].Action);
            Assert.Equal("/test", actual[2].Url);
            Assert.Null(actual[2].Area);
        }

        [Fact]
        public void BreadcrumbFor_NoAction_ReturnsEmpty()
        {
            route["controller"] = "profile";
            route["action"] = "edit";
            route["area"] = "area";

            Assert.Empty(siteMap.BreadcrumbFor(context));
        }

        private static String CreateSiteMap()
        {
            return @"<siteMap>
                <siteMapNode icon=""fa fa-home"" controller=""Home"" action=""Index"">
                    <siteMapNode icon=""fa fa-user"" controller=""Profile"">
                        <siteMapNode icon=""fa fa-pencil-alt"" action=""Edit"" />
                    </siteMapNode>
                    <siteMapNode menu=""true"" icon=""fa fa-cogs"" area=""Administration"">
                        <siteMapNode menu=""true"" icon=""fa fa-user"" controller=""Accounts"" action=""Index"">
                            <siteMapNode icon=""fa fa-info"" action=""Details"" route-id=""id"">
                                <siteMapNode icon=""fa fa-pencil-alt"" action=""Edit"" />
                            </siteMapNode>
                        </siteMapNode>
                        <siteMapNode menu=""true"" icon=""fa fa-users"" controller=""Roles"">
                            <siteMapNode menu=""true"" icon=""far fa-file"" action=""Create"" />
                            <siteMapNode icon=""fa fa-pencil-alt"" action=""Edit"" />
                        </siteMapNode>
                    </siteMapNode>
                </siteMapNode>
            </siteMap>";
        }
    }
}
