// Copyright (c) xCIT (https://www.xcit.org). All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityModel.Client
{
	/// <summary>
	/// An implementation of AbstractHttpClient that delegates operations to <see cref="System.Net.Http.HttpClient"/>.
	/// </summary>
	public class DefaultHttpClient : AbstractHttpClient
	{
		/// <summary>
		/// A HTTP message handler that forwards operations to a HttpClient.
		/// </summary>
		private class HttpClientHandler : HttpMessageHandler
		{
			public HttpClient HttpClient { get; private set; }

			private bool _disposeClient;

			public HttpClientHandler(HttpClient httpClient, bool disposeClient)
			{
				this.HttpClient = httpClient;
				this._disposeClient = disposeClient;
			}

			protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
				=> HttpClient.SendAsync(request, cancellationToken);

			protected override void Dispose(bool disposing)
			{
				base.Dispose(disposing);

				if (disposing && _disposeClient)
				{
					HttpClient.Dispose();
				}
			}
		}

		private HttpClientHandler _handler;
		private bool _disposeClient;

		/// <summary>
		/// Initializes a new instance of the DefaultHttpClient class, using a new HttpClient as backend..
		/// </summary>
		public DefaultHttpClient() 
			: this(new HttpClient())
		{ }

		/// <summary>
		/// Initializes a new instance of the DefaultHttpClient class, using a specific HttpClient as backend.
		/// </summary>
		/// <param name="httpClient">The HTTP client instance to use.</param>
		public DefaultHttpClient(HttpClient httpClient) 
			: this(httpClient, false)
		{ }

		/// <summary>
		/// Initializes a new instance of the DefaultHttpClient class, using a specific HttpClient as backend.
		/// </summary>
		/// <param name="httpClient">The HTTP client instance to use.</param>
		/// <param name="disposeClient">true to dispose the client when Dispose(bool) is called, false otherwise.</param>
		public DefaultHttpClient(HttpClient httpClient, bool disposeClient)
			: this(new HttpClientHandler(httpClient, disposeClient))
		{
			this._disposeClient = disposeClient;
		}

		private DefaultHttpClient(HttpClientHandler handler)
			: base(handler, true)
		{
			this._handler = handler;
		}

		/// <summary>
		/// Gets or sets the base address of Uniform Resource Identifier (URI) of the Internet resource 
		/// used when sending requests.
		/// </summary>
		public override Uri BaseAddress
		{
			get => _handler.HttpClient.BaseAddress;
			set => _handler.HttpClient.BaseAddress = value;
		}

		/// <summary>
		/// Gets the headers which should be sent with each request.
		/// </summary>
		public override HttpRequestHeaders DefaultRequestHeaders => _handler.HttpClient.DefaultRequestHeaders;

		/// <summary>
		/// Send a GET request to the specified Uri with a cancellation token as an asynchronous operation. 
		/// This implementation forwards the call to the underlying <see cref="HttpClient"/>.
		/// </summary>
		/// <param name="requestUri">The Uri the request is sent to.</param>
		/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to 
		/// receive notice of cancellation.</param>
		/// <returns>The task object representing the asynchronous operation.</returns>
		public override Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken) 
			=> _handler.HttpClient.GetAsync(requestUri, cancellationToken);

		/// <summary>
		/// Send an HTTP request as an asynchronous operation.
		/// This implementation forwards the call to the underlying <see cref="HttpClient"/>.
		/// </summary>
		/// <param name="request">The HTTP request message to send.</param>
		/// <param name="cancellationToken">The cancellation token to cancel operation.</param>
		/// <returns>The task object representing the asynchronous operation.</returns>
		public override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) 
			=> _handler.HttpClient.SendAsync(request, cancellationToken);
	}
}
