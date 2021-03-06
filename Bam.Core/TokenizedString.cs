#region License
// Copyright (c) 2010-2019, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
using System.Linq;
namespace Bam.Core
{
    /// <summary>
    /// Strings are tokenized by macros and functions. Macros are themselves TokenizedStrings
    /// and so there is a recursive expansion to evaluate the resulting string (referred to as parsing).
    /// </summary>
    /// <remarks>
    /// Tokens are identified by $( and ).
    /// A numeric index within a token, e.g. $(0), represents the index into the list of macros passed into the TokenizedString creation function. Parsing is performed recursively, although macros are not shared between repeated parsing calls.
    /// <para />
    /// Functions can run before or after token expansion.
    /// <para />
    /// Pre-functions are run before token expansion, and are identified by \#name(...):
    /// <list type="bullet">
    /// <item><description><code>#valid(expr[,default])</code></description> If the expression is a valid
    /// TokenizedString, expand it and use it, otherwise the entire function call is replaced with the 'default' expression, unless
    /// this is omitted, and an empty string is used.</item>
    /// </list>
    /// Post-functions are run after token expansion, and are identified by @(...):
    /// <list type="bullet">
    /// <item><description><code>@basename(path)</code></description> Return the filename excluding extension in the path.</item>
    /// <item><description><code>@filename(path)</code></description> Return the filename including extension in the path.</item>
    /// <item><description><code>@extension(path)</code></description> Return the extension of the file path. Leading periods are not included.</item>
    /// <item><description><code>@dir_(path)</code></description> Return the parent directory of path. (Remove the underscore from the name)</item>
    /// <item><description><code>@normalize(path)</code></description> Return the full path of path, without any special directories.</item>
    /// <item><description><code>@changeextension(path,ext)</code></description> Change the extension of the file in path, to ext.</item>
    /// <item><description><code>@removetrailingseparator(path)</code></description> Remove any directory separator characters from the end of path.</item>
    /// <item><description><code>@relativeto(path,baseDir)</code></description> Return the relative path from baseDir. If there is no common root between them, path is returned.</item>
    /// <item><description><code>@isrelative(path,fallback)</code></description> If path is relative, use it, otherwise use the fallback.</item>
    /// <item><description><code>@trimstart(path,totrim)</code></description> Trim string from the start of path.</item>
    /// <item><description><code>@escapedquotes(path)</code></description> Ensure that the path is double quoted, suitable for use with preprocessor definitions.</item>
    /// <item><description><code>@ifnotempty(path,whennotempty,whenempty)</code></description> If path is not empty, replace the expression with that in whennotempty, otherwise use whenempty.</item>
    /// <item><description><code>@tounix(path)</code></description> Convert any Windows paths (using back slashes) to Unix paths (using forward slashes).</item>
    /// <item><description><code>@exists(path,fallback)</code></description> If the path exists, use it, otherwise use the fallback.</item>
    /// </list>
    /// Custom unary post-functions can be registered using <code>registerPostUnaryFunction</code>.
    /// </remarks>
    public sealed class TokenizedString
    {
        [System.Flags]
        private enum EFlags
        {
            None = 0,
            ForcedInline = 0x1,
            NoCache = 0x2
        }

        /// <summary>
        /// The marker for entry to a token.
        /// </summary>
        public static readonly string TokenEntryMarker = @"$";

        /// <summary>
        /// Prefix of each token.
        /// </summary>
        public static readonly string TokenPrefix = @"$(";

        /// <summary>
        /// Suffix of each token.
        /// </summary>
        public static readonly string TokenSuffix = @")";

        private static readonly string TokenRegExPattern = @"(\$\([^)]+\))";
        private static readonly string ExtractTokenRegExPattern = @"\$\(([^)]+)\)";
        private static readonly string PositionalTokenRegExPattern = @"\$\(([0-9]+)\)";

        // pre-functions look like: #functionname(expression)
        // note: this is using balancing groups in order to handle nested function calls, or any other instances of parentheses in paths (e.g. Windows 'Program Files (x86)')
        private static readonly string PreFunctionRegExPattern = @"(#(?<func>[a-z]+)\((?<expression>[^\(\)]+|\((?<Depth>)|\)(?<-Depth>))*(?(Depth)(?!))\))";

        // post-functions look like: @functionname(expression)
        // note: this is using balancing groups in order to handle nested function calls, or any other instances of parentheses in paths (e.g. Windows 'Program Files (x86)')
        private static readonly string PostFunctionRegExPattern = @"(@(?<func>[a-z]+)\((?<expression>[^\(\)]+|\((?<Depth>)|\)(?<-Depth>))*(?(Depth)(?!))\))";

        private static readonly string[] BuiltInPostFunctionNames =
        {
            "basename",
            "filename",
            "extension",
            "dir",
            "normalize",
            "changeextension",
            "removetrailingseparator",
            "relativeto",
            "isrelative",
            "trimstart",
            "escapedquotes",
            "ifnotempty",
            "tounix",
            "exists"
        };

        // static fields, initialised in reset()
        private static System.Collections.Generic.Dictionary<System.Int64, TokenizedString> VerbatimCacheMap;
        private static System.Collections.Generic.Dictionary<System.Int64, TokenizedString> NoModuleCacheMap;
        private static System.Collections.Generic.List<TokenizedString> AllStrings;
        private static System.Collections.Generic.List<TokenizedString> StringsForParsing = new System.Collections.Generic.List<TokenizedString>();
        private static bool AllStringsParsed;
        private static System.Collections.Generic.Dictionary<string, System.Func<string, string>> CustomPostUnaryFunctions;
        private static System.TimeSpan RegExTimeout;
        private static bool RecordStackTraces;

