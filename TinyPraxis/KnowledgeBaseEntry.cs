// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="KnowledgeBaseEntry.cs" company="Ian Horswill">
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace TinyPraxis
{
    /// <summary>
    /// A node in the trie representing a collection of exclusion logic assertions
    /// </summary>
    [DebuggerDisplay("{Name}")]
    public class KnowledgeBaseEntry
    {
        /// <summary>
        /// Unparses the entry into key+key+key format
        /// </summary>
        /// <returns>Name in key+key+key format</returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Unparses the entry into key+key+key format
        /// </summary>
        public string Name
        {
            get
            {
                if (Parent == null)
                    return "<root>";
                var b = new StringBuilder();
                this.BuildName(b);
                return b.ToString();
            }
        }

        void BuildName(StringBuilder b)
        {
            if (Parent != null && Parent.Parent != null)
            {
                Parent.BuildName(b);
                b.Append(Parent.mode == ExclusionMode.Exclusive ? "-" : "+");
            }
            b.Append(Key);
        }

        internal KnowledgeBaseEntry(KnowledgeBaseEntry parent, object key)
        {
            Key = key;
            Parent = parent;
            Children = EmptyChildren;
        }

        /// <summary>
        /// Parent node of this KB node.  Used primarily for printing.
        /// </summary>
        public KnowledgeBaseEntry Parent { get; private set; }

        /// <summary>
        /// The value associated with this node.
        /// </summary>
        public object Key;
        /// <summary>
        /// List of the child nodes of this node.
        /// </summary>
        public List<KnowledgeBaseEntry> Children { get; private set; }

        /// <summary>
        /// An empty list.  Cached so there need only be one lying around.
        /// </summary>
        static readonly List<KnowledgeBaseEntry> EmptyChildren = new List<KnowledgeBaseEntry>(); 

        private enum ExclusionMode { Empty, Exclusive, NonExclusive };

        private ExclusionMode mode;

        /// <summary>
        /// Store an exclusive child inside this node.
        /// </summary>
        /// <param name="v">Bound variable holding the key for the child</param>
        /// <param name="overwrite">If true, this will overwrite any existing child with a different value.</param>
        /// <returns>The child node</returns>
        /// <exception cref="KBExclusionException">If a non-exclusive child has already been written.</exception>
        public KnowledgeBaseEntry StoreExclusive(Variable v, bool overwrite)
        {
            return StoreExclusive(v.Value, overwrite);
        }

        /// <summary>
        /// Store an exclusive child inside this node.
        /// </summary>
        /// <param name="key">Key for the child</param>
        /// <param name="overwrite">If true, this will overwrite any existing child with a different value.</param>
        /// <returns>The child node</returns>
        /// <exception cref="KBExclusionException">If a non-exclusive child has already been written.</exception>
        public KnowledgeBaseEntry StoreExclusive(object key, bool overwrite)
        {
            switch (mode)
            {
                case ExclusionMode.Empty:
                    {
                        mode = ExclusionMode.Exclusive;
                        var result = new KnowledgeBaseEntry(this, key);
                        if (this.Children == EmptyChildren)
                            this.Children = new List<KnowledgeBaseEntry> { result };
                        else
                            this.Children.Add(result);

                        return result;
                    }

                case ExclusionMode.NonExclusive:
                    throw new KBExclusionException("Exclusive store on non-exclusive node.");


                case ExclusionMode.Exclusive:
                    if (overwrite)
                        this.Children[0].OverwriteExclusive(key);
                    else if (key != this.Children[0].Key)
                        throw new KBExclusionException("Exclusive store doesn't match previous store.");
                    return this.Children[0];

                default:
                    throw new InvalidOperationException("Invalid exclusion mode");
            }
        }

        /// <summary>
        /// Store a non-exclusive child inside this node.
        /// </summary>
        /// <param name="v">Bound variable holding the key for the child</param>
        /// <returns>The child node</returns>
        /// <exception cref="KBExclusionException">If an exclusive child has already been written.</exception>
        public KnowledgeBaseEntry StoreNonExclusive(Variable v)
        {
            return StoreNonExclusive(v.Value);
        }

        /// <summary>
        /// Store a non-exclusive child inside this node.
        /// </summary>
        /// <param name="key">Key for the new child</param>
        /// <returns>The child node</returns>
        /// <exception cref="KBExclusionException">If an exclusive child has already been written.</exception>
        public KnowledgeBaseEntry StoreNonExclusive(object key)
        {
            KnowledgeBaseEntry result =null;
            switch (mode)
            {
                case ExclusionMode.Empty:
                    {
                        mode = ExclusionMode.NonExclusive;
                        result = new KnowledgeBaseEntry(this, key);
                        if (this.Children == EmptyChildren)
                            this.Children = new List<KnowledgeBaseEntry> { result };
                        else
                            this.Children.Add(result);
                    }
                    break;

                case ExclusionMode.Exclusive:
                    throw new KBExclusionException("Non-exclusive store on exclusive node.");

                case ExclusionMode.NonExclusive:
                    foreach (var c in this.Children)
                    {
                        if (c.Key == key)
                            return c;
                    }
                    this.Children.Add(result = new KnowledgeBaseEntry(this, key));
                    break;
            }
            return result;
        }

        private void OverwriteExclusive(object key)
        {
            Key = key;
            this.Children.Clear();
            mode = ExclusionMode.Empty;
        }

        #region Store operators
        /// <summary>
        /// Write the specified key as a non-exclusive child.  If key is already a child, this has no effect
        /// </summary>
        /// <param name="e">KB node</param>
        /// <param name="v">Bound variable holding the key for the child</param>
        /// <returns>The child node containing key.</returns>
        /// <exception cref="KBExclusionException">If an exclusive child has already been written in this node.</exception>
        public static KnowledgeBaseEntry operator +(KnowledgeBaseEntry e, Variable v)
        {
            return e.StoreNonExclusive(v);
        }

        /// <summary>
        /// Write the specified key as a non-exclusive child.  If key is already a child, this has no effect
        /// </summary>
        /// <param name="e">KB node</param>
        /// <param name="key">Key to write</param>
        /// <returns>The child node containing key.</returns>
        /// <exception cref="KBExclusionException">If an exclusive child has already been written in this node.</exception>
        public static KnowledgeBaseEntry operator +(KnowledgeBaseEntry e, object key)
        {
            return e.StoreNonExclusive(key);
        }

        /// <summary>
        /// Write the specified key as an exclusive child.
        /// If key is already the child, this has no effect.
        /// Otherwise, the current child is replaced with this key.
        /// </summary>
        /// <param name="e">KB node</param>
        /// <param name="v">Bound variable holding the key for the child</param>
        /// <returns>The child node containing key.</returns>
        /// <exception cref="KBExclusionException">If a non-exclusive child has already been written in this node.</exception>
        public static KnowledgeBaseEntry operator -(KnowledgeBaseEntry e, Variable v)
        {
            return e.StoreExclusive(v, true);
        }

        /// <summary>
        /// Write the specified key as an exclusive child.
        /// If key is already the child, this has no effect.
        /// Otherwise, the current child is replaced with this key.
        /// </summary>
        /// <param name="e">KB node</param>
        /// <param name="key">Key to write</param>
        /// <returns>The child node containing key.</returns>
        /// <exception cref="KBExclusionException">If a non-exclusive child has already been written in this node.</exception>
        public static KnowledgeBaseEntry operator -(KnowledgeBaseEntry e, object key)
        {
            return e.StoreExclusive(key, true);
        }
        #endregion
    }
}
