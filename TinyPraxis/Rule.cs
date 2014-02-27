// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="Rule.cs" company="Ian Horswill">
// //   Copyright (C) 2013 Ian Horswill
// //   
// //   Permission is hereby granted, free of charge, to any person obtaining a copy of
// //   this software and associated documentation files (the "Software"), to deal in the
// //   Software without restriction, including without limitation the rights to use, copy,
// //   modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,
// //   and to permit persons to whom the Software is furnished to do so, subject to the
// //   following conditions:
// //   
// //   The above copyright notice and this permission notice shall be included in all
// //   copies or substantial portions of the Software.
// //   
// //   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// //   INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
// //   PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// //   HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// //   OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// //   SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------
using System;
using System.Diagnostics;

namespace TinyPraxis
{
    /// <summary>
    /// A forward-chaining inference rule
    /// </summary>
    [DebuggerDisplay("{Name}")]
    public class Rule
    {
        /// <summary>
        /// Name of the rule for debugging purpose
        /// </summary>
        public string Name;
        private readonly KBQuery preconditions;
        private readonly Action action;

        /// <summary>
        /// A rule that runs action for each match to the preconditions
        /// </summary>
        /// <param name="name">Name for debugging purposes</param>
        /// <param name="preconditions">Antecedents for the rule</param>
        /// <param name="action">Code to run when the rule matches the database</param>
        public Rule(string name, KBQuery preconditions, Action action)
        {
            this.Name = name;
            this.preconditions = preconditions;
            this.action = action;
        }

        /// <summary>
        /// Matches preconditions to database and runs action for each match
        /// </summary>
        public void Run()
        {
            preconditions.DoAll(action);
        }
    }
}
