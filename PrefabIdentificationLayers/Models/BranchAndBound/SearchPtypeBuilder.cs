using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections;
using PrefabIdentificationLayers.Prototypes;
using Prefab;

namespace PrefabIdentificationLayers.Models
{
	public sealed class SearchPtypeBuilder : PtypeBuilder
    {

        private string _model; //name of the model so that we can use it to find occurrences when matching negative examples.
        private ICostFunction _costFunction; //the cost function
        private IPartGetter _partGetter;  //used to get the parts and their initial possible values
        private IConstraintGetter _constraintGetter; //returns the set of constraints
        private IPartOrderSelecter _orderSelecter;
        private IPtypeFromAssignment _constructor;
        private class State
        {

            public State(Dictionary<string,Part> parts, IEnumerable<Constraint> constraints, IEnumerable<Bitmap> positives, IEnumerable<Bitmap> negatives)
            {
                Parts = parts;
                Constraints = constraints;

                List<Bitmap> pos = new List<Bitmap>();
                if (positives != null)
                    pos.AddRange(positives);

                List<Bitmap> neg = new List<Bitmap>();
                if (negatives != null)
                    neg.AddRange(negatives);

                PositiveExamples = pos;
                NegativeExamples = neg;

                BestCost = double.PositiveInfinity;
                Cache = new Dictionary<object, object>();
            }
            public Dictionary<string, Part> Parts
            {
                get;
                private set;
            }
            public Dictionary<object, object> Cache
            {
                get;
                private set;
            }
            public IEnumerable<Constraint> Constraints
            {
                get;
                private set;
            }

            public Ptype.Mutable BestRendition;

            public double BestCost;

            public IEnumerable<Bitmap> PositiveExamples
            {
                get;
                private set;
            }

            public IEnumerable<Bitmap> NegativeExamples
            {
                get;
                private set;
            }
        }

        public SearchPtypeBuilder(string model, ICostFunction costFunc, IPartGetter partGetter, 
            IConstraintGetter constraintGetter, IPartOrderSelecter orderSelecter, IPtypeFromAssignment constructor)
        {
            _model = model;
            _costFunction = costFunc;
            _constraintGetter = constraintGetter;
            _partGetter = partGetter;
            _orderSelecter = orderSelecter;
            _constructor = constructor;
        }

        /// <summary>
        /// Parameterizes a prototype using a branch and bound search algorithm.
        /// </summary>
        /// <param name="positives">Positive example bitmaps</param>
        /// <param name="negatives">Negative example bitmaps</param>
        /// <returns></returns>
        public Ptype.Mutable BuildPrototype(IBuildPrototypeArgs args)
        {
            Examples eargs = args.Examples;
            IEnumerable<Bitmap> positives = eargs.Positives;
            IEnumerable<Bitmap> negatives = eargs.Negatives;

            Dictionary<string, Part> parts = _partGetter.GetParts(positives, negatives);
            IEnumerable<Constraint> constraints = _constraintGetter.GetConstraints(parts, positives, negatives);

            State state = new State(parts, constraints, positives, negatives);


            //Run an initial constraint propagation to reduce the branching factor
            AC_3(false, constraints);


            return Search(state);
        }

        /// <summary>
        /// Searches for the best rendition given the variables and constraints.
        /// </summary>
        private Ptype.Mutable Search(State state)
        {
            //Removes any paths that conflict with the given constraints
            if (!Propagate(state.Parts.Values, state.Constraints))
                return state.BestRendition;

            //Computes the cost of the full or partial assignment
            double cost = _costFunction.Cost(state.Parts, state.PositiveExamples, state.NegativeExamples, state.Cache );

            if (cost >= state.BestCost)
                return state.BestRendition;


            //If the assignment is complete, then we know it's 
            //the best cost so far, so assign it as the best rendition
            if (CompleteAssignment(state.Parts.Values))
            {
                Ptype.Mutable rendition = GetPrototype(state);
                if (!MatchesAnyNegatives(rendition, state.NegativeExamples))
                {
                    state.BestCost = cost;
                    state.BestRendition = rendition;
                }
                return state.BestRendition;
            }

            //Select the next variable to assign
            Part partToAssign = _orderSelecter.SelectNextPartToAssign(state.Parts, state.PositiveExamples, state.NegativeExamples);

            //Copy the variable's values so we can iterate through them
            ArrayList partValues = new ArrayList(partToAssign.CurrentValidValues);

            //Assign each value to the variable and continue searching
            foreach (object value in partValues)
            {
                partToAssign.AssignValue(value);
                Search(state);
                UndoPropogate(state.Parts.Values);
            }

            //Give the variable all of it's values back
            partToAssign.UnassignValueAndRestoreDomain();

            //Put all of the possible search paths back
            UndoPropogate(state.Parts.Values);


            return state.BestRendition;
        }

        private Ptype.Mutable GetPrototype(State state)
        {
            Ptype.Mutable ptype =  _constructor.ConstructPtype(state.Parts, state.PositiveExamples, state.NegativeExamples, state.Cache);
            ptype.Model = _model;
            return ptype;
        }

