using Autofac;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Scorables;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XamUBot.Dialogs;

namespace XamUBot
{
	public class GlobalMessageHandlerModule : Autofac.Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			base.Load(builder);

			// Register a handler for general input which gets triggered if the bot
			// receives certain keywords.
			builder
				.Register(c => new GlobalHandlerDialog(c.Resolve<IDialogTask>()))
				.As<IScorable<IActivity, double>>()
				.InstancePerLifetimeScope();
		}
	}
}