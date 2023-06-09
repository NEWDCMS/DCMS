﻿using DCMS.Core;
using DCMS.Core.Domain.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;


namespace DCMS.Services.Common
{
    /// <summary>
    /// Represents the HTTP client to request jsdcms official site
    /// </summary>
    public partial class DCMSHttpClient
    {
        #region Fields

        private readonly AdminAreaSettings _adminAreaSettings;
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IStoreContext _storeContext;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public DCMSHttpClient(AdminAreaSettings adminAreaSettings,
            HttpClient client,
            IHttpContextAccessor httpContextAccessor,
            IStoreContext storeContext,
            IWebHelper webHelper)
        {
            //configure client
            client.BaseAddress = new Uri("https://www.jsdcms.com/");
            client.Timeout = TimeSpan.FromMilliseconds(5000);
            client.DefaultRequestHeaders.Add(HeaderNames.UserAgent, $"jsdcms-{DCMSVersion.CurrentVersion}");

            _adminAreaSettings = adminAreaSettings;
            _httpClient = client;
            _httpContextAccessor = httpContextAccessor;
            _storeContext = storeContext;
            _webHelper = webHelper;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Check whether the site is available
        /// </summary>
        /// <returns>The asynchronous task whose result determines that request is completed</returns>
        public virtual async Task PingAsync()
        {
            await _httpClient.GetStringAsync("/");
        }

        /// <summary>
        /// Check the current store for the copyright removal key
        /// </summary>
        /// <returns>The asynchronous task whose result contains the warning text</returns>
        public virtual async Task<string> GetCopyrightWarningAsync()
        {
            //prepare URL to request
            var url = string.Format(DCMSCommonDefaults.DCMSCopyrightWarningPath,
                _webHelper.IsLocalRequest(_httpContextAccessor.HttpContext.Request),
                _storeContext.CurrentStore.Url)
                .ToLowerInvariant();

            //get response
            return await _httpClient.GetStringAsync(url);
        }


        /// <summary>
        /// Get a response regarding available categories of marketplace extensions
        /// </summary>
        /// <returns>The asynchronous task whose result contains the result string</returns>
        public virtual async Task<string> GetExtensionsCategoriesAsync()
        {
            //prepare URL to request
            var url = DCMSCommonDefaults.DCMSExtensionsCategoriesPath.ToLowerInvariant();

            //get response
            return await _httpClient.GetStringAsync(url);
        }

        /// <summary>
        /// Get a response regarding available versions of marketplace extensions
        /// </summary>
        /// <returns>The asynchronous task whose result contains the result string</returns>
        public virtual async Task<string> GetExtensionsVersionsAsync()
        {
            //prepare URL to request
            var url = DCMSCommonDefaults.DCMSExtensionsVersionsPath.ToLowerInvariant();

            //get response
            return await _httpClient.GetStringAsync(url);
        }

        /// <summary>
        /// Get a response regarding marketplace extensions
        /// </summary>
        /// <param name="categoryId">Category identifier</param>
        /// <param name="versionId">Version identifier</param>
        /// <param name="price">Price; 0 - all, 10 - free, 20 - paid</param>
        /// <param name="searchTerm">Search term</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>The asynchronous task whose result contains the result string</returns>
        public virtual async Task<string> GetExtensionsAsync(int categoryId = 0,
            int versionId = 0, int price = 0, string searchTerm = null,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            //prepare URL to request
            var url = string.Format(DCMSCommonDefaults.DCMSExtensionsPath,
                categoryId, versionId, price, WebUtility.UrlEncode(searchTerm), pageIndex, pageSize)
                .ToLowerInvariant();

            //get response
            return await _httpClient.GetStringAsync(url);
        }

        #endregion
    }
}