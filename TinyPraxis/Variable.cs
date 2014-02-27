// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="Variable.cs" company="Ian Horswill">
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
using System.Diagnostics.CodeAnalysis;

namespace TinyPraxis
{
    /// <summary>
    /// Variables used within KBQueries.
    /// </summary>
    [DebuggerDisplay("{Name}={Value}")]
#pragma warning disable 660,661
    public class Variable
#pragma warning restore 660,661
    {
        /// <summary>
        /// Creates a variable for use in a KBQuery.
        /// </summary>
        /// <param name="name">Name of variable (for debugging purposes)</param>
        public Variable(string name)
        {
            Name = name;
        }

        private object mValue;

        /// <summary>
        /// Current value of the variable
        /// </summary>
        public object Value
        {
            get
            {
                if (!Used)
                    throw new UnboundVariableException("Attempt to read value of unbound variable "+Name);

                return mValue;
            }
            set
            {
                mValue = value;
                Used = true;
            }
        }
        /// <summary>
        /// True if the variable has already been used once in the query.
        /// If false, this is the first reference to the variable, and so this
        /// reference should bind it.  If true, it's already bound and so the
        /// reference should match it.
        /// </summary>
        public bool Used { get; set; }
        /// <summary>
        /// Name (for debugging purposes)
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// True if the two variables are bound to different values.
        /// IMPORTANT:
        /// - Cannot be used on unbound variables
        /// - Doesn't test if they're the same variable, just if they
        ///   have the same value.
        /// </summary>
        /// <param name="v1">First variable</param>
        /// <param name="v2">Second variable</param>
        /// <returns>True if they have different values</returns>
        /// <exception cref="InvalidOperationException">If either variable is unbound</exception>
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public static KBQuery operator !=(Variable v1, Variable v2)
        {
// ReSharper disable PossibleNullReferenceException
            if (!v1.Used || !v2.Used)
// ReSharper restore PossibleNullReferenceException
                throw new InvalidOperationException("!= can only be used on bound variables.");
            return new PredicateQuery(
                () => !(v1.Value.Equals(v2.Value))
                );
        }

        /// <summary>
        /// True if the two variables are bound to the same value.
        /// IMPORTANT:
        /// - Cannot be used on unbound variables
        /// - Doesn't test if they're the same variable, just if they
        ///   have the same value.
        /// </summary>
        /// <param name="v1">First variable</param>
        /// <param name="v2">Second variable</param>
        /// <returns>True if they have the same value</returns>
        /// <exception cref="InvalidOperationException">If either variable is unbound</exception>
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public static KBQuery operator ==(Variable v1, Variable v2)
        {
// ReSharper disable PossibleNullReferenceException
            if (!v1.Used || !v2.Used)
// ReSharper restore PossibleNullReferenceException
                throw new InvalidOperationException("== can only be used on bound variables.");
            return new PredicateQuery(() => v1.Value.Equals(v2.Value));
        }
    }
}
