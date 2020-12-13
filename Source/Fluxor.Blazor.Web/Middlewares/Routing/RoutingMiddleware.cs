using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using System;
using System.Threading.Tasks;

namespace Fluxor.Blazor.Web.Middlewares.Routing
{
	/// <summary>
	/// Adds support for routing <see cref="Microsoft.AspNetCore.Components.NavigationManager"/>
	/// via a Fluxor store.
	/// </summary>
	internal class RoutingMiddleware : Middleware
	{
		private readonly NavigationManager NavigationManager;
		private readonly IState<RoutingState> State;
		private IDispatcher Dispatcher;

		/// <summary>
		/// Creates a new instance of the routing middleware
		/// </summary>
		/// <param name="navigationManager">Uri helper</param>
		/// <param name="state">The routing feature</param>
		public RoutingMiddleware(NavigationManager navigationManager, IState<RoutingState> state)
		{
			NavigationManager = navigationManager;
			State = state;
			NavigationManager.LocationChanged += LocationChanged;
		}

		/// <see cref="IMiddleware.InitializeAsync(IStore)"/>
		public override Task InitializeAsync(IDispatcher dispatcher, IStore store)
		{
			Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
			// If the URL changed before we initialized then dispatch an action
			Dispatcher.Dispatch(new GoAction(NavigationManager.Uri));
			return Task.CompletedTask;
		}

		/// <see cref="Middleware.OnInternalMiddlewareChangeEnding"/>
		protected override void OnInternalMiddlewareChangeEnding()
		{
			if (State.Value.Uri != NavigationManager.Uri && State.Value.Uri != null)
				NavigationManager.NavigateTo(State.Value.Uri);
		}

		private void LocationChanged(object sender, LocationChangedEventArgs e)
		{
			if (Dispatcher != null && !IsInsideMiddlewareChange && e.Location != State.Value.Uri)
				Dispatcher.Dispatch(new GoAction(e.Location));
		}
	}
}
