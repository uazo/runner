using GitHub.Runner.Common.Util;
using GitHub.Runner.Sdk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;

namespace GitHub.Runner.Common
{
  public sealed partial class RunnerSettings
  {
    [DataMember(Name = "RequestSecurity", EmitDefaultValue = false)]
    public RequestSecuritySettings RequestSecuritySettings { get; set; }
  }

  [DataContract]
  public sealed class RequestSecuritySettings
  {
    // RequestSecurity is not optional in the config
    [DataMember(EmitDefaultValue = false)]
    public HashSet<string> AllowedAuthors = new HashSet<string>();

    [DataMember(EmitDefaultValue = false)]
    public bool AllowContributors = false;
  }
}
