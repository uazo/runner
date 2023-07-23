using GitHub.DistributedTask.WebApi;
using GitHub.Runner.Worker;
using Moq;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;
using System.Threading;
using Pipelines = GitHub.DistributedTask.Pipelines;
using GitHub.DistributedTask.Pipelines.ContextData;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;

namespace GitHub.Runner.Common.Tests.Worker
{
	public class JobRunnerL0_Uazo : JobRunnerL0
	{
		private void AddActorToMessage(Pipelines.AgentJobRequestMessage message, string Actor)
		{
			DictionaryContextData gitHub = null;
			if (message.ContextData.ContainsKey("github"))
				gitHub = message.ContextData["github"] as DictionaryContextData;

			if (gitHub == null)
			{
				gitHub = new Pipelines.ContextData.DictionaryContextData();
				message.ContextData.Add("github", gitHub);
			}
			if (Actor != null)
				gitHub["actor"] = new StringContextData(Actor);
		}

		[Fact]
		[Trait("Level", "L0")]
		[Trait("Category", "Worker")]
		public async Task WorksWithRunnerJobRequestMessageType()
		{
			using (TestHostContext hc = CreateTestContext(
								overrideSettings: x => x.RequestSecuritySettings = new RequestSecuritySettings()
								{
									AllowedAuthors = new HashSet<string>() { "allowed-author" }
								}))
			{
				var message = GetMessage(JobRequestMessageTypes.RunnerJobRequest);
				AddActorToMessage(message, "allowed-author");
				await _jobRunner.RunAsync(message, _tokenSource.Token);
				Assert.Equal(TaskResult.Succeeded, _jobEc.Result);
			}
		}

		[Fact]
		[Trait("Level", "L0")]
		[Trait("Category", "Worker")]
		public async Task JobExtensionInitializeNotAllowed_WithoutConfig()
		{
			using (TestHostContext hc = CreateTestContext())
			{
				var message = GetMessage(JobRequestMessageTypes.RunnerJobRequest);
				await _jobRunner.RunAsync(message, _tokenSource.Token);

				Assert.Equal(TaskResult.Failed, _jobEc.Result);
				_stepRunner.Verify(x => x.RunAsync(It.IsAny<IExecutionContext>()), Times.Never);
			}
		}

		[Fact]
		[Trait("Level", "L0")]
		[Trait("Category", "Worker")]
		public async Task JobExtensionInitializeNotAllowed_WithoutActor()
		{
			using (TestHostContext hc = CreateTestContext(
				overrideSettings: x => x.RequestSecuritySettings = new RequestSecuritySettings()
				{
					AllowedAuthors = new HashSet<string>() { "allowed-author" }
				}))
			{
				var message = GetMessage(JobRequestMessageTypes.RunnerJobRequest);
				await _jobRunner.RunAsync(message, _tokenSource.Token);

				Assert.Equal(TaskResult.Failed, _jobEc.Result);
				_stepRunner.Verify(x => x.RunAsync(It.IsAny<IExecutionContext>()), Times.Never);
			}
		}

		[Fact]
		[Trait("Level", "L0")]
		[Trait("Category", "Worker")]
		public async Task JobExtensionInitializeNotAllowed_ActorNotAllowed()
		{
			using (TestHostContext hc = CreateTestContext(
				overrideSettings: x => x.RequestSecuritySettings = new RequestSecuritySettings()
				{
					AllowedAuthors = new HashSet<string>() { "allowed-author" }
				}))
			{
				var message = GetMessage(JobRequestMessageTypes.RunnerJobRequest);
				AddActorToMessage(message, "noallowed-author");
				await _jobRunner.RunAsync(message, _tokenSource.Token);

				Assert.Equal(TaskResult.Failed, _jobEc.Result);
				_stepRunner.Verify(x => x.RunAsync(It.IsAny<IExecutionContext>()), Times.Never);
			}
		}

		[Fact]
		[Trait("Level", "L0")]
		[Trait("Category", "Worker")]
		public async Task JobExtensionInitializeNotAllowed_ActorAllowed()
		{
			using (TestHostContext hc = CreateTestContext(
				overrideSettings: x => x.RequestSecuritySettings = new RequestSecuritySettings()
				{
					AllowedAuthors = new HashSet<string>() { "allowed-author" }
				}))
			{
				var message = GetMessage(JobRequestMessageTypes.RunnerJobRequest);
				AddActorToMessage(message, "allowed-author");
				await _jobRunner.RunAsync(message, _tokenSource.Token);

				Assert.Equal(TaskResult.Succeeded, _jobEc.Result);
				_stepRunner.Verify(x => x.RunAsync(It.IsAny<IExecutionContext>()), Times.Once);
			}
		}
	}
}