        // instance fields
        private System.Collections.Generic.List<string> Tokens = null;
        private Module ModuleWithMacros = null;
        private string OriginalString = null;
        private string ParsedString = null;
        private readonly object ParsedStringGuard = new object(); // since you can't lock ParsedString as it may be null
        private bool Verbatim;
        private TokenizedStringArray PositionalTokens = null;
        private string CreationStackTrace = null;
        private int RefCount = 1;
        private readonly EFlags Flags = EFlags.None;
        private long hash = 0;
        private string parsedStackTrace = null;

        /// <summary>
        /// Register a custom unary post function to use in TokenizedString parsing.
        /// The name must not collide with any built-in functions, or any existing custom unary post functions.
        /// </summary>
        /// <param name="name">Name of the function that must be unique.</param>
        /// <param name="function">Function to apply to any usage of \p name in TokenizedStrings.</param>
        public static void
        RegisterPostUnaryFunction(
            string name,
            System.Func<string, string> function)
        {
            if (BuiltInPostFunctionNames.Contains(name))
            {
                throw new Exception($"Unable to register post unary function due to name collision with builtin functions, '{name}'");
            }
            if (CustomPostUnaryFunctions.ContainsKey(name))
            {
                throw new Exception($"Unable to register post unary function because post function '{name}' already exists.");
            }
            CustomPostUnaryFunctions.Add(name, function);
        }

        static private System.Collections.Generic.IEnumerable<string>
        SplitIntoTokens(
            string original,
            string regExPattern)
        {
            var regExSplit = System.Text.RegularExpressions.Regex.Split(original, regExPattern);
            var filtered = regExSplit.Where(item => !System.String.IsNullOrEmpty(item));
            return filtered;
        }

        static private System.Collections.Generic.IEnumerable<string>
        GetMatches(
            string original,
            string regExPattern)
        {
            var matches = System.Text.RegularExpressions.Regex.Matches(original, regExPattern);
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                // was at least one substring captured by the regex?
                if (!match.Success)
                {
                    continue;
                }
                // there is >1 groups, as the first is the original expression, so skip it
                foreach (var group in match.Groups.Cast<System.Text.RegularExpressions.Group>().Skip(1))
                {
                    yield return group.Value;
                }
            }
        }

        private static string
        GetStacktrace()
        {
            if (RecordStackTraces)
            {
                return System.Environment.StackTrace;
            }
            return string.Empty;
        }

        /// <summary>
        /// Reset all static state of the TokenizedString class.
        /// This function is only really useful in unit tests.
        /// </summary>
        public static void
        Reset()
        {
            VerbatimCacheMap = new System.Collections.Generic.Dictionary<System.Int64, TokenizedString>();
            NoModuleCacheMap = new System.Collections.Generic.Dictionary<System.Int64, TokenizedString>();
            AllStrings = new System.Collections.Generic.List<TokenizedString>();
            StringsForParsing = new System.Collections.Generic.List<TokenizedString>();
            AllStringsParsed = false;
            CustomPostUnaryFunctions = new System.Collections.Generic.Dictionary<string, System.Func<string, string>>();
            RecordStackTraces = false;
            RegExTimeout = System.TimeSpan.FromSeconds(5);
        }

        static TokenizedString()
        {
            Reset();
            RecordStackTraces = CommandLineProcessor.Evaluate(new Options.RecordStackTrace());
            if (RecordStackTraces)
            {
                Log.Info("WARNING: TokenizedString stack trace recording enabled. This will slow down your build.");
            }
        }

        private TokenizedString(
            string original,
            Module moduleWithMacros,
            bool verbatim,
            TokenizedStringArray positionalTokens,
            EFlags flags)
        {
            this.ModuleWithMacros = moduleWithMacros;
            this.Verbatim = verbatim;
            this.Flags |= flags;
            this.SetInternal(original, (null != positionalTokens) ? positionalTokens.ToArray() : null);

            if (verbatim)
            {
                this.ParsedString = original;
                this.parsedStackTrace = GetStacktrace();
            }
        }

        private static System.Int64
        CalculateHash(
            string tokenizedString,
            Module macroSource,
            bool verbatim,
            TokenizedStringArray positionalTokens)
        {
            // https://cs.stackexchange.com/questions/45287/why-does-this-particular-hashcode-function-help-decrease-collisions
            System.Int64 hash = 17;
            hash = hash * 31 + tokenizedString.GetHashCode();

            if (!verbatim)
            {
                if (null != macroSource)
                {
                    hash = hash * 31 + macroSource.GetHashCode();
                }
                if (null != positionalTokens)
                {
                    foreach (var posToken in positionalTokens)
                    {
                        hash = hash * 31 + posToken.GetHashCode();
                    }
                }
            }

            return hash;
        }

