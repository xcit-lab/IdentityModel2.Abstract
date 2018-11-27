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
	/// A base class for Http clients.
	/// </summary>
	public abstract class AbstractHttpClient : HttpMessageInvoker
	{
		/// <summary>
		/// Gets or sets the base address of Uniform Resource Identifier (URI) of the Internet resource 
		/// used when sending requests.
		/// </summary>
		public virtual Uri BaseAddress { get; set; }

		/// <summary>
		/// Gets the headers which should be sent with each request.
		/// </summary>
		public virtual HttpRequestHeaders DefaultRequestHeaders { get; } = CreateHttpRequestHeaders();

		/// <summary>
		/// Initializes a new instance of the AbstractHttpClient class with a specific handler.
		/// </summary>
		/// <param name="handler">The HTTP handler stack to use for sending requests.</param>
		public AbstractHttpClient(HttpMessageHandler handler) : base(handler)
		{
		}

		/// <summary>
		/// Initializes a new instance of the HttpClient class with a specific handler.
		/// </summary>
		/// <param name="handler">The HttpMessageHandler responsible for processing the HTTP response messages.</param>
		/// <param name="disposeHandler">true if the inner handler should be disposed of by HttpClient.Dispose, 
		/// false if you intend to reuse the inner handler.</param>
		public AbstractHttpClient(HttpMessageHandler handler, bool disposeHandler) : base(handler, disposeHandler)
		{
		}

		/// <summary>
		/// Send a GET request to the specified Uri with a cancellation token as an asynchronous operation.
		/// </summary>
		/// <param name="requestUri">The Uri the request is sent to.</param>
		/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to 
		/// receive notice of cancellation.</param>
		/// <returns>The task object representing the asynchronous operation.</returns>
		public virtual Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken)
		{
			return SendAsync(new HttpRequestMessage(HttpMethod.Get, requestUri), cancellationToken);
		}

		/// <summary>
		/// Send an HTTP request as an asynchronous operation.
		/// </summary>
		/// <param name="request">The HTTP request message to send.</param>
		/// <param name="cancellationToken">The cancellation token to cancel operation.</param>
		/// <returns>The task object representing the asynchronous operation.</returns>
		public override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			InjectRequestProperties(request);

			return base.SendAsync(request, cancellationToken);
		}

		/// <summary>
		/// Injects the values of RequestUri and DefaultRequestHeaders into a Http request message.
		/// </summary>
		/// <param name="request">The HTTP request message to modify.</param>
		protected virtual void InjectRequestProperties(HttpRequestMessage request)
		{
			// From https://github.com/mono/mono/blob/master/mcs/class/System.Net.Http/System.Net.Http/HttpClient.cs#L260

			Uri uri = request.RequestUri;

			if (uri == null)
			{
				if (BaseAddress == null)
					throw new InvalidOperationException("The request URI must either be an absolute URI or BaseAddress must be set");

				request.RequestUri = BaseAddress;
			}
			else if (!uri.IsAbsoluteUri || uri.Scheme == Uri.UriSchemeFile && uri.OriginalString.StartsWith("/", StringComparison.Ordinal))
			{
				if (BaseAddress == null)
					throw new InvalidOperationException("The request URI must either be an absolute URI or BaseAddress must be set");

				request.RequestUri = new Uri(BaseAddress, uri);
			}

			if (DefaultRequestHeaders != null)
			{
				// From https://github.com/mono/mono/blob/master/mcs/class/System.Net.Http/System.Net.Http.Headers/HttpRequestHeaders.cs#L315

				foreach (var header in DefaultRequestHeaders)
				{
					request.Headers.TryAddWithoutValidation(header.Key, header.Value);
				}
			}
		}

		private static HttpRequestHeaders CreateHttpRequestHeaders()
		{
			using (HttpRequestMessage req = new HttpRequestMessage())
			{
				return req.Headers;
			}
		}
	}
}
