// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="KB.cs" company="Ian Horswill">
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

namespace TinyPraxis
{
    /// <summary>
    /// The default knowledgebase
    /// </summary>
    public static class KB
    {
        /// <summary>
        /// Initializes the KB.
        /// </summary>
        static KB()
        {
            Clear();
 
        }

        /// <summary>
        /// The actual root node of the KB.
        /// </summary>
        private static KnowledgeBaseEntry root;

        /// <summary>
        /// Begins an operation for storing an entry in the KB.
        /// Follow this with + and - expressions to specify the data to store.
        /// </summary>
        public static KnowledgeBaseEntry Store
        {
            get
            {
                return root;
            }
        }

        /// <summary>
        /// Starts a query of the KB.  Follow this with + and - expressions to add elements to the query.
        /// </summary>
        public static PrimitiveQuery Query
        {
            get
            {
                return new SingletonQuery(root);
            }
        }

        /// <summary>
        /// Deletes all data from the KB.
        /// </summary>
        public static void Clear()
        {
            root = new KnowledgeBaseEntry(null, null);
        }

        /// <summary>
        /// Adds entries to the KB.
        /// </summary>
        /// <param name="assertions">Entries to add</param>
        public static void Assert(params KnowledgeBaseEntry[] assertions)
        {
            // Does nothing.  This is just because KB.Store+A+B won't compile because the compiler doesn't think it has side effects.
        }

        /// <summary>
        /// Removes the first KB entry matching query.
        /// </summary>
        /// <param name="query">Query to specify what entry to remove</param>
        public static void Retract(PrimitiveQuery query)
        {
            query.DeleteFirst();
        }

        /// <summary>
        /// Removes all KB entries matchign query.
        /// </summary>
        /// <param name="query">Query specifying entries to remove</param>
        public static void RetractAll(PrimitiveQuery query)
        {
            query.DeleteAll();
        }
    }
}
