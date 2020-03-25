using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NssmAssistUI
{
    /// <summary>
    /// Service Class Entity
    /// Author: Chancel.Yang
    /// </summary>
    public class ServiceInfoEntity
    {
        public ServiceInfoEntity() { }

        public string ServiceName { get; set; }

        public string ServiceProgramPath { get; set; }
        public string ServiceProcessAlias { get; set; }
    }
}
