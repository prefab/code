using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace PrefabIdentificationLayers.Models {
    /// <summary>
    /// A part of a prototype (either a feature or a region).
    /// This is used for building prototypes (assigning values to parts).
    /// </summary>
    public sealed class Part {

        /// <summary>
        /// The possible values this part can take.
        /// </summary>
        public ArrayList AllValues;

        
        /// <summary>
        /// The values that are valid given the current
        /// partial assignment of a prototype.
        /// </summary>
        public ArrayList CurrentValidValues
        {
            get;
            set;
        }

        /// <summary>
        /// Current assigned value of the part.
        /// </summary>
        public object AssignedValue
        {
            get;
            private set;
        }

        /// <summary>
        /// True if the part is assigned a value.
        /// </summary>
        public bool IsAssigned
        {
            get;
            private set;
        }

        /// <summary>
        /// Construcs the part with the given possible values
        /// that can be assigned to the part.
        /// </summary>
        /// <param name="values"></param>
        public Part(ICollection values) {
            AllValues = new ArrayList(values);
            CurrentValidValues = new ArrayList();
        }

        /// <summary>
        /// Creates a copy of the given part.
        /// </summary>
        /// <param name="toCopy"></param>
        public Part(Part toCopy) : this(toCopy.AllValues)
        {
            CurrentValidValues.AddRange(toCopy.CurrentValidValues);
            IsAssigned = toCopy.IsAssigned;
            AssignedValue = toCopy.AssignedValue;
        }

        /// <summary>
        /// Assigns a value to this part.
        /// </summary>
        /// <param name="value"></param>
        public void AssignValue(object value) {
            CurrentValidValues.Clear();
            CurrentValidValues.Add(value);
            IsAssigned = true;
            AssignedValue = value;
        }

        /// <summary>
        /// Unassigns a value to this part and restores teh current valid
        /// values to be all possible values.
        /// </summary>
        public void UnassignValueAndRestoreDomain() {
            CurrentValidValues.Clear();
            CurrentValidValues.AddRange(AllValues);
            IsAssigned = false;
            AssignedValue = null;
        }

        /// <summary>
        /// Removes a value from the list of current valid values.
        /// </summary>
        /// <param name="value"></param>
        public void RemoveValue(object value) {
            CurrentValidValues.Remove(value);
        }

        /// <summary>
        /// Removes the list of values from the current list of valid values.
        /// </summary>
        /// <param name="values"></param>
        public void RemoveValues(ArrayList values)
        {
            foreach (object obj in values)
                RemoveValue(obj);
        }

        public override string ToString()
        {
            if (IsAssigned)
                return AssignedValue.ToString();
            else
                return CurrentValidValues.ToString();
        }
    }
}
