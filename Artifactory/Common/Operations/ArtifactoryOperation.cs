﻿#if BuildMaster
using Inedo.BuildMaster.Extensibility;
using Inedo.BuildMaster.Extensibility.Credentials;
using Inedo.BuildMaster.Extensibility.Operations;
using Inedo.BuildMaster.Web;
#elif Otter
using Inedo.Otter.Extensibility;
using Inedo.Otter.Extensibility.Credentials;
using Inedo.Otter.Extensibility.Operations;
using Inedo.Otter.Extensions;
#endif
using Inedo.Diagnostics;
using Inedo.Extensions.Artifactory.Credentials;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace Inedo.Extensions.Artifactory.Operations
{
    [ScriptNamespace("Artifactory", PreferUnqualified = false)]
    public abstract class ArtifactoryOperation : ExecuteOperation, IHasCredentials<ArtifactoryCredentials>
    {
        public abstract string CredentialName { get; set; }

        [DisplayName("Base URL")]
        [Category("Credentials")]
        [ScriptAlias("BaseUrl")]
        [MappedCredential(nameof(ArtifactoryCredentials.BaseUrl))]
        public string BaseUrl { get; set; }

        [DisplayName("User name")]
        [Category("Credentials")]
        [ScriptAlias("UserName")]
        [MappedCredential(nameof(ArtifactoryCredentials.UserName))]
        public string UserName { get; set; }

        [FieldEditMode(FieldEditMode.Password)]
        [DisplayName("Password or API key")]
        [Category("Credentials")]
        [ScriptAlias("Password")]
        [MappedCredential(nameof(ArtifactoryCredentials.Password))]
        public SecureString Password { get; set; }

        protected HttpClient CreateClient()
        {
            return new ArtifactoryCredentials { BaseUrl = this.BaseUrl, UserName = this.UserName, Password = this.Password }.CreateClient();
        }

        protected async Task PostAsync(string path, object payload, Func<HttpResponseMessage, Task> handleResponse, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var client = this.CreateClient())
            using (var content = new StringContent(JsonConvert.SerializeObject(payload), InedoLib.UTF8Encoding, "application/json"))
            using (var response = await client.PostAsync(path, content, cancellationToken).ConfigureAwait(false))
            {
                await handleResponse(response).ConfigureAwait(false);
            }
        }
    }
}