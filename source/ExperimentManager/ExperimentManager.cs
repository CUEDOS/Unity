using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class ExperimentManager : MonoBehaviour
{
    [Serializable]
    public class IndependentVariable
    {
        public string name;
        public string longName;
        public float min;
        public float step;
        public float max;
        public float[] range;
        public float output;
        public int scriptId;
    }
    [Serializable]
    public class DependentVariable
    {
        public string name;
        public string longName;
        public Type dataType;
        public int num;
        public float output;
        public int scriptId;
    }

    [FormerlySerializedAs("TargetExperiment")] public MonoBehaviour[] targetExperiment;
    [FormerlySerializedAs("IndependentVariables")] public IndependentVariable[] independentVariables = new IndependentVariable[1];
    [FormerlySerializedAs("DependentVariables")] public DependentVariable[] dependentVariables = new DependentVariable[1];
    
    [Serializable]
    public enum Type {Average, Final, Max, Min};

    [FormerlySerializedAs("ExperimentTime")] public float experimentTime;
    [FormerlySerializedAs("FileName")] public string fileName;
    [FormerlySerializedAs("Directory")] public string directory;
    
    private Queue<Dictionary<string,float>> _experiments;
    private float _nextTime;
    private int _totalNumberOfExperiments;
    private int _numberOfExperiments;
    private string _path;

    private void Start()
    {
        _path = directory + "/" + fileName + ".csv";
        if (File.Exists(_path))
        {
            Debug.Log("Found existing file, Pausing...");
            Debug.Break();
            
        }
        else
        {
            // Create the new file
            var f = File.Create(_path);
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
        foreach (var dv in dependentVariables)
        {
            float value;
            switch (dv.dataType)
            {
                case Type.Average:
                    value = (float) targetExperiment[dv.scriptId].GetType().GetField(dv.name).GetValue(targetExperiment[dv.scriptId]);
                    if (!float.IsNaN(value))
                    {
                        dv.num++;
                        dv.output += (value - dv.output) / dv.num; 
                    }
                    
                    break;
                case Type.Final:
                    break;
                case Type.Max:
                    value = (float) targetExperiment[dv.scriptId].GetType().GetField(dv.name).GetValue(targetExperiment[dv.scriptId]);
                    if (value > dv.output)
                    {
                        dv.output = value;
                    }
                    break;
                case Type.Min:
                    value = (float) targetExperiment[dv.scriptId].GetType().GetField(dv.name).GetValue(targetExperiment[dv.scriptId]);
                    if (value < dv.output)
                    {
                        dv.output = value;
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
        var variables = _experiments.Dequeue();
        foreach (var t in targetExperiment)
        {
            t.SendMessage("ResetExperiment", variables);
        }
        _nextTime = Time.time + experimentTime;
        
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

    private void AddHeaders()
    {
        string headers = "";
        foreach (var t in independentVariables)
        {
            headers += $"{t.name}, ";
        }
        foreach (var t in dependentVariables)
        {
            headers += $"{t.name}, ";
        }
        headers += "expRunTime\n";

        File.AppendAllText(_path, headers);
    }
    
    private void Save()
    {
        var line = independentVariables.Aggregate("", (current, t) 
            => current + $"{t.output}, ");
        line = dependentVariables.Aggregate(line, (current, t) 
            => current + $"{t.output}, ");
        line += experimentTime + "\n";
        
        File.AppendAllText(_path, line); // If there is an error here, check you don't have the file open elsewhere
     
    }

    private void ResetOutputs()
    {
        var variables = _experiments.Peek();
        foreach (var t in independentVariables)
        {
            t.output = variables[t.name];
        }
        
        foreach (var dv in dependentVariables)
        {
            switch (dv.dataType)
            {
                case Type.Average:
                    dv.num = 0;
                    break;
                case Type.Final:
                    break;
                case Type.Max:
                    dv.output = float.NegativeInfinity;
                    break;
                case Type.Min:
                    dv.output = float.PositiveInfinity;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void ComputeOutputs()
    {
        foreach (var dv in dependentVariables)
        {
            switch (dv.dataType)
            {
                case Type.Average:
                    break;
                case Type.Final:
                    dv.output = (float) targetExperiment[dv.scriptId].GetType().GetField(dv.name).GetValue(targetExperiment[dv.scriptId]);
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

    private void CheckAllVariables()
    {
        foreach (var dv in dependentVariables)
        {
            try
            {
                var value = (float) targetExperiment[dv.scriptId].GetType().GetField(dv.name).GetValue(targetExperiment[dv.scriptId]);
            }catch (Exception)
            {
                Debug.LogError("A variable was unable to be accessed: " + dv.name +
                               " - ensure it is set as a public variable.");
            }
        }

        try
        {
            foreach (var t in targetExperiment)
            {
                t.SendMessage("ResetExperiment", 
                    new Dictionary<string, float>());
            }

        }
        catch (Exception)
        {
            Debug.LogError("Unable to use Reset Function. Ensure the reset " +
                           "function is spelt correctly and takes a dictionary as its argument. ");
        }
    }

    private void DefineExperimentList()
    {
        _experiments = new Queue<Dictionary<string, float>>();
        var exps = FindNumShapeExperiments();
        var shape = exps.Item2;
        var indicies = new int[independentVariables.Length];
        _totalNumberOfExperiments = exps.Item1;
        
        for (int i = 0; i < _totalNumberOfExperiments; i++)
        {
            var dict = new Dictionary<string,float>();
            for (var j = 0; j < independentVariables.Length; j++)
            {
                dict.Add(independentVariables[j].name, independentVariables[j].range[indicies[j]]);
            }
            
            _experiments.Enqueue(dict);
            indicies = IncrementIndicies(indicies, shape);
        }

    }

    private int[] IncrementIndicies(int[] ind, int[] shape)
    {
        for (var i = 0; i < ind.Length; i++)
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

    public (int, int[]) FindNumShapeExperiments()
    {
        var num = 1;
        int[] shape = new int[independentVariables.Length];
        for (int i = 0; i < independentVariables.Length; i++)
        {
            num *= independentVariables[i].range.Length;

            shape[i] = independentVariables[i].range.Length;
        }
        
        return (num, shape);
    }
    
    public void FindRanges()
    {
        foreach (var iv in independentVariables)
        {
            iv.range = FindRange(iv);
        }
    }

    private float[] FindRange(IndependentVariable iv)
    {
        var subRange = iv.max - iv.min;
        var numSteps = Mathf.CeilToInt(subRange / iv.step) + 1;
        var range = new float[numSteps];
        for (var i = 0; i < numSteps; i++)
        {
            var num = i * iv.step + iv.min;
            if (num > iv.max)
            {
                range[i] = iv.max;
            }
            else
            {
                range[i] = num;
            }
        }
        return range;
    }
}
