﻿using Autofac;
using Microsoft.Bot.Builder.Dialogs;
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
			GlobalConfiguration.Configure(WebApiConfig.Register);

			// AutoFac
			var builder = new ContainerBuilder();
			builder.RegisterModule(new GlobalMessageHandlerModule());
			builder.Update(Conversation.Container);
		}
	}
}
