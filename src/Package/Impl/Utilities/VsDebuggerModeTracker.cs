﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using EnvDTE;
using Microsoft.Common.Core;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Debugger;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.Utilities {
    [Export]
    [Export(typeof(IDebuggerModeTracker))]
    internal class VsDebuggerModeTracker : IDebuggerModeTracker, IVsDebuggerEvents {
        public int OnModeChange(DBGMODE dbgmodeNew) {
            if (IsInBreakMode) {
                LeaveBreakMode?.Invoke(this, EventArgs.Empty);
            }

            IsInBreakMode = dbgmodeNew == DBGMODE.DBGMODE_Break;
            IsDebugging = dbgmodeNew != DBGMODE.DBGMODE_Design;

            if (IsInBreakMode) {
                EnterBreakMode?.Invoke(this, EventArgs.Empty);
            }

            return VSConstants.S_OK;
        }

        public bool IsFocusStolenOnBreak =>
#if VS14
            true;
#else
            false;
#endif

        public bool IsInBreakMode { get; private set; }

        public bool IsDebugging { get; private set; }

        public event EventHandler EnterBreakMode;

        public event EventHandler LeaveBreakMode;

        public bool IsRDebugger() {
            DTE dte = VsAppShell.Current.GetGlobalService<DTE>();
            var process2 = dte?.Debugger?.CurrentProcess as EnvDTE80.Process2;
            var transportId = process2?.Transport?.ID;
            Guid transportGuid;
            if (!string.IsNullOrEmpty(transportId) && Guid.TryParse(transportId, out transportGuid)) {
                return transportGuid == DebuggerGuids.PortSupplier;
            }
            return false;
        }
    }
}