        private static TokenizedString
        CreateInternal(
            string tokenizedString,
            Module macroSource,
            bool verbatim,
            TokenizedStringArray positionalTokens,
            EFlags flags)
        {
            if (null == tokenizedString)
            {
                return null;
            }

            var hash = CalculateHash(tokenizedString, macroSource, verbatim, positionalTokens);

            // strings can be created during the multithreaded phase, so synchronize on the cache used
            if (verbatim)
            {
                // covers all verbatim strings
                lock (VerbatimCacheMap)
                {
                    var useCache = (0 == (flags & EFlags.NoCache));
                    if (useCache)
                    {
                        if (VerbatimCacheMap.TryGetValue(hash, out TokenizedString foundTS))
                        {
                            ++foundTS.RefCount;
                            return foundTS;
                        }
                    }
                    var newTS = new TokenizedString(tokenizedString, macroSource, verbatim, positionalTokens, flags);
                    if (useCache)
                    {
                        VerbatimCacheMap.Add(hash, newTS);
                    }
                    newTS.hash = hash;
                    lock (AllStrings)
                    {
                        AllStrings.Add(newTS);
                    }
                    return newTS;
                }
            }
            else
            {
                // covers all strings associated with a module (for macros), or no module but with positional arguments
                var stringCache = (null != macroSource) ? macroSource.TokenizedStringCacheMap : NoModuleCacheMap;
                lock (stringCache)
                {
                    var useCache = (0 == (flags & EFlags.NoCache));
                    if (useCache)
                    {
                        if (stringCache.TryGetValue(hash, out TokenizedString foundTS))
                        {
                            ++foundTS.RefCount;
                            return foundTS;
                        }
                    }
                    var newTS = new TokenizedString(tokenizedString, macroSource, verbatim, positionalTokens, flags);
                    if (useCache)
                    {
                        stringCache.Add(hash, newTS);
                    }
                    newTS.hash = hash;
                    lock (AllStrings)
                    {
                        AllStrings.Add(newTS);
                    }
                    if (!newTS.IsForcedInline)
                    {
                        lock (StringsForParsing)
                        {
                            StringsForParsing.Add(newTS);
                        }
                    }
                    return newTS;
                }
            }
        }

        /// <summary>
        /// Utility method to create a TokenizedString associated with a module, or return a cached version.
        /// </summary>
        /// <param name="tokenizedString">Tokenized string.</param>
        /// <param name="macroSource">Macro source.</param>
        /// <param name="positionalTokens">Positional tokens.</param>
        public static TokenizedString
        Create(
            string tokenizedString,
            Module macroSource,
            TokenizedStringArray positionalTokens = null) => CreateInternal(tokenizedString, macroSource, false, positionalTokens, EFlags.None);

        /// <summary>
        /// Utility method to create a TokenizedString with no macro replacement, or return a cached version.
        /// </summary>
        /// <returns>The verbatim.</returns>
        /// <param name="verboseString">Verbose string.</param>
        public static TokenizedString
        CreateVerbatim(
            string verboseString) => CreateInternal(verboseString, null, true, null, EFlags.None);

        /// <summary>
        /// Utility method to create a TokenizedString that can be inlined into other TokenizedStrings
        /// , or return a cached version.
        /// </summary>
        /// <returns>The inline.</returns>
        /// <param name="inlineString">Inline string.</param>
        public static TokenizedString
        CreateForcedInline(
            string inlineString) => CreateInternal(inlineString, null, false, null, EFlags.ForcedInline);

        /// <summary>
        /// Utility method to create a TokenizedString which will not be cached with any other existing
        /// TokenizedStrings that share the same original string.
        /// Such TokenizedStrings are intended to be aliased at a future time.
        /// </summary>
        /// <param name="uncachedString">The string that will be uncached.</param>
        /// <param name="macroSource">The Module containing macros that will be eventually referenced.</param>
        /// <param name="positionalTokens">Positional tokens.</param>
        /// <returns>A unique TokenizedString.</returns>
        public static TokenizedString
        CreateUncached(
            string uncachedString,
            Module macroSource,
            TokenizedStringArray positionalTokens = null) => CreateInternal(uncachedString, macroSource, false, positionalTokens, EFlags.NoCache);

        /// <summary>
        /// Determine if the TokenizedString has been parsed already.
        /// Sometimes useful if a TokenizedString is created after the ParseAll step, but is repeated
        /// as a dependency.
        /// </summary>
        public bool IsParsed
        {
            get
            {
                if (this.Verbatim)
                {
                    return true;
                }
                if (this.IsForcedInline)
                {
                    return false;
                }
                var hasTokens = (null != this.Tokens);
                lock (this.ParsedStringGuard)
                {
                    var hasParsedString = (null != this.ParsedString);
                    return !hasTokens && hasParsedString;
                }
            }
        }

        private static string
        NormalizeDirectorySeparators(
            string path) => OSUtilities.IsWindowsHosting ? path.Replace('/', '\\') : path.Replace('\\', '/');

        /// <summary>
        /// Return the parsed string.
        /// If the string has not been parsed, or unsuccessfully parsed, an exception is thrown.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="Bam.Core.TokenizedString"/>.</returns>
        public override string
        ToString()
        {
            if (null == this.ParsedString)
            {
                var message = new System.Text.StringBuilder();
                var parsingPhase = AllStringsParsed ? " after the string parsing phase" : string.Empty;
                message.AppendLine($"TokenizedString '{this.OriginalString}' has not been parsed{parsingPhase}");
                message.AppendLine();
                message.AppendLine("Created at:");
                message.AppendLine(this.CreationStackTrace);
                throw new Exception(message.ToString());
            }
            if (null != this.Tokens)
            {
                var message = new System.Text.StringBuilder();
                var parsingPhase = AllStringsParsed ? " after the string parsing phase" : string.Empty;
                message.AppendLine($"TokenizedString '{this.OriginalString}' has been parsed to");
                message.AppendLine($"'{this.ParsedString}'");
                message.AppendLine($"but the following tokens remain unresolved{parsingPhase}:");
                foreach (var token in this.Tokens)
                {
                    if (!token.StartsWith(TokenPrefix, System.StringComparison.Ordinal))
                    {
                        continue;
                    }
                    message.AppendLine($"\t{token}");
                }
                message.AppendLine();
                message.AppendLine("Created at:");
                message.AppendLine(this.CreationStackTrace);
                throw new Exception(message.ToString());
            }
            return this.ParsedString;
        }

