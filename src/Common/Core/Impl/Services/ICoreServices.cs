﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Security;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Tasks;
using Microsoft.Common.Core.Telemetry;
using Microsoft.Common.Core.Threading;

namespace Microsoft.Common.Core.Services {
    public interface ICoreServices {
        IActionLog Log { get; }
        IFileSystem FileSystem { get; }
        ILoggingPermissions LoggingPermissions { get; }
        IProcessServices ProcessServices { get; }
        IRegistry Registry { get; }
        ISecurityService Security { get; }
        ITelemetryService Telemetry { get; }
        ITaskService Tasks { get; }
        IMainThread MainThread { get; }
    }
}
