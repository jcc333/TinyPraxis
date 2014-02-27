// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="KBQuery.cs" company="Ian Horswill">
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
    /// Used to search KB for all solutions to some query
    /// These are different from normal IEnumerable enumerators primarily so that
    /// we can reuse the damn things rather than having to constantly allocate and GC
    /// new ones.
    /// </summary>
    [DebuggerDisplay("{Name}")]
    public abstract class KBQuery
    {
        /// <summary>
        /// Restarts the query from the first solution
        /// </summary>
        public abstract void Reset();
        /// <summary>
        /// Finds the next solution, if any.
        /// </summary>
        /// <returns>True if another solution was found</returns>
        public abstract bool TryNext();

        /// <summary>
        /// Runs thunk once for each solution to the query
        /// </summary>
        /// <param name="thunk">Parameterless procedure to run</param>
        public void DoAll(Action thunk)
        {
            this.Reset();
            while (this.TryNext())
                thunk();
        }

        /// <summary>
        /// Returns values of variable from all solutions of query, retaining duplicates.
        /// </summary>
        /// <param name="v">Variable to find values of</param>
        public List<object> FindAll(Variable v)
        {
            var result = new List<object>();
            this.Reset();
            while (this.TryNext())
                result.Add(v.Value);
            return result;
        }

        /// <summary>
        /// Returns values of variable from all solutions of query, omitting duplicates.
        /// </summary>
        /// <param name="v">Variable to find values of</param>
        public List<object> FindAllUnique(Variable v)
        {
            var result = new List<object>();
            this.Reset();
            while (this.TryNext())
                if (!result.Contains(v.Value))
                    result.Add(v.Value);
            return result;
        }

        /// <summary>
        /// Returns value of variable in first result of query
        /// </summary>
        /// <param name="v">Variable to find values of</param>
        public object Find(Variable v)
        {
            this.Reset();
            if (this.TryNext())
                return v.Value;
            throw new NoSolutionException("No solution to query");
        }

        /// <summary>
        /// Creates a composite query from two queries that's satisfied when each of the component queries is satisfied.
        /// </summary>
        /// <param name="q1">First query</param>
        /// <param name="q2">Second query</param>
        /// <returns>New conjunctive query.</returns>
        public static KBQuery operator &(KBQuery q1, KBQuery q2)
        {
            return new AndQuery(q1, q2);
        }

        /// <summary>
        /// Converts a query to a bool.  True if the query matches at least one entry in the KB.
        /// </summary>
        /// <param name="q">The query.</param>
        /// <returns>True if the query matches something in the KB.</returns>
        public static implicit operator bool(KBQuery q)
        {
            q.Reset();
            return q.TryNext();
        }

        /// <summary>
        /// Unparses the query into key+key+key format
        /// </summary>
        /// <returns>Name in key+key+key format</returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Unparses the query into key+key+key format
        /// </summary>
        public string Name
        {
            get
            {
                var b = new StringBuilder();
                this.BuildName(b);
                return b.ToString();
            }
        }

        internal abstract void BuildName(StringBuilder b);
    }

    /// <summary>
    /// A query that returns a particular KB node, as opposed to e.g. and AndQuery
    /// </summary>
    public abstract class PrimitiveQuery : KBQuery
    {
        /// <summary>
        /// The KnowledgeBaseEntry that was found as part of this query.
        /// </summary>
        public KnowledgeBaseEntry Current { get; protected set; }

        /// <summary>
        /// Extends a query with a search for a non-exclusive child
        /// </summary>
        /// <param name="e">Query for parent node</param>
        /// <param name="v">Variable for child key</param>
        /// <returns></returns>
        public static PrimitiveQuery operator +(PrimitiveQuery e, Variable v)
        {
            if (v.Used)
                return new BoundVariableQuery(e, v);
            v.Used = true;
            return new UnboundVariableQuery(e, v);
        }

        /// <summary>
        /// Extends a query with a search for a non-exclusive child
        /// </summary>
        /// <param name="e">Query for parent node</param>
        /// <param name="o">Key value for desired child</param>
        /// <returns></returns>
        public static PrimitiveQuery operator +(PrimitiveQuery e, object o)
        {
            return new ConstantQuery(e, o);
        }

        /// <summary>
        /// Extends a query with a search for an exclusive child
        /// </summary>
        /// <param name="e">Query for parent node</param>
        /// <param name="v">Variable for child key</param>
        /// <returns></returns>
        public static PrimitiveQuery operator -(PrimitiveQuery e, Variable v)
        {
            if (v.Used)
                return new BoundVariableQuery(e, v);
            v.Used = true;
            return new UnboundVariableQuery(e, v);
        }

        /// <summary>
        /// Extends a query with a search for an exclusive child
        /// </summary>
        /// <param name="e">Query for parent node</param>
        /// <param name="o">Key value for desired child</param>
        /// <returns></returns>
        public static PrimitiveQuery operator -(PrimitiveQuery e, object o)
        {
            return new ConstantQuery(e, o);
        }

        internal abstract void DeleteFirst();

        internal abstract void DeleteAll();
    }

    /// <summary>
    /// A query that always returns exactly one KnowledgeBaseEntry
    /// </summary>
    class SingletonQuery : PrimitiveQuery
    {
        /// <summary>
        /// True if we've already returned our one solution.
        /// </summary>
        private bool done;

        /// <summary>
        /// Creates a QueryEnumerator that always returns exactly one KnowledgeBaseEntry
        /// </summary>
        /// <param name="e">The KnowledgeBaseEntry to return</param>
        public SingletonQuery(KnowledgeBaseEntry e)
        {
            Current = e;
        }

        /// <summary>
        /// Restarts the query
        /// </summary>
        public override void Reset()
        {
            done = false;
        }

        /// <summary>
        /// Attempts to find the first/next solution
        /// </summary>
        /// <returns>True on success</returns>
        public override bool TryNext()
        {
            if (done)
                return false;
            done = true;
            return done;
        }

        internal override void DeleteFirst()
        {
            throw new NotImplementedException();
        }

        internal override void DeleteAll()
        {
            throw new NotImplementedException();
        }

        internal override void BuildName(StringBuilder b)
        {
            b.Append("<singleton query>");
        }
    }

    /// <summary>
    /// Finds all the children under all the entries found by the parent, and binds the keys of those children to variable
    /// </summary>
    [DebuggerDisplay("Enumerate {variable.Name}={variable.Value}")]
    class UnboundVariableQuery : PrimitiveQuery
    {
        public UnboundVariableQuery(PrimitiveQuery parent, Variable variable)
        {
            this.parent = parent;
            this.variable = variable;
        }

        private readonly Variable variable;
        private readonly PrimitiveQuery parent;

        /// <summary>
        /// The position within parent's Current solution's Children list of our Current solution
        /// </summary>
        private int index;

        /// <summary>
        /// Restart query.
        /// </summary>
        public override void Reset()
        {
            parent.Reset();
            index = -1;
        }

        /// <summary>
        /// Try for another solution, asking parent for new solution if necessary.
        /// </summary>
        /// <returns>Success</returns>
        public override bool TryNext()
        {
            if (index == -1 && !parent.TryNext())
                    return false;   // Parent didn't have any solutions
            index++;
            while (index == parent.Current.Children.Count)
            {
                // We've exhaused parent's Current children; ask parent for another solution
                index = 0;
                if (!parent.TryNext())
                    // Parent is depleted
                    return false;
            }
            // Got one.
            Current = parent.Current.Children[index];
            variable.Value = Current.Key;
            return true;
        }

        internal override void DeleteFirst()
        {
            this.Reset();
            if (this.TryNext())
            {
                parent.Current.Children.Remove(Current);
            }
        }

        internal override void DeleteAll()
        {
            this.Reset();
            while (this.TryNext())
            {
                parent.Current.Children.Clear();
                index = -1;  // Force TryN
            }
        }

        internal override void BuildName(StringBuilder b)
        {
            if (!(parent is SingletonQuery))
            {
                parent.BuildName(b);
                b.Append("+");
            }
            b.Append(variable.Name);
        }
    }

    /// <summary>
    /// Finds  the child (if any) under each solution found by the parent, that matches the value of a bound variable
    /// </summary>
    [DebuggerDisplay("Filter {variable.Name}={variable.Value}")]
    class BoundVariableQuery : PrimitiveQuery
    {
        public BoundVariableQuery(PrimitiveQuery parent, Variable variable)
        {
            this.parent = parent;
            this.variable = variable;
        }

        private readonly Variable variable;

        private readonly PrimitiveQuery parent;

        private bool done;

        /// <summary>
        /// Restarts query
        /// </summary>
        public override void Reset()
        {
            parent.Reset();
            done = false;
        }

        /// <summary>
        /// Attempts to find the next solution
        /// </summary>
        /// <returns>Success</returns>
        public override bool TryNext()
        {
            if (!done)
            {
                while (parent.TryNext())
                    foreach (var c in parent.Current.Children)
                        if (c.Key.Equals(variable.Value))
                        {
                            Current = c;
                            return true;
                        }
                done = true;
            }

            return false;
        }

        internal override void DeleteFirst()
        {
            this.Reset();
            if (this.TryNext())
            {
                parent.Current.Children.Remove(Current);
            }
        }

        internal override void DeleteAll()
        {
            this.Reset();
            while (this.TryNext())
            {
                parent.Current.Children.Remove(Current);
            }
        }

        internal override void BuildName(StringBuilder b)
        {
            if (!(parent is SingletonQuery))
            {
                parent.BuildName(b);
                b.Append("+");
            }
            b.Append(variable.Name);
        }
    }

    /// <summary>
    /// Enumerates the child contains key (if any) of each solution of parent
    /// </summary>
    [DebuggerDisplay("Match constant {key}")]
    class ConstantQuery : PrimitiveQuery
    {
        /// <summary>
        /// Enumerates the child contains key (if any) of each solution of parent
        /// </summary>
        public ConstantQuery(PrimitiveQuery parent, object key)
        {
            this.parent = parent;
            this.key = key;
        }

        private readonly object key;
        private readonly PrimitiveQuery parent;

        private bool done;

        /// <summary>
        /// Restarts query
        /// </summary>
        public override void Reset()
        {
            parent.Reset();
            done = false;
        }

        /// <summary>
        /// Attempts to find the next solution
        /// </summary>
        /// <returns>Success</returns>
        public override bool TryNext()
        {
            if (!done)
            {
                while (parent.TryNext())
                    foreach (var c in parent.Current.Children)
                        if (c.Key.Equals(key))
                        {
                            Current = c;
                            return true;
                        }
                done = true;
            }

            return false;
        }

        internal override void DeleteFirst()
        {
            this.Reset();
            if (this.TryNext())
            {
                parent.Current.Children.Remove(Current);
            }
        }

        internal override void DeleteAll()
        {
            this.Reset();
            while (this.TryNext())
            {
                parent.Current.Children.Remove(Current);
            }
        }

        internal override void BuildName(StringBuilder b)
        {
            if (!(parent is SingletonQuery))
            {
                parent.BuildName(b);
                b.Append("+");
            }
            b.Append(key);
        }
    }

    class AndQuery : KBQuery
    {
        public AndQuery(KBQuery q1, KBQuery q2)
        {
            query1 = q1;
            query2 = q2;
            state = State.Reset;
        }

        private readonly KBQuery query1;

        private readonly KBQuery query2;

        enum State { Reset, InProgress, Exhausted };

        private State state;

        public override void Reset()
        {
            query1.Reset();
            state = State.Reset;
        }
        
        public override bool TryNext()
        {
            switch (state)
            {
                case State.Reset:
                    while (query1.TryNext())
                    {
                        query2.Reset();
                        if (query2.TryNext())
                        {
                            state = State.InProgress;
                            return true;
                        }
                    }
                    state = State.Exhausted;
                    return false;

                case State.InProgress:
                    do
                    {
                        if (query2.TryNext())
                            return true;
                        query2.Reset();
                    }
                    while (query1.TryNext());
                    state = State.Exhausted;
                    return false;

                case State.Exhausted:
                    return false;
            }
            return false;
        }

        internal override void BuildName(StringBuilder b)
        {
            query1.BuildName(b);
            b.Append(" & ");
            query2.BuildName(b);
        }
    }

    internal class PredicateQuery : KBQuery
    {
        private readonly Func<bool> predicate;
        private bool done;
        internal PredicateQuery(Func<bool> predicate)
        {
            this.predicate = predicate;
        }

        public override void Reset()
        {
            this.done = false;
        }

        public override bool TryNext()
        {
            if (done)
                return false;
            done = true;
            return predicate();
        }

        internal override void BuildName(StringBuilder b)
        {
            b.Append("<predicate>");
        }
    }
}