        /// <summary>
        /// Determine if the string is empty.
        /// </summary>
        /// <value><c>true</c> if empty; otherwise, <c>false</c>.</value>
        public bool Empty => (null == this.Tokens) || !this.Tokens.Any();

        private bool IsForcedInline => (EFlags.ForcedInline == (this.Flags & EFlags.ForcedInline));

        /// <summary>
        /// Parse every TokenizedString.
        /// </summary>
        public static void
        ParseAll()
        {
            Log.Detail("Parsing strings...");
            var scale = 100.0f / StringsForParsing.Count;
            var count = 0;
            foreach (var t in StringsForParsing)
            {
                t.ParseInternalWithAlreadyParsedCheck(null);
                Log.DetailProgress("{0,3}%", (int)(++count * scale));
            }
            AllStringsParsed = true;
        }

        /// <summary>
        /// Parsed a TokenizedString.
        /// Pre-functions are evaluated first.
        /// The order of source of tokens are checked in the follow order:
        /// - Positional tokens.
        /// - Any custom macros (will be none in this context)
        /// - Global macros (from the Graph)
        /// - Macros in the associated Module
        /// - Macros in the Tool associated with the Module
        /// - Environment variables
        /// After token expansion, post-functions are then evaluated.
        /// No string is returned, use ToString().
        /// Failure to parse are stored in the TokenizedString and will be displayed as an exception
        /// message when used.
        /// </summary>
        public void
        Parse()
        {
            if (this.ParsedString != null)
            {
                var message = new System.Text.StringBuilder();
                var parsePhase = AllStringsParsed ? " after the string parsing phase" : string.Empty;
                message.AppendLine($"TokenizedString '{this.OriginalString}' is already parsed{parsePhase}.");
                message.AppendLine();
                message.AppendLine("Created at:");
                message.AppendLine(this.CreationStackTrace);
                message.AppendLine();
                message.AppendLine("Parsed at:");
                message.AppendLine(this.parsedStackTrace);
                throw new Exception(message.ToString());
            }
            lock (this.ParsedStringGuard)
            {
                this.ParseInternalWithAlreadyParsedCheck(null);
            }
            lock (StringsForParsing)
            {
                StringsForParsing.Remove(this);
            }
        }

        /// <summary>
        /// Parsed a TokenizedString with a custom source of macro overrides.
        /// This performs a similar operation to Parse(), except that the parsed string is not saved, but is returned
        /// from the function.
        /// This allows TokenizedStrings to be re-parsed with different semantics to their tokens, but will not affect
        /// the existing parse result.
        /// The array of MacroLists is evaluated from front to back, so if there are duplicate macros in several MacroLists
        /// the first encountered will be the chosen value.
        /// No errors or exceptions are reported or saved from using this function, so use it sparingly and with care.
        /// </summary>
        /// <param name="customMacroArray">Array of custom macros.</param>
        public string
        UncachedParse(
            Array<MacroList> customMacroArray) => this.ParseInternal(customMacroArray);

        private string
        ParseInternalWithAlreadyParsedCheck(
            Array<MacroList> customMacroArray) => this.ParsedString ?? this.ParseInternal(customMacroArray);

        private string
        GetParsedString(
            Array<MacroList> customMacroArray)
        {
            if (null == this.ParsedString)
            {
                this.ParseInternal(customMacroArray);
                //throw new Exception($"String '{this.OriginalString}' has yet to be parsed");
            }
            return this.ParsedString;
        }

        private void
        ExtendParsedStringWrapper(
            TokenizedString stringToExtendWith,
            System.Text.StringBuilder parsedString,
            Array<MacroList> customMacroArray,
            System.Collections.Generic.List<string> tokens,
            int index)
        {
            if (stringToExtendWith.IsForcedInline)
            {
                var extTokens = SplitIntoTokens(this.EvaluatePreFunctions(stringToExtendWith.OriginalString, customMacroArray), TokenRegExPattern).ToList<string>();
                if (null != extTokens)
                {
                    tokens.InsertRange(index, extTokens);
                }
                else
                {
                    parsedString.Append(stringToExtendWith.OriginalString);
                }
            }
            else
            {
                stringToExtendWith.ExtendParsedString(parsedString, customMacroArray, tokens, index);
            }
        }

        private void
        ExtendParsedString(
            System.Text.StringBuilder parsedString,
            Array<MacroList> customMacroArray,
            System.Collections.Generic.List<string> tokens,
            int index)
        {
            var parsedResult = this.GetParsedString(customMacroArray);
            if (null != this.Tokens)
            {
                tokens.InsertRange(index, this.Tokens);
            }
            else
            {
                parsedString.Append(parsedResult);
            }
        }

