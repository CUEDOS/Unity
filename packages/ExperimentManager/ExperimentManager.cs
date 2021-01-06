using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Reflection;
using UnityEngine;
using Samspace;

namespace Samspace
{
    public class ExperimentManager : MonoBehaviour
    {
        [System.Serializable]
        public class IndependentVariable
        {
            public string Name;
            public float MIN;
            public float Step;
            public float MAX;
            public float[] Range;
            public float Output;
        }
        [System.Serializable]
        public class DependentVariable
        {
            public string Name;
            public Type DataType;
            public List<float> _data;
            public float Output;
        }

        public MonoBehaviour targetExperiment;
        public IndependentVariable[] IndependentVariables = new IndependentVariable[1];
        public DependentVariable[] DependentVariables = new DependentVariable[1];
        
        [System.Serializable]
        public enum Type {Average, Final, Max, Min};

        public float ExperimentTime;
        public string FileName;
        public string Directory;
        
        private Queue<Dictionary<string,float>> _experiments;
        private float _nextTime;
        private int _totalNumberOfExperiments;
        private int _numberOfExperiments;
        private string _path;

        private void Start()
        {
            _path = Directory + "/" + FileName + ".csv";
            if (File.Exists(_path))
            {
                Debug.Log("Found existing file, Pausing...");
                Debug.Break();
                return;
            }
            else
            {
                // Create the new file
                FileStream f = File.Create(_path);
                f.Close();
                AddHeaders();
            }

            CheckAllVariables();
            FindRanges();
            DefineExperimentList();
            ExperimentStart();
        }

        private void FixedUpdate()
        {
            float value;
                
            foreach (var dv in DependentVariables)
            {
                switch (dv.DataType)
                {
                    case Type.Average:
                        dv._data.Add((float) targetExperiment.GetType().GetField(dv.Name).GetValue(targetExperiment)); 
                        break;
                    case Type.Final:
                        break;
                    case Type.Max:
                        value = (float) targetExperiment.GetType().GetField(dv.Name).GetValue(targetExperiment);
                        if (value > dv.Output)
                        {
                            dv.Output = value;
                        }
                        break;
                    case Type.Min:
                        value = (float) targetExperiment.GetType().GetField(dv.Name).GetValue(targetExperiment);
                        if (value < dv.Output)
                        {
                            dv.Output = value;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
            if (Time.time > _nextTime)
            {
                ExperimentEnd();
            }
        }

        private void ExperimentStart()
        {
            ResetOutputs();
            
            targetExperiment.SendMessage("ResetExperiment", _experiments.Dequeue());
            _nextTime = Time.time + ExperimentTime;

        }

        private void ExperimentEnd()
        {
            ComputeOutputs();
            Save();
            
            if (_totalNumberOfExperiments - 1 == _numberOfExperiments++)
            {
                Debug.Break();
                _nextTime = float.PositiveInfinity;
                return;
            }

            ExperimentStart();
        }

        void AddHeaders()
        {
            string headers = "";
            foreach (var t in IndependentVariables)
            {
                headers += $"{t.Name}, ";
            }
            foreach (var t in DependentVariables)
            {
                headers += $"{t.Name}, ";
            }
            headers += "\n";

            File.AppendAllText(_path, headers);
        }
        private void Save()
        {
            string line = "";
            foreach (var t in IndependentVariables)
            {
                line += $"{t.Output}, ";
            }
            foreach (var t in DependentVariables)
            {
                line += $"{t.Output}, ";
            }
            line += "\n";
            
            
            File.AppendAllText(_path, line);
        }

        void ResetOutputs()
        {
            var variables = _experiments.Peek();
            foreach (var t in IndependentVariables)
            {
                t.Output = variables[t.Name];
            }
            
            foreach (var dv in DependentVariables)
            {
                switch (dv.DataType)
                {
                    case Type.Average:
                        dv._data = new List<float>();
                        break;
                    case Type.Final:
                        break;
                    case Type.Max:
                        dv.Output = float.NegativeInfinity;
                        break;
                    case Type.Min:
                        dv.Output = float.PositiveInfinity;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
            }
        }

        void ComputeOutputs()
        {
            foreach (var dv in DependentVariables)
            {
                switch (dv.DataType)
                {
                    case Type.Average:
                        float sum = 0;
                        int rem = 0;
                        foreach (var d in dv._data)
                        {
                            if (!float.IsNaN(d))
                            {
                                sum += d;
                            }
                            else
                            {
                                rem++;
                            }
                            
                        }
                        dv.Output = sum / (dv._data.Count - rem);
                        break;
                    case Type.Final:
                        dv.Output = (float) targetExperiment.GetType().GetField(dv.Name).GetValue(targetExperiment);
                        break;
                    case Type.Max:
                        break;
                    case Type.Min:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        void CheckAllVariables()
        {
            foreach (var dv in DependentVariables)
            {
                try
                {
                    var value = (float) targetExperiment.GetType().GetField(dv.Name).GetValue(targetExperiment);
                }catch (Exception)
                {
                    Debug.LogError("An Independent Variable was unable to be accessed, ensure it is set as a public variable.");
                }
            }

            try
            {
                targetExperiment.SendMessage("ResetExperiment", new Dictionary<string, float>());
            }
            catch (Exception)
            {
                Debug.LogError("Unable to use Reset Function. Ensure the reset function is spelt correctly and takes a dictionary as its argument. ");
            }
        }
        void DefineExperimentList()
        {
            _experiments = new Queue<Dictionary<string, float>>();
            var exps = FindNumShapeExperiments();
            var shape = exps.Item2;
            var indicies = new int[IndependentVariables.Length];
            _totalNumberOfExperiments = exps.Item1;
            
            for (int i = 0; i < _totalNumberOfExperiments; i++)
            {
                var dict = new Dictionary<string,float>();
                for (int j = 0; j < IndependentVariables.Length; j++)
                {
                    dict.Add(IndependentVariables[j].Name, IndependentVariables[j].Range[indicies[j]]);
                }
                
                _experiments.Enqueue(dict);
                indicies = IncrementIndicies(indicies, shape);
            }

        }

        int[] IncrementIndicies(int[] ind, int[] shape)
        {
            for (int i = 0; i < ind.Length; i++)
            {
                ind[i]++;                
                if (ind[i] == shape[i])
                {
                    ind[i] = 0;
                }
                else
                {
                    return ind;
                }
            }

            return ind;
        }
        

        (int, int[]) FindNumShapeExperiments()
        {
            var num = 1;
            int[] shape = new int[IndependentVariables.Length];
            for (int i = 0; i < IndependentVariables.Length; i++)
            {
                num *= IndependentVariables[i].Range.Length;

                shape[i] = IndependentVariables[i].Range.Length;
            }
            

            return (num, shape);
        }
        private void FindRanges()
        {
            foreach (var iv in IndependentVariables)
            {
                iv.Range = FindRange(iv);
            }
        }

        private float[] FindRange(IndependentVariable iv)
        {
            float subRange = iv.MAX - iv.MIN;
            int numSteps = Mathf.CeilToInt(subRange / iv.Step) + 1;
            float[] Range = new float[numSteps];
            for (int i = 0; i < numSteps; i++)
            {
                var num = i * iv.Step + iv.MIN;
                if (num > iv.MAX)
                {
                    Range[i] = iv.MAX;
                }
                else
                {
                    Range[i] = num;
                }
            }

            return Range;
        }
        
        
    }
    
}