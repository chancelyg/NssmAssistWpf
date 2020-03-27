using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NssmAssistWpf
{
    /// <summary>
    /// Service Class Entity
    /// Author: Chancel.Yang
    /// </summary>
    public class ProgramArgsEntity
    {
        public string ProgramTitle { get; set; }
        public NSSMInfoEntity NSSMInfo { get; set; }
        public ServiceInfoEntity[] Services { get; set; }
    }

    public class NSSMInfoEntity
    {
        public string Url { get; set; }
        public string HttpAuthBasic { get; set; }

    }
    public class ServiceInfoEntity
    {
        public string ServiceName { get; set; }
        public string ServiceAlias { get; set; }
        public string ServiceInstallStatus { get; set; }
        public string ServiceRunningStatus { get; set; }
        public string ServiceProgramPath { get; set; }
        public string ServiceProgramName { get; set; }
        public string HttpAuthBasic { get; set; }
        public string DependentService { get; set; }


    }
}