        private string
        ParseInternal(
            Array<MacroList> customMacroArray)
        {
            if (this.IsForcedInline)
            {
                throw new Exception($"Forced inline TokenizedString cannot be parsed, {this.OriginalString}");
            }
            var graph = Graph.Instance;
            var parsedString = new System.Text.StringBuilder();
            var tokens = SplitIntoTokens(this.EvaluatePreFunctions(this.OriginalString, customMacroArray), TokenRegExPattern).ToList<string>();
            for (int index = 0; index < tokens.Count;)
            {
                var token = tokens[index];

                // if not identified as a token, just add the string, and move along
                if (!(token.StartsWith(TokenPrefix, System.StringComparison.Ordinal) && token.EndsWith(TokenSuffix, System.StringComparison.Ordinal)))
                {
                    // unless it sort of looks like a token, but has bad formatting
                    // $token will not throw though
                    if (token.StartsWith(TokenEntryMarker))
                    {
                        if (token.StartsWith(TokenPrefix, System.StringComparison.Ordinal) ||
                            token.EndsWith(TokenSuffix, System.StringComparison.Ordinal))
                        {
                            throw new BadTokenFormatException(token);
                        }
                    }
                    parsedString.Append(token);
                    tokens.Remove(token);
                    continue;
                }

                // step 1: if the token is a positional token, inline it, and add outstanding tokens
                var positional = GetMatches(token, PositionalTokenRegExPattern).FirstOrDefault();
                if (!System.String.IsNullOrEmpty(positional))
                {
                    var positionalIndex = System.Convert.ToInt32(positional);
                    if (positionalIndex > this.PositionalTokens.Count)
                    {
                        throw new Exception(
                            $"TokenizedString positional token at index {positionalIndex} requested, but only {this.PositionalTokens.Count} positional values given. Created at {this.CreationStackTrace}."
                        );
                    }
                    try
                    {
                        var posTokenStr = this.PositionalTokens[positionalIndex];
                        tokens.Remove(token);
                        this.ExtendParsedStringWrapper(posTokenStr, parsedString, customMacroArray, tokens, index);
                    }
                    catch (System.ArgumentOutOfRangeException ex)
                    {
                        throw new Exception(
                            ex,
                            $"Positional token index {positionalIndex} exceeded number of tokens available {this.PositionalTokens.Count}"
                        );
                    }
                    continue;
                }

                // step 2 : try to resolve with custom macros passed to the Parse function
                if (null != customMacroArray?.FirstOrDefault(item => item.Dict.ContainsKey(token)))
                {
                    var containingMacroList = customMacroArray.First(item => item.Dict.ContainsKey(token));
                    var customTokenStr = containingMacroList.Dict[token];
                    tokens.Remove(token);
                    this.ExtendParsedStringWrapper(customTokenStr, parsedString, customMacroArray, tokens, index);
                    continue;
                }

                // step 3 : try macros in the global Graph, common to all modules
                if (graph.Macros.Dict.ContainsKey(token))
                {
                    var graphTokenStr = graph.Macros.Dict[token];
                    tokens.Remove(token);
                    this.ExtendParsedStringWrapper(graphTokenStr, parsedString, customMacroArray, tokens, index);
                    continue;
                }

                if (this.ModuleWithMacros != null)
                {
                    var tool = this.ModuleWithMacros.Tool;
                    // step 4 : try macros in the specific module
                    if (this.ModuleWithMacros.Macros.Dict.ContainsKey(token))
                    {
                        var moduleMacroStr = this.ModuleWithMacros.Macros.Dict[token];
                        tokens.Remove(token);
                        this.ExtendParsedStringWrapper(moduleMacroStr, parsedString, customMacroArray, tokens, index);
                        continue;
                    }
                    // step 5 : try macros in the Tool attached to the specific module
                    else if (null != tool && tool.Macros.Dict.ContainsKey(token))
                    {
                        var moduleToolMacroStr = tool.Macros.Dict[token];
                        tokens.Remove(token);
                        this.ExtendParsedStringWrapper(moduleToolMacroStr, parsedString, customMacroArray, tokens, index);
                        continue;
                    }
                }

                // step 6 : try the immediate environment
                var strippedToken = SplitIntoTokens(token, ExtractTokenRegExPattern).First();
                if (System.String.Equals(token, strippedToken, System.StringComparison.Ordinal))
                {
                    throw new EmptyStringException();
                }
                var envVar = System.Environment.GetEnvironmentVariable(strippedToken);
                if (null != envVar)
                {
                    tokens.Remove(token);
                    parsedString.Append(envVar);
                    continue;
                }

                // step 7 : original token must be honoured, as it might be resolved in a later inlining step
                parsedString.Append(token);
                ++index;
            }

            if (tokens.Any())
            {
                if (null != customMacroArray)
                {
                    throw new Exception("String cannot be fully parsed with the custom macros provided");
                }
                // need to split into tokens again
                // so that both unresolved tokens and literal text can be inserted into future strings
                this.Tokens = SplitIntoTokens(parsedString.ToString(), TokenRegExPattern).ToList<string>();
                this.ParsedString = parsedString.ToString();
                Log.DebugMessage($"\t'{this.OriginalString}' --> '{this.ParsedString}'");
                return this.ParsedString;
            }
            else
            {
                this.Tokens = null;
                var functionEvaluated = this.EvaluatePostFunctions(NormalizeDirectorySeparators(parsedString.ToString()));
                // when using a custom array of MacroLists, do not store the parsed string
                // instead just return it
                // this allows a TokenizedString to be re-parsed with different semantics, but does not
                // permanently change it
                if (null == customMacroArray)
                {
                    this.ParsedString = functionEvaluated;
                    this.parsedStackTrace = GetStacktrace();
                    Log.DebugMessage($" '{this.OriginalString}' --> '{this.ToString()}'");
                }
                else
                {
                    Log.DebugMessage($" '{this.OriginalString}' --> '{functionEvaluated}' (using custom macros)");
                }
                return functionEvaluated;
            }
        }

