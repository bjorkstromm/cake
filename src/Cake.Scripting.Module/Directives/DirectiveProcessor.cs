// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;
using Cake.Core.Scripting.Analysis;
using Cake.Core.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cake.Scripting.Module.Directives
{
    /// <summary>
    /// Abstract line processor.
    /// </summary>
    internal abstract class DirectiveProcessor
    {
        public abstract bool CanProcess(DirectiveTriviaSyntax directive);

        public abstract SyntaxNode Process(IScriptAnalyzerContext analyzer, DirectiveTriviaSyntax directive);

        /// <summary>
        /// Splits the specified line into tokens.
        /// </summary>
        /// <param name="line">The line to split.</param>
        /// <returns>The parts that make up the line.</returns>
        protected static string[] Split(string line)
        {
            if (line == null)
            {
                throw new ArgumentNullException(nameof(line));
            }

            return QuoteAwareStringSplitter.Split(line).ToArray();
        }
    }
}