        private bool MatchesAnyNegatives(Ptype.Mutable ptype, IEnumerable<Bitmap> negatives)
        {
            
            foreach (Bitmap negative in negatives)
            {
				IEnumerable<Tree> occurrences = PrototypeDetectionLayer.FindPrototypeOccurrences (negative, ptype);
				foreach(Tree occurrence in occurrences){

                    if (   occurrence != null 
                        && occurrence.Left == 0 
                        && occurrence.Top == 0 
                        && occurrence.Width == negative.Width
                        && occurrence.Height == negative.Height)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Puts all of the values that were removed
        /// during propogate back into the variables
        /// </summary>
        private void UndoPropogate(IEnumerable<Part> parts)
        {
            foreach (Part part in parts)
            {
                if (!part.IsAssigned)
                    part.UnassignValueAndRestoreDomain();
                else
                {
                    part.AssignValue(part.AssignedValue);
                }
            }
        }

        /// <summary>
        /// Returns true if all of the variables are assigned. False otherwise.
        /// </summary>
        /// <param name="vars">The variables to check if assigned.</param>
        /// <returns>True if all of the variables are assigned</returns>
        public static bool CompleteAssignment(IEnumerable<Part> parts)
        {
            foreach (Part p in parts)
            {
                if (!p.IsAssigned)
                    return false;
            }

            return true;
        }


        /// <summary>
        /// Removes values from variables if they fail any constraints
        ///   Then checks if any variables completely drop out
        /// </summary>
        /// <returns>returns true if it does not fail, false otherwise</returns>
        private bool Propagate(IEnumerable<Part> parts, IEnumerable<Constraint> constraints)
        {

            UndoPropogate(parts);
            AC_3(true, constraints);

            foreach (Part part in parts)
            {
                if (part.CurrentValidValues.Count == 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Removes the inconsistent values of each part using the given
        /// constraints. This is a standard propagation technique.
        /// See documentation for it at http://en.wikipedia.org/wiki/AC-3_algorithm
        /// </summary>
        /// <param name="useCurrentValidValues">False specifies if this is the initial
        /// propagation that permanentally removes states. True specifies if the values
        /// will only be temporarily removed.</param>
        private void AC_3(bool useCurrentValidValues, IEnumerable<Constraint> constraints)
        {
            Queue<Constraint> queue = new Queue<Constraint>(constraints);
            if (!useCurrentValidValues)
            {
                foreach (Constraint c in constraints)
                    queue.Enqueue(new Constraint(c.Satisfied, c.Part2, c.Part1));
            }

            while (queue.Count > 0)
            {
                Constraint c = queue.Dequeue();
                ArrayList values;
                if (useCurrentValidValues)
                    values = c.Part1.CurrentValidValues;
                else
                    values = c.Part1.AllValues;


                if (RemoveInconsistentValues(c, useCurrentValidValues))
                {
                    if (values.Count == 0)
                        return;
                    else
                    {
                        IEnumerable<Constraint> neighbors = ConstraintsWith(c.Part1, c, constraints);
                        foreach (Constraint toEnqueue in neighbors)
                            queue.Enqueue(toEnqueue);
                    }
                }
            }
        }


        /// <summary>
        /// Returns a list of constraints with the given variable.
        /// </summary>
        /// <param name="var">Variable to check for in each constraint</param>
        /// <param name="excludeThisConstraint">A constraint to ignore</param>
        /// <param name="constraints">List of constraints to check</param>
        /// <returns>The constraints that have var as a variable, excluding excludeThisConstraint</returns>
        private IEnumerable<Constraint> ConstraintsWith(Part part, Constraint excludeThisConstraint, IEnumerable<Constraint> constraints)
        {
            List<Constraint> toReturn = new List<Constraint>();
            foreach (Constraint c in constraints)
            {
                if ((c.Part1 == part || c.Part2 == part) && c != excludeThisConstraint)
                {
                    toReturn.Add(c);
                }
                //else if (c.Part2 == part && c != excludeThisConstraint)
                //{
                //    toReturn.Add(c);
                //    //Constraint reverse = Reverse(constraints, c);
                //    //if(reverse != excludeThisConstraint)
                //    //    toReturn.Add(reverse);
                //}
            }

            return toReturn;
        }

        /// <summary>
        /// Given a constraint, it removes the values from any variables
        /// that do not satisify the constraint.
        /// </summary>
        /// <param name="constraint">The constraint to apply</param>
        /// <returns>True if any values were removed</returns>
        private bool RemoveInconsistentValues(Constraint constraint, bool useCurrentValidValues)
        {

            bool removed = false;
            ArrayList toRemove = new ArrayList();
            ArrayList constraintValues = constraint.Part1.CurrentValidValues;

            ArrayList correspondingValues = constraint.Part2.CurrentValidValues;

            if (!useCurrentValidValues)
            {
                constraintValues = constraint.Part1.AllValues;
                correspondingValues = constraint.Part2.AllValues;
            }


            foreach (object value in constraintValues)
            {
                if (NoValueSatisfies(constraint, value, correspondingValues))
                {
                    toRemove.Add(value);
                    removed = true;
                }
            }
            foreach (object rm in toRemove)
                constraintValues.Remove(rm);

            return removed;
        }

        /// <summary>
        /// Returns true if there is no such value v2 in the
        /// given consraint that allow the constraint {v1,v2} to be true.
        /// </summary>
        /// <param name="constraint">Constraint to check</param>
        /// <param name="v1">Value to satisfy</param>
        /// <returns>True if nothing satisfies v1</returns>
        private bool NoValueSatisfies(Constraint constraint, object v1, IEnumerable values)
        {

            foreach (object v2 in values)
            {
                if (constraint.Satisfied(v1, v2))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
