﻿using System.Collections.Generic;
using System.Linq;

namespace Io.JoeMoceri.ExpressionEvaluator
{
    /// <summary>
    /// 
    /// </summary>
    public class HL7V2Field : HL7V2FieldBase
    {
        /// <summary>
        /// 
        /// </summary>
        public HL7V2Field()
        {
            FieldRepetitions = new List<HL7V2FieldRepetition>();
        }

        /// <summary>
        /// 
        /// </summary>
        public override int Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public override string Delimiter { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public override string Value { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IList<HL7V2FieldRepetition> FieldRepetitions { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public HL7V2FieldRepetition GetFieldRepetition(int id)
        {
            return FieldRepetitions.FirstOrDefault(fr => fr.Id.Equals(id));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="repetition"></param>
        /// <returns></returns>
        public IList<HL7V2Component> Components(int repetition = 1)
        {
            return FieldRepetitions?.FirstOrDefault(fr => fr.Id.Equals(repetition))?.Components;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="repetition"></param>
        /// <returns></returns>
        public HL7V2Component GetComponent(int id, int repetition = 1)
        {
            return FieldRepetitions?.FirstOrDefault(fr => fr.Id.Equals(repetition))?.Components?.FirstOrDefault(f => f.Id.Equals(id));
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Rebuild()
        {
            if (FieldRepetitions.Count > 0)
            {
                for (var i = 0; i < FieldRepetitions.Count; i++)
                {
                    FieldRepetitions[i].Rebuild();
                    FieldRepetitions[i].Id = i + 1;
                }

                Value = CombineHL7Fields(FieldRepetitions.Cast<HL7V2FieldBase>().ToList());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="repetition"></param>
        /// <returns></returns>
        public HL7V2Component this[int id, int repetition = 1]
        {
            get
            {
                return FieldRepetitions?.FirstOrDefault(fr => fr.Id.Equals(repetition))?.Components?.FirstOrDefault(f => f.Id.Equals(id));
            }
        }

        #region Component Operations
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="repetition"></param>
        /// <returns></returns>
        public HL7V2Component AddComponent(string value, int repetition = 1)
        {
            var fieldRepetition = FieldRepetitions.FirstOrDefault(fr => fr.Id.Equals(repetition));

            if (fieldRepetition == null)
            {
                return null;
            }

            var result = new HL7V2Component
            {
                Delimiter = HL7V2ExpressionConfiguration.componentDelimiter,
                Id = fieldRepetition.Components.Count > 0 ? fieldRepetition.Components.Last().Id + 1 : 1,
                Value = value
            };

            if (value.Contains(HL7V2ExpressionConfiguration.subComponentDelimiter))
            {
                var subComponents = result.Value.Split(HL7V2ExpressionConfiguration.subComponentDelimiter);
                for (var i = 0; i < subComponents.Length; i++)
                {
                    result.AddSubComponent(subComponents[i]);
                }
            }

            fieldRepetition.Components.Add(result);

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="repetition"></param>
        /// <returns></returns>
        public bool RemoveComponent(int id, int repetition = 1)
        {
            var fieldRepetition = FieldRepetitions.FirstOrDefault(fr => fr.Id.Equals(repetition));

            if (fieldRepetition == null)
            {
                return false;
            }

            var component = fieldRepetition.Components.FirstOrDefault(c => c.Id.Equals(id));

            if (component == null)
            {
                return false;
            }

            return fieldRepetition.Components.Remove(component);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <param name="repetition"></param>
        /// <returns></returns>
        public HL7V2Component InsertComponent(int id, string value, int repetition = 1)
        {
            var fieldRepetition = FieldRepetitions.FirstOrDefault(fr => fr.Id.Equals(repetition));

            if (fieldRepetition == null)
            {
                return null;
            }

            if (id >= fieldRepetition.Components.Max(fr => fr.Id) || id <= 0)
            {
                return null;
            }

            var component = new HL7V2Component
            {
                Delimiter = HL7V2ExpressionConfiguration.componentDelimiter,
                Id = id,
                Value = value
            };

            var currentComponent = fieldRepetition.Components.FirstOrDefault(c => c.Id.Equals(id));

            if (currentComponent == null)
            {
                return null;
            }

            var currentIndex = fieldRepetition.Components.IndexOf(currentComponent);

            // increment the Ids of the components after
            foreach (var c in fieldRepetition.Components)
            {
                if (c.Id > currentIndex)
                {
                    c.Id++;
                }
            }

            fieldRepetition.Components.Insert(currentIndex, component);

            return component;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <param name="repetition"></param>
        /// <returns></returns>
        public HL7V2Component UpdateComponent(int id, string value, int repetition = 1)
        {
            var fieldRepetition = FieldRepetitions.FirstOrDefault(fr => fr.Id.Equals(repetition));

            if (fieldRepetition == null)
            {
                return null;
            }

            if (id >= fieldRepetition.Components.Max(fr => fr.Id) || id <= 0)
            {
                return null;
            }

            var component = fieldRepetition.Components.FirstOrDefault(c => c.Id.Equals(id));

            if (component == null)
            {
                return null;
            }

            component.Value = value;

            return component;
        }
        #endregion

        #region Field Repetition Operations
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HL7V2FieldRepetition AddFieldRepetition(string value)
        {
            var result = new HL7V2FieldRepetition
            {
                Delimiter = HL7V2ExpressionConfiguration.fieldRepetitionDelimiter,
                Id = FieldRepetitions.Count > 0 ? FieldRepetitions.Last().Id + 1 : 1,
                Value = value,
            };

            if (value.Contains(HL7V2ExpressionConfiguration.componentDelimiter))
            {
                var components = result.Value.Split(HL7V2ExpressionConfiguration.componentDelimiter);
                for (var i = 0; i < components.Length; i++)
                {
                    result.AddComponent(components[i]);
                }
            }

            FieldRepetitions.Add(result);

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool RemoveFieldRepetition(int id)
        {
            var fr = FieldRepetitions.FirstOrDefault(f => f.Id.Equals(id));

            if (fr == null)
            {
                return false;
            }

            return FieldRepetitions.Remove(fr);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public HL7V2FieldRepetition InsertFieldRepetition(int id, string value)
        {
            if (id >= FieldRepetitions.Max(fr => fr.Id) || id <= 0)
            {
                return null;
            }

            var fieldRepetition = new HL7V2FieldRepetition
            {
                Delimiter = HL7V2ExpressionConfiguration.fieldRepetitionDelimiter,
                Id = id,
                Value = value
            };

            var pFr = FieldRepetitions.FirstOrDefault(fr => fr.Id.Equals(id));

            if (pFr == null)
            {
                return pFr;
            }

            var previousIndex = FieldRepetitions.IndexOf(pFr);

            foreach (var f in FieldRepetitions)
            {
                if (f.Id > previousIndex)
                {
                    f.Id++;
                }
            }

            FieldRepetitions.Insert(previousIndex, fieldRepetition);

            return fieldRepetition;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public HL7V2FieldRepetition UpdateFieldRepetition(int id, string value)
        {
            if (id >= FieldRepetitions.Max(fr => fr.Id) || id <= 0)
            {
                return null;
            }

            var fr = FieldRepetitions.FirstOrDefault(f => f.Id.Equals(id));

            if (fr == null)
            {
                return null;
            }

            fr.Value = value;

            return fr;
        }
        #endregion
    }
}