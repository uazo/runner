using GitHub.DistributedTask.Pipelines.ContextData;
using GitHub.Runner.Common;

namespace GitHub.Runner.Worker
{
  public sealed partial class JobRunner
  {
    private bool CheckPermissions(IExecutionContext jobContext)
    {
      if (!JobPassesSecurityRestrictions(jobContext))
      {
        var configurationStore = HostContext.GetService<IConfigurationStore>();
        RunnerSettings settings = configurationStore.GetSettings();
        jobContext.Error($"Running job on worker {settings.AgentName} disallowed by security policy");
        return false;
      }

      return true;
    }

    private bool JobPassesSecurityRestrictions(IExecutionContext jobContext)
    {
      var gitHubContext = jobContext.ExpressionValues["github"] as GitHubContext;

      try
      {
        return OkayToRun(gitHubContext);
      }
      catch //(Exception ex)
      {
        Trace.Error("You are not allowing this job to run");
        return false;
      }
    }

    private bool OkayToRun(GitHubContext gitHubContext)
    {
      var configStore = HostContext.GetService<IConfigurationStore>();
      var settings = configStore.GetSettings();
      var prSecuritySettings = settings.RequestSecuritySettings;

      if (prSecuritySettings == null)
      {
        Trace.Info("No RequestSecurity defined in settings, not allowing this run");
        return false;
      }

      // Actor is the user who performed the run.
      var actor = gitHubContext.TryGetValue("actor", out var value)
        ? value as StringContextData : null;

      Trace.Info($"GitHub PR actor is {actor}");

      if (actor == null)
      {
        Trace.Info("Unable to get actor, not allowing to run");
        return false;
      }

      if (prSecuritySettings.AllowedAuthors.Contains(actor))
      {
        Trace.Info($"Author {actor} in allowed list");
        return true;
      }
      else
      {
        Trace.Info($"Not running job as actor ({actor}) is not in allowed authors");

        return false;
      }
    }
  }
}
