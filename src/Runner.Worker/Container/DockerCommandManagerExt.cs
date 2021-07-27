using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using GitHub.Runner.Common;
using GitHub.Runner.Sdk;

namespace GitHub.Runner.Worker.Container
{
  public partial class DockerCommandManager
  {
    public void ParseMountVolumes(IList<string> dockerOptions, ContainerInfo container)
    {
      bool removeDockerSupport = container.ContainerEnvironmentVariables.ContainsKey("REMOVEDOCKERSUPPORT");
      foreach (var volume in container.MountVolumes)
      {
        if (removeDockerSupport && volume.SourceVolumePath == "/var/run/docker.sock")
          continue;

        // replace `"` with `\"` and add `"{0}"` to all path.
        String volumeArg;
        if (String.IsNullOrEmpty(volume.SourceVolumePath))
        {
          // Anonymous docker volume
          volumeArg = $"-v \"{volume.TargetVolumePath.Replace("\"", "\\\"")}\"";
        }
        else
        {
          // Named Docker volume / host bind mount
          volumeArg = $"-v \"{volume.SourceVolumePath.Replace("\"", "\\\"")}\":\"{volume.TargetVolumePath.Replace("\"", "\\\"")}\"";
        }
        if (volume.ReadOnly)
        {
          volumeArg += ":ro";
        }
        dockerOptions.Add(volumeArg);
      }
    }
  }
}
