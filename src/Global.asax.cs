using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.History;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace XamUBot
{
	public class WebApiApplication : System.Web.HttpApplication
	{
		protected void Application_Start()
		{
			Conversation.UpdateContainer(builder =>
			{
				builder.RegisterModule<GlobalMessageHandlerModule>();

                // a custom logger
                builder.RegisterType<DebugActivityLogger>().AsImplementedInterfaces().InstancePerDependency();
                // History namespace reference
                //https://docs.botframework.com/en-us/csharp/builder/sdkreference/dc/dc6/namespace_microsoft_1_1_bot_1_1_builder_1_1_history.html

                // if we want to show the full trace of an activity JSON you can include this
                //builder.RegisterType<TraceActivityLogger>().AsImplementedInterfaces().InstancePerDependency();
            });

			GlobalConfiguration.Configure(WebApiConfig.Register);
		}
	}
}
