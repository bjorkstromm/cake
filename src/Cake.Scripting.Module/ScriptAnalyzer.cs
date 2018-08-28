// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Core.Scripting.Analysis;
using Cake.Scripting.Module.Directives.Loading;
using Cake.Scripting.Module.Directives;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Cake.Core.Scripting.Processors.Loading;

namespace Cake.Scripting.Module
{
    /// <summary>
    /// The script analyzer.
    /// </summary>
    public sealed class ScriptAnalyzer : IScriptAnalyzer
    {
        private readonly IFileSystem _fileSystem;
        private readonly ICakeEnvironment _environment;
        private readonly ICakeLog _log;
        private readonly DirectiveProcessor[] _directiveProcessors;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptAnalyzer"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="log">The log.</param>
        /// <param name="providers">The load directive providers.</param>
        public ScriptAnalyzer(
            IFileSystem fileSystem,
            ICakeEnvironment environment,
            ICakeLog log,
            IEnumerable<ILoadDirectiveProvider> providers)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _log = log ?? throw new ArgumentNullException(nameof(log));

            _directiveProcessors = new DirectiveProcessor[]
            {
                new LoadDirectiveProcessor(providers),
                new ReferenceDirectiveProcessor(_fileSystem, _environment),
                // new UsingStatementProcessor(),
                new AddInDirectiveProcessor(),
                new ToolDirectiveProcessor(),
                // new ShebangProcessor(),
                // new BreakDirectiveProcessor(),
                // new DefineDirectiveProcessor(),
                new ModuleDirectiveProcessor()
            };
        }

        /// <summary>
        /// Analyzes the specified script path.
        /// </summary>
        /// <param name="path">The path to the script to analyze.</param>
        /// <returns>The script analysis result.</returns>
        public ScriptAnalyzerResult Analyze(FilePath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            // Make the script path absolute.
            path = path.MakeAbsolute(_environment);

            var (script, lines, errors) = AnalyzeCore(path);

            // Create and return the results.
            return new ScriptAnalyzerResult(script, lines, errors);
        }

        private (IScriptInformation script, string[] lines, ScriptAnalyzerError[] errors) AnalyzeCore(FilePath path)
        {
            var text = ReadFile(path);

            // TODO: Preprocessor symbols.
            var syntaxTree = CSharpSyntaxTree.ParseText(
                text, new CSharpParseOptions(
                    LanguageVersion.Latest,
                    DocumentationMode.None,
                    SourceCodeKind.Script));

            var directives = syntaxTree
                .GetRoot()
                .DescendantNodes(descendIntoTrivia: true)
                .OfType<DirectiveTriviaSyntax>();

            foreach(var directive in directives)
            {

            }

            return (null, syntaxTree.GetText().Lines.Select(l => l.ToString()).ToArray(), null);
        }

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        private string ReadFile(FilePath path)
        {
            path = path.MakeAbsolute(_environment);

            // Get the file and make sure it exist.
            var file = _fileSystem.GetFile(path);
            if (!file.Exists)
            {
                var message = string.Format(CultureInfo.InvariantCulture, "Could not find script '{0}'.", path);
                throw new CakeException(message);
            }

            // Read the content from the file.
            using (var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}