        private string
        EvaluatePostFunctions(
            string sourceExpression)
        {
            System.Text.RegularExpressions.MatchCollection matches = null;
            try
            {
                matches = System.Text.RegularExpressions.Regex.Matches(
                    sourceExpression,
                    PostFunctionRegExPattern,
                    System.Text.RegularExpressions.RegexOptions.None,
                    RegExTimeout);
                if (!matches.Any())
                {
                    return sourceExpression;
                }
            }
            catch (System.Text.RegularExpressions.RegexMatchTimeoutException)
            {
                var message = new System.Text.StringBuilder();
                message.AppendLine($"TokenizedString post-function regular expression matching timed out after {RegExTimeout.Seconds} seconds. Check details below for errors.");
                message.AppendLine($"String being parsed: {sourceExpression}");
                message.AppendLine($"Regex              : {PostFunctionRegExPattern}");
                message.AppendLine($"Tokenized string {this.OriginalString} created at");
                message.AppendLine(this.CreationStackTrace);
                throw new Exception(message.ToString());
            }
            var modifiedString = sourceExpression;
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                var functionName = match.Groups["func"].Value;
                // this correctly obtains the expression when nested functions are present
                var expressionText = new System.Text.StringBuilder();
                foreach (System.Text.RegularExpressions.Capture capture in match.Groups["expression"].Captures)
                {
                    expressionText.Append(capture.Value);
                }
                var expression = this.EvaluatePostFunctions(expressionText.ToString());
                var expandedExpression = this.FunctionExpression(functionName, expression);
                modifiedString = modifiedString.Replace(match.Value, expandedExpression);
            }
            return modifiedString;
        }

        private string
        FunctionExpression(
            string functionName,
            string argument)
        {
            switch (functionName)
            {
                case "basename":
                    return System.IO.Path.GetFileNameWithoutExtension(argument);

                case "filename":
                    return System.IO.Path.GetFileName(argument);

                case "extension":
                    return System.IO.Path.GetExtension(argument).TrimStart('.');

                case "dir":
                    return System.IO.Path.GetDirectoryName(argument);

                case "normalize":
                    return System.IO.Path.GetFullPath(argument);

                case "changeextension":
                    {
                        var split = argument.Split(',');
                        if (split.Length != 2)
                        {
                            throw new Exception(
                                $"Expected 2, not {split.Length}, arguments in the function call {functionName}({argument}) in {this.OriginalString}"
                            );
                        }
                        var original = split[0];
                        var extension = split[1].Trim();
                        var changed = System.IO.Path.ChangeExtension(original, extension);
                        return changed;
                    }

                case "removetrailingseparator":
                    return argument.TrimEnd(System.IO.Path.DirectorySeparatorChar);

                case "relativeto":
                    {
                        var split = argument.Split(',');
                        if (split.Length != 2)
                        {
                            throw new Exception(
                                $"Expected 2, not {split.Length}, arguments in the function call {functionName}({argument}) in {this.OriginalString}"
                            );
                        }
                        var path = split[0];
                        var root = split[1] + System.IO.Path.DirectorySeparatorChar;
                        var relative = System.IO.Path.GetRelativePath(root, path);
                        return relative;
                    }

                case "isrelative":
                    {
                        var split = argument.Split(',');
                        if (split.Length != 2)
                        {
                            throw new Exception(
                                $"Expected 2, not {split.Length}, arguments in the function call {functionName}({argument}) in {this.OriginalString}"
                            );
                        }
                        var path = split[0];
                        var fallback = split[1];
                        if (System.IO.Path.IsPathRooted(path))
                        {
                            return fallback;
                        }
                        else
                        {
                            return path;
                        }
                    }

                case "trimstart":
                    {
                        var split = argument.Split(',');
                        if (split.Length != 2)
                        {
                            throw new Exception(
                                $"Expected 2, not {split.Length}, arguments in the function call {functionName}({argument}) in {this.OriginalString}"
                            );
                        }
                        var original = split[0];
                        var totrim = split[1];
                        while (true)
                        {
                            var index = original.IndexOf(totrim);
                            if (index != 0)
                            {
                                break;
                            }
                            original = original.Substring(index + totrim.Length);
                        }
                        return original;
                    }

                case "escapedquotes":
                    {
                        if (OSUtilities.IsWindowsHosting)
                        {
                            // on Windows, escape any backslashes, as these are normal Windows paths
                            // so don't interpret them as control characters
                            argument = argument.Replace("\\", "\\\\");
                        }
                        return $"\"{argument}\"";
                    }

                case "ifnotempty":
                    {
                        var split = argument.Split(',');
                        if (split.Length != 3)
                        {
                            throw new Exception(
                                $"Expected 3, not {split.Length}, arguments in the function call {functionName}({argument}) in {this.OriginalString}"
                            );
                        }
                        var predicateString = split[0];
                        if (!System.String.IsNullOrEmpty(predicateString))
                        {
                            var positiveString = split[1];
                            return positiveString;
                        }
                        else
                        {
                            var negativeString = split[2];
                            return negativeString;
                        }
                    }

                case "tounix":
                    {
                        argument = argument.Replace("\\", "/");
                        return argument;
                    }

                case "exists":
                    {
                        var split = argument.Split(',');
                        if (split.Length != 2)
                        {
                            throw new Exception(
                                $"Expected 2, not {split.Length}, arguments in the function call {functionName}({argument}) in {this.OriginalString}"
                            );
                        }
                        var path = split[0];
                        var fallback = split[1];
                        if (System.IO.File.Exists(path) || System.IO.Directory.Exists(path))
                        {
                            return path;
                        }
                        return fallback;
                    }

                default:
                    {
                        // search through custom functions
                        if (CustomPostUnaryFunctions.ContainsKey(functionName))
                        {
                            return CustomPostUnaryFunctions[functionName](argument);
                        }
                        throw new Exception(
                            $"Unknown post-function '{functionName}' in TokenizedString '{this.OriginalString}'"
                        );
                    }
            }
        }

        /// <summary>
        /// Does the string contain a space?
        /// </summary>
        /// <value><c>true</c> if contains space; otherwise, <c>false</c>.</value>
        public bool ContainsSpace
        {
            get
            {
                if (!this.IsParsed)
                {
                    throw new Exception($"TokenizedString, '{this.OriginalString}', is not yet expanded");
                }
                if (null != this.ParsedString)
                {
                    return this.ParsedString.Contains(' ');
                }
                else
                {
                    if (this.Tokens.Count != 1)
                    {
                        throw new Exception("Tokenized string that is expanded, but has more than one token");
                    }
                    return this.Tokens[0].Contains(' ');
                }
            }
        }

        /// <summary>
        /// Are two strings identical? This includes comparing how the string was constructed.
        /// It does not necessarily mean that the parsed strings are identical. Use ToString().Equals() to achieve that test.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="Bam.Core.TokenizedString"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current
        /// <see cref="Bam.Core.TokenizedString"/>; otherwise, <c>false</c>.</returns>
        public override bool
        Equals(
            object obj)
        {
            var other = obj as TokenizedString;
            var equals = this.hash == other.hash;
            return equals;
        }

        /// <summary>
        /// Required by the Equals override.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int
        GetHashCode() => base.GetHashCode();

        /// <summary>
        /// Quote the string if it contains a space
        /// </summary>
        /// <returns>The string and quote if necessary.</returns>
        public string ToStringQuoteIfNecessary() => IOWrapper.EncloseSpaceContainingPathWithDoubleQuotes(this.ToString());

        /// <summary>
        /// Static utility method to return the number of TokenizedStrings cached.
        /// </summary>
        /// <value>The count.</value>
        public static int Count => AllStrings.Count();

        /// <summary>
        /// Static utility method to return the number of strings with a single refcount.
        /// </summary>
        /// <value>The unshared count.</value>
        public static int UnsharedCount => AllStrings.Where(item => item.RefCount == 1).Count();

        /// <summary>
        /// In debug builds, dump data representing all of the tokenized strings.
        /// </summary>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void
        DumpCache()
        {
            Log.DebugMessage("Tokenized string cache");
            foreach (var item in AllStrings.OrderBy(item => item.RefCount).ThenBy(item => !item.Verbatim))
            {
                var isVerbatimOpen = item.Verbatim ? "<verbatim>" : string.Empty;
                var isVerbatimClose = item.Verbatim ? "</verbatim>" : string.Empty;
                var moduleReference = item.ModuleWithMacros != null ? $"(ref: {item.ModuleWithMacros.GetType().ToString()})" : string.Empty;
                Log.DebugMessage(
                    $"#{item.RefCount} {isVerbatimOpen}'{item.OriginalString}'{isVerbatimClose} {moduleReference}"
                );
            }
        }

        private bool
        IsTokenValid(
            string token,
            Array<MacroList> customMacroArray)
        {
            // step 1 : is the token positional, i.e. was set up at creation time
            var positional = GetMatches(token, PositionalTokenRegExPattern).FirstOrDefault();
            if (!System.String.IsNullOrEmpty(positional))
            {
                var positionalIndex = System.Convert.ToInt32(positional);
                return (positionalIndex <= this.PositionalTokens.Count);
            }
            // step 2 : try to resolve with custom macros passed to the Parse function
            else if (null != customMacroArray?.FirstOrDefault(item => item.Dict.ContainsKey(token)))
            {
                return true;
            }
            // step 3 : try macros in the global Graph, common to all modules
            else if (Graph.Instance.Macros.Dict.ContainsKey(token))
            {
                return true;
            }
            else if (this.ModuleWithMacros != null)
            {
                var tool = this.ModuleWithMacros.Tool;
                // step 4 : try macros in the specific module
                if (this.ModuleWithMacros.Macros.Dict.ContainsKey(token))
                {
                    return true;
                }
                // step 5 : try macros in the Tool attached to the specific module
                else if (null != tool && tool.Macros.Dict.ContainsKey(token))
                {
                    return true;
                }
            }

            // step 6 : try the immediate environment
            var strippedToken = SplitIntoTokens(token, ExtractTokenRegExPattern).First();
            var envVar = System.Environment.GetEnvironmentVariable(strippedToken);
            if (null != envVar)
            {
                return true;
            }
            // step 7 : fail
            else
            {
                return false;
            }
        }

        private string
        EvaluatePreFunctions(
            string originalExpression,
            Array<MacroList> customMacroArray)
        {
            System.Text.RegularExpressions.MatchCollection matches = null;
            try
            {
                matches = System.Text.RegularExpressions.Regex.Matches(
                    originalExpression,
                    PreFunctionRegExPattern,
                    System.Text.RegularExpressions.RegexOptions.None,
                    RegExTimeout);
                if (!matches.Any())
                {
                    return originalExpression;
                }
            }
            catch (System.Text.RegularExpressions.RegexMatchTimeoutException)
            {
                var message = new System.Text.StringBuilder();
                message.AppendLine($"TokenizedString pre-function regular expression matching timed out after {RegExTimeout.Seconds} seconds. Check details below for errors.");
                message.AppendLine($"String being parsed: {originalExpression}");
                message.AppendLine($"Regex              : {PreFunctionRegExPattern}");
                message.AppendLine($"Tokenized string {this.OriginalString} created at");
                message.AppendLine(this.CreationStackTrace);
                throw new Exception(message.ToString());
            }
            var modifiedString = originalExpression;
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                var functionName = match.Groups["func"].Value;
                // this correctly obtains the expression when nested functions are present
                var expressionText = new System.Text.StringBuilder();
                foreach (System.Text.RegularExpressions.Capture capture in match.Groups["expression"].Captures)
                {
                    expressionText.Append(capture.Value);
                }
                var expression = this.EvaluatePreFunctions(expressionText.ToString(), customMacroArray);
                switch (functionName)
                {
                    case "valid":
                        {
                            var split = expression.Split(',');
                            var replacement = (1 == split.Length) ? string.Empty : split[1];
                            var tokens = SplitIntoTokens(split[0], TokenRegExPattern);
                            var allTokensValid = true;
                            foreach (var token in tokens)
                            {
                                if (!(token.StartsWith(TokenPrefix, System.StringComparison.Ordinal) && token.EndsWith(TokenSuffix, System.StringComparison.Ordinal)))
                                {
                                    continue;
                                }

                                // Note: with nested valid pre-functions, macros can be validated as many times as they are nested
                                if (this.IsTokenValid(token, customMacroArray))
                                {
                                    continue;
                                }

                                allTokensValid = false;
                                break;
                            }

                            if (allTokensValid)
                            {
                                modifiedString = modifiedString.Replace(match.Value, split[0]);
                            }
                            else
                            {
                                modifiedString = modifiedString.Replace(match.Value, replacement);
                            }
                        }
                        break;

                    default:
                        throw new Exception($"Unknown pre-function '{functionName}' in TokenizedString '{this.OriginalString}'");
                }
            }
            return modifiedString;
        }

        /// <summary>
        /// Remove all strings referencing a module type, including those that are not yet parsed.
        /// </summary>
        /// <param name="moduleType"></param>
        static public void
        RemoveEncapsulatedStrings(
            System.Type moduleType)
        {
            lock (AllStrings)
            {
                var toRemove = AllStrings.Where(
                    item => item.ModuleWithMacros != null && item.ModuleWithMacros.GetType() == moduleType);
                foreach (var i in toRemove.ToList())
                {
                    i.RefCount--;

                    if (0 == i.RefCount)
                    {
                        Log.DebugMessage($"Removing string {i.OriginalString} from {moduleType.ToString()}");
                        AllStrings.Remove(i);
                        // Don't believe a separate lock is needed for StringsForParsing
                        if (StringsForParsing.Contains(i))
                        {
                            StringsForParsing.Remove(i);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Clone a TokenizedString, but reassign the Module containing macros.
        /// Verbatim strings are returned directly.
        /// </summary>
        /// <returns>Clone of the string, using the specified module as macro source. Or the verbatim string directly.</returns>
        public TokenizedString
        Clone(
            Module moduleWithMacros)
        {
            if (this.Verbatim)
            {
                return this;
            }
            else
            {
                return Create(this.OriginalString, moduleWithMacros, this.PositionalTokens);
            }
        }

        /// <summary>
        /// Determine if a macro is referred to in the string
        /// or any of it's positional string arguments.
        /// </summary>
        /// <param name="macro">Macro name to look up, including $( and $) prefix and suffix</param>
        /// <returns></returns>
        public bool
        RefersToMacro(
            string macro)
        {
            if (!(macro.StartsWith(TokenizedString.TokenPrefix, System.StringComparison.Ordinal) && macro.EndsWith(TokenizedString.TokenSuffix, System.StringComparison.Ordinal)))
            {
                throw new Exception($"Invalid macro key: {macro}");
            }
            var inString = this.OriginalString.Contains(macro);
            if (inString)
            {
                return true;
            }
            foreach (var positional in this.PositionalTokens)
            {
                if (positional.RefersToMacro(macro))
                {
                    return true;
                }
            }
            return false;
        }

        private void
        SetInternal(
            string newString,
            TokenizedString[] positionalTokens)
        {
            if (null != this.ParsedString)
            {
                throw new Exception(
                    $"Cannot change the TokenizedString '{this.OriginalString}' to '{newString}' as it has been parsed already"
                );
            }
            this.CreationStackTrace = GetStacktrace();
            this.PositionalTokens = new TokenizedStringArray();
            if (null != positionalTokens)
            {
                this.PositionalTokens.AddRange(positionalTokens);
            }
            this.OriginalString = newString;
        }

        /// <summary>
        /// Change an existing TokenizedString's definition. This is only possible when the string has yet to be parsed.
        /// </summary>
        /// <param name="newString">Unparsed token based string to use.</param>
        /// <param name="positionalTokens">Any positional arguments referenced in the unparsed string.</param>
        public void
        Set(
            string newString,
            TokenizedString[] positionalTokens)
        {
            this.SetInternal(newString, positionalTokens);
            var newHash = CalculateHash(this.OriginalString, this.ModuleWithMacros, this.Verbatim, this.PositionalTokens);
            if (0 == (Flags & EFlags.NoCache))
            {
                // update previous caches
                if (this.Verbatim)
                {
                    lock (VerbatimCacheMap)
                    {
                        VerbatimCacheMap.Remove(this.hash);
                        VerbatimCacheMap.Add(newHash, this);
                    }
                }
                else
                {
                    var cache = (this.ModuleWithMacros != null) ? this.ModuleWithMacros.TokenizedStringCacheMap : NoModuleCacheMap;
                    lock (cache)
                    {
                        cache.Remove(this.hash);
                        cache.Add(newHash, this);
                    }
                }
            }
            this.hash = newHash;
        }

        /// <summary>
        /// Extract the unparsed string, containing all original tokens.
        /// </summary>
        public string UnparsedString => this.OriginalString;

        /// <summary>
        /// Exception thrown when a TokenizedString has an empty token, i.e. "$()"
        /// </summary>
        public sealed class EmptyStringException :
            Exception
        {}

        /// <summary>
        /// Exception thrown when a TokenizedString contains bad token formatting, e.g. "$token)" is missing the opening parenthesis scope
        /// </summary>
        public sealed class BadTokenFormatException :
            Exception
        {
            /// <summary>
            /// Exception constructor.
            /// </summary>
            /// <param name="message">Exception message.</param>
            public BadTokenFormatException(
                string message)
                :
                base(message)
            {}
        }
    }
}
