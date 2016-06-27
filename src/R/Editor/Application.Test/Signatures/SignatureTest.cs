﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Signatures;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.R.Support.Help.Functions;
using Microsoft.R.Support.Test.Utility;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Language.Intellisense;
using Xunit;

namespace Microsoft.R.Editor.Application.Test.Signatures {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class SignatureTest : IDisposable {
        private readonly IExportProvider _exportProvider;
        private readonly EditorHostMethodFixture _editorHost;

        public SignatureTest(REditorApplicationMefCatalogFixture catalogFixture, EditorHostMethodFixture editorHost) {
            _exportProvider = catalogFixture.CreateExportProvider();
            _editorHost = editorHost;
        }

        public void Dispose() {
            _exportProvider.Dispose();
        }

        [Test]
        [Category.Interactive]
        public async Task R_SignatureParametersMatch() {
            using (var script = await _editorHost.StartScript(_exportProvider, RContentTypeDefinition.ContentType)) {
                FunctionRdDataProvider.HostStartTimeout = 10000;
                using (new RHostScript(_exportProvider.GetExportedValue<IRSessionProvider>())) {
                    var functionIndex = PrepareFunctionIndex();
                    FunctionIndexUtility.GetFunctionInfoAsync(functionIndex, "lm").Wait(3000);

                    script.Type("x <- lm(");
                    script.DoIdle(2000);

                    ISignatureHelpSession session = script.GetSignatureSession();
                    session.Should().NotBeNull();
                    IParameter parameter = session.SelectedSignature.CurrentParameter;
                    parameter.Should().NotBeNull();

                    parameter.Name.Should().Be("formula");

                    script.Type("sub");
                    script.DoIdle(500);
                    script.Type("{TAB}");
                    script.DoIdle(1000);

                    parameter = session.SelectedSignature.CurrentParameter;
                    parameter.Name.Should().Be("subset");

                    string actual = script.EditorText;
                    actual.Should().Be("x <- lm(subset = )");

                    session = script.GetSignatureSession();
                    parameter = session.SelectedSignature.CurrentParameter;
                }
            }
        }

        [Test]
        [Category.Interactive]
        public async Task R_SignatureSessionNavigation() {
            using (var script = await _editorHost.StartScript(_exportProvider, RContentTypeDefinition.ContentType)) {
                FunctionRdDataProvider.HostStartTimeout = 10000;
                using (new RHostScript(_exportProvider.GetExportedValue<IRSessionProvider>())) {
                    var functionIndex = PrepareFunctionIndex();
                    FunctionIndexUtility.GetFunctionInfoAsync(functionIndex, "lm").Wait(3000);

                    script.Type("x <- lm(subset = a, sing");
                    script.DoIdle(1000);
                    script.Type("{TAB}");
                    script.DoIdle(1000);

                    ISignatureHelpSession session = script.GetSignatureSession();
                    session.Should().NotBeNull();

                    script.DoIdle(200);
                    IParameter parameter = session.SelectedSignature.CurrentParameter;
                    parameter.Should().NotBeNull();
                    parameter.Name.Should().Be("singular.ok");

                    script.MoveLeft(17);
                    script.DoIdle(200);
                    parameter = session.SelectedSignature.CurrentParameter;
                    parameter.Name.Should().Be("subset");

                    script.MoveRight(3);
                    script.DoIdle(200);
                    parameter = session.SelectedSignature.CurrentParameter;
                    parameter.Name.Should().Be("singular.ok");
                }
            }
        }

        [Test]
        [Category.Interactive]
        public async Task R_EqualsCompletion01() {
            using (var script = await _editorHost.StartScript(_exportProvider, RContentTypeDefinition.ContentType)) {
                var functionIndex = PrepareFunctionIndex();
                FunctionIndexUtility.GetFunctionInfoAsync(functionIndex, "addmargins").Wait(3000);

                script.DoIdle(100);
                script.Type("addmargins(FU");
                script.DoIdle(300);
                script.Type("=");
                script.DoIdle(300);

                string expected = "addmargins(FUN = )";
                string actual = script.EditorText;

                actual.Should().Be(expected);
            }
        }

        private FunctionIndex PrepareFunctionIndex() {
            var functionIndex = new FunctionIndex(_exportProvider.GetExportedValue<ICoreShell>());
            functionIndex.Initialize();
            functionIndex.BuildIndexAsync().Wait();
            return functionIndex;
        }
    }